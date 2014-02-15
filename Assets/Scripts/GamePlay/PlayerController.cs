using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ControlScheme))]
public class PlayerController : MonoBehaviour 
{
    public ControlScheme ControlScheme;

    public enum PlayerActions
    {
        Jump = 0,
        Dash = 1,
        Interact = 2
    }

    public float JumpMinForce = 300;
    public float JumpMaxForce = 500;
    public float JumpMaxHoldTime = 1.0f;

    //public float RunAccelPSec = 5;
    public float RunMaxVelocity = 10;
    public float RunAccelTime = 0.5f;
    public float RunDeAccelTime = 0.25f;


    private bool facingRight = true;
    private bool grounded;

    private float lastTime = 0;

	// Use this for initialization
	void Start () 
    {
        ControlScheme = new ControlScheme();
        ControlScheme.Horizontal = new Axis(ControlScheme,"Horizontal");
        ControlScheme.Horizontal.AxisKeys.Add(AxisKey.XboxAxis(XboxCtrlrInput.XboxAxis.LeftStickX));
        ControlScheme.Horizontal.AxisKeys.Add(AxisKey.XboxDpad(AxisKey.HorVert.Horizontal));
        ControlScheme.Horizontal.AxisKeys.Add(AxisKey.PC(KeyCode.A,KeyCode.D));

        ControlScheme.Vertical = new Axis(ControlScheme,"Vertical");
        ControlScheme.Vertical.AxisKeys.Add(AxisKey.XboxAxis(XboxCtrlrInput.XboxAxis.LeftStickY));
        ControlScheme.Vertical.AxisKeys.Add(AxisKey.XboxDpad(AxisKey.HorVert.Vertical));
        ControlScheme.Vertical.AxisKeys.Add(AxisKey.PC(KeyCode.W, KeyCode.S));

        ControlScheme.Actions.Insert((int)PlayerActions.Jump, new Action(ControlScheme,PlayerActions.Jump.ToString()));
        ControlScheme.Actions[(int)PlayerActions.Jump].Keys.Add(ControlKey.PCKey(KeyCode.Space));
        ControlScheme.Actions[(int)PlayerActions.Jump].Keys.Add(ControlKey.XboxButton(XboxCtrlrInput.XboxButton.A));

	}
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        // Move hor
        float hor = ControlScheme.Horizontal.Value();

        float dir = rigidbody2D.velocity.x;

        float accel = Time.fixedDeltaTime * RunMaxVelocity;
        
        float dirNormalized = dir/Mathf.Abs(dir);

        float dt = Time.fixedDeltaTime;
        float mv = RunMaxVelocity;
        float t = RunAccelTime;

        float ct = Time.timeSinceLevelLoad;
        Debug.Log("Velocity: " + rigidbody2D.velocity + " h: " + hor + " d: " + dirNormalized);// +" h: " + horNormalized);
        Debug.Log("dt: " + dt + " t: " + t + " mv: " + mv + " a: " + dt*mv/t + " dt?:" + (ct-lastTime));
        lastTime = ct;
        //DeAccel
        if (hor == 0 && dir!=0 || (Mathf.Abs(dir) > 0 && dirNormalized != hor / Mathf.Abs(hor)))
        {
            // Possible to do fraction deaccel if wanted
            accel *= -dirNormalized / RunDeAccelTime;
            Debug.Log("DeAccel a: " + accel);

            // If it ends up going in the other direction after accel, clamp it to 0
            float newXVelocity = dir + accel;
            if (newXVelocity / Mathf.Abs(newXVelocity) != dirNormalized)
            {
                rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
                Debug.Log("CLAMP 0000");
            }
            else
                rigidbody2D.velocity += new Vector2(accel, 0);
        }
        else
        {//Accel
            accel *= hor / RunAccelTime;
            if (Mathf.Abs(dir + accel) > RunMaxVelocity)
            {
                rigidbody2D.velocity = new Vector2(RunMaxVelocity*dirNormalized, rigidbody2D.velocity.y);
                Debug.Log("CLAMP MAXXX");
            }
            else
                rigidbody2D.velocity += new Vector2(accel, 0);

            Debug.Log("Accel a: " + accel);
        }
        
        
            
        
        
        //accel /= RunDeAccelTime;
        //if (rigidbody2D.velocity.x > 0)
        //    accel *= -1;
        

        //Debug.Log("fd: " + Time.fixedDeltaTime + " accel: " + accel);

        

        if (hor > 0 && !facingRight)
            Flip();
        else if(hor < 0 && facingRight)
            Flip();

        // Jump
        if (ControlScheme.Actions[(int)PlayerActions.Jump].IsPressed())
        {
            //Start jump charge coroutine
        }
	}

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;

    }

}
