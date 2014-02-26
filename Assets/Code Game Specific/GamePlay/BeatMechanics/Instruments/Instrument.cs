using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Instrument : MonoBehaviour
{
    #region Enums

    public enum InstrumentActivationMode
    {
        DurationSoundClip,
        DurationMinimumFromInstrumentClip,
        SinglePressOverride,
        //SinglePressOverrideCD,
        MultiplePresses
    }

    public enum InstrumentCollisionMode
    {
        Direct,
        PulsingOnVolume
    }

    private enum TimingType
    {
        Critical,
        Normal,
        Low
    }

    #endregion

    [System.Serializable]
    public class FlyingNoteSettings
    {
        public FlyingNote Note;
        [Range(0,100)]
        public int NotesPerPulse;
        [Range(0,200)]
        public float InitialVelocity;
        [Range(0,100)]
        public float RandomVelocityOffset;
        [Range(0,180)]
        public float ConeAngle;
        [Range(-180,180)]
        public float OffsetAngle;
    }

    [System.Serializable]
    public class InstrumentSettings
    {
        public float Damage;
        public float CriticalModifier;
        public int Cost;
    }

    #region fields

    public InstrumentSettings Settings;
    public FlyingNoteSettings NoteSettings;

    public List<InstrumentClip> Sounds;
    public bool AClipForEveryBeat;
    public InstrumentActivationMode ActivationMode;
    public InstrumentCollisionMode CollisionMode;

    public Particle Test;

    private string LastSampleName;
    private InstrumentClip lastInstrumentClip;

    private bool isPlaying = false;
    private TimingType timingType;

    #endregion

    #region Main Activate

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
                Debug.Log("Sorry " + CollisionMode.ToString() + "not Implemented yet");
                break;
        }

        switch (CollisionMode)
        {
            case InstrumentCollisionMode.Direct:
                GenerateDirectNote();
                break;
            default:
                Debug.Log("Sorry " + CollisionMode.ToString() + "not Implemented yet");
                break;
        }
    }

    #endregion

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

        //Debug.Log("Play " + sample.Name);

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

        //Debug.Log("New Clip Possible");
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

    #region Instantiate Instrument

    

    #endregion

    #region Generate Direct Note

    private void GenerateDirectNote()
    {
        float baseDir = Mathf.Sign(transform.parent.transform.localScale.x);

        for (int i = 0; i < NoteSettings.NotesPerPulse; i++)
        {
            // Calculate a random vector direction based on the angles in NoteSettings
            Vector2 noteDir = GetRandomAngleFromSettings(baseDir);
            Test.Create();
            NoteSettings.Note.CreateNote(transform.position, noteDir, (int)Settings.Damage);
        }
    }

    #endregion

    private Vector2 GetRandomAngleFromSettings(float baseDirection)
    {
        // Calculate the a random Angle in the range from the noteSettings
        float angle = NoteSettings.OffsetAngle + NoteSettings.ConeAngle * Random.Range(-1.0f, 1.0f);

        // Rotate the baseDirection
        Vector3 noteDir = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * Vector3.right;
        noteDir.x *= baseDirection;

        return (Vector2)noteDir;
    }

    // Use this for initialization
    void Start() 
    {
	    
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Collision");
    }
}
