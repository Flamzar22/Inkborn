using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public LayerMask whatIsWall;
    public PlayerMovement pm;
    public Transform playerObj;

    [Header("Climbing Camera")]
    public CinemachineCamera climbingCamera;
    public Transform climbingCameraTarget;

    [Header("Climbing")]
    public float climbSpeed;
    public float wallStrafeSpeed = 3f;

    private bool climbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    public KeyCode jumpKey = KeyCode.Space;
    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Wall Sliding")]
    public float slideSpeed;

    [Header("Detection")]
    public float detectionLenght;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLenght, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall || MathF.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || pm.grounded)
        {
            climbJumpsLeft = climbJumps;
        }
    }
    private bool IsMovingTowardWall()
    {
        if (orientation == null || !wallFront) { return false; }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = orientation.forward * vertical + orientation.right * horizontal;

        if (inputDirection.sqrMagnitude < 0.01f) {return false; }

        Vector3 directionTowardWall = -frontWallHit.normal;

        return Vector3.Dot(inputDirection.normalized, directionTowardWall) > 0.1f;
    }
    void Update()
    {
        if (pm != null && pm.wallJumpScript != null && pm.wallJumpScript.IsActive)
        {
            StopClimbing();
            return;
        }

        WallCheck();
        StateMachine();

        if (climbing && !exitingWall)
        {
            ClimbingMovement();
        }
    }
    private void LateUpdate()
    {
        if (!climbing || !wallFront || climbingCameraTarget == null){ return; }

        climbingCameraTarget.position = transform.position;

        Vector3 forward = -frontWallHit.normal;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
        {
            climbingCameraTarget.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }
    private void StateMachine()
    {
        bool validWall = wallFront && wallLookAngle < maxWallLookAngle;

        if (!exitingWall && validWall)
        {
            if (!climbing && IsMovingTowardWall())
            {
                StartClimbing();
            }
        }
        else if (exitingWall)
        {
            if (climbing)
            {
                StopClimbing();
            }

            if (exitWallTimer > 0f)
            {
                exitWallTimer -= Time.deltaTime;
            }
            else
            {
                exitingWall = false;
            }
        }
        else
        {
            StopClimbing();
        }

        if (wallFront && climbing && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0)
        {
            ClimbJump();
        }
    }
    private void StartClimbing()
    {
        climbing = true;
        pm.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;

        if (climbingCamera != null)
        {
            climbingCamera.Priority = 30;
        }

        Vector3 lookDirection = -frontWallHit.normal;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);

            if (playerObj != null)
            {
                playerObj.rotation = targetRotation;
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }
    private void ClimbingMovement()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        Vector3 wallRight = Vector3.Cross(Vector3.up, frontWallHit.normal).normalized;

        float verticalSpeed = verticalInput * climbSpeed;

        if (Mathf.Abs(verticalInput) < 0.01f)
        {
            verticalSpeed = -slideSpeed;
        }

        Vector3 velocity = Vector3.up * verticalSpeed + wallRight * horizontalInput * wallStrafeSpeed;

        rb.linearVelocity = velocity;
    }
    private void StopClimbing()
    {
        climbing = false;
        pm.climbing = false;

        if (climbingCamera != null)
        {
            climbingCamera.Priority = 10;
        }
    }
    private void ClimbJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToAplly = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToAplly, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}