using UnityEngine;
using System.Collections;

public delegate void TriggerDelegate(Collider2D other);

public class ChildTrigger2DDelegates : MonoBehaviour 
{
    public TriggerDelegate OnTriggerEnter, OnTriggerStay, OnTriggerExit;

    public void FixedUpdate()
    {
        GetComponent<Collider2D>().isTrigger = true;
        transform.position = transform.position;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ENTER " + other.tag);
        GetComponent<Collider2D>().isTrigger = true;
        transform.position = transform.position;
        if(OnTriggerEnter!=null)
            OnTriggerEnter(other);
        
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        //Debug.Log("Stay " + other.tag);
        //Get .isTrigger = true; 
        //GetComponent<BoxCollider2D>().isTrigger = true;
        GetComponent<Collider2D>().isTrigger = true;
        transform.position = transform.position;
        if (OnTriggerStay != null)
            OnTriggerStay(other);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("EXIT " + other.tag);
        GetComponent<Collider2D>().isTrigger = true;
        transform.position = transform.position;
        if (OnTriggerExit != null) 
            OnTriggerExit(other);
    }
}
