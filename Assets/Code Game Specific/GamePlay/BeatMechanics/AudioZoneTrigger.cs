using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AudioZoneTrigger : MonoBehaviour 
{
    public string Name;
    [AudioManagerLibrarySample]
    public string AudioSampleName;
    public float CrossFadeTimeInMeasure = 1.0f;
    private AudioSample Sample;
    private float Radius;
    
    
    void Start()
    {
        Sample = AudioManager.FindSampleFromCurrentLibrary(AudioSampleName);
        Radius = gameObject.GetComponent<CircleCollider2D>().radius;
        //Debug.Log(Radius);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player" || Sample == null)
            return;

        GlobalBeat.StartBeat(Name, Sample, CrossFadeTimeInMeasure);
         //   AudioManager.Play(Sample);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 1);
        DrawGizmo();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 1);
        DrawGizmo();
    }

    private void DrawGizmo()
    {
        float rad = Radius;
        
        
        if(GlobalBeat.SecondsFromBeat()<0.3f)
            rad+= 1;

        if (GlobalBeat.SecondsFromAccent() < 0.3f)
            rad += 1;

        Gizmos.DrawWireSphere(transform.position, rad);
        Gizmos.DrawIcon(transform.position + new Vector3(0, rad), "icon_zoneTrigger.png");
        //Gizmos.DrawGUITexture(transform.position + new Vector3(0, Radius), "icon_zoneTrigger.png");
    }
}
