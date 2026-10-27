using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WallJumpZone : MonoBehaviour
{
    [Header("Wall Jump Zone")]
    public Collider leftWall;
    public Collider rightWall;

    [Header("Movement Direction")]
    public Transform lateralReference;

    [Header("Cameras")]
    public CinemachineCamera normalCamera;
    public CinemachineCamera wallJumpCamera;

    private void Awake()
    {
        normalCamera = GameObject.FindGameObjectWithTag("ThirdPersonCam").GetComponent<CinemachineCamera>();
    }
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        WallJumping wallJump = other.GetComponentInParent<WallJumping>();

        if(wallJump != null)
        {
            wallJump.EnterZone(this);
            ActivateWallJumpCamera();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        WallJumping wallJump = other.GetComponentInParent<WallJumping>();

        if (wallJump != null)
        {
            wallJump.ExitZone(this);
            ActivateNormalCamera();
        }
    }
    public Vector3 GetLeftDirection()
    {
        Transform reference = lateralReference != null ? lateralReference : transform;

        return -reference.right;
    }
    public Vector3 GetRightDirection()
    {
        Transform reference = lateralReference != null ? lateralReference : transform;

        return reference.right;
    }
    public void ActivateWallJumpCamera()
    {
        if (normalCamera != null)
        {
            normalCamera.Priority = 10;
        }

        if (wallJumpCamera != null)
        {
            wallJumpCamera.Priority = 20;
        }
    }
    public void ActivateNormalCamera()
    {
        if (normalCamera != null)
        {
            normalCamera.Priority = 20;
        }

        if (wallJumpCamera != null)
        {
            wallJumpCamera.Priority = 10;
        }
    }
}
