using UnityEngine;
using System.Collections;

public enum ControlKeyType
{
    PC,
    Xbox
}

// Needs custom inspector
[System.Serializable]
public class ControlKey 
{
    public ControlKeyType Type;
    public string KeyValue;

    public ControlKey(ControlKeyType type, string value)
    {
        Type = type;
        KeyValue = value;
    }
}
