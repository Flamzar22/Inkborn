using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float climbSpeed;
    public float dashSpeed;
    public float dashSpeedChangeFactor;

    [Header("Drag")]
    public float groundDrag;
    public float climbDrag;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool doubleJump;

    [Header("Coyote Time")]
    public float coyoteTime;
    private float coyoteTimeCounter;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    bool readyToJump = true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public LayerMask whatIsJumpPlatform;
    public bool grounded;
    public bool onJumpPlatform;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Slow Falling")]
    public float slowFallGravity;
    public float maxFallSpeed;

    [Header("Honey Obstacle")]
    public float slowPercentage;
    [Range(0f, 1f)]
    public float fallingReduction;

    [Header("References")]
    public Climbing climbingScript;
    public GrapplinHook grapplingHook;
    public WallJumping wallJumpScript;
    public Transform orientation;
    public AudioClip jumpSound;
    public AudioSource audioSource;
    private Animator animator;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        freeze,
        walking,
        climbing,
        dashing,
        grappling,
        wallJumping,
        slowFalling,
        inHoney,
        air
    }

    private bool slowFalling;
    public bool dashing;
    public bool climbing;
    public bool grappling;
    public bool freeze;
    public bool inHoney;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        
        if (wallJumpScript == null)
        {
            wallJumpScript = GetComponent<WallJumping>();
        }
        if (climbingScript == null)
        {
            climbingScript = GetComponent<Climbing>();
        }
        if (grapplingHook == null)
        {
            grapplingHook = GetComponent<GrapplinHook>();
        }
        rb.freezeRotation = true;
    }
    void Update()
    {
        StateHandler();
        Animations();

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        onJumpPlatform = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsJumpPlatform);

        if (grounded || onJumpPlatform)
        {
            coyoteTimeCounter = coyoteTime;
            doubleJump = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        MyInput();
        SpeedControl();

        if (grounded && !dashing && !grappling && !climbing || onJumpPlatform && !dashing && !grappling && !climbing)
        {
            rb.linearDamping = groundDrag;
        }
        else if(climbing)
        {
            rb.linearDamping = climbDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void MyInput()
    {
        if (state == MovementState.freeze) { return; }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (jumpBufferCounter > 0f && readyToJump && coyoteTimeCounter > 0f && !grappling && (wallJumpScript == null || !wallJumpScript.IsActive))
        {
            readyToJump = false;
            jumpBufferCounter = 0f;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if(Input.GetKeyDown(jumpKey) && doubleJump && coyoteTimeCounter <= 0 && !grounded && !grappling && (wallJumpScript == null || !wallJumpScript.IsActive))
        {
            doubleJump = false;
            Jump();
        }

        slowFalling = !grappling && Input.GetKey(KeyCode.Space) && !grounded;
    }
    private void MovePlayer()
    {
        if (climbing) {  return; }
        if(state == MovementState.freeze) { return; }
        if (grappling) { return; }
        if (climbingScript.exitingWall) { return; }
        if (wallJumpScript != null && wallJumpScript.IsActive && !grounded){ return; }
        if (state == MovementState.dashing) { return; }

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 15f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (grounded || onJumpPlatform)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (OnSlope())
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;

            if (slowFalling && rb.linearVelocity.y < 0f)
            {
                rb.AddForce(Physics.gravity * (slowFallGravity - 1f), ForceMode.Acceleration);

                if (rb.linearVelocity.y < -maxFallSpeed)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, -maxFallSpeed, rb.linearVelocity.z);
                }
            }
            if (inHoney && rb.linearVelocity.y < 0 && !slowFalling)
            {
                rb.AddForce(Physics.gravity * (fallingReduction - 1f), ForceMode.Acceleration);
            }
        }
    }
    private void SpeedControl()
    {
        if (wallJumpScript != null && wallJumpScript.IsActive) { return; }
        if (OnSlope())
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatSpeed.magnitude > moveSpeed)
            {
                Vector3 limitedSpeed = flatSpeed.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedSpeed.x, rb.linearVelocity.y, limitedSpeed.z);
            }
        }
    }
    private float desiredJumpForce;
    private void Jump()
    {
        desiredJumpForce = jumpForce;
        if (inHoney)
        {
            desiredJumpForce -= desiredJumpForce * slowPercentage / 100;
        }
        animator.SetTrigger("Jump");
        audioSource.PlayOneShot(jumpSound);
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * desiredJumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;
    private void StateHandler()
    {
        if (freeze)
        {
            state = MovementState.freeze;
            desiredMoveSpeed = 0f;
            rb.linearVelocity = Vector3.zero;
        }
        else if (wallJumpScript != null && wallJumpScript.IsActive)
        {
            state = MovementState.wallJumping;
            desiredMoveSpeed = walkSpeed;
        }
        else if (grappling)
        {
            state = MovementState.grappling;
            desiredMoveSpeed = walkSpeed;
        }
        else if (slowFalling)
        {
            state = MovementState.slowFalling;
            desiredMoveSpeed = walkSpeed;
        }
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        else if (grounded || onJumpPlatform)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;

            desiredMoveSpeed = walkSpeed;
        }

        if (inHoney)
        {
            desiredMoveSpeed -= desiredMoveSpeed * slowPercentage / 100f;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) { keepMomentum = true; }

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }
    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.50f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0f;
        }
        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void Animations()
    {
        if(horizontalInput == 0f && verticalInput == 0f && !climbing && grounded && !grappling && !dashing || climbing || wallJumpScript.touchingLeftWall || wallJumpScript.touchingRightWall)
        {
            animator.SetBool("IsRunning", false);
        }
        else
        {
            animator.SetBool("IsRunning", true);
        }

        if(!grounded && !onJumpPlatform && !wallJumpScript.touchingLeftWall && !wallJumpScript.touchingRightWall && !climbing)
        {
            animator.SetBool("Falling", true);
        }
        else
        {
            animator.SetBool("Falling", false);
        }

        if(climbing && rb.linearVelocity.y > 0)
        {
            animator.SetBool("ClimbUp", true);
            animator.SetBool("ClimbDown", false);
        }
        else if (climbing && rb.linearVelocity.y < 0)
        {
            animator.SetBool("ClimbUp", false);
            animator.SetBool("ClimbDown", true);
        }
        else if (climbing && rb.linearVelocity.sqrMagnitude > 0)
        {
            animator.SetBool("ClimbUp", true);
            animator.SetBool("ClimbDown", false);
        }
        else if(climbing && rb.linearVelocity == Vector3.zero)
        {
            animator.SetBool("ClimbUp", false);
            animator.SetBool("ClimbDown", false);
            animator.SetTrigger("Climb");
        }
        else
        {
            animator.SetBool("ClimbUp", false);
            animator.SetBool("ClimbDown", false);
        }

        if (wallJumpScript.IsActive)
        {
            if(wallJumpScript.touchingLeftWall || wallJumpScript.touchingRightWall)
            {
                animator.SetBool("ClimbUp", false);
                animator.SetBool("ClimbDown", false);
                animator.SetTrigger("Climb");
            }
        }

        if (grappling)
        {
            animator.SetBool("Grappling", true);
        }
        else
        {
            animator.SetBool("Grappling", false);
        }
    }
}

