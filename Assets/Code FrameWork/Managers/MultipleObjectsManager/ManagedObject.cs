﻿using UnityEngine;
using System;
using System.Collections;

public class ManagedObject : MonoBehaviour
{
    private MultipleObjectsManager manager;

    //void Awake()
    //{
    //    if (manager == null)
    //        Debug.Log(this + "Will not be registerd");
    //    else
    //        manager.Register(this);
    //}

    void OnEnable()
    {
        CheckManager(this.GetType());
        Debug.Log("Enable " + this + " " + manager.ToString());
        
        manager.Activate(this);
    }

    void OnDisable()
    {
        CheckManager(this.GetType());
        Debug.Log("Disable " + this + " " + manager.ToString());
        manager.Deactivate(this);
    }

    private void CheckManager(Type t)
    {
        if (manager == null)
            manager = ObjectCEO.GetManager(t);
    }

    /// <summary>
    /// Creates the object via a manager that activates, creates, 
    /// deactivates and destroys in a smart manner 
    /// instead of just creating and destroying objects.
    /// </summary>
    protected GameObject Create(GameObject go)
    {
        CheckManager(this.GetType());
        Debug.Log(manager + "  a  " + this.GetType());
        //managedObject = 
        //ID = managedObject.GetInstanceID();
        //Debug.Log(ID);
        ManagedObject obj = manager.GetManagedObject(go);
        Debug.Log(obj.manager);
        obj.manager = manager;
        
        
        return obj.gameObject;
    }

    /// <summary>
    /// Use this instead of destroy
    /// </summary>
    protected void Deactivate()
    {
        if (manager == null)
        {
            Debug.LogError("ManagedObject: The object needs to be created via Create function that uses the manager first.");
            return;
        }

        manager.Deactivate(this);
    }

    
}
