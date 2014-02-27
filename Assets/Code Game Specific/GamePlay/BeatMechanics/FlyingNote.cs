using UnityEngine;
using System.Collections;


public class FlyingNote : Particle
{
    [HideInInspector]
    public int Damage;

    public FlyingNote LaunchNote(Vector3 origin, Vector2 direction, float velocity, int Damage)
    {
        FlyingNote fn = Launch(origin, direction, velocity).GetComponent<FlyingNote>();
        fn.Damage = Damage;
        return fn;
    }
}
