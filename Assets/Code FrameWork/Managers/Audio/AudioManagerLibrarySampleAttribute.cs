using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManagerLibrarySampleAttribute : PropertyAttribute 
{
    public readonly List<string> list;

    public AudioManagerLibrarySampleAttribute()//List<string> list)
    {
        this.list = AudioManager.Instance.AudioLibrary.SampleNames;
        //this.list = list;
    }
}
