using UnityEngine;
using System.Collections;

public class AnimationGlobalBeat : MonoBehaviour 
{
    Animator anim;

	// Use this for initialization
	void Start () 
    {
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        anim.ForceStateNormalizedTime(GlobalBeat.ProgressInMeasure()/GlobalBeat.Measures);
	}
}
