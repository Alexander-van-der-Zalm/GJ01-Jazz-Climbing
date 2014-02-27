using UnityEngine;
using System.Collections;

public class Particle : ManagedObject
{
    #region Classes, Delegates & Enums

    [System.Serializable]
    public class ParticlePhysicsSettings
    {
        public float Gravity;
        
        public bool Collides;
        public float Radius;
        public PhysicsMaterial2D Material;

        public float Mass;
        public bool FixedAngle;
        public float LinearDrag;
        public float AngularDrag = 0.05f;
    }

    protected delegate void OnCollisionDelegate(Collision2D other);

    #endregion

    #region fields

    public float LifeTime;
    public ParticlePhysicsSettings PhysicsSettings;
    protected OnCollisionDelegate collisionEnterDelegate, collisionStayDelegate, collisionExitDelegate;

    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();
        //Debug.Log("Test");
        StartCoroutine(DeactivateAfterLifeTime());
    }

    private IEnumerator DeactivateAfterLifeTime()
    {
        float startTime = Time.realtimeSinceStartup;
        float dt = 0;
        while (dt < LifeTime)
        {
            if (!gameObject.activeSelf)
            {
                Debug.Log("DeactivateAfterLifeTime Disabled prematurely");
                yield break;
            }
                //startTime = Time.realtimeSinceStartup;
            dt = Time.realtimeSinceStartup - startTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public override GameObject Create()
    {
        if (PhysicsSettings.Collides)
        {
            if (collider2D == null)
            {
                CircleCollider2D cc2d = transform.GetOrAddComponent<CircleCollider2D>();
                cc2d.radius = PhysicsSettings.Radius;
            }
            if (rigidbody2D == null)
            {
                transform.GetOrAddComponent<Rigidbody2D>();
            }
        }
        SetParticleVariables(gameObject);
        return base.Create();
    }

    private void SetParticleVariables(GameObject go)
    {
        Particle p = go.GetComponent<Particle>();
    
        go.rigidbody2D.fixedAngle = p.PhysicsSettings.FixedAngle;
        go.rigidbody2D.drag = p.PhysicsSettings.LinearDrag;
        go.rigidbody2D.angularDrag = p.PhysicsSettings.AngularDrag;
        go.rigidbody2D.mass = p.PhysicsSettings.Mass;
        go.rigidbody2D.gravityScale = -p.PhysicsSettings.Gravity / Physics.gravity.y;

        if (!p.PhysicsSettings.Collides)
        {
            if (go.rigidbody2D != null)
                go.rigidbody2D.Sleep();// = true;
            if (go.collider2D != null)
                go.collider2D.enabled = false;
        }
        else
        {
            go.rigidbody2D.WakeUp();
            go.collider2D.enabled = true;
        }

        CircleCollider2D cc2d = go.transform.GetComponent<CircleCollider2D>();
        if (cc2d != null)
            cc2d.radius = p.PhysicsSettings.Radius; 

        if (p.PhysicsSettings.Material != null)
            go.collider2D.sharedMaterial = p.PhysicsSettings.Material;

        
    }

    protected override void SetVariables(GameObject set, GameObject get)
    {
 	    base.SetVariables(set, get);
        SetParticleVariables(set);

        //Debug.Log(GetInstanceID() +  " s " + set.GetInstanceID() + " g " + get.GetInstanceID());
        Particle p = set.GetComponent<Particle>();
        set.rigidbody2D.mass = p.PhysicsSettings.Mass;

        //Debug.Log(p.PhysicsSettings.Mass + " " + set.rigidbody2D.mass);
        //SetParticleVariables(set,get);
    }

    public GameObject Launch(Vector3 pos, Vector2 dir, float velocity)
    {
        GameObject go = Create();
        go.transform.position = pos;
        go.rigidbody2D.velocity = dir*velocity;
        return go;
    }

    #region Collision

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!PhysicsSettings.Collides)
            return;

        if(collisionEnterDelegate!=null)
            collisionEnterDelegate(other);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (!PhysicsSettings.Collides)
            return;

        if (collisionStayDelegate != null)
            collisionStayDelegate(other);
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (!PhysicsSettings.Collides)
            return;

        if (collisionExitDelegate != null)
            collisionExitDelegate(other);
    }

    #endregion
}
