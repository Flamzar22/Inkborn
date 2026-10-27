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


    public float groundDrag;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    [Header("Coyote Time")]
    public float coyoteTime;
    private float coyoteTimeCounter;

    bool readyToJump = true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Slow Falling")]
    public float slowFallGravity;
    public float maxFallSpeed;

    [Header("References")]
    public Climbing climbingScript;
    public GrapplinHook grapplingHook;
    public WallJumping wallJumpScript;
    public Transform orientation;

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
        air
    }

    private bool slowFalling;
    public bool dashing;
    public bool climbing;
    public bool grappling;
    public bool freeze;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
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

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        MyInput();
        SpeedControl();

        if (grounded && !dashing && !grappling && !climbing)
        {
            rb.linearDamping = groundDrag;
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
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey) && readyToJump && coyoteTimeCounter > 0f && !grappling && (wallJumpScript == null || !wallJumpScript.IsActive))
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        slowFalling = !grappling && Input.GetKey(KeyCode.Space) && !grounded;
    }
    private void MovePlayer()
    {
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
        else if (grounded)
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
    private void Jump()
    {
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
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
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;

            desiredMoveSpeed = walkSpeed;
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
}

