using UnityEngine;
using System.Collections;


public class FlyingNote : Particle
{
    [HideInInspector]
    public int Damage;

    public FlyingNote CreateNote(Vector3 origin, Vector2 direction, int Damage, float additionalVelocity = 0)
    {
        //GameObject go = (GameObject)GameObject.Instantiate(this.gameObject, origin, Quaternion.identity);
        //go.SetActive(true);
        //go.transform.parent = transform.parent.transform;
        //FlyingNote note = go.GetComponent<FlyingNote>();
        //note.Damage = Damage;

        ////Debug.Log(direction.normalized * (InitialVelocity + additionalVelocity));
        //Vector2 vel = direction.normalized * (InitialVelocity + additionalVelocity);
        ////Debug.Log(vel);
        //go.rigidbody2D.velocity = vel;

        ////Debug.Log(go.rigidbody2D.velocity);

        //return note;
        return null;
    }
}
