using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MultipleObjectsManager : MonoBehaviour
{
    //public Type Type;

    public readonly int MaxInactiveObjects = 100;

    private List<ManagedObject> activeObjects = new List<ManagedObject>();
    private List<ManagedObject> inactiveObjects = new List<ManagedObject>();

    public ManagedObject GetManagedObject(GameObject objectoToClone)
    {
        ManagedObject obj;
        int count = inactiveObjects.Count;
        // Activate an inactive object before creating a new one
        if (count > 0)
        {
            obj = inactiveObjects[count - 1];
            inactiveObjects.RemoveAt(count - 1);
        }
        else
        {
            GameObject go = GameObject.Instantiate(objectoToClone) as GameObject;
            go.transform.parent = transform;
            obj = go.transform.GetOrAddComponent<ManagedObject>();
        }

        // Make active and add to list
        Activate(obj);
        
        return obj;
    }
    
    public void Activate(ManagedObject obj)
    {
        if (!activeObjects.Contains(obj))
            activeObjects.Add(obj);
        
        obj.gameObject.SetActive(true);
    }

    public void Deactivate(ManagedObject obj)
    {
        if (!activeObjects.Contains(obj))
            Debug.LogError("MultipleObjectManager.Deactivate(obj) not active");
        
        // Remove from the active list
        activeObjects.Remove(obj);

        // Destroy if over the max inactive
        if (inactiveObjects.Count > MaxInactiveObjects)
            GameObject.Destroy(obj.gameObject);
        else
        {
            obj.gameObject.SetActive(false);
            inactiveObjects.Add(obj);
        }
    } 
}
