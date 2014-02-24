using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AudioLibrary : MonoBehaviour 
{
    // AudioSamples
    public List<AudioSample> Samples = new List<AudioSample>();

    public List<string> SampleNames { get { return Samples.Select(s => s.Name).ToList<string>(); } }
}
