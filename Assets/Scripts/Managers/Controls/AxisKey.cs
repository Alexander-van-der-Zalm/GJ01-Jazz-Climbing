using UnityEngine;
using System.Collections;
using XboxCtrlrInput;
using System.Collections.Generic;

[System.Serializable]
public class AxisKey
{
    public ControlKeyType Type;
    public List<string> keys = new List<string>();
    public XboxAxisType xboxAxisType;
    //private HorVert horVert;

    public enum HorVert
    {
        Horizontal,
        Vertical
    }

    public enum XboxAxisType
    {
        dpad,
        axis
    }

    public static AxisKey XboxAxis(XboxAxis axis)
    {
        AxisKey ak = new AxisKey();
        ak.Type = ControlKeyType.Xbox;
        ak.xboxAxisType = XboxAxisType.axis;
        ak.keys.Add(axis.ToString());

        return ak;
    }

    public static AxisKey XboxDpad(HorVert horintalOrVertical)
    {
        AxisKey ak = new AxisKey();
        ak.Type = ControlKeyType.Xbox;
        ak.xboxAxisType = XboxAxisType.dpad;
        
        if(horintalOrVertical == HorVert.Horizontal)
        {
            ak.keys.Add(XboxDPad.Left.ToString());
            ak.keys.Add(XboxDPad.Right.ToString());
        }
        else
        {
            ak.keys.Add(XboxDPad.Down.ToString());
            ak.keys.Add(XboxDPad.Up.ToString());
        }

        return ak;
    }

    public static AxisKey PC(KeyCode neg, KeyCode pos)
    {
        AxisKey ak = new AxisKey();
        ak.Type = ControlKeyType.PC;

        ak.keys.Add(neg.ToString());
        ak.keys.Add(pos.ToString());

        return ak;
    }

    public float Value(int xboxController)
    {
        float v = 0;
        switch (Type)
        {
            case ControlKeyType.PC:
                if(Input.GetKey(ControlHelper.ReturnKeyCode(keys[0])))
                    v--;
                if(Input.GetKey(ControlHelper.ReturnKeyCode(keys[1])))
                    v++;
                break;

            case ControlKeyType.Xbox:
                if (xboxAxisType == XboxAxisType.axis)
                    v = XCI.GetAxis(ControlHelper.ReturnXboxAxis(keys[0]),xboxController);
                else
                {
                    if (XCI.GetDPad(ControlHelper.ReturnXboxDPad(keys[0]), xboxController))
                        v--;
                    if (XCI.GetDPad(ControlHelper.ReturnXboxDPad(keys[1]), xboxController))
                        v++;
                }

                break;
            default:
                return 0;
        }
        return v;
    }
}
