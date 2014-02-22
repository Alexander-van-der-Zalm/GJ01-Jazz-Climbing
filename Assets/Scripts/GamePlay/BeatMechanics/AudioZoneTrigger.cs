using UnityEngine;
using System.Collections;

public class AudioZoneTrigger : MonoBehaviour 
{
    public string Name;
    public AudioSample Sample;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player" || Sample == null)
            return;

        Debug.Log("SOUND GIEBB");

        if(GlobalBeat.StartBeat(Name))
            AudioManager.Play(Sample);
    }
    
}
