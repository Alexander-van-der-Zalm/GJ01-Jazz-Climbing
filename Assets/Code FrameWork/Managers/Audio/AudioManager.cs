using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Singleton AudioManager
[RequireComponent(typeof(AudioLayerManager))]
public class AudioManager : Singleton<AudioManager>
{
    #region fields

    // AudioSources    
    // Find by lambda:
    // - Layer (m)
    // - Clip (m)
    // - Sample (m)
    // - ID (s)
    // - ETC.
    public List<AudioSourceContainer> AudioSources { get; private set; }
    
    public AudioLayerManager AudioLayerManager;
    public AudioLibrary AudioLibrary;



    #endregion

    #region Instantiate

    public void Awake()
    {
        AudioSources = new List<AudioSourceContainer>();
    }

    #endregion

    // Functions

    #region Stop

    /// <summary>
    /// Stop all sound
    /// </summary>
    public static void Stop() { Stop(Instance.AudioSources); }
    public static void Stop(int AudioSourceID) { Stop(Instance.FindSource(AudioSourceID)); }
    public static void Stop(AudioLayer layer) { Stop(Instance.FindSources(layer)); }
    public static void Stop(AudioSample sample) { Stop(Instance.FindSources(sample)); }
    public static void Stop(AudioClip clip) { Stop(Instance.FindSources(clip)); }
   
    public static void Stop(List<AudioSourceContainer> sources) 
    { 
        foreach(AudioSourceContainer source in sources)
        {
            source.AudioSource.Stop();
        }
    }
    public static void Stop(AudioSource source) { source.Stop(); }
    public static void Stop(AudioSourceContainer source) { source.AudioSource.Stop(); }

    #endregion

    #region Play

    // Three types:
    // Transform based (3D Sound)
    // Point or Vector3 Based (3D Sound)
    // None (Directly to the main audio source (2D sound)
    
    
    /// <summary>
    /// Plays directly at the cameras audioSource
    /// </summary>
    /// <param name="sample"></param>
    public static AudioSourceContainer Play(AudioSample sample)
    {
        // There should only be one audioListener (Usually main camera)
        Transform audioListenerTransform = GameObject.FindObjectOfType<AudioListener>().transform;
        return Play(sample, audioListenerTransform);
    }

    /// <summary>
    /// Parents to the transform (hence follows it)
    /// </summary>
    /// <param name="sample"></param>
    /// <param name="transform"></param>
    public static AudioSourceContainer Play(AudioSample sample, Transform transform)
    {
        AudioSourceContainer soundObject = RegisterAndCreateAudioSourceContainer(sample);
        soundObject.transform.parent = transform;
        soundObject.transform.position = transform.position;

        return Play(soundObject);
    }

    public static AudioSourceContainer Play(AudioSample sample, Vector3 position)
    {
        AudioSourceContainer soundObject = RegisterAndCreateAudioSourceContainer(sample);
        soundObject.transform.position = position;
        
        return Play(soundObject);
    }

    public static AudioSourceContainer Play(AudioClip clip, AudioLayer layer = AudioLayer.None, AudioSample.AudioSettings settings = null) 
    {
        AudioSample sample = new AudioSample();
        sample.Clip = clip;
        sample.Layer = layer;
        sample.Settings = settings;

        // There should only be one audioListener (Usually main camera)
        Transform audioListenerTransform = GameObject.FindObjectOfType<AudioListener>().transform;
        return Play(sample, audioListenerTransform);
    }

    private static AudioSourceContainer Play(AudioSourceContainer audioSource)
    {
        audioSource.AudioSource.Play();
        return audioSource;
    }
   

    #endregion

    #region Pause

    public static void Pause()
    {
        AudioListener.pause = true;
    }

    #endregion

    #region Resume

    public static void Resume()
    {
        AudioListener.pause = false;
    }

    #endregion

    #region Mute

    public static void Mute(AudioLayer layer)
    {
        AudioLayerSettings settings = AudioLayerManager.GetAudioLayerSettings(layer);
        List<AudioSourceContainer> containers = Instance.FindSources(layer);
        foreach (AudioSourceContainer cont in containers)
        {
            cont.AudioSource.mute = settings.Mute;
        }
    }

    #endregion

    #region Seek

    #endregion

    //public static void UpdateAllLayerSettings(AudioLayer layer)
    //{
    //    Mute(layer);
    //    UpdateVolume(layer);
    //}

    #region CrossFade

    public static void CrossFade(AudioSample sampleFrom, AudioSample sampleTO, float duration)
    {
        Instance.StartCoroutine(crossFadeCR(sampleFrom, sampleTO, duration));
    }

    private static IEnumerator crossFadeCR(AudioSample sampleFrom, AudioSample sampleTO, float duration)
    {
        float dt = 0;
        AudioSourceContainer con1 = FindOrPlay(sampleFrom);
        AudioSourceContainer con2 = FindOrPlay(sampleTO);

        float v1 = con1.VolumeModifier;//con1.AudioSource.volume;
        float v2 = con2.VolumeModifier;//con2.AudioSource.volume;

        ResumeUnmuteEtc(con1);
        ResumeUnmuteEtc(con2);
        
        con2.AudioSource.Play();

        //Debug.Log("XF in AM: " + con1.VolumeModifier + "," + con2.VolumeModifier + " dur " + duration + " dt " + dt + " " + con1.gameObject.GetInstanceID() + ", " + con2.gameObject.GetInstanceID());

        float startTime = Time.timeSinceLevelLoad;

        while (dt < duration)
        {
            float t = dt / duration;

            con1.VolumeModifier = Mathf.Max((1 - t),0) * v1;
            con2.VolumeModifier = Mathf.Min(t,1)* v2;

            //Debug.Log(con1.AudioSource.volume + " " + con2.AudioSource.volume);

            dt = Time.timeSinceLevelLoad - startTime;
            yield return null;
        }

        //Debug.Log("XF FIN in AM: " + con1.VolumeModifier + "," + con2.VolumeModifier + " dur " + duration + " dt " + dt);
        con2.VolumeModifier = v2;
        con1.VolumeModifier = v1;
        con1.AudioSource.mute = true;

        MuteAndDestroyAfter(con1, 5.0f);

        //con1.AudioSource.volume = 0;

        //con1.AudioSource.Stop();
    }

    private static void MuteAndDestroyAfter(AudioSourceContainer source, float time)
    {
        Instance.StartCoroutine(MuteAndDestroyAfterCR(source, time));
    }

    private static IEnumerator MuteAndDestroyAfterCR(AudioSourceContainer source, float time)
    {
        source.AudioSource.mute = true;
        float dt = 0;

        //Debug.Log("MuteAndDestroyAfterCR");

        while (dt < time && source.AudioSource.mute)
        {
            dt += Time.deltaTime;
            yield return null;
        }

        if (source.AudioSource.mute)
            Instance.Destroy(source);
        
        yield break;
    }



    private static void ResumeUnmuteEtc(AudioSourceContainer con1)
    {
        if (!con1.AudioSource.isPlaying)
            con1.AudioSource.Play();

        AudioLayerSettings settings = AudioLayerManager.GetAudioLayerSettings(con1.Layer);
        if(!settings.Mute)
            con1.AudioSource.mute = false;
    }

    #endregion

    private static AudioSourceContainer FindOrPlay(AudioSample sample)//, Transform tr = null, Vector3 pos = null)
    {
        List<AudioSourceContainer> conts = Instance.FindSources(sample);
        AudioSourceContainer cont;

        if (conts.Count > 0)
        {
            cont = conts.First();
            //Debug.Log("FOUND");
        }
        else
        {
            cont = Play(sample);
            
            //Debug.Log("PLAY");
        }

        return cont;
    }

    public static void UpdateVolume(AudioLayer layer)
    {
        AudioLayerSettings settings = AudioLayerManager.GetAudioLayerSettings(layer);
        List<AudioSourceContainer> containers = Instance.FindSources(layer);
        foreach (AudioSourceContainer cont in containers)
        {
            cont.UpdateVolume();
        }
    }
    
    public static void Seek() { }
    public static void Transition() { }
    
    public static void ChangeVolume() 
    { 

    }

    //public static void Lerp(AudioSample clip1, AudioSample clip2, float t) { }

    //private 
    public static AudioSample FindSampleFromCurrentLibrary(string name)
    {
        return Instance.AudioLibrary.Samples.Where(t => t.Name == name).First();
    }


    #region Audio Source Management

    #region Add & Destroy Coroutine

    private static AudioSourceContainer RegisterAndCreateAudioSourceContainer(AudioSample sample)
    {
        GameObject soundObject = AudioSourceContainer.CreateContainer(sample);
        
        AudioSourceContainer cont = soundObject.GetComponent<AudioSourceContainer>();
        AudioLayerSettings audioLayerSettings = AudioLayerManager.GetAudioLayerSettings(sample.Layer);
       
        //Register
        Instance.AddAudioSource(cont);
        audioLayerSettings.ClipsPlaying++;
        
        return cont;
    }

    private void AddAudioSource(AudioSourceContainer source)
    {
        // Handle destroy
        // Handle limit
        StartCoroutine(AddAudioSourceCR(source));
    }

    /// <summary>
    /// Garbage collection, a coroutine that keeps on running till the object is destroyed
    /// </summary>
    private IEnumerator AddAudioSourceCR(AudioSourceContainer source)
    {
        Instance.AudioSources.Add(source);

        float TimeStopped = 0;

        // Keep Running till it has stopped for one second
        while (TimeStopped < 0.5f)
        {
            if (source == null)
                yield break;

            if (source.AudioSource.time == 0)
                TimeStopped += Time.deltaTime;
            else
                TimeStopped = 0;
            
            yield return null;
        }

        Instance.Destroy(source);
        //source = null;
    }

    #endregion

    // AudioSources    
    // Find by lambda:
    // - ID (s)
    // - Layer (m)
    // - Clip (m)
    // - Sample (m)
    private AudioSourceContainer FindSource(int AudioSourceID) { return AudioSources.Where(a => a.AudioSource.GetInstanceID() == AudioSourceID).First(); }

    private List<AudioSourceContainer> FindSources(AudioLayer layer) { return AudioSources.Where(a => a.Layer == layer).ToList(); }
    private List<AudioSourceContainer> FindSources(AudioClip clip) { return AudioSources.Where(a => a.AudioSource.clip == clip).ToList(); }
    private List<AudioSourceContainer> FindSources(AudioSample sample) { return AudioSources.Where(a => a.Sample == sample).ToList(); }

    #endregion


    #region Remove

    private void Destroy(AudioSourceContainer source)
    {
        //Debug.Log("DESTROY " + source.gameObject.name);

        AudioLayerSettings settings = AudioLayerManager.GetAudioLayerSettings(source.Layer);

        Instance.AudioSources.Remove(source);
        GameObject.DestroyImmediate(source.gameObject);
        settings.ClipsPlaying--;
    }

    #endregion

}
