using UnityEngine;
using System.Collections;

public class GlobalBeat : Singleton<GlobalBeat> 
{
    public int BPM;
    public int Measure;
    public int Accent;

    public float InMeasure;

    private float StartTime;

    public static void StartBeat()
    {
        Instance.StartTime = Time.realtimeSinceStartup;
    }

    public static float ProgressInMeasure()
    {
        float dt = Time.realtimeSinceStartup - Instance.StartTime;
        dt *= Instance.BPM / 60;
        return dt % Instance.Measure;
    }

    void Start()
    {
        GlobalBeat.StartBeat();
    }


    void Update()
    {
        InMeasure = GlobalBeat.ProgressInMeasure();
    }
}
