using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoteSpawnerTester : MonoBehaviour 
{
    public List<FlyingNote> notes;


	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKey(KeyCode.I))
            notes[0].Launch(transform.position, Vector2.right + Random.Range(0, 1) * Vector2.up, 10);
        if (Input.GetKey(KeyCode.O))
            notes[1].Launch(transform.position, Vector2.right + Random.Range(0, 1) * Vector2.up, 10);

	}
}
