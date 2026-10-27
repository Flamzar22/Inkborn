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

    [Header("Climbing Camera")]
    public CinemachineCamera climbingCamera;
    public Transform climbingCameraTarget;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    private bool climbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    public KeyCode jumpKey = KeyCode.Space;
    public int climbJumps;
    private int climbJumpsLeft;

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
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
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

        if (climbing && !exitingWall) { ClimbingMovement(); }
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
        if (wallFront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!climbing && climbTimer > 0) { StartClimbing(); }

            if (climbTimer > 0) { climbTimer -= Time.deltaTime; }
            if (climbTimer < 0) { StopClimbing(); }
        }
        else if (exitingWall)
        {
            if (climbing) { StopClimbing(); }

            if (exitWallTimer > 0) { exitWallTimer -= Time.deltaTime; }
            if (exitWallTimer < 0) { exitingWall = false; }
        }
        else
        {
            StopClimbing();
        }

        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0) { ClimbJump(); }
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
    }
    private void ClimbingMovement()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbSpeed, rb.linearVelocity.z);
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