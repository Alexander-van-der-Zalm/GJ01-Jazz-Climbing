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
        [Range(0,1.0f)]
        public float Bounciness;
        public float Friction;
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
            }
            
            //if (collider2D.sharedMaterial.bounciness != PhysicsSettings.Bounciness || collider2D.sharedMaterial.friction != PhysicsSettings.Friction)
            //{
            //PhysicsMaterial2D mat = Object.Instantiate(PhysicsMaterial2D("newMat");
            Debug.Log(collider2D.sharedMaterial);
            //    mat.bounciness = PhysicsSettings.Bounciness;
            //    mat.friction = PhysicsSettings.Friction;
            //    collider2D.enabled = false;
            //    collider2D.sharedMaterial = mat;
            //    collider2D.enabled = true;
            //}
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
