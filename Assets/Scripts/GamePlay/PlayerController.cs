using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        WallSliding,
        WallJump,
        Airborne,
        Falling,
        Grabbing
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

    // Fall Settings
    public float FallGravity = 50;
    public float MaxFallTimeToRespawn = 3.0f;

    // WallJump/SlideSettings
    public float WallSlideGravity = 5;
    public float WallSlideInitialVelocity = 2;
    //public float SlideBeforeFall = 0.5f;
    private float WallJumpLastDirection = 0;


    // GrabSettings
    [Range(0, 0.5f)]
    public float GrabMinNegYDist = 0.1f; // Check colliders tbh
    private Transform Grab;
    private Vector3 GrabOffset;


    private ControlScheme ControlScheme;

    // Debug
    public PlayerState playerState;
    public bool DebugStateChanges = false;
    private bool facingRight = true;

    private bool grounded;
    public bool Grounded { get { return grounded; } private set { grounded = value; animator.SetBool("Grounded", value); } }

    // Input mini-cache
    private float HorizontalInput = 0;
    private float VerticalInput = 0;
    private bool InputJump = false;

    // Storage for collided 
    //private List<int> floorIds = new List<int>();
    private List<GameObject> floorsInCollision = new List<GameObject>();
    private List<int> lastSlided = new List<int>();
    private List<GameObject> wallInCollision = new List<GameObject>();
    private int lastGrabbedID = 0;

    private Animator animator;

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
        ControlScheme.Vertical.AxisKeys.Add(AxisKey.PC(KeyCode.S, KeyCode.W));
        ControlScheme.Vertical.AxisKeys.Add(AxisKey.PC(KeyCode.DownArrow, KeyCode.UpArrow));

        ControlScheme.Actions.Insert((int)PlayerActions.Jump, new Action(ControlScheme,PlayerActions.Jump.ToString()));
        ControlScheme.Actions[(int)PlayerActions.Jump].Keys.Add(ControlKey.PCKey(KeyCode.Space));
        ControlScheme.Actions[(int)PlayerActions.Jump].Keys.Add(ControlKey.XboxButton(XboxCtrlrInput.XboxButton.A));
        #endregion

        Grab = transform.parent.transform.Find("Grab");

        ChildTrigger2DDelegates grabDels = ChildTrigger2DDelegates.AddChildTrigger2D(Grab.gameObject, transform);

        //grabDels.OnTriggerEnter = new TriggerDelegate(OnWallTriggerEnter);
        grabDels.OnTriggerStay = new TriggerDelegate(OnWallTriggerEnter);
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
        //float hor = HorizontalInput;
        float velocityX = rigidbody2D.velocity.x;

        // do some precalculaltions 
        float accel = Time.fixedDeltaTime * RunSettings.RunMaxVelocity;
        float velocityDirection = Mathf.Sign(velocityX);
        float horInputDirection = Mathf.Sign(HorizontalInput);
        #endregion

        #region Grab

        if (playerState == PlayerState.Grabbing)
        {
            HandleGrabbing();
            EndOfUpdate();
            return;
        }

        #endregion

        #region WallSlide

        if (WallSliding())
        {
            
            EndOfUpdate();
            return;
        }

        #endregion

        #region WallJump

        if (!Grounded && playerState == PlayerState.WallJump)
        {
            // Exit the walljumpstate if no or opposite direction input
            // This state ignores 
            if (HorizontalInput == 0 || HorizontalInput != WallJumpLastDirection)
            {
                SetState(PlayerState.Airborne);
                //Debug.Log("ExitWalljump");
            }
            CheckFlipByVelocity();
            EndOfUpdate();
            return;
        }

        #endregion

        #region Movement



        // Passive
        if (HorizontalInput == 0 && velocityX == 0)
        {
            if (Grounded)
            {
                SetState(PlayerState.Idle);
            }

        }//DeAccel if on the ground with no input
        else if (HorizontalInput == 0 && Grounded && velocityX != 0) //No Input & onground     
        {
            SetState(PlayerState.Running);

            //Debug.Log("DEACCEL");
            //DeAccel();
            // Possible to do fraction deaccel if wanted
            accel *= -velocityDirection / this.RunSettings.RunDeAccelTime;

            // CLAMP: If it ends up going in the other direction after accel, clamp it to 0
            float newXVelocity = velocityX + accel;
            if (newXVelocity / Mathf.Abs(newXVelocity) != velocityDirection)
                rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
            else
                rigidbody2D.velocity += new Vector2(accel, 0);

            
        }
        else if (HorizontalInput != 0 && Mathf.Abs(velocityX) > 0 && velocityDirection != HorizontalInput)
        {
            //Debug.Log("DEACCEL OTHER DIRECTION " + velocityX);
            DeAccel();
        }
        else if(HorizontalInput != 0)
        {//Accel
            if (Grounded)
                SetState(PlayerState.RunningStopping);

            //Debug.Log("ACCEL");

            accel *= HorizontalInput / RunSettings.RunAccelTime;
            //CLAMP
            if (Mathf.Abs(velocityX + accel) > RunSettings.RunMaxVelocity)
                rigidbody2D.velocity = new Vector2(RunSettings.RunMaxVelocity * velocityDirection, rigidbody2D.velocity.y);
            else
                rigidbody2D.velocity += new Vector2(accel, 0);
        }
        #endregion

        #region Jump

        // Jump
        if (Grounded && InputJump)
        {
            Jump();
        }

        #endregion

        Fall();

        CheckFlipBy(HorizontalInput);

        //CheckFlipByVelocity();

        EndOfUpdate();
	}

    private void SetInputValues()
    {
        HorizontalInput =ControlScheme.Horizontal.Value();
        VerticalInput= ControlScheme.Vertical.Value();
        InputJump = ControlScheme.Actions[(int)PlayerActions.Jump].IsPressed();

        // Set Animator floats
        animator.SetFloat("Horizontal", HorizontalInput);
        animator.SetFloat("Vertical", VerticalInput);

        Grounded = floorsInCollision.Count > 0;
    }

    private void EndOfUpdate()
    {
        ////Reset grounded
        //if (playerState == PlayerState.Airborne && playerState == PlayerState.Grabbing && playerState == PlayerState.Falling && playerState == PlayerState.WallSliding)
        //    Grounded = false;
        animator.SetFloat("VelocityX", rigidbody2D.velocity.x);
        animator.SetFloat("VelocityY", rigidbody2D.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(rigidbody2D.velocity.x)/RunSettings.RunMaxVelocity);
        InputJump = false;
    }

    #endregion

    #region Movement

    private void DeAccel()
    {
        float XVelocity = Mathf.Sign(rigidbody2D.velocity.x);
        float velXDirS = Mathf.Sign(XVelocity);

        // Possible to do fraction deaccel if wanted
        float accel = (Time.fixedDeltaTime * RunSettings.RunMaxVelocity * -velXDirS) / this.RunSettings.RunDeAccelTime;

        float newXVelocity = XVelocity + accel;
        
        float newVelDirS = Mathf.Sign(newXVelocity);
        // CLAMP: If it ends up going in the other direction after accel, clamp it to 0
        if (newVelDirS != velXDirS)
        {
            Debug.Log("Clamp");
            rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
        }
        else // add to velocity
            rigidbody2D.velocity += new Vector2(accel, 0);
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

    #region GrabTriggers

    private void OnWallTriggerEnter(Collider2D other)
    {
        int id = other.GetInstanceID();

        if (other.tag == "GrabMe" && id != lastGrabbedID)
        {
            float yDist = Grab.position.y - other.transform.position.y;
            Debug.Log(yDist + " Gr " + GrabMinNegYDist + " bool: " + (yDist+GrabMinNegYDist<0));
            if (yDist + GrabMinNegYDist < 0)
                return;
            
            // Grab the object
            Vector3 relPos = other.transform.position - transform.position;
            float dir = Mathf.Sign(relPos.x);
            //if (!facingRight)
            //    dir = -1;

            Vector3 offset = GrabOffset;
            offset.x *= dir;
            Vector3 newPos = other.transform.position - offset;

            // WIP BUGFIX
            //Collider2D[] cols = Physics2D.OverlapCircleAll(new Vector2(newPos.x,newPos.y),0.1f);
            
            //if (cols.Length>0)
            //{
            //    Debug.Log("STOUT");
            //    GameObject bla = new GameObject();
            //    bla.transform.position = newPos;
            //    return;
            //}

            transform.position = newPos;

            SetState(PlayerState.Grabbing);
            lastGrabbedID = id;

            CheckFlipBy(dir);

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

    #endregion

    #region WallJump & Slide

    private bool WallSliding()
    {
        // During sliding:
        // On the floor - Or - No More slide space
        if (playerState == PlayerState.WallSliding && (Grounded || lastSlided.Count == 0))
        {
            //Fall();
            SetState(PlayerState.Idle);
            //lastSlided.Clear();
            //wallInCollision.Clear();
            //Debug.Log("SLIDING Clear");
        }

        // Start a new slide 
        if (!Grounded && playerState != PlayerState.WallSliding && lastSlided.Count > 0 && HorizontalInput != 0)
        {
            // Slide only if the input is correct(to the wall)
            foreach (GameObject col in wallInCollision)
            {
                float relPosSign = Mathf.Sign(col.transform.position.x - transform.position.x);
                float inputSign = Mathf.Sign(HorizontalInput);
                 
                // inputdir is correct in relation to position differance
                if(relPosSign == inputSign)
                    SetState(PlayerState.WallSliding);

                CheckFlipBy(relPosSign);
            }
        }
        
        if (playerState == PlayerState.WallSliding && InputJump)
        {
            float dir = 1;
            if (facingRight)
                dir = -1;

            rigidbody2D.velocity = new Vector2(RunSettings.RunMaxVelocity * dir, 0);

            WallJump();

            lastSlided.Clear();
            wallInCollision.Clear();
            //Debug.Log("SLIDING Clear");
        }

        if (playerState == PlayerState.WallSliding && HorizontalInput == 0)
        {
            Fall();
        }

        return playerState == PlayerState.WallSliding;    
    }

    private void WallJump()
    {
        SetState(PlayerState.WallJump);
        WallJumpLastDirection = facingRight ? 1.0f : -1.0f;
        Debug.Log("WallJump");
        StartCoroutine(MarioJump());
    }

    #endregion

    #region Jump

    public void Jump()
    { 
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

        Debug.Log("JUMP");
        //Debug.Log("g: " + g + " v " + v + " gnow " + Physics2D.gravity.y);
        //Debug.Log(rigidbody2D.velocity + " " + playerState + " g: " + Grounded);

        rigidbody2D.gravityScale = g / Mathf.Abs(Physics2D.gravity.y);
        rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, v);

        float timeStart = Time.timeSinceLevelLoad;
        float flyTime = 0;

        //float vearly = Mathf.Sqrt(v*v+2*g(

        while (flyTime < t && ControlScheme.Actions[(int)PlayerActions.Jump].IsDown())
        {
            flyTime = Time.timeSinceLevelLoad - timeStart;
            //Debug.Log(rigidbody2D.velocity + " " + playerState + " g: " + Grounded);
            yield return null;
        }
        //Debug.Log("JUMP");
        if(playerState == PlayerState.Airborne)
            rigidbody2D.gravityScale = 10; 
    }

    #endregion

    #region Flip

    private void CheckFlipByVelocity()
    {
        // Face the right direction
        if (rigidbody2D.velocity.x > 0 && !facingRight)
            Flip();
        else if (rigidbody2D.velocity.x < 0 && facingRight)
            Flip();
    }


    /// <summary>
    /// dir positive for right
    /// dir negative for left
    /// </summary>
    /// <param name="dir"></param>
    private void CheckFlipBy(float dir)
    {
        if (dir == 0)
            return;
        dir = Mathf.Sign(dir);
        

        if (dir > 0 && !facingRight)
            Flip();
        else if (dir < 0 && facingRight)
            Flip();
    }

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
        if (!Grounded && (rigidbody2D.velocity.y < 0 || falldown))
        {
            //Debug.Log("FALLING " + falldown + " " + playerState);
            SetState(PlayerState.Falling);
            SetGravity(FallGravity);
            
            //rigidbody2D.gravityScale = FallGravity / Mathf.Abs(Physics2D.gravity.y);
        }
        else if (!Grounded)
        {
            SetState(PlayerState.Airborne);
        }
    }

    private IEnumerator RespawnWhenFallingTooLong()
    {
        float time = Time.timeSinceLevelLoad;
        playerState = PlayerState.Falling;
        while (playerState == PlayerState.Falling)
        {
            float dt = Time.timeSinceLevelLoad - time;
            if ((dt) > MaxFallTimeToRespawn)
            {
                PlayerSpawn.Respawn();
            }
            yield return null;
        }
    }


    #endregion

    #region State Switch

    private void SetState(PlayerState state)
    {
        if (state == playerState)
            return;
        
        animator.SetBool("Grab", false);
        animator.SetBool("WallSlide", false);
        rigidbody2D.isKinematic = false;


        if(DebugStateChanges)
            Debug.Log(state + " vel: " + rigidbody2D.velocity);

        switch (state)
        {
            case PlayerState.Idle:

                break;
            case PlayerState.WallJump:
            case PlayerState.Airborne:
                animator.SetTrigger("Jump");
                break;

            case PlayerState.Falling:
                Debug.Log("HOI");
                StartCoroutine(RespawnWhenFallingTooLong());
                break;
            case PlayerState.WallSliding:
                animator.SetBool("WallSlide", true);
                SetGravity(WallSlideGravity);
                rigidbody2D.velocity = new Vector2(0, -WallSlideInitialVelocity);
                //Debug.Log("StartSlide grav: " + rigidbody2D.gravityScale);
                break;
            case PlayerState.Grabbing:
                animator.SetBool("Grab", true);
                rigidbody2D.gravityScale = 0;
                rigidbody2D.velocity = Vector2.zero;
                rigidbody2D.isKinematic = true;
                break;
            case PlayerState.Running:

                break;
            default:

                break;

        }

        playerState = state;
    }

    #endregion

    #region Grounded Trigger

    public void OnTriggerEnter2D(Collider2D other)
    {
        int id = other.GetInstanceID();
        if (other.tag == "Floor" && !floorsInCollision.Contains(other.gameObject))//floorIds.Contains(id))
        {
            //floorIds.Add(id);
            floorsInCollision.Add(other.gameObject);
            Debug.Log("ENTER Floors: " + floorsInCollision.Count + " id " + id);
        }

        if (other.tag == "Wall" && !lastSlided.Contains(id))
        {
            lastSlided.Add(id);
            wallInCollision.Add(other.gameObject);
            //Debug.Log("ENTER Walls: " + lastSlided.Count + " id " + id);
        }
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        //int id = GetInstanceID();

        //if (other.tag == "Wall")
        //{
        //    if (Grounded && lastSlided.Count != 0)
        //    {
        //        lastSlided.Clear();
        //        wallInCollision.Clear();
        //        Debug.Log("SLIDING Clear");
        //        return;
        //    }

        //    if (!lastSlided.Contains(id) && playerState == PlayerState.WallSliding)
        //    {
        //        lastSlided.Add(id);
        //        wallInCollision.Add(other.gameObject);
        //        Debug.Log("SLIDING count" + lastSlided.Count);
        //        return;
        //    }

        //    if (!Grounded && playerState != PlayerState.WallSliding && rigidbody2D.velocity.y < 0 && lastSlided.Count == 0)
        //    {
        //        lastSlided.Add(id);
        //        wallInCollision.Add(other.gameObject);

        //        float relPosSign = Mathf.Sign(other.transform.position.x - transform.position.x);
        //        float inputSign = Mathf.Sign(HorizontalInput);
        //        Debug.Log(relPosSign + " input " + inputSign);
        //        if(HorizontalInput != 0 && relPosSign == inputSign)
        //            SetState(PlayerState.WallSliding);

        //        Debug.Log("NewSlide count" + lastSlided.Count);
        //        return;
        //    }

        //    if (!Grounded && HorizontalInput == 0)
        //        Fall();

        //}
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        int id = other.GetInstanceID();

        if (other.tag == "Wall" && lastSlided.Contains(id))
        {
            lastSlided.Remove(id);
            wallInCollision.Remove(other.gameObject);
            //Debug.Log("EXIT Walls: " + floorIds.Count);
        }

        if (other.tag == "Floor" && floorsInCollision.Contains(other.gameObject))
        {
            //floorIds.Remove(id);
            floorsInCollision.Remove(other.gameObject);
            Debug.Log("EXIT Floors: " + floorsInCollision.Count);
        }
    }

    #endregion

    #region Private Helpers

    private void SetGravity(float targetGravity)
    {
        rigidbody2D.gravityScale = targetGravity / Mathf.Abs(Physics2D.gravity.y);
    }

    #endregion
}
