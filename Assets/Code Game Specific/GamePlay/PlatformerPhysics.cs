using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public enum JazzClimbingPlayerActions
{
    Jump = 0,
    Dash = 1,
    Interact = 2,
    Slide = 3,
    PlayInstrument = 4
}

[RequireComponent(typeof(ControlScheme))]
public class PlatformerPhysics : MonoBehaviour
{
    #region Helper Enums & Classes

    private enum Side
    {
        Top,Left,Right,Bottom
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

        public RayCastSettings RaySettings;

        [System.Serializable]
        public class RayCastSettings
        {
            public float JumpMaxDistToGround = 0.1f;
            public Vector2 FeetOffset;
            public float FeetWidth;
            public float RayDepth;
            public int FeetRays;
        }
    }

    [System.Serializable]
    public class RunSettingsC
    {
        public float RunMaxVelocity = 4;
        public float RunAccelTime = 0.5f;
        public float RunDeAccelTime = 0.25f;
    }

    [System.Serializable]
    public class WallSlideSettingsC
    {
        public float WallSlideGravity = 4;
        public float WallSlideInitialVelocity = 0.5f;
        public float SlideBeforeFall = 0.25f;
    }

    [System.Serializable]
    public class FallSettingsC
    {
        // Fall Settings
        public float FallGravity = 50;
        public float MaxFallTimeToRespawn = 3.0f;
    }

    [System.Serializable]
    public class GrabSettingsC
    {
        [Range(0, 1f)]
        public float GrabRayDistance = 0.3f;
        [Range(0,30)]
        public int GrabRayAmount = 4;
        [Range(-360, 360)]
        public float GrabRayStartAngle = -15;
        [Range(-360, 360)]
        public float GrabRayEndAngle = 45;
        public float GrabMinNegYDist = 0.1f;
        public float MaxYVelocity = 10;
        public Vector2 RayOffset = Vector2.zero;
    }

    #endregion

    #region Fields

    public Rigidbody2D RigidBody;
    public Animator Animator;

    public JumpSettingsC JumpSettings;
    public RunSettingsC RunSettings;
    public WallSlideSettingsC WallSlideSettings;
    public FallSettingsC FallSettings;
    public GrabSettingsC GrabSettings;

    // WallJump/SlideSettings
    private float WallJumpLastDirection = 0;

    // GrabSettings
    private Transform Grab;
    private Vector3 GrabOffset;

    // Debug
    public PlayerState playerState;
    public bool DebugStateChanges = false;
    public bool DebugRayText = false;

    // Jump and direction
    private bool facingRight = true;
    private bool grounded;
    public bool Grounded { get { return grounded; } private set { grounded = value; animator.SetBool("Grounded", value); } }

    // Input mini-cache
    private float InputHorizontal = 0;
    private float InputVertical = 0;
    private bool InputJump = false;
    private bool InputJumpDown = false;
    private bool InputDash = false;

    // Storage for collided 
    //private List<int> floorIds = new List<int>();
    private List<GameObject> floorsInCollision = new List<GameObject>();
    private List<int> lastSlided = new List<int>();
    private List<GameObject> wallInCollision = new List<GameObject>();
    private List<GameObject> allInTriggerRange = new List<GameObject>();
    private int lastGrabbedID = 0;

    private Animator animator;
    private Rigidbody2D rigid;
    private Transform tr;

    private string floorTag = "Floor";
    private string wallTag = "Wall";

    private Vector2 feetOffset1, feetOffset2;

    #endregion

    #region Start

    // Use this for initialization
	void Awake ()
    {
        Grab = transform.Find("Grab");
        Transform Instrument = transform.Find("InstrumentLocation");
        Transform PlayerCharacter = transform.Find("PlayerCharacter");

        // Get components
        animator = Animator;// GetComponent<Animator>();
        rigid = RigidBody;// rigidbody2D;
        tr = PlayerCharacter.transform;
        
        ChildTrigger2DDelegates instrumentDels = ChildTrigger2DDelegates.AddChildTrigger2D(Instrument.gameObject, PlayerCharacter.transform);
        ChildTrigger2DDelegates grabDels = ChildTrigger2DDelegates.AddChildTrigger2D(Grab.gameObject, PlayerCharacter.transform);
        ChildTrigger2DDelegates playerDels = ChildTrigger2DDelegates.AddChildTrigger2D(PlayerCharacter.gameObject, null);

        //grabDels.OnTriggerEnter = new TriggerDelegate(OnWallTriggerEnter);
        //grabDels.OnTriggerStay = new TriggerDelegate(OnWallTriggerEnter);
        //grabDels.OnTriggerExit = new TriggerDelegate(LeaveWallTrigger);

        GrabOffset = Grab.position - tr.position;

        playerDels.OnTriggerEnter = new TriggerDelegate(OnGroundedTriggerEnter);
        playerDels.OnTriggerStay = new TriggerDelegate(OnGroundedTriggerStay);
        playerDels.OnTriggerExit = new TriggerDelegate(OnGroundedTriggerExit);
    }

    #endregion

    #region FixedUpdate
    
	void FixedUpdate ()
    {
        // Handled by PlayerInput or AI
        StartOfUpdate();

        #region Vars

        // hor: Input & Dir is a mini cached var
        //float hor = HorizontalInput;
        float velocityX = rigid.velocity.x;

        // do some precalculaltions 
        float accel = Time.fixedDeltaTime * RunSettings.RunMaxVelocity;
        float velocityDirection = Mathf.Sign(velocityX);
        float horInputDirection = Mathf.Sign(InputHorizontal);

        #endregion

        #region Grounded Check (Ray casts)

        // Maybe cache a little
        Vector2 startPos = tr.position;

        startPos += JumpSettings.RaySettings.FeetOffset + Vector2.right * -JumpSettings.RaySettings.FeetWidth;
        Vector2 endPos = startPos + Vector2.right * 2 * JumpSettings.RaySettings.FeetWidth;

        List<RaycastHit2D> hits = CastRays(tr.collider2D, Side.Bottom, Vector2.up * -1, JumpSettings.RaySettings.FeetRays, JumpSettings.RaySettings.RayDepth, LayerMask.NameToLayer("Tiles"), true);

        if (hits.Count > 2)
        {
            // Make sure it steps up in height if it hits an object to the left or right side if possible
            // Maybe also in collision method?
        }

        Grounded = hits.Count > 2;

        #endregion

        #region Grab (RayCasts)

        // Cast Grab Rays
        Vector2 grabPos = Grab.position;
        // Offset
        grabPos += !facingRight ? new Vector2(-GrabSettings.RayOffset.x, GrabSettings.RayOffset.y) : GrabSettings.RayOffset;

        hits = CastRaysInAnglesFromPoint(grabPos, GrabSettings.GrabRayStartAngle, GrabSettings.GrabRayEndAngle,
                                         GrabSettings.GrabRayAmount, GrabSettings.GrabRayDistance, 
                                         LayerMask.NameToLayer("GrabMe"), true, !facingRight);

        // Not grabbing, but colliding with grab object
        if (hits.Count() > 0 && playerState != PlayerState.Grabbing)
        {
            GrabFunction(hits[0].collider);
            EndOfUpdate();
            return;
        } // Grabbing
        else if (playerState == PlayerState.Grabbing)
        {
            HandleGrabbing();
            EndOfUpdate();
            return;
        } // Not colliding with grab object, but id still in memory
        else if(lastGrabbedID != 0) // Reset
        {
            lastGrabbedID = 0;
        }

        #endregion

        #region WallSlide

        if (CasulWallSliding())
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
            if (InputHorizontal == 0 || InputHorizontal != WallJumpLastDirection)
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
        if (InputHorizontal == 0 && velocityX == 0)
        {
            if (Grounded)
                SetState(PlayerState.Idle);
        }//DeAccel if on the ground with no input
        else if (InputHorizontal == 0 && Grounded && velocityX != 0) //No Input & onground     
        {
            DeAccel();
        } // Has input in the other direction (also in AIR)
        else if (InputHorizontal != 0 && velocityX != 0 && velocityDirection != horInputDirection)
        { 
            DeAccel();
        } // Accel
        else if(InputHorizontal != 0)
        {
            
            Accel();
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

        CheckFlipBy(InputHorizontal);

        //CheckFlipByVelocity();

        EndOfUpdate();
	}

    private void StartOfUpdate()
    {
        // Set Animator floats
        animator.SetFloat("Horizontal", InputHorizontal);
        animator.SetFloat("Vertical", InputVertical);
    }

    private void EndOfUpdate()
    {
        animator.SetFloat("VelocityX", rigid.velocity.x);
        animator.SetFloat("VelocityY", rigid.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(rigid.velocity.x)/RunSettings.RunMaxVelocity);
        
        InputJump = false;
        
        //allInTriggerRange.Clear();
    }

    #endregion

    #region Handle Input

    public void SetMovementInput(float horizontalInput, float verticalInput, bool jump, bool jumpDown, bool dash)
    {
        InputHorizontal = horizontalInput;
        InputVertical   = verticalInput;
        InputJump       = jump;
        InputJumpDown   = jumpDown;
        InputDash       = dash;
    }

    #endregion

    #region Movement

    private void Accel()
    {
        if (Grounded)
            SetState(PlayerState.Running);
        //else
        //   todo air Accel animation

        // Calculate the acceleration and add it to velocity
        float accel = (Time.fixedDeltaTime * RunSettings.RunMaxVelocity * Mathf.Sign(InputHorizontal)) / RunSettings.RunAccelTime;

        rigid.velocity += new Vector2(accel, 0);

        float velX = rigid.velocity.x;

        //CLAMP by this speed (lower max if input is not full)
        float maxSpeed = Mathf.Abs(RunSettings.RunMaxVelocity * InputHorizontal);
        if (Mathf.Abs(velX) > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed * Mathf.Sign(velX), rigid.velocity.y);
    }

    private void DeAccel()
    {
        if(Grounded)
            SetState(PlayerState.RunningStopping);
        //else
        //    todo air stop animation
        
        float velocityX = rigid.velocity.x;
        float velocityDirection = Mathf.Sign(velocityX);

        float accel = (Time.fixedDeltaTime * RunSettings.RunMaxVelocity * -velocityDirection) / this.RunSettings.RunDeAccelTime;

        rigid.velocity += new Vector2(accel, 0);

        // Only clamp to 0 with no input and an actual velocity
        if (InputHorizontal != 0 || rigid.velocity.x == 0)
            return;

        // CLAMP: If it ends up going in the other direction after accel, clamp it to 0
        if (Mathf.Sign(rigid.velocity.x) != velocityDirection)
            rigid.velocity = new Vector2(0, rigid.velocity.y);
    }

    #endregion

    #region Grabbing

    private void HandleGrabbing()
    {
        float hor = InputHorizontal;
        float vert = InputVertical;

        if (InputJump)
        {
            if (hor != 0)
            {
                if (hor < 0 && facingRight || hor > 0 && !facingRight)
                {
                    rigid.velocity = new Vector2(RunSettings.RunMaxVelocity * hor,0);
                }
            }
            if (vert < 0)
                Fall(true);
            else
                Jump();

        }
    }

    private void GrabFunction(Collider2D other)
    {
        #region Cancel grab if one of these conditions is true (id, dist, yUpSpeed)

        // The id is used to make sure it can actually leave the grab state 
        // and not regrab immediately
        int id = other.GetInstanceID();

        if (id == lastGrabbedID)
            return;

        // Makes sure its not to far down?
        float yDist = Grab.position.y - other.transform.position.y;

        if (yDist + GrabSettings.GrabMinNegYDist < 0)
            return;

        // If the upvelocity is too high, it will pass till falling down
        if (rigid.velocity.y > GrabSettings.MaxYVelocity)
            return;

        #endregion

        // Grab the object by translation
        Vector3 relPos = other.transform.position - tr.position;
        float dir = Mathf.Sign(relPos.x);

        Vector3 offset = GrabOffset;
        offset.x *= dir;
        Vector3 newPos = other.transform.position - offset;

        tr.position = newPos;

        // Change the state
        SetState(PlayerState.Grabbing);
        lastGrabbedID = id;

        // Make sure the animation clip is facing the right direction
        CheckFlipBy(dir);
    }

    #endregion

    #region WallJump & Slide

    private bool OriginalWallSliding()
    {
        // During sliding:
        // On the floor - Or - No More slide space
        if (playerState == PlayerState.WallSliding && (Grounded || lastSlided.Count == 0))
        {
            SetState(PlayerState.Idle);
        }

        // Start a new slide 
        if (!Grounded && playerState != PlayerState.WallSliding && lastSlided.Count > 0 && InputHorizontal != 0)
        {
            // Slide only if the input is correct(to the wall)
            foreach (GameObject col in wallInCollision)
            {
                float relPosSign = Mathf.Sign(col.transform.position.x - tr.position.x);
                float inputSign = Mathf.Sign(InputHorizontal);

                // inputdir is correct in relation to position differance
                if (relPosSign == inputSign)
                    SetState(PlayerState.WallSliding);

                CheckFlipBy(relPosSign);
            }
        }

        if (playerState == PlayerState.WallSliding && InputJump)
        {
            float dir = 1;
            if (facingRight)
                dir = -1;

            rigid.velocity = new Vector2(RunSettings.RunMaxVelocity * dir, 0);

            WallJump();

            lastSlided.Clear();
            wallInCollision.Clear();
            //Debug.Log("SLIDING Clear");
        }

        if (playerState == PlayerState.WallSliding && InputHorizontal == 0)
        {
            Fall();
        }

        return playerState == PlayerState.WallSliding;   
    }

    private bool CasulWallSliding()
    {
        // During sliding:
        // On the floor - Or - No More slide space
        if (playerState == PlayerState.WallSliding && (Grounded || lastSlided.Count == 0))
        {
            SetState(PlayerState.Idle);
        }

        // Start a new slide 
        if (!Grounded && playerState != PlayerState.WallSliding && lastSlided.Count > 0 )
        {
            SetState(PlayerState.WallSliding);
            
            // Still needed???
            foreach (GameObject col in wallInCollision)
            {
                float relPosSign = Mathf.Sign(col.transform.position.x - tr.position.x);

                CheckFlipBy(relPosSign);
            }
        }
        
        // Jump
        if (playerState == PlayerState.WallSliding && InputJump)
        {
            float dir = 1;
            if (facingRight)
                dir = -1;

            rigid.velocity = new Vector2(RunSettings.RunMaxVelocity * dir, 0);

            WallJump();

            lastSlided.Clear();
            wallInCollision.Clear();
            //Debug.Log("SLIDING Clear");
        }

        // Fall 
        if (playerState == PlayerState.WallSliding )
        {
            // only if the input is away from the wall
            foreach (GameObject col in wallInCollision)
            {
                if(InputHorizontal == 0)
                    break;
                float relPosSign = Mathf.Sign(col.transform.position.x - tr.position.x);
                float inputSign = Mathf.Sign(InputHorizontal);

                if(relPosSign!=inputSign)
                    Fall(true);
            }
            
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

    //    rigid.AddForce(new Vector2(0, JumpForce),ForceMode.VelocityChange);

    //    while (chargeTime <= JumpMaxHoldTime && ControlScheme.Actions[(int)PlayerActions.Jump].IsDown())
    //    {
    //        //rigid.AddForce(new Vector2(0, JumpMaxForce));
            

    //        //float dt = 1 - (chargeTime / JumpMaxHoldTime);
    //        //float force = JumpForce * dt;
    //        rigid.AddForce(new Vector2(0, JumpAccelForce),ForceMode.Acceleration);
    //        //Debug.Log("InbetweenJump: dt: " + dt);

    //        chargeTime = Time.timeSinceLevelLoad - timeStart;
    //        yield return null;
    //    }
        
    //    //if (chargeTime > )
    //    //{
    //    //    rigid.AddForce(new Vector2(0, JumpMaxForce));
    //    //    Debug.Log("maxJump ");
    //    //}
    //    //else
    //    //{
    //    //    float dt = chargeTime / JumpMaxHoldTime;
    //    //    float force = JumpMinForce + (JumpMaxForce - JumpMinForce) * dt;
    //    //    rigid.AddForce(new Vector2(0, force));
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

        //Debug.Log("JUMP");
        //Debug.Log("g: " + g + " v " + v + " gnow " + Physics2D.gravity.y);
        //Debug.Log(rigid.velocity + " " + playerState + " g: " + Grounded);

        rigid.gravityScale = g / Mathf.Abs(Physics2D.gravity.y);
        rigid.velocity = new Vector2(rigid.velocity.x, v);

        float timeStart = Time.timeSinceLevelLoad;
        float flyTime = 0;

        //float vearly = Mathf.Sqrt(v*v+2*g(

        while (flyTime < t && InputJumpDown)
        {
            flyTime = Time.timeSinceLevelLoad - timeStart;
            //Debug.Log(rigid.velocity + " " + playerState + " g: " + Grounded);
            yield return null;
        }
        //Debug.Log("JUMP");
        if(playerState == PlayerState.Airborne)
            rigid.gravityScale = 10; 
    }

    #endregion

    #region Flip

    private void CheckFlipByVelocity()
    {
        // Face the right direction
        if (rigid.velocity.x > 0 && !facingRight)
            Flip();
        else if (rigid.velocity.x < 0 && facingRight)
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
        Vector3 localScale = rigid.transform.localScale;
        localScale.x *= -1;
        rigid.transform.localScale = localScale;
    }
    #endregion

    #region Fall

    private void Fall(bool falldown = false)
    {
        // Fall gravity
        if (!Grounded && (rigid.velocity.y < 0 || falldown))
        {
            //Debug.Log("FALLING " + falldown + " " + playerState);
            SetState(PlayerState.Falling);
            SetGravity(FallSettings.FallGravity);
            
            //rigid.gravityScale = FallGravity / Mathf.Abs(Physics2D.gravity.y);
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
            if ((dt) > FallSettings.MaxFallTimeToRespawn)
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
        rigid.isKinematic = false;


        if(DebugStateChanges)
            Debug.Log(state + " vel: " + rigid.velocity);

        switch (state)
        {
            case PlayerState.Idle:

                break;
            case PlayerState.WallJump:
            case PlayerState.Airborne:
                animator.SetTrigger("Jump");
                break;

            case PlayerState.Falling:
                StartCoroutine(RespawnWhenFallingTooLong());
                break;

            case PlayerState.WallSliding:
                animator.SetBool("WallSlide", true);
                SetGravity(WallSlideSettings.WallSlideGravity);
                rigid.velocity = new Vector2(0, -WallSlideSettings.WallSlideInitialVelocity);
                //Debug.Log("StartSlide grav: " + rigid.gravityScale);
                break;

            case PlayerState.Grabbing:
                animator.SetBool("Grab", true);
                rigid.gravityScale = 0;
                rigid.velocity = Vector2.zero;
                rigid.isKinematic = true;
                break;

            case PlayerState.Running:

                break;
            default:

                break;

        }

        playerState = state;
    }

    #endregion

    #region Grounded Triggers

    public void OnGroundedTriggerEnter(Collider2D other)
    {
        int id = other.GetInstanceID();
        if (other.CompareTag(floorTag) && !floorsInCollision.Contains(other.gameObject))//floorIds.Contains(id))
        {
            //floorIds.Add(id);
            floorsInCollision.Add(other.gameObject);
            allInTriggerRange.Add(other.gameObject);
            Debug.Log("add floor. Count of floors: " + (from s in allInTriggerRange where s.CompareTag(floorTag) select s).Count());
            
            //Debug.Log("ENTER Floors: " + floorsInCollision.Count + " id " + id);
        }

        if (other.CompareTag(wallTag) && !lastSlided.Contains(id))
        {
            lastSlided.Add(id);
            wallInCollision.Add(other.gameObject);
            allInTriggerRange.Add(other.gameObject);
            //Debug.Log("ENTER Walls: " + lastSlided.Count + " id " + id);
        }
    }

    //private float triggerTime;

    public void OnGroundedTriggerStay(Collider2D other)
    {
        //if (triggerTime != Time.timeSinceLevelLoad)
        //{
            
        //    allInTriggerRange.Clear();
        //    triggerTime = Time.timeSinceLevelLoad;
        //}
        //allInTriggerRange.Add(other.gameObject);
    }

    public void OnGroundedTriggerExit(Collider2D other)
    {
        int id = other.GetInstanceID();

        if (other.CompareTag(wallTag) && lastSlided.Contains(id))
        {
            lastSlided.Remove(id);
            wallInCollision.Remove(other.gameObject);
            //Debug.Log("EXIT Walls: " + floorIds.Count);
        }

        if (other.CompareTag(floorTag) && floorsInCollision.Contains(other.gameObject))
        {
            //floorIds.Remove(id);
            floorsInCollision.Remove(other.gameObject);
            //Debug.Log("EXIT Floors: " + floorsInCollision.Count);
        }
    }

    #endregion

    #region Private Helpers

    private void SetGravity(float targetGravity)
    {
        rigid.gravityScale = targetGravity / Mathf.Abs(Physics2D.gravity.y);
    }

    private List<RaycastHit2D> CastRays(Collider2D collider, Side side, Vector2 rayDirection, int amount, float rayLength, LayerMask layerMask, bool debug = false)
    {
        Vector2 v1, v2;
        Vector2 min = collider.bounds.min;
        Vector2 max = collider.bounds.max;

        switch(side)
        {
            case Side.Bottom:
                v1 = min;
                v2 = new Vector2(max.x,min.y);
                break;
            case Side.Left:
                v1 = min;
                v2 = new Vector2(min.x,max.y);
                break;
            case Side.Right:
                v1 = max;
                v2 = new Vector2(max.x,min.y);
                break;
            default://TOP
                v1 = max;
                v2 = new Vector2(min.x,max.y);
                break;
        }
        return CastRays(v1, v2, rayDirection, amount, rayLength, layerMask, debug);
    }

    private List<RaycastHit2D> CastRays(Vector2 startPos, Vector2 endPos, Vector2 rayDirection, int amount, float rayLength, LayerMask layerMask, bool debug = false)
    {
        List<RaycastHit2D> collided = new List<RaycastHit2D>();
        
        Vector2 offset = (endPos-startPos)/(amount-1);
        Vector2 ray = rayDirection.normalized * rayLength;

        for (int i = 0; i < amount; i++)
        {
            Vector2 v = startPos + i * offset;
            // Only select non null hits and colliders
            List<RaycastHit2D> hits = Physics2D.RaycastAll(v, ray, rayLength, 1 << layerMask).Where(h => h != null && h.collider != null).ToList();
            
            if (debug)
            {
                Color color = hits.Count > 0 ? Color.green : Color.red;
                Debug.DrawRay(v, ray, color);
            }

            collided.AddRange(hits);
        }

        return collided;
    }

    private List<RaycastHit2D> CastRaysInAnglesFromPoint(Vector2 centerPoint, float startAngle, float endAngle, int amount, float rayLength, LayerMask layerMask, bool debug = false, bool flipX = false)
    {
        List<RaycastHit2D> collided = new List<RaycastHit2D>();

        float angleStep = (endAngle - startAngle) / (amount - 1);

        for (int i = 0; i < amount; i++)
        {
            float a = startAngle + angleStep * i;
            Vector2 ray = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad)).normalized * rayLength;

            if (flipX)
                ray.x = ray.x * -1;

            List<RaycastHit2D> hits = Physics2D.RaycastAll(centerPoint, ray, rayLength, 1 << layerMask).Where(h => h != null && h.collider != null).ToList();

            if (debug)
            {
                Color color = hits.Count > 0 ? Color.green : Color.red;
                Debug.DrawRay(centerPoint, ray, color);
            }
            collided.AddRange(hits);
        }
        return collided;
    }

    private float GetMinDistance(List<RaycastHit2D> hits)
    {
        float minDist = float.MaxValue;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.fraction < minDist)
                minDist = hit.fraction;
        }
        return minDist;
    }

    #endregion
}
