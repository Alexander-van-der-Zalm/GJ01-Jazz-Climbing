using UnityEngine;
using System.Collections;

public class Particle : ManagedObject
{
    public float LifeTime;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Create()
    {
        GameObject go = Create(this.GetType(), gameObject);
    }
}
