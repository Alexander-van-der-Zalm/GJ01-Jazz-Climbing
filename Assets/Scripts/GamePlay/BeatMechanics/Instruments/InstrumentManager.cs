using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class InstrumentManager : MonoBehaviour 
{
    public Instrument InstrumentInHand;
    public List<Instrument> InstrumentsInCollection;
    private ControlScheme controlScheme;
    private Transform player;

	// Use this for initialization
	void Start () 
    {
        controlScheme = ControlManager.Instance.ControlSchemes[0];
        InstrumentInHand = ((GameObject)GameObject.Instantiate(InstrumentsInCollection.First().gameObject)).GetComponent<Instrument>();
        InstrumentInHand.gameObject.SetActive(true);
        InstrumentInHand.transform.position = transform.position;
        InstrumentInHand.transform.parent = transform;
        player = transform.parent.Find("PlayerCharacter");
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (controlScheme.Actions[(int)JazzClimbingPlayerActions.PlayInstrument].IsPressed())
        {
            InstrumentInHand.ActivateInstrument();
        }

        Vector3 scale = transform.localScale;
        scale.x = player.localScale.x;
        transform.localScale = scale;
	}
}
