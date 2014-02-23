using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class InstrumentManager : MonoBehaviour 
{
    public Instrument InstrumentInHand;
    public List<Instrument> InstrumentsInCollection;
    private ControlScheme controlScheme;

	// Use this for initialization
	void Start () 
    {
        controlScheme = ControlManager.Instance.ControlSchemes[0];
        InstrumentInHand = InstrumentsInCollection.First();

	}
	
	// Update is called once per frame
	void Update () 
    {
        if (controlScheme.Actions[(int)JazzClimbingPlayerActions.PlayInstrument].IsPressed())
        {
            InstrumentInHand.ActivateInstrument();
        }
	}
}
