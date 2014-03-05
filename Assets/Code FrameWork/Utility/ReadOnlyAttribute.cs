using UnityEngine;
using System.Collections;

public class ReadOnlyAttribute : PropertyAttribute 
{
    //public readonly List<string> list;

    public ReadOnlyAttribute()//List<string> list)
    {
       // this.list = AudioManager.Instance.AudioLibrary.SampleNames;
        //this.list = list;
    }
}
