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
        manager.Activate(this);
    }

    protected virtual void OnDisable()
    {
        CheckManager(this.GetType());
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
    public virtual GameObject Create()
    {
        CheckManager(this.GetType());
        this.ID = GetInstanceID();

        ManagedObject obj = manager.GetManagedObject(gameObject);

        if (ID != obj.ID)
        {
            //// Reflection?
            //// Method
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var fieldValues = this.GetType().GetFields(bindingFlags).ToList();

            int count = fieldValues.Count;
            for (int i = 0; i < count; i++)
            {
                fieldValues[i].SetValue(obj, fieldValues[i].GetValue(this));
            }
            Debug.Log("this " + GetInstanceID() + " " + gameObject.GetType() + " " + gameObject.GetComponent<ManagedObject>().GetInstanceID());
            SetVariables(obj.gameObject, gameObject);
            obj.ID = ID;
            obj.name = gameObject.name;
        }       
        return obj.gameObject;
    }

    protected virtual void SetVariables(GameObject set, GameObject get)
    {
        Debug.Log("base");
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
