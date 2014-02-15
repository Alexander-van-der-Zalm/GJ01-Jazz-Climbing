using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XboxCtrlrInput;

public class Action : Control 
{
    public List<ControlKey> Keys;// = new List<ControlKey>();

    public Action(ControlScheme scheme):base(scheme)
    {
        Keys = new List<ControlKey>();
    }

    public bool IsDown()
    {
        bool down = false;
        foreach (ControlKey key in Keys)
        {
            switch (key.Type)
            {
                case ControlKeyType.PC:
                    if (Input.GetKey(ControlHelper.ReturnKeyCode(key.KeyValue)))
                        down = true;
                    break;
                case ControlKeyType.Xbox:
                    if(XCI.GetButton(ControlHelper.ReturnXboxButton(key.KeyValue),scheme.controllerID))
                        down = true;
                    break;
                default:
                    break;
            }
            if (down)
            {
                scheme.InputType = key.Type;
                break;
            }
        }
        return down;
    }

    public bool IsPressed()
    {
        bool down = false;
        foreach (ControlKey key in Keys)
        {
            switch (key.Type)
            {
                case ControlKeyType.PC:
                    if (Input.GetKeyDown(ControlHelper.ReturnKeyCode(key.KeyValue)))
                        down = true;
                    break;
                case ControlKeyType.Xbox:
                    if (XCI.GetButtonDown(ControlHelper.ReturnXboxButton(key.KeyValue), scheme.controllerID))
                        down = true;
                    break;
                default:
                    break;
            }
            if (down)
            {
                scheme.InputType = key.Type;
                break;
            }
        }
        return down;
    }

    public bool IsReleased()
    {
        bool down = false;
        foreach (ControlKey key in Keys)
        {
            switch (key.Type)
            {
                case ControlKeyType.PC:
                    if (Input.GetKeyUp(ControlHelper.ReturnKeyCode(key.KeyValue)))
                        down = true;
                    break;
                case ControlKeyType.Xbox:
                    if (XCI.GetButtonUp(ControlHelper.ReturnXboxButton(key.KeyValue), scheme.controllerID))
                        down = true;
                    break;
                default:
                    break;
            }
            if (down)
            {
                scheme.InputType = key.Type;
                break;
            }
        }
        

        return down;
    }
}
