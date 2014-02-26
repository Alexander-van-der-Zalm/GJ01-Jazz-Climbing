using UnityEngine;
//using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;


public class ManagedObject : MonoBehaviour
{
    [HideInInspector, SerializeField]
    private MultipleObjectsManager manager;
    [HideInInspector,SerializeField]
    private int ID;

    protected virtual void OnEnable()
    {
        CheckManager(this.GetType());
        //Debug.Log("Enable " + this + " " + manager.ToString());
        
        manager.Activate(this);
    }

    protected virtual void OnDisable()
    {
        CheckManager(this.GetType());
        //Debug.Log("Disable " + this + " " + manager.ToString());
        manager.Deactivate(this);
    }

    private void CheckManager(Type t)
    {
        if (manager == null)
        {
            manager = ObjectCEO.GetManager(t);
            Debug.Log("New manager");
        }
    }

    /// <summary>
    /// Creates the object via a manager that activates, creates, 
    /// deactivates and destroys in a smart manner 
    /// instead of just creating and destroying objects.
    /// </summary>
    public virtual GameObject Create()
    {
        CheckManager(this.GetType());
        //Debug.Log(go.GetInstanceID() + "  asdf  " + gameObject.GetInstanceID());

        //PrefabUtility.InstantiatePrefab
        this.ID = GetInstanceID();

        ManagedObject obj = manager.GetManagedObject(gameObject);


        int objID = obj.GetInstanceID();
        Debug.Log(ID + " " + objID + " " + obj.ID);
        if (ID != obj.ID)
        {
            // Reflection?
            // Method
            obj.ID = ID;
        }
        //var bindingFlags= BindingFlags.
        List<string> fieldValues = this.GetType().GetFields().Select(f => f.Name).ToList();

        DebugHelper.LogList<string>(fieldValues);
        
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
