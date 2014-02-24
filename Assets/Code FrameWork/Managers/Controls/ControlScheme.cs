using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// Make generic
[Serializable]
public class ControlScheme// : MonoBehaviour 
{
    public enum UpdateTypeE
    {
        Update,
        FixedUpdate,
        LateUpdate
    }

    #region Fields

    public string Name;
    public int controllerID = 1;
    public int playerID = 1;

    public UpdateTypeE UpdateType = UpdateTypeE.FixedUpdate;

    public ControlKeyType InputType = ControlKeyType.PC;

    public Axis Horizontal;
    public Axis Vertical;

    public List<Action> Actions = new List<Action>();

    #endregion

    public static ControlScheme CreateScheme<T>(UpdateTypeE updateType = UpdateTypeE.FixedUpdate, bool xboxLeftStick = true, bool xboxDPad = true, bool arrows = true, bool wasd = true) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }

        ControlScheme controlScheme = new ControlScheme();
        controlScheme.Name = typeof(T).ToString();
        controlScheme.UpdateType = updateType;
        controlScheme.SetActionsFromEnum<T>();

        controlScheme.Horizontal = new Axis(controlScheme, "Horizontal");
        controlScheme.Vertical = new Axis(controlScheme, "Vertical");

        if (xboxLeftStick)
        {
            controlScheme.Horizontal.AxisKeys.Add(AxisKey.XboxAxis(XboxCtrlrInput.XboxAxis.LeftStickX));
            controlScheme.Vertical.AxisKeys.Add(AxisKey.XboxAxis(XboxCtrlrInput.XboxAxis.LeftStickY));
        }
        if (xboxDPad)
        {
            controlScheme.Horizontal.AxisKeys.Add(AxisKey.XboxDpad(AxisKey.HorVert.Horizontal));
            controlScheme.Vertical.AxisKeys.Add(AxisKey.XboxDpad(AxisKey.HorVert.Vertical));
        }
        if (wasd)
        {
            controlScheme.Horizontal.AxisKeys.Add(AxisKey.PC(KeyCode.A, KeyCode.D));
            controlScheme.Vertical.AxisKeys.Add(AxisKey.PC(KeyCode.S, KeyCode.W));
        }
        if (arrows)
        {
            controlScheme.Horizontal.AxisKeys.Add(AxisKey.PC(KeyCode.LeftArrow, KeyCode.RightArrow));
            controlScheme.Vertical.AxisKeys.Add(AxisKey.PC(KeyCode.DownArrow, KeyCode.UpArrow));
        }

        return controlScheme;
    }

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

    public void Update()
    {
        foreach (Action action in Actions)
        {
            action.Update();
        }
    }
}
