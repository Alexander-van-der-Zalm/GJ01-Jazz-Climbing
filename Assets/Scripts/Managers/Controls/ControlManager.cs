using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ControlManager : Singleton<ControlManager> 
{
    public List<ControlScheme> ControlSchemes = new List<ControlScheme>(1);

    public static ControlScheme GetControlScheme(string name)
    {
        return Instance.ControlSchemes.Where(c => c.Name == name).First();
    }

    public static ControlScheme GetControlScheme(string name, int player)
    {
        return Instance.ControlSchemes.Where(c => c.Name == name && c.playerID == player).First();
    }

    public static ControlScheme GetControlScheme(int player)
    {
        return Instance.ControlSchemes.Where(c => c.playerID == player).First();
    }

	// Use this for initialization
	void Start () 
    {
	
	}

    #region Updates

    void FixedUpdate()
    {
        foreach (ControlScheme cs in ControlSchemes)
        {
            if (cs.UpdateType == ControlScheme.UpdateTypeE.FixedUpdate)
                cs.Update();
        }
    }

    void Update()
    {
        foreach (ControlScheme cs in ControlSchemes)
        {
            if (cs.UpdateType == ControlScheme.UpdateTypeE.Update)
                cs.Update();
        }
    }

    void LateUpdate()
    {
        foreach (ControlScheme cs in ControlSchemes)
        {
            if (cs.UpdateType == ControlScheme.UpdateTypeE.LateUpdate)
                cs.Update();
        }
    }

    #endregion
}
