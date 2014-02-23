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


    private InstrumentClip lastInstrumentClip;

    public void ActivateInstrument()
    {
        AudioSourceContainer audioContainer = Activate();
        switch (ActivationMode)
        {
            case InstrumentActivationMode.DurationSoundClip:
                ActivateDurationSoundClip(audioContainer);
                break;
            default:
                Debug.Log("Sorry not Implemented yet");
                break;
        }
    }


    #region SoundActivate

    private AudioSourceContainer Activate()
    {
        int beat = (int)Mathf.Round(GlobalBeat.ProgressInMeasure());
        float dist = GlobalBeat.SecondsFromBeat();
        
        // Clips in range
        // Filter out the farther ones?
        List<InstrumentClip> clips = Sounds.Where(c => dist < c.range).ToList();
        
        if (AClipForEveryBeat)
            clips = clips.Where(c => c.BeatTarget == beat).ToList();

        // Check for criticals (else just return the same list)
        clips = ReturnCritsIfCritical(clips);

        // Play a different clip than the last one if its possible
        if(clips.Count > 1)
            clips = clips.Where(c => c != lastInstrumentClip).ToList();

        // Find a random clip from the list
        InstrumentClip clip = clips[Random.Range(0, clips.Count)];
        
        // Find & PlayClip
        AudioSample sample = AudioManager.FindSampleFromCurrentLibrary(clip.SampleName);
        AudioSourceContainer container = AudioManager.Play(sample);

        lastInstrumentClip = clip;

        return container;
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

    #region Duration Activate

    private IEnumerator ActivateDurationSoundClipCR()
    {
        // Not implemented
        yield return null;
    }

    private void ActivateDurationSoundClip(AudioSourceContainer audioContainer)
    {

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
