using UnityEngine;
using System.Collections;

public delegate void TriggerDelegate(Collider2D other);

public class ChildTrigger2DDelegates : MonoBehaviour 
{
    public TriggerDelegate OnTriggerEnter, OnTriggerStay, OnTriggerExit;
    public Transform Parent;
    private Vector3 offset;

    public void Start()
    {
        offset = transform.position - Parent.position;
    }

    public void FixedUpdate()
    {
        Vector3 curOffset = offset;
        curOffset.x *= Parent.localScale.x;
        transform.position = Parent.position + curOffset;
        //Debug.Log(Parent.localScale);
    }

    public static ChildTrigger2DDelegates AddChildTrigger2D(GameObject child, Transform parent, TriggerDelegate onTriggerStay = null, TriggerDelegate onTriggerEnter = null, TriggerDelegate onTriggerExit = null)
    {
        ChildTrigger2DDelegates grabDels = child.gameObject.AddComponent<ChildTrigger2DDelegates>();
        grabDels.Parent = parent;
        grabDels.OnTriggerEnter = onTriggerEnter;
        grabDels.OnTriggerStay = onTriggerStay;
        grabDels.OnTriggerExit = onTriggerExit;

        if (child.rigidbody2D == null)
            child.AddComponent<Rigidbody2D>();

        child.rigidbody2D.isKinematic = false;
        child.rigidbody2D.gravityScale = 0;
        child.rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        child.rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        child.rigidbody2D.fixedAngle = true;


        return grabDels;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("ENTER " + other.tag);

        if(OnTriggerEnter!=null)
            OnTriggerEnter(other);
        
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        //Debug.Log("Stay " + other.tag);
        //Get .isTrigger = true; 
        //GetComponent<BoxCollider2D>().isTrigger = true;
        //GetComponent<Collider2D>().isTrigger = true;
        //transform.position = transform.position;
        if (OnTriggerStay != null)
            OnTriggerStay(other);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("EXIT " + other.tag);

        if (OnTriggerExit != null) 
            OnTriggerExit(other);
    }
}
