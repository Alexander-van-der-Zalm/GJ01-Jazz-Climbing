using UnityEngine;
using System.Collections;

public delegate void TriggerDelegate(Collider2D other);

public class ChildTrigger2DDelegates : MonoBehaviour 
{
    public TriggerDelegate OnTriggerEnter, OnTriggerStay, OnTriggerExit;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if(OnTriggerEnter!=null)
            OnTriggerEnter(other);
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        if (OnTriggerStay != null)
            OnTriggerStay(other);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (OnTriggerExit != null) 
            OnTriggerExit(other);
    }
}
