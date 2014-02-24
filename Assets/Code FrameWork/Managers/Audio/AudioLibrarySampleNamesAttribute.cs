using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioLibrarySampleNamesAttribute : PropertyAttribute 
{
    public readonly List<string> list;

    public AudioLibrarySampleNamesAttribute()//List<string> list)
    {
        this.list = AudioManager.Instance.AudioLibrary.SampleNames;
        //this.list = list;
    }
}
