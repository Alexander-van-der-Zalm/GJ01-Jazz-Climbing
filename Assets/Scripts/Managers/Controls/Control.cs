using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class Control 
{
    public string Name;
    protected ControlScheme scheme;

    public Control(ControlScheme scheme)
    {
        this.scheme = scheme;
    }
}
