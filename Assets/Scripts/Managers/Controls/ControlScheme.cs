using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlScheme : MonoBehaviour 
{
    public int controllerID = 1;
    public int playerID = 1;

    public ControlKeyType InputType = ControlKeyType.PC;

    // Expose this
    public List<Control> Controls;

    public ControlScheme(int controllerID = 1, int playerID = 1)
    {
        this.controllerID = controllerID;
        this.playerID = playerID;
        Controls = new List<Control>();
    }
}
