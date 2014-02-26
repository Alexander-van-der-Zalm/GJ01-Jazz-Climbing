using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DebugHelper 
{

    public static void LogList<T>(List<T> list)
    {
        int i = 0;
        foreach (T t in list)
        {
            Debug.Log(i + "st in list: " + t.ToString());
            i++;
        }
    }
}
