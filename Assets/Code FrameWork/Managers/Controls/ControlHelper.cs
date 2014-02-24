using UnityEngine;
using System.Collections;
using XboxCtrlrInput;
using System;

public class ControlHelper 
{
    public static XboxButton ReturnXboxButton(string str)
    {
        return ParseEnum<XboxButton>(str);
    }

    public static XboxAxis ReturnXboxAxis(string str)
    {
        return ParseEnum<XboxAxis>(str);
    }

    public static XboxDPad ReturnXboxDPad(string str)
    {
        return ParseEnum<XboxDPad>(str);
    }

    public static KeyCode ReturnKeyCode(string str)
    {
        return ParseEnum<KeyCode>(str);
    }

    private static T ParseEnum<T>(string value)
    {
        T en = (T)Enum.Parse(typeof(T), value, true);
        if (!Enum.IsDefined(typeof(T), en))
            Debug.LogError("String "+ value+ " is not contained in enumtype:"+  typeof(T).ToString());
        return en;
    }
}
