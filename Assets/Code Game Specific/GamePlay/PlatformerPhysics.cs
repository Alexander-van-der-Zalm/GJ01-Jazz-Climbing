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

    public enum WallJumpState
    {
        LastLeft,
        Free,
        LastRight
    }

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
        [Range(0,10.0f)]
        public float JumpMaxHeight = 4.0f;
        [Range(0, 10.0f)]
        public float JumpMinHeight = 1.0f;
        [Range(0.01f,20.0f)]
        public float JumpTimeToApex = 0.44f;
        [Range(0,1.0f)]
        public float JumpQueueTime = 0.1f;
        [Range(0, 5.0f)]
        public float JumpSinceGrounded = 0.1f;

        //public float JumpMinHeight;
    }

    [System.Serializable]
    public class RunSettingsC
    {
        public float RunMaxVelocity = 4;
        public float RunAccelTime = 0.5f;
        public float RunDeAccelTime = 0.25f;
    }

    [System.Serializable]
    public class GroundingRayCastSettingsC
    {
        public float GroundedDistanceToGround = 0.1f;
        
        public Vector2 FeetOffset;
        public float RayDepth = 0.3f;
        public int FeetRays = 5;

        public float JumpMaxDistToGround = 0.15f;

    }

    [System.Serializable]
    public class WallSlideSettingsC
    {
        public float WallSlideDownGravity = 4;
        public float WallSlideUpGravity = 7;
        public float WallSlideInitialVelocity = 0.5f;
        //public float SlideBeforeFall = 0.25f;
        [Range(0,1.0f)]
        public float minSideVelocityFraction = 0.1f;
        public int WallSlideRays = 3;
        public int WallSlideRaysDisconnectedFall = 1;
        public float WallSlideRayDepth = 0.3f;
    }

    [System.Serializable]
    public class FallSettingsC
    {
        // Fall Settings
        public float FallGravity = 50;
        //public float MaxFallTimeToRespawn = 3.0f;
    }

    [System.Serializable]
    public class EdgeWonkSettingsC
    {
        public float HopHeight = 0.3f;
        public float HopTimeToApex = 0.5f;
        public float MinHopXVelocity = 2.0f;
        public float MaxVelocityForStop = 8.0f;
        public float MinFractionForHop = 0.3f;
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

    public struct DoubleVector2
    {
        public Vector2 V1,V2;

        public DoubleVector2(Vector2 v1, Vector2 v2)
        {
            V1 = v1;
            V2 = v2;
        }

        public Vector2 Mid { get { return V1 + 0.5f * (V2 - V1); } }
    }

    #endregion

    #region Fields

    public Rigidbody2D RigidBody;
    public Animator Animator;

    public JumpSettingsC JumpSettings;
    public RunSettingsC RunSettings;
    public GroundingRayCastSettingsC GroundingRayCastSettings;
    public WallSlideSettingsC WallSlideSettings;
    public FallSettingsC FallSettings;
    public EdgeWonkSettingsC EdgeWonkSettings;
    public GrabSettingsC GrabSettings;

    // WallJump/SlideSettings
    private float WallJumpLastDirection = 0;

    // GrabSettings
    private Transform Grab;
    private Vector3 GrabOffset;

    // Debug
    public PlayerState playerState;
    [ReadOnly]
    public float Gravity;
    public bool DebugStateChanges = false;
    public bool DebugRayText = false;
    public bool DebugFeetRays = true;
    public bool DebugWallSlideRays = true;
    public bool DebugGrabRays = true;

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
    //private List<GameObject> floorsInCollision = new List<GameObject>();
    //private List<int> lastSlided = new List<int>();
    //private List<GameObject> wallInCollision = new List<GameObject>();
    //private List<GameObject> allInTriggerRange = new List<GameObject>();
    private int lastGrabbedID = 0;

    // Small optimisation
    private Animator animator;
    private Rigidbody2D rigid;
    private Transform tr;

    private string floorTag = "Floor";
    private string wallTag = "Wall";

    private Vector2 feetOffset1, feetOffset2;
    
    private float jumpQueue;
    private float timeSinceGrounded;
    private int jumpAmount;
    public WallJumpState lastJumpSide;

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

        //playerDels.OnTriggerEnter = new TriggerDelegate(OnGroundedTriggerEnter);
        //playerDels.OnTriggerStay = new TriggerDelegate(OnGroundedTriggerStay);
        //playerDels.OnTriggerExit = new TriggerDelegate(OnGroundedTriggerExit);
    }

    #endregion

    #region FixedUpdate
    
	void FixedUpdate ()
    {
        // Set some animator values
        StartOfUpdate();
        
        #region Vars

        // hor: Input & Dir is a mini cached var
        //float hor = HorizontalInput;
        float velocityX = rigid.velocity.x;
        float velocityY = rigid.velocity.y;

        // do some precalculaltions 
        //float accel = Time.fixedDeltaTime * RunSettings.RunMaxVelocity;
        //float velocityDirection = Mathf.Sign(velocityX);
        //float horInputDirection = Mathf.Sign(InputHorizontal);

        Vector2 colliderBotMid = GetColliderEdges(tr.collider2D, Side.Bottom).Mid;

        bool lastGrounded = Grounded;

        #endregion

        #region Grounded Check (Ray casts)

        // Calculate a safe ray depth
        float rayDepth = GroundingRayCastSettings.RayDepth;
        rayDepth -= velocityY < 0 ? velocityY * Time.fixedDeltaTime * 1.5f : 0;

        List<RaycastHit2D> hits = CastRaysAll(tr.collider2D, Side.Bottom, Vector2.up * -1, GroundingRayCastSettings.FeetRays, rayDepth, LayerMask.NameToLayer("Floor"), DebugFeetRays);
        
        // Get the index of the closest ray (-1 is no hit)
        int closestCollidingRayIndex = GetMinDistanceRay(hits);
        float distToGround = closestCollidingRayIndex >= 0 ? hits[closestCollidingRayIndex].fraction : float.MaxValue;
        
        Grounded = hits.Count > 2 && distToGround < GroundingRayCastSettings.JumpMaxDistToGround; // and min dist

        // Time since grounded for doing jumps
        if (!Grounded)
            timeSinceGrounded += Time.fixedDeltaTime;
        else
            timeSinceGrounded = 0;

        // Reset jumpTimes
        if (Grounded && velocityY < 0)
            jumpAmount = 0;

        #endregion

        #region Grab (RayCasts)

        // Cast Grab Rays
        Vector2 grabPos = Grab.position;
        // Offset
        grabPos += !facingRight ? new Vector2(-GrabSettings.RayOffset.x, GrabSettings.RayOffset.y) : GrabSettings.RayOffset;

        List<RaycastHit2D>  grabHits = CastRaysInAnglesFromPoint(grabPos, GrabSettings.GrabRayStartAngle, GrabSettings.GrabRayEndAngle,
                                         GrabSettings.GrabRayAmount, GrabSettings.GrabRayDistance,
                                         LayerMask.NameToLayer("GrabMe"), DebugGrabRays, !facingRight);

        // Not grabbing, but colliding with grab object
        if (grabHits.Count() > 0 && playerState != PlayerState.Grabbing)
        {
            GrabFunction(grabHits[0].collider);
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

        if (CazulWallSliding())
        {
            EndOfUpdate();
            return;
        }

        #endregion

        #region WallJump (Inactive)

        //if (!Grounded && playerState == PlayerState.WallJump)
        //{
        //    // Exit the walljumpstate if no or opposite direction input
        //    // This state ignores 
        //    if (InputHorizontal == 0 || InputHorizontal != WallJumpLastDirection)
        //    {
        //        SetState(PlayerState.Airborne);
        //        //Debug.Log("ExitWalljump");
        //    }
        //    CheckFlipByVelocity();
        //    EndOfUpdate();
        //    return;
        //}

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
        else if (InputHorizontal != 0 && velocityX != 0 && Mathf.Sign(velocityX) != Mathf.Sign(InputHorizontal))
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
        // JumpQueue:
        //  remembers jump input for a short while
        // TimeSinceGrounded:
        //  allows jumping for a short while after leaving the grounded state
        if ((Grounded && (InputJump || jumpQueue > 0)) || (InputJump && jumpAmount == 0 && timeSinceGrounded < JumpSettings.JumpSinceGrounded))
        {
            Jump();
            CheckFlipBy(InputHorizontal);
            EndOfUpdate();
        }
        #endregion

        #region Edgde Wonk

        if (hits.Count < 4 && hits.Count > 1 && playerState != PlayerState.Airborne)
        {
            // EDGE WONK TIME!
            animator.SetBool("EdgeWonk", true);
            // EDGE WONK STOP :O
            if ((InputHorizontal == 0 || Mathf.Abs(InputHorizontal) < EdgeWonkSettings.MinFractionForHop) && Mathf.Abs(rigid.velocity.x) < EdgeWonkSettings.MaxVelocityForStop)
                rigid.velocity = Vector2.zero;
        }
        else
        {
            animator.SetBool("EdgeWonk", false);
        }

        #endregion

        #region EdgeHop

        if (lastGrounded && !Grounded && jumpAmount == 0 && playerState != PlayerState.Airborne)
        {
            // Gieb x velocity in the right direction (so it doest just jump up)
            if (Mathf.Abs(rigid.velocity.x) < EdgeWonkSettings.MinHopXVelocity)
            {
                float dir = facingRight ? 1 : -1;
                rigid.velocity = new Vector2(EdgeWonkSettings.MinHopXVelocity * dir, rigid.velocity.y);
            }
            // Do the edge hop
            EdgeHop();
            EndOfUpdate();
            return;
        }

        #endregion

        Fall();

        CheckFlipBy(InputHorizontal);

        #region Correct y position

        // Make sure it steps up in height if it hits an object to the left or right side if possible
        // Maybe also in collision method?

        // Y correct up
        //CorrectStuckYPosition();

        // Y correct when falling
        //if (Grounded && -rigid.velocity.y*Time.fixedDeltaTime > distToGround)
        //{
        //    Debug.Log("Correct y when falling | dist: " + distToGround + " | localpos: " + tr.localPosition.y + " world pos: " + tr.position.y + " | col: " + colliderBotMid.y + " |  y vel: " + rigid.velocity.y + " |  rayDepth: " + rayDepth);
        //    Debug.Log("Collider Pos " + hits[0].transform.position);// + " + center: " + hits[0].rigidbody.gameObject.transform.position + new Vector3(0,0.5f,0));
        //    //Debug.Break();
            
        //    // Set transform to GroundedDistanceToGroundd
        //    float yPos = hits[closestCollidingRayIndex].point.y + GroundingRayCastSettings.GroundedDistanceToGround;
        //    tr.position = new Vector2(colliderBotMid.x, yPos);

        //    // Kill gravity and stop y velocity
        //    rigid.velocity = new Vector2(rigid.velocity.x, 0);
        //    //rigid.gravityScale = 0;

        //    Debug.Log("Corrected y when falling | dist: " + distToGround + " | pos: " + tr.position.y);
        //}

        #endregion

        EndOfUpdate();
	}

    private void CorrectStuckYPosition()
    {
        DoubleVector2 bottomCollider = GetColliderEdges(tr.collider2D, Side.Bottom);
        Vector2 rayPos = !facingRight ? bottomCollider.V1 : bottomCollider.V2;
        Vector2 dir = Vector2.right * Mathf.Abs(rigid.velocity.x) * Time.fixedDeltaTime;
        dir *= facingRight ? 1 : -1;
        List<RaycastHit2D> hits = Physics2D.RaycastAll(rayPos, dir, dir.magnitude, 1 << LayerMask.NameToLayer("Tiles")).ToList();
        DrawHits(hits, rayPos, dir);
    }

    #endregion

    #region Start and End of update

    private void StartOfUpdate()
    {
        // Set Animator floats
        animator.SetFloat("Horizontal", InputHorizontal);
        animator.SetFloat("Vertical", InputVertical);
    }

    private void EndOfUpdate()
    {
        // Set end of update animator values
        animator.SetFloat("VelocityX", rigid.velocity.x);
        animator.SetFloat("VelocityY", rigid.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(rigid.velocity.x)/RunSettings.RunMaxVelocity);
        
        InputJump = false;
        // Jump queue plx
        jumpQueue -= Time.fixedDeltaTime;
        jumpQueue = jumpQueue < 0 ? 0 : jumpQueue;

        // Reset the walljumpstate if grounded
        if (Grounded)
            resetWallJump();

        // For debug purposes
        Gravity = GetGravity();
    }

    #endregion

    #region Handle Input

    public void SetMovementInput(float horizontalInput, float verticalInput, bool jump, bool jumpDown, bool dash)
    {
        InputHorizontal = horizontalInput;
        InputVertical   = verticalInput;
        
        InputJump       = jump;

        // Jump Queue
        if (InputJump != jumpDown && jumpDown)
            jumpQueue = JumpSettings.JumpQueueTime;

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
        
        // Can walljump in any direction after grabbing
        resetWallJump();

        // Make sure the animation clip is facing the right direction
        CheckFlipBy(dir);
    }

    private void resetWallJump()
    {
        lastJumpSide = WallJumpState.Free;
    }

    #endregion

    #region WallJump & Slide

    //private bool OriginalWallSliding()
    //{
    //    // During sliding:
    //    // On the floor - Or - No More slide space
    //    if (playerState == PlayerState.WallSliding && (Grounded || lastSlided.Count == 0))
    //    {
    //        SetState(PlayerState.Idle);
    //    }

    //    // Start a new slide 
    //    if (!Grounded && playerState != PlayerState.WallSliding && lastSlided.Count > 0 && InputHorizontal != 0)
    //    {
    //        // Slide only if the input is correct(to the wall)
    //        foreach (GameObject col in wallInCollision)
    //        {
    //            float relPosSign = Mathf.Sign(col.transform.position.x - tr.position.x);
    //            float inputSign = Mathf.Sign(InputHorizontal);

    //            // inputdir is correct in relation to position differance
    //            if (relPosSign == inputSign)
    //                SetState(PlayerState.WallSliding);

    //            CheckFlipBy(relPosSign);
    //        }
    //    }

    //    if (playerState == PlayerState.WallSliding && InputJump)
    //    {
    //        float dir = 1;
    //        if (facingRight)
    //            dir = -1;

    //        rigid.velocity = new Vector2(RunSettings.RunMaxVelocity * dir, 0);

    //        WallJump();

    //        lastSlided.Clear();
    //        wallInCollision.Clear();
    //        //Debug.Log("SLIDING Clear");
    //    }

    //    if (playerState == PlayerState.WallSliding && InputHorizontal == 0)
    //    {
    //        Fall();
    //    }

    //    return playerState == PlayerState.WallSliding;   
    //}

    private bool CazulWallSliding()
    {
        #region EndSlide when on the floor

        // During sliding:
        // On the floor - Or - No More slide space
        if (playerState == PlayerState.WallSliding && Grounded)
        {
            SetState(PlayerState.Idle);
            return false;
        }

        #endregion

        #region RayCasts

        // Only cast to the side your facing
        Side raycastSide = facingRight ? Side.Right : Side.Left;
        Vector2 rayDir = facingRight ? Vector2.right : Vector2.right * -1;
        List<RaycastHit2D> hits = CastRays(tr.collider2D, raycastSide, rayDir, WallSlideSettings.WallSlideRays, WallSlideSettings.WallSlideRayDepth, LayerMask.NameToLayer("Wall"),DebugWallSlideRays);

        #endregion

        #region Start a new slide (when x out of yd raycasts hit)

        if (!Grounded && playerState != PlayerState.WallSliding && hits.Count >= WallSlideSettings.WallSlideRays -WallSlideSettings.WallSlideRaysDisconnectedFall)//Raycasts 
        {
            SetState(PlayerState.WallSliding);
            //float relPosSign = Mathf.Sign(col.transform.position.x - tr.position.x);
            //CheckFlipBy(relPosSign);
        }

        #endregion

        // Exit out
        if (playerState != PlayerState.WallSliding)
            return false;

        #region Jump (Input)

        if (InputJump)
        {
            if (WallJump()) // Exit out
                return false;
        }

        #endregion

        #region Stop sliding (when above or below wall) (less than x raycasts)

        // Wall cancel
        if (!(hits.Count >= WallSlideSettings.WallSlideRays - WallSlideSettings.WallSlideRaysDisconnectedFall))
        {
            Fall();
            return false; // Exit out
        }

        #endregion

        // Change Gravity during slide when going down
        if (rigid.velocity.y < 0)
            SetGravity(WallSlideSettings.WallSlideDownGravity);
        else
            SetGravity(WallSlideSettings.WallSlideUpGravity);

        // Stop x velocity when moving away from the wall
        float facingDir = facingRight ? -1 : 1;
        if (facingDir == Mathf.Sign(rigid.velocity.x))
            rigid.velocity = new Vector2(0, rigid.velocity.y);
           
        return true;    
    }

    private bool WallJump()
    {
        // Ideas:
        // - Walljump height based on upwards velocity
        // - Walljump same side only once (reset per dash, other side hit, grounded, grab and/or possibly a timer)
        // 

        // Only walljump if it did not jump on that same side already
        WallJumpState jumpSideNow = facingRight ? WallJumpState.LastLeft : WallJumpState.LastRight;

        if (jumpSideNow == lastJumpSide) //Cancel out
            return false;

        //Debug.Log(jumpSideNow + " " + lastJumpSide);

        lastJumpSide = jumpSideNow;

        // If jumping is allowed:
        // Handled as several cases:
        // - jump up when vertical input >= 0
        // - otherwise fall down

        float fr = WallSlideSettings.minSideVelocityFraction;
        float dir = facingRight ? -fr : fr;

        float hor = InputHorizontal;

        // Fake analogue input when there is no analogue value
        if (InputVertical != 0 && !(Mathf.Abs(InputHorizontal) < 1.0f) && InputHorizontal != 0)
        {
            hor *= 0.5f;
            Debug.Log("WallJumpFraction");
        }

        // Only add the input fraction if the input is away from the wall (otherwise ignore)
        dir += Mathf.Sign(dir) == Mathf.Sign(hor) ? hor * (1 - fr) : 0;

        // Add the velocity
        rigid.velocity = new Vector2(RunSettings.RunMaxVelocity * dir, 0);

        // Fall
        if (InputVertical < 0)
            Fall();
        else
            Jump();

        // Double check the direction
        CheckFlipByVelocity();

        return true;
    }

    #endregion

    #region Jump

    public void Jump()
    { 
        SetState(PlayerState.Airborne);
        jumpAmount++;
        StartCoroutine(MarioJump(JumpSettings.JumpMaxHeight, JumpSettings.JumpTimeToApex, JumpSettings.JumpMinHeight, true));
    }

    public void EdgeHop()
    {
        SetState(PlayerState.Airborne);
        StartCoroutine(MarioJump(EdgeWonkSettings.HopHeight, EdgeWonkSettings.HopTimeToApex, EdgeWonkSettings.HopHeight, false));
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

    /// <summary>
    /// A jump that changes the entities gravityscale to create the desired jump effect.
    /// </summary>
    /// <param name="height"></param>
    /// <param name="timeToApex"></param>
    /// <param name="minimumHeight">Optional parameter</param>
    /// <param name="holdInputForHeight">Optional parameter</param>
    /// <returns></returns>
    public IEnumerator MarioJump(float height, float timeToApex, float minimumHeight, bool holdInputForHeight = false)
    {
        float h = height;//height
        float t = timeToApex;//time to apex
        float hmin = minimumHeight;//minimum height (Not implemented yet)

        float g = (2*h)/(t*t); // gravity
        float v = Mathf.Sqrt(2*g*h); // initial y velocity

        #region debug stuff
        //Debug.Log("JUMP");
        //Debug.Log("g: " + g + " v " + v + " gnow " + Physics2D.gravity.y);
        //Debug.Log(rigid.velocity + " " + playerState + " g: " + Grounded);
        #endregion

        // Set velocity and gravity
        SetGravity(g);
        rigid.velocity = new Vector2(rigid.velocity.x, v);

        float timeStart = Time.timeSinceLevelLoad;
        float flyTime = 0;
        float initialHeight = rigid.position.y;

        bool holdDown = holdInputForHeight ? InputJump : false;

        // Keep initial gravity till apex or player releases input (when the min height has been reached)
        while (flyTime < t && (holdDown || initialHeight + minimumHeight > rigid.position.y))
        {
            flyTime = Time.timeSinceLevelLoad - timeStart;
            if(holdInputForHeight)
                holdDown = InputJumpDown;
            
            //Debug.Log(holdInputForHeight + " " + InputJump +  " " + )
            
            //Debug.Log("startY " + initialHeight + " |y " + rigid.position.y + " |min " + minimumHeight);
            //Debug.Log("holdDown " + holdDown + " |flytime " + (flyTime) + " |t " + (t));
            //Debug.Log("inputBool " + holdInputForHeight + " |  Input " + InputJumpDown);
            //Debug.Log(rigid.velocity + " g " + Grounded);
            yield return null;
        }
        //Debug.Log("JumpFinish");
        
        
        // Time for falling gravity (till it hits the apex then it auto changes to falling gravity)
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
        if (Grounded)
            return;

        // Fall gravity
        if (rigid.velocity.y > 0)// || falldown)
        {
            SetState(PlayerState.Airborne);
        }
        else if (playerState != PlayerState.Falling)
        {
            //Debug.Log("FALLING " + falldown + " " + playerState);
            SetState(PlayerState.Falling);
            SetGravity(FallSettings.FallGravity);
            //Debug.Log("Fall");
            //rigid.gravityScale = FallGravity / Mathf.Abs(Physics2D.gravity.y);
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
                //StartCoroutine(RespawnWhenFallingTooLong());
                break;

            case PlayerState.WallSliding:
                animator.SetBool("WallSlide", true);
                
                //rigid.velocity = new Vector2(0, -WallSlideSettings.WallSlideInitialVelocity);
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

    #region Grounded Triggers (Legacy)

    //public void OnGroundedTriggerEnter(Collider2D other)
    //{
    //    int id = other.GetInstanceID();
    //    if (other.CompareTag(floorTag) && !floorsInCollision.Contains(other.gameObject))//floorIds.Contains(id))
    //    {
    //        //floorIds.Add(id);
    //        floorsInCollision.Add(other.gameObject);
    //        allInTriggerRange.Add(other.gameObject);
    //        Debug.Log("add floor. Count of floors: " + (from s in allInTriggerRange where s.CompareTag(floorTag) select s).Count());
            
    //        //Debug.Log("ENTER Floors: " + floorsInCollision.Count + " id " + id);
    //    }

    //    if (other.CompareTag(wallTag) && !lastSlided.Contains(id))
    //    {
    //        lastSlided.Add(id);
    //        wallInCollision.Add(other.gameObject);
    //        allInTriggerRange.Add(other.gameObject);
    //        //Debug.Log("ENTER Walls: " + lastSlided.Count + " id " + id);
    //    }
    //}

    ////private float triggerTime;

    //public void OnGroundedTriggerStay(Collider2D other)
    //{
    //    //if (triggerTime != Time.timeSinceLevelLoad)
    //    //{
            
    //    //    allInTriggerRange.Clear();
    //    //    triggerTime = Time.timeSinceLevelLoad;
    //    //}
    //    //allInTriggerRange.Add(other.gameObject);
    //}

    //public void OnGroundedTriggerExit(Collider2D other)
    //{
    //    int id = other.GetInstanceID();

    //    if (other.CompareTag(wallTag) && lastSlided.Contains(id))
    //    {
    //        lastSlided.Remove(id);
    //        wallInCollision.Remove(other.gameObject);
    //        //Debug.Log("EXIT Walls: " + floorIds.Count);
    //    }

    //    if (other.CompareTag(floorTag) && floorsInCollision.Contains(other.gameObject))
    //    {
    //        //floorIds.Remove(id);
    //        floorsInCollision.Remove(other.gameObject);
    //        //Debug.Log("EXIT Floors: " + floorsInCollision.Count);
    //    }
    //}

    #endregion

    #region Private Helpers

    private void SetGravity(float targetGravity)
    {
        rigid.gravityScale = targetGravity / Mathf.Abs(Physics2D.gravity.y);
    }

    private float GetGravity()
    {
        return rigid.gravityScale * Mathf.Abs(Physics2D.gravity.y);
    }

    private DoubleVector2 GetColliderEdges(Collider2D collider, Side side)
    {
        Vector2 v1, v2;
        Vector2 min = collider.bounds.min;
        Vector2 max = collider.bounds.max;

        switch (side)
        {
            case Side.Bottom:
                v1 = min;
                v2 = new Vector2(max.x, min.y);
                break;
            case Side.Left:
                v1 = min;
                v2 = new Vector2(min.x, max.y);
                break;
            case Side.Right:
                v1 = max;
                v2 = new Vector2(max.x, min.y);
                break;
            default://TOP
                v1 = max;
                v2 = new Vector2(min.x, max.y);
                break;
        }
        return new DoubleVector2(v1, v2);
    }

    #endregion

    #region Ray cast helpers

    private List<RaycastHit2D> CastRaysAll(Collider2D collider, Side side, Vector2 rayDirection, int amount, float rayLength, LayerMask layerMask, bool debug = false)
    {
        DoubleVector2 vec = GetColliderEdges(collider, side);
        return CastRaysAll(vec.V1, vec.V2, rayDirection, amount, rayLength, layerMask, debug);
    }

    private List<RaycastHit2D> CastRaysAll(Vector2 startPos, Vector2 endPos, Vector2 rayDirection, int amount, float rayLength, LayerMask layerMask, bool debug = false)
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
                DrawHits(hits, v, ray);
            }

            collided.AddRange(hits);
        }

        return collided;
    }

    private List<RaycastHit2D> CastRays(Collider2D collider, Side side, Vector2 rayDirection, int amount, float rayLength, LayerMask layerMask, bool debug = false)
    {
        DoubleVector2 vec = GetColliderEdges(collider, side);
        return CastRays(vec.V1, vec.V2, rayDirection, amount, rayLength, layerMask, debug);
    }

    private List<RaycastHit2D> CastRays(Vector2 startPos, Vector2 endPos, Vector2 rayDirection, int amount, float rayLength, LayerMask layerMask, bool debug = false)
    {
        List<RaycastHit2D> collided = new List<RaycastHit2D>();

        Vector2 offset = (endPos - startPos) / (amount - 1);
        Vector2 ray = rayDirection.normalized * rayLength;

        for (int i = 0; i < amount; i++)
        {
            Vector2 v = startPos + i * offset;
            
            RaycastHit2D hit = Physics2D.Raycast(v, ray, rayLength, 1 << layerMask);

            if (debug)
                DrawHit(hit, v, ray);
            
            // Only select non null hits and colliders
            if(hit!= null && hit.collider != null)
                collided.Add(hit);
        }

        return collided;
    }

    private bool DrawHit(RaycastHit2D hit, Vector2 v, Vector2 ray)
    {
        bool hitSomething = (hit != null && hit.collider != null);

        Color color = hitSomething ? Color.green : Color.red;
        Debug.DrawRay(v, ray, color);

        if(!hitSomething)
            return false;

        // Cross product foor orthognal v2
        Vector3 a = ray.normalized;
        Vector3 b = Vector3.forward;
        Vector2 x = Vector3.Cross(-a, b).normalized;

        Debug.DrawRay(v + ray * hit.fraction, x * 0.05f, Color.magenta);
        
        return true;
    }

    private void DrawHits(List<RaycastHit2D> hits, Vector2 v, Vector2 ray)
    {
        Color color = hits.Count > 0 ? Color.green : Color.red;
        Debug.DrawRay(v, ray, color);

        if (hits.Count == 0)
            return;

        // Cross product foor orthognal v2
        Vector3 a = ray.normalized;
        Vector3 b = Vector3.forward;
        Vector2 x = Vector3.Cross(-a,b).normalized;

        foreach(RaycastHit2D hit in hits)
        {
            Debug.DrawRay(v + ray * hit.fraction, x * 0.05f, Color.magenta);
        }
        
        //Debug.Log(minDist);
        
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

    private int GetMinDistanceRay(List<RaycastHit2D> hits)
    {
        float minDist = float.MaxValue;
        int index = -1;

        for (int i = 0; i < hits.Count; i++)
        {
            if (hits[i].fraction < minDist)
            {
                minDist = hits[i].fraction;
                index = i;
            }
        }

        return index;
    }

    #endregion
}
