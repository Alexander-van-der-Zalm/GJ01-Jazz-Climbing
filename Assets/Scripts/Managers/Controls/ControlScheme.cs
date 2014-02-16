using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlScheme : MonoBehaviour 
{
    public int controllerID = 1;
    public int playerID = 1;

    public ControlKeyType InputType = ControlKeyType.PC;

    public Axis Horizontal;
    public Axis Vertical;

    // Expose this
    public List<Action> Actions = new List<Action>();

    //public ControlScheme(int controllerID = 1, int playerID = 1)
    //{
    //    this.controllerID = controllerID;
    //    this.playerID = playerID;
    //    Actions = new List<Action>();
    //}

    public void FixedUpdate()
    {
        foreach (Action action in Actions)
        {
            action.Update();
        }
    }
}
