using UnityEngine;
using System.Collections;

public class Particle : ManagedObject
{
    #region Classes, Delegates & Enums

    [System.Serializable]
    public class ParticlePhysicsSettings
    {
        public float Gravity;
        public float InitialVelocity;
        
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
        Debug.Log("Test");
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

                if( PhysicsSettings.Material!=null)
                    collider2D.sharedMaterial = PhysicsSettings.Material;
            }
            if (rigidbody2D == null)
            {
                transform.GetOrAddComponent<Rigidbody2D>();
            }

            rigidbody2D.fixedAngle = PhysicsSettings.FixedAngle;
            rigidbody2D.drag = PhysicsSettings.LinearDrag;
            rigidbody2D.angularDrag = PhysicsSettings.AngularDrag;
        }
        return base.Create();
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
