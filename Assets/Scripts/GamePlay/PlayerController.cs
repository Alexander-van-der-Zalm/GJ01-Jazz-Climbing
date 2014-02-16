using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ControlScheme))]
public class PlayerController : MonoBehaviour
{
    #region Fields

    private ControlScheme ControlScheme;

    public enum PlayerActions
    {
        Jump = 0,
        Dash = 1,
        Interact = 2,
        Slide = 3
    }

    [System.Serializable]
    public class JumpSettingsC
    {
        public float JumpMaxHeight = 4.0f;
        public float JumpMinHeight = 1.0f;
        public float JumpTimeToApex = 0.44f;
    }

    public JumpSettingsC JumpSettings;

    [System.Serializable]
    public class RunSettingsC
    {
        public float RunMaxVelocity = 4;
        public float RunAccelTime = 0.5f;
        public float RunDeAccelTime = 0.25f;
    }

    public RunSettingsC RunSettings;

    public float FallGravity = 50;

    private bool facingRight = true;
    private bool grounded;

    

    #endregion

    #region Start

    // Use this for initialization
	void Start () 
    {
        ControlScheme = gameObject.GetComponent<ControlScheme>();
        if (ControlScheme == null)
            ControlScheme = gameObject.AddComponent<ControlScheme>();

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
        #region Vars
        // hor: Input & Dir is a mini cached var
        float hor = ControlScheme.Horizontal.Value();
        float dir = rigidbody2D.velocity.x;

        // do some precalculaltions 
        float accel = Time.fixedDeltaTime * RunSettings.RunMaxVelocity;
        float dirNormalized = dir/Mathf.Abs(dir);
        #endregion

        #region Movement
        // Passive
        if (hor == 0 && dir == 0)
        {
           //// Debug.Log("Passive  ");
        }//DeAccel
        else if (hor == 0 && dir != 0 || (Mathf.Abs(dir) > 0 && dirNormalized != hor / Mathf.Abs(hor)))
        {
            // Possible to do fraction deaccel if wanted
            accel *= -dirNormalized / this.RunSettings.RunDeAccelTime;

            // CLAMP: If it ends up going in the other direction after accel, clamp it to 0
            float newXVelocity = dir + accel;
            if (newXVelocity / Mathf.Abs(newXVelocity) != dirNormalized)
                rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
            else
                rigidbody2D.velocity += new Vector2(accel, 0);
        }
        else
        {//Accel
            accel *= hor / RunSettings.RunAccelTime;
            //CLAMP
            if (Mathf.Abs(dir + accel) > RunSettings.RunMaxVelocity)
                rigidbody2D.velocity = new Vector2(RunSettings.RunMaxVelocity * dirNormalized, rigidbody2D.velocity.y);
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
        if (grounded&&ControlScheme.Actions[(int)PlayerActions.Jump].IsPressed())
        {
            JumpNHold();
        }

        Fall();

        //Reset grounded
        grounded = false;
	}

    #endregion

    #region Jump

    public void JumpNHold()
    {
        StartCoroutine(MarioJump());
    }

    #region Old
    //public IEnumerator JumpCharge()
    //{
    //    float timeStart = Time.timeSinceLevelLoad;
    //    float chargeTime = 0;

    //    rigidbody2D.AddForce(new Vector2(0, JumpForce),ForceMode.VelocityChange);

    //    while (chargeTime <= JumpMaxHoldTime && ControlScheme.Actions[(int)PlayerActions.Jump].IsDown())
    //    {
    //        //rigidbody2D.AddForce(new Vector2(0, JumpMaxForce));
            

    //        //float dt = 1 - (chargeTime / JumpMaxHoldTime);
    //        //float force = JumpForce * dt;
    //        rigidbody2D.AddForce(new Vector2(0, JumpAccelForce),ForceMode.Acceleration);
    //        //Debug.Log("InbetweenJump: dt: " + dt);

    //        chargeTime = Time.timeSinceLevelLoad - timeStart;
    //        yield return null;
    //    }
        
    //    //if (chargeTime > )
    //    //{
    //    //    rigidbody2D.AddForce(new Vector2(0, JumpMaxForce));
    //    //    Debug.Log("maxJump ");
    //    //}
    //    //else
    //    //{
    //    //    float dt = chargeTime / JumpMaxHoldTime;
    //    //    float force = JumpMinForce + (JumpMaxForce - JumpMinForce) * dt;
    //    //    rigidbody2D.AddForce(new Vector2(0, force));
    //    //    Debug.Log("InbetweenJump: dt: " +dt);
    //    //}
    //}
    #endregion

    public IEnumerator MarioJump()
    {
        float h = JumpSettings.JumpMaxHeight;//height
        float t = JumpSettings.JumpTimeToApex;//time
        float hmin = JumpSettings.JumpMinHeight;

        float g = (2*h)/(t*t);
        float v = Mathf.Sqrt(2*g*h);

        //Debug.Log("g: " + g + " v " + v + " gnow " + Physics2D.gravity.y);

        rigidbody2D.gravityScale = g / Mathf.Abs(Physics2D.gravity.y);
        rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, v);

        float timeStart = Time.timeSinceLevelLoad;
        float flyTime = 0;

        //float vearly = Mathf.Sqrt(v*v+2*g(

        while (flyTime < t && ControlScheme.Actions[(int)PlayerActions.Jump].IsDown())
        {
            flyTime = Time.timeSinceLevelLoad - timeStart;
            yield return null;
        }

        rigidbody2D.gravityScale = 10; 
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

    #region Fall

    private void Fall()
    {
        // Fall gravity
        if (rigidbody2D.velocity.y < 0)
        {
            rigidbody2D.gravityScale = FallGravity / Mathf.Abs(Physics2D.gravity.y);
        }
    }

    #endregion

    public void OnTriggerStay2D(Collider2D other)
    {
        // Only on the right elements
        //Ray2D ray;
        //for(int i = 0; i < 5; i++)
        //{
        //    //Debug.DrawRay(
        //    ray = new Ray2D(new Vector2(,);

        //Ray2D ray = new Ray2D(new Vector2(,);
            grounded = true;  
        //}
    }
}
