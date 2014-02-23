using UnityEngine;
using System.Collections;


public class FlyingNote : MonoBehaviour
{
    public float Gravity;
    [Range(0, 1)]
    public float Bouncy;
    public float InitialVelocity;
    
    public int Damage;

    // Use this for initialization
    void Start()
    {
        rigidbody2D.gravityScale = Gravity / Mathf.Abs(Physics2D.gravity.y);
        collider2D.sharedMaterial.bounciness = Bouncy;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public FlyingNote CreateNote(Vector3 origin, Vector2 direction, int Damage, float additionalVelocity = 0)
    {
        GameObject go = (GameObject)GameObject.Instantiate(this.gameObject, origin, Quaternion.identity);
        go.SetActive(true);
        FlyingNote note = go.GetComponent<FlyingNote>();
        note.Damage = Damage;

        rigidbody2D.velocity = direction.normalized * (InitialVelocity + additionalVelocity);

        return note;
    }
}
