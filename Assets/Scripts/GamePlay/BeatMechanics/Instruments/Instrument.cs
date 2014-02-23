using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Instrument : MonoBehaviour 
{
    public enum InstrumentActivationMode
    {
        DurationSoundClip,
        SinglePress,
        MultiplePresses
    }


    public List<InstrumentClip> Sounds;
    public float Damage;
    public float CriticalModifier;
    public int Cost;
    public bool AClipForEveryBeat;
    public InstrumentActivationMode ActivationMode;
    //public delegate void ActivateDel();
    //public ActivateDel ActivateDelegate;

    private string LastSampleName;
    private InstrumentClip lastInstrumentClip;

    private bool isPlaying = false;

    public void ActivateInstrument()
    {
        InstrumentClip clip = GetInstrumentClip();

        switch (ActivationMode)
        {
            case InstrumentActivationMode.DurationSoundClip:
                ActivateDurationSoundClip(clip);
                break;
            default:
                Debug.Log("Sorry not Implemented yet");
                break;
        }
    }


    #region SoundActivate

    private InstrumentClip GetInstrumentClip()
    {
        int beat = (int)Mathf.Round(GlobalBeat.ProgressInMeasure());
        float dist = GlobalBeat.SecondsFromBeat();
        
        // Clips in range
        // Filter out the farther ones?
        List<InstrumentClip> clips = Sounds.Where(c => dist < c.range).ToList();
        
        if (AClipForEveryBeat)
            clips = clips.Where(c => c.BeatTarget == beat).ToList();

        Debug.Log("bf crits: " + dist + " " + clips.Count);

        // Check for criticals (else just return the same list)
        //clips = ReturnCritsIfCritical(clips);


        // Play a different clip than the last one if its possible
        List<InstrumentClip> alternatives = clips.Where(c => c.SampleName != LastSampleName).ToList();//.Where(c => c != lastInstrumentClip).ToList();
        if (alternatives.Count > 1)
            clips = alternatives;

        Debug.Log("alternatives: " + dist + " " + clips.Count + " " + LastSampleName);
        //DebugList
        // Find a random clip from the list
        InstrumentClip clip = clips[Random.Range(0, clips.Count)];

        return clip;
    }

    private void DebugList<T>(List<T> list)
    {
        int i = 0;
        foreach (T t in list)
        {
            Debug.Log(i + " " + t.ToString());
            i++;
        }
    }

    /// <summary>
    /// Returns crits, else just return the same list
    /// </summary>
    private List<InstrumentClip> ReturnCritsIfCritical(List<InstrumentClip> clips)
    {
        List<InstrumentClip> crits = clips.Where(c => c.IsCritical == true).ToList();
        if (crits.Count() > 0)
            clips = crits;
        return clips;
    }


    private AudioSourceContainer PlayClip(InstrumentClip clip)
    {
        // Find & PlayClip
        AudioSample sample = AudioManager.FindSampleFromCurrentLibrary(clip.SampleName);

        // Seperate the play from the clip
        AudioSourceContainer container = AudioManager.Play(sample);

        lastInstrumentClip = clip;
        LastSampleName = clip.SampleName;
        isPlaying = true;

        return container;
    }

    
    #endregion

    #region Duration Activate

    private IEnumerator ActivateDurationSoundClipCR(InstrumentClip clip)
    {
        if (isPlaying)
            yield break;
        
        AudioSourceContainer container = PlayClip(clip);

        while (container.AudioSource.isPlaying)
            yield return null;

        //GameObject.DestroyImmediate(container.gameObject);
        isPlaying = false;
    }

    private void ActivateDurationSoundClip(InstrumentClip clip)
    {
        StartCoroutine(ActivateDurationSoundClipCR(clip));
    }

    #endregion

    // Use this for initialization
	void Start () 
    {
	    
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collision");
    }
}
