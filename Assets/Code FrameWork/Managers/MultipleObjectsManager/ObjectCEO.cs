using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ObjectCEO : Singleton<ObjectCEO> 
{
    private Dictionary<Type, MultipleObjectsManager> Managers = new Dictionary<Type,MultipleObjectsManager>();

    public static MultipleObjectsManager GetManager(Type type)
    {
        if (instance == null)
        {
            GameObject go = new GameObject();
            instance = go.AddComponent<ObjectCEO>();
            go.name = "ManagedObjects";
        }

        MultipleObjectsManager manager; 
        
        // Create a new manager if it is not there yet
        if (!Instance.Managers.ContainsKey(type))
        {
            // Create new gameObject and parent it to the ceo
            GameObject go = new GameObject();
            go.transform.parent = Instance.gameObject.transform;
            go.name = type.ToString() + " Manager";

            // Add the manager to the ceo's managers
            manager = go.AddComponent<MultipleObjectsManager>();
            Instance.Managers.Add(type, manager);
        }
        else
            manager = Instance.Managers[type];

        return manager;    
    }
}
