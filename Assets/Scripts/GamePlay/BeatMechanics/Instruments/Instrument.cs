﻿using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Instrument : MonoBehaviour 
{
    public enum InstrumentActivationMode
    {
        DurationSoundClip,
        DurationMinimumFromInstrumentClip,
        SinglePressOverride,
        //SinglePressOverrideCD,
        MultiplePresses
    }

    private enum TimingType
    {
        Critical,
        Normal,
        Low
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
    private TimingType timingType;

    public void ActivateInstrument()
    {
        if (isPlaying)
            return;

        InstrumentClip clip = GetInstrumentClip();

        switch (ActivationMode)
        {
            case InstrumentActivationMode.DurationSoundClip:
                ActivateDurationSoundClip(clip);
                break;
            case InstrumentActivationMode.DurationMinimumFromInstrumentClip:
                ActivateDurationInstrumentClip(clip);
                break;
            case InstrumentActivationMode.SinglePressOverride:
                ActivateSingleOverride(clip);
                break;
            default:
                Debug.Log("Sorry not Implemented yet");
                break;
        }
    }

    #region GetInstrumentClip

    private InstrumentClip GetInstrumentClip()
    {
        int beat = (int)Mathf.Round(GlobalBeat.ProgressInMeasure());
        float dist = GlobalBeat.SecondsFromBeat();

        Debug.Log("Range: " + dist);

        // Clips in range
        // Filter out the farther ones?
        List<InstrumentClip> clips = Sounds.Where(c => dist < c.range).ToList();
        
        if (AClipForEveryBeat)
            clips = clips.Where(c => c.BeatTarget == beat).ToList();

        
        // Check for criticals (else just return the same list)
        clips = ReturnCritsIfCritical(clips);


        // Play a different clip than the last one if its possible
        List<InstrumentClip> alternatives = clips.Where(c => c.SampleName != LastSampleName).ToList();//.Where(c => c != lastInstrumentClip).ToList();
        if (alternatives.Count > 0)
            clips = alternatives;

        //Debug.Log("alternatives: " + alternatives.Count + " " + LastSampleName + " " + dist + " ");
        //DebugList<string>(alternatives.Select(c => c.SampleName).ToList());

        // Find a random clip from the list
        InstrumentClip clip = clips[Random.Range(0, clips.Count)];

        if (clip.IsCritical)
            timingType = TimingType.Critical;
        else
            timingType = TimingType.Normal;
        //No low yet

        return clip;
    }

    private void DebugList<T>(List<T> list)
    {
        int i = 0;
        foreach (T t in list)
        {
            Debug.Log(i + "st in list: " + t.ToString());
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

#endregion

    #region PlayCLip


    private AudioSourceContainer PlayClip(InstrumentClip clip)
    {
        // Find & PlayClip
        AudioSample sample = AudioManager.FindSampleFromCurrentLibrary(clip.SampleName);

        // Seperate the play from the clip
        AudioSourceContainer container = AudioManager.Play(sample);

        Debug.Log("Play " + sample.Name);

        lastInstrumentClip = clip;
        LastSampleName = clip.SampleName;
        isPlaying = true;

        container.VolumeModifier = clip.Volume;
        //cont

        return container;
    }

    
    #endregion

    #region Activate Single Override

    private void ActivateSingleOverride(InstrumentClip clip)
    {
        // Find & StopClip
        if (lastInstrumentClip != null)
        {
            AudioSample sample = AudioManager.FindSampleFromCurrentLibrary(lastInstrumentClip.SampleName);
            AudioManager.Stop(sample);
        }

        AudioSourceContainer container = PlayClip(clip);
        isPlaying = false;
    }

    #endregion

    #region Duration Minimum InstrumentClip

    private void ActivateDurationInstrumentClip(InstrumentClip clip)
    {
        StartCoroutine(ActivateDurationInstrumentClipCR(clip));
    }

    private IEnumerator ActivateDurationInstrumentClipCR(InstrumentClip clip)
    {
        //if (isPlaying)
        //    yield break;

        AudioSourceContainer container = PlayClip(clip);

        float dt = 0;

        while (dt < clip.MinDuration)
        {
            dt += Time.deltaTime;
            yield return null;
        }

        Debug.Log("New Clip Possible");
        isPlaying = false;
    }

    #endregion

    #region Duration SoundClip

    private void ActivateDurationSoundClip(InstrumentClip clip)
    {
        StartCoroutine(ActivateDurationSoundClipCR(clip));
    }

    private IEnumerator ActivateDurationSoundClipCR(InstrumentClip clip)
    {
        //if (isPlaying)
        //    yield break;
        
        AudioSourceContainer container = PlayClip(clip);

        while (container.AudioSource.isPlaying)
            yield return null;

        //Debug.Log("Not Playing");
        isPlaying = false;
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
