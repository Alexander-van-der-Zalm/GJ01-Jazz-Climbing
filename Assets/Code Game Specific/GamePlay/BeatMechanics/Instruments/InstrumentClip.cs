using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class InstrumentClip 
{
    [AudioManagerLibrarySample]
    public string SampleName;
    public int BeatTarget;
    public float range;
    public bool IsCritical;
    [Range(0,1)]
    public float Volume;
    [Range(0,10)]
    public float MinDuration;
}
