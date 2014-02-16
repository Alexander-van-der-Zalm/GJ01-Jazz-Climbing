using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ControlScheme))]
public class PlayerController : MonoBehaviour
{
    #region Fields

    public ControlScheme ControlScheme;

    public enum PlayerActions
    {
        Jump = 0,
        Dash = 1,
        Interact = 2,
        Slice = 3
    }

    public float JumpMinForce = 300;
    public float JumpMaxForce = 500;
    public float JumpMaxHoldTime = 1.0f;

    public float RunMaxVelocity = 10;
    public float RunAccelTime = 0.5f;
    public float RunDeAccelTime = 0.25f;

    private bool facingRight = true;
    private bool grounded;

    #endregion

    #region Start

    // Use this for initialization
	void Start () 
    {
        ControlScheme = new ControlScheme();
        ControlScheme.Horizontal = new Axis(ControlScheme,"Horizontal");
        ControlScheme.Horizontal.AxisKeys.Add(AxisKey.XboxAxis(XboxCtrlrInput.XboxAxis.LeftStickX));
        ControlScheme.Horizontal.AxisKeys.Add(AxisKey.XboxDpad(AxisKey.HorVert.Horizontal));
        ControlScheme.Horizontal.AxisKeys.Add(AxisKey.PC(KeyCode.A,KeyCode.D));
        ControlScheme.Horizontal.AxisKeys.Add(AxisKey.PC(KeyCode.LeftArrow, KeyCode.RightArrow));

        ControlScheme.Vertical = new Axis(ControlScheme,"Vertical");
        ControlScheme.Vertical.AxisKeys.Add(AxisKey.XboxAxis(XboxCtrlrInput.XboxAxis.LeftStickY));
        ControlScheme.Vertical.AxisKeys.Add(AxisKey.XboxDpad(AxisKey.HorVert.Vertical));
        ControlScheme.Vertical.AxisKeys.Add(AxisKey.PC(KeyCode.W, KeyCode.S));
        ControlScheme.Horizontal.AxisKeys.Add(AxisKey.PC(KeyCode.UpArrow, KeyCode.DownArrow));

        ControlScheme.Actions.Insert((int)PlayerActions.Jump, new Action(ControlScheme,PlayerActions.Jump.ToString()));
        ControlScheme.Actions[(int)PlayerActions.Jump].Keys.Add(ControlKey.PCKey(KeyCode.Space));
        ControlScheme.Actions[(int)PlayerActions.Jump].Keys.Add(ControlKey.XboxButton(XboxCtrlrInput.XboxButton.A));

	}

    #endregion

    #region FixedUpdate
    
	void FixedUpdate () 
    {
        // hor: Input & Dir is a mini cached var
        float hor = ControlScheme.Horizontal.Value();
        float dir = rigidbody2D.velocity.x;

        // do some precalculaltions 
        float accel = Time.fixedDeltaTime * RunMaxVelocity;
        float dirNormalized = dir/Mathf.Abs(dir);

        #region Movement
        // Passive
        if (hor == 0 && dir == 0)
        {
           //// Debug.Log("Passive  ");
        }//DeAccel
        else if (hor == 0 && dir != 0 || (Mathf.Abs(dir) > 0 && dirNormalized != hor / Mathf.Abs(hor)))
        {
            // Possible to do fraction deaccel if wanted
            accel *= -dirNormalized / RunDeAccelTime;

            // CLAMP: If it ends up going in the other direction after accel, clamp it to 0
            float newXVelocity = dir + accel;
            if (newXVelocity / Mathf.Abs(newXVelocity) != dirNormalized)
                rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
            else
                rigidbody2D.velocity += new Vector2(accel, 0);
        }
        else
        {//Accel
            accel *= hor / RunAccelTime;
            //CLAMP
            if (Mathf.Abs(dir + accel) > RunMaxVelocity)
                rigidbody2D.velocity = new Vector2(RunMaxVelocity * dirNormalized, rigidbody2D.velocity.y);
            else
                rigidbody2D.velocity += new Vector2(accel, 0);
        }
        #endregion

        #region Flip sprite direction
        // Face the right direction
        if (hor > 0 && !facingRight)
            Flip();
        else if(hor < 0 && facingRight)
            Flip();
        #endregion

        // Jump
        if (ControlScheme.Actions[(int)PlayerActions.Jump].IsPressed())
        {
            //Start jump charge coroutine
        }
	}

    #endregion

    #region Flip
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
    #endregion

    public void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("Trigger: "+ other.name);
    }
}
