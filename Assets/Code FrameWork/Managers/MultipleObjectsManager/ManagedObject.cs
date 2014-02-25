using UnityEngine;
using System;
using System.Collections;

public class ManagedObject : MonoBehaviour
{
    private MultipleObjectsManager manager;
    private ManagedObject managedObject;

    void Awake()
    {
        //if(managedObject == null)

    }

    void OnDestroy()
    {

    }

    /// <summary>
    /// Creates the object via a manager that activates, creates, 
    /// deactivates and destroys in a smart manner 
    /// instead of just creating and destroying objects.
    /// </summary>
    protected GameObject Create(Type t, GameObject go)
    {
        if (manager == null)
            manager = ObjectCEO.GetManager(t);
        
        managedObject = manager.GetManagedObject(go);
        return managedObject.gameObject;
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
