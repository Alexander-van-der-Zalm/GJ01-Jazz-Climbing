using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{

    public Transform Target;
    public float followSpeed = 1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	    Vector3 target = Target.position;
        target.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);
	}
}
