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

    public static bool StartBeat(string currentZoneName = "")
    {
        if (Instance.started && Instance.CurrentZoneName == currentZoneName)
            return false;
        
        Instance.started = true;
        Instance.StartTime = Time.realtimeSinceStartup;
        Instance.CurrentZoneName = currentZoneName;

        return true;
    }

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
