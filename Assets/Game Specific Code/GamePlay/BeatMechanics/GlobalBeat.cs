using UnityEngine;
using System.Collections;

public class GlobalBeat : Singleton<GlobalBeat> 
{
    public int BPM;
    public int Measure;
    public int Accent;
    public string CurrentZoneName = "";

    public float InMeasure;
    public float SecDistanceToAccent;
    public float SecDisToBeat;

    private float StartTime;
    private bool started = false;
    private bool doingXF = false;
    private AudioSample oldSample;

    public static bool StartBeat(string currentZoneName, AudioSample sample, float crossFadeDurationInMeasures)
    {
        if (Instance.started && Instance.CurrentZoneName == currentZoneName)
            return false;

        //// TODO stop
        // FadeOut
        //if (sample == null)
        //    Instance.started = false;

        if (Instance.started && !Instance.doingXF)
        {
            // CrossFade
            Instance.StartCoroutine(Instance.CrossFadeOnBeat(currentZoneName, sample, crossFadeDurationInMeasures));
        }
        else if(!Instance.doingXF)
        {
            // No Crossfade
            AudioManager.Play(sample);
            BeatStarted(currentZoneName, sample);
        }

        return true;
    }

    private static void BeatStarted(string currentZoneName, AudioSample sample)
    {
        Instance.StartTime = Time.realtimeSinceStartup;
        Instance.CurrentZoneName = currentZoneName;
        Instance.started = true;
        Instance.oldSample = sample;
    }

    private IEnumerator CrossFadeOnBeat(string currentZoneName, AudioSample sample, float crossFadeDurationInMeasures)
    {
        int beatInMeasure = (int)Mathf.Round(ProgressInMeasure());
        float dist = SecondsFromBeat();
        Instance.doingXF = true;
        bool keepWaiting = true;
        float lastDist = float.MaxValue;

        while (keepWaiting)
        {
            beatInMeasure = (int)Mathf.Round(ProgressInMeasure());
            lastDist = dist;
            dist = SecondsFromBeat();

            if(beatInMeasure == 0 && lastDist < dist)
            {
                //Debug.Log("XF " + beatInMeasure + " " + dist + " old samp " + Instance.oldSample.Clip.name + " samp " + sample.Clip.name);
                break;
            }

            
            yield return null;
        }

        //Debug.Log("XF " + beatInMeasure + " " + dist + " old samp " + Instance.oldSample.Clip.name + " samp " + sample.Clip.name);
        float crossFadeTime = (crossFadeDurationInMeasures * Measure) / (BPM / 60);
        //Debug.Log("XFT " + crossFadeTime);
        AudioManager.CrossFade(Instance.oldSample, sample, crossFadeTime);
        Instance.doingXF = false;
        BeatStarted(currentZoneName, sample);
    }



    //private IEnumerator CrossFadeOnBeat(

    public static float ProgressInMeasure()
    {
        float dt = InBeat();

        // How far into the measure?
        return dt % Instance.Measure;
    }

    public static float SecondsFromAccent()
    {
        float dt = InBeat();
        float cur = dt % Instance.Accent;

        return Mathf.Min(cur, Instance.Accent-cur);
    }

    public static float SecondsFromBeat()
    {
        float dt = InBeat();
        float cur = dt % 1;

        return Mathf.Min(cur, 1 - cur);
    }

    private static float InBeat()
    {
        if (!Instance.started)
            return 0;
        
        float dt = Time.realtimeSinceStartup - Instance.StartTime;
        dt *= Instance.BPM / 60;
        return dt;
    }

    void Start()
    {
        //GlobalBeat.StartBeat();
    }

    void Update()
    {
        InMeasure = GlobalBeat.ProgressInMeasure();
        SecDistanceToAccent = GlobalBeat.SecondsFromAccent();
        SecDisToBeat = GlobalBeat.SecondsFromBeat();
    }


}
