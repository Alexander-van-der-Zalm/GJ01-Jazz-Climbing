using UnityEngine;
using System.Collections;


public class FlyingNote : MonoBehaviour
{
    public float Gravity;
    [Range(0, 1)]
    public float Bouncy;
    public float InitialVelocity;

    public float LifeTime = 5;
 
    [HideInInspector]
    public int Damage;

    private float timeAlive = 0;

    // Use this for initialization
    void Start()
    {
        rigidbody2D.gravityScale = Gravity / Mathf.Abs(Physics2D.gravity.y);
        collider2D.sharedMaterial.bounciness = Bouncy;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeAlive >= LifeTime)
        {
            GameObject.DestroyImmediate(this);
            return;
        }
        timeAlive += Time.deltaTime;

        Debug.Log(rigidbody2D.velocity + " " + (LifeTime - timeAlive));
    }

    public FlyingNote CreateNote(Vector3 origin, Vector2 direction, int Damage, float additionalVelocity = 0)
    {
        GameObject go = (GameObject)GameObject.Instantiate(this.gameObject, origin, Quaternion.identity);
        go.SetActive(true);
        go.transform.parent = transform.parent.transform;
        FlyingNote note = go.GetComponent<FlyingNote>();
        note.Damage = Damage;

        Debug.Log(direction.normalized * (InitialVelocity + additionalVelocity));

        rigidbody2D.velocity = direction.normalized * (InitialVelocity + additionalVelocity);

        return note;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        //Debug.Log(other.gameObject.name);
    }
}
