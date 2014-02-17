using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{

    public Transform Target;
    public float followSpeed = 1f;
    public float HeightOffset = 2f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	    Vector3 target = Target.position;
        target.z = transform.position.z;
        target.y += HeightOffset;

        transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);// + new Vector3(0,,0);
	}
}
