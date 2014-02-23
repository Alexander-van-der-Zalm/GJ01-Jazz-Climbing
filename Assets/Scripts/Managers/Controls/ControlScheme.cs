using UnityEngine;
using System;
using System.Linq;
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


    /// <summary>
    /// 
    /// </summary>
    public void SetActionsFromEnum<T>() where T : struct, IConvertible
    {
       if (!typeof(T).IsEnum) 
       {
          throw new ArgumentException("T must be an enumerated type");
       }

        IEnumerable<T> values = Enum.GetValues(typeof(T)).Cast<T>();

        foreach (T value in values)
        {
            Actions.Add(new Action(this, value.ToString()));
        }
    }

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
