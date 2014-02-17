using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ControlScheme))]
public class PlayerController : MonoBehaviour
{
    #region Helper Enums & Classes

    public enum PlayerActions
    {
        Jump = 0,
        Dash = 1,
        Interact = 2,
        Slide = 3
    }

    public enum PlayerState
    {
        Idle,
        Running,
        RunningStopping,
        WallHugging,
        Airborne,
        Falling,
        Grabbing,
        Sliding
    }
    
    [System.Serializable]
    public class JumpSettingsC
    {
        public float JumpMaxHeight = 4.0f;
        public float JumpMinHeight = 1.0f;
        public float JumpTimeToApex = 0.44f;
    }

    [System.Serializable]
    public class RunSettingsC
    {
        public float RunMaxVelocity = 4;
        public float RunAccelTime = 0.5f;
        public float RunDeAccelTime = 0.25f;
    }

    #endregion

    #region Fields

    public JumpSettingsC JumpSettings;
    public RunSettingsC RunSettings;

    public float FallGravity = 50;

    private ControlScheme ControlScheme;
    public PlayerState playerState;

    private bool facingRight = true;
    
    
    public bool grounded;

    // Input mini-cache
    private float HorizontalInput = 0;
    private float VerticalInput = 0;
    private bool InputJump = false;

    private int lastGrabbedID = -1;

    private Transform WallDetector, Grab;
    private Animator animator;
    private Vector3 GrabOffset;

    #endregion

    #region Start

    // Use this for initialization
	void Awake ()
    {
        #region Controls
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
        #endregion

        WallDetector = transform.parent.transform.Find("Wall");
        Grab = transform.parent.transform.Find("Grab");


        ChildTrigger2DDelegates grabDels = ChildTrigger2DDelegates.AddChildTrigger2D(Grab.gameObject, transform);
        ChildTrigger2DDelegates wallDels = ChildTrigger2DDelegates.AddChildTrigger2D(WallDetector.gameObject, transform);

        grabDels.OnTriggerEnter = new TriggerDelegate(OnWallTriggerEnter);
        //grabDels.OnTriggerStay = new TriggerDelegate(OnWallTrigger);
        grabDels.OnTriggerExit = new TriggerDelegate(LeaveWallTrigger);

        GrabOffset = Grab.position - transform.position;

        animator = GetComponent<Animator>();
        //animator.
    }

    #endregion

    #region FixedUpdate
    
	void FixedUpdate ()
    {
        SetInputValues();
        #region Vars
        // hor: Input & Dir is a mini cached var
        float hor = HorizontalInput;
        float dir = rigidbody2D.velocity.x;

        // do some precalculaltions 
        float accel = Time.fixedDeltaTime * RunSettings.RunMaxVelocity;
        float dirNormalized = dir/Mathf.Abs(dir);
        #endregion

        if (playerState == PlayerState.Grabbing)
        {
            HandleGrabbing();
            ResetAtEndOfUpdate();
            return;
        }

        
        #region Movement
        // Passive
        if (hor == 0 && dir == 0)
        {
            if(grounded)
                SetState(PlayerState.Idle);
        }//DeAccel
        else if (hor == 0 && dir != 0 || (Mathf.Abs(dir) > 0 && dirNormalized != hor / Mathf.Abs(hor)))
        {
            if(grounded)
                SetState(PlayerState.Running);
            
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
            if (grounded)
                SetState(PlayerState.RunningStopping);

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
        if (grounded && InputJump)
        {
            Jump();
        }

        Fall();

        ResetAtEndOfUpdate();
	}

    private void SetInputValues()
    {
        HorizontalInput =ControlScheme.Horizontal.Value();
        VerticalInput= ControlScheme.Vertical.Value();
        InputJump = ControlScheme.Actions[(int)PlayerActions.Jump].IsPressed();

        // Set Animator floats
        animator.SetFloat("Horizontal", HorizontalInput);
        animator.SetFloat("Vertical", VerticalInput);            
    }

    private void ResetAtEndOfUpdate()
    {
        //Reset grounded
        if(playerState == PlayerState.Airborne || playerState == PlayerState.Falling || playerState == PlayerState.Grabbing)
            grounded = false;

        InputJump = false;
    }

    #endregion

    #region Grabbing

    private void HandleGrabbing()
    {
        float hor = HorizontalInput;
        float vert = VerticalInput;

        if (InputJump)
        {
            
            if (hor != 0)
            {
                if (hor < 0 && facingRight || hor > 0 && !facingRight)
                {
                    rigidbody2D.velocity = new Vector2(RunSettings.RunMaxVelocity * hor,0);
                }
            }
            if (vert < 0)
                Fall(true);
            else
                Jump();

        }
    }

    #endregion

    #region WallJump & Slide

    #endregion

    #region Jump

    public void Jump()
    {
        animator.SetTrigger("Jump");
        SetState(PlayerState.Airborne);
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

    private void Fall(bool falldown = false)
    {
        
        // Fall gravity
        if (rigidbody2D.velocity.y < 0 || falldown)
        {
            SetState(PlayerState.Falling);
            rigidbody2D.gravityScale = FallGravity / Mathf.Abs(Physics2D.gravity.y);
        }
    }

    #endregion

    #region Grab

    private void OnWallTriggerEnter(Collider2D other)
    {
        int id = other.GetInstanceID();

        if (other.tag == "GrabMe" && id != lastGrabbedID)
        {
            // Grab the object
            int dir = 1;
            if (!facingRight)
                dir = -1;

            Vector3 offset = GrabOffset;
            offset.x *= dir;
            
            transform.position = other.transform.position - offset;

            SetState(PlayerState.Grabbing);
            lastGrabbedID = id;
        }
    }

    private void LeaveWallTrigger(Collider2D other)
    {
        int id = other.GetInstanceID();

        if (other.tag == "GrabMe" && lastGrabbedID == id)
        {
            lastGrabbedID = 0;
        }
    }

    #endregion

    #region State Switch

    private void SetState(PlayerState state)
    {
        animator.SetBool("Grab", false);
        rigidbody2D.isKinematic = false;
        switch (state)
        {
            case PlayerState.Idle:

                break;
            case PlayerState.Airborne:

                break;
            case PlayerState.WallHugging:

                break;
            case PlayerState.Grabbing:
                animator.SetBool("Grab", true);
                rigidbody2D.gravityScale = 0;
                rigidbody2D.velocity = Vector2.zero;
                rigidbody2D.isKinematic = true;
                break;
            case PlayerState.Running:

                break;
            case PlayerState.Sliding:

                break;
            default:

                break;

        }
        playerState = state;

    }

    #endregion

    #region Grounded Collider

    public void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Floor")
        {
            grounded = true;
        }

        if (!grounded && other.tag == "Wall")
        {
            SetState(PlayerState.WallHugging);
            Debug.Log("HUG LE WALL");
        }
       
    }

    #endregion
}
