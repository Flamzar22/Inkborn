using UnityEngine;

public class WallJumping : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public PlayerMovement pm;
    private Collider playerCollider;

    [Header("Wall Detection")]
    public float wallContactDistance;

    [Header("Jump")]
    public float jumpUpForce;
    public float jumpSideForce;

    [Header("Lateral Movement")]
    public float lateralAcceleration;
    public float maxLateralSpeed;

    private WallJumpZone activeZone;

    private bool touchingLeftWall;
    private bool touchingRightWall;

    public bool IsActive => activeZone != null;

    private void Awake()
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (pm == null)
        {
            pm = GetComponent<PlayerMovement>();
        }
        playerCollider = GetComponentInChildren<Collider>();
    }
    void Update()
    {
        if (!IsActive){ return; }

        CheckWalls();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryWallJump();
        }
    }
    private void FixedUpdate()
    {
        if (!IsActive){ return; }

        MoveSideways();
    }
    public void EnterZone(WallJumpZone zone)
    {
        activeZone = zone;

        touchingLeftWall = false;
        touchingRightWall = false;
    }
    public void ExitZone(WallJumpZone zone)
    {
        if (activeZone != zone)
        {
            return;
        }

        activeZone = null;

        touchingLeftWall = false;
        touchingRightWall = false;
    }
    private void CheckWalls()
    {
        touchingLeftWall = IsNearWall(activeZone.leftWall);
        touchingRightWall = IsNearWall(activeZone.rightWall);
    }
    private bool IsNearWall(Collider wall)
    {
        if (wall == null || playerCollider == null){ return false; }

        Vector3 closestPointOnWall = wall.ClosestPoint(playerCollider.bounds.center);

        Vector3 closestPointOnPlayer = playerCollider.ClosestPoint(closestPointOnWall);

        float distance = Vector3.Distance(closestPointOnPlayer, closestPointOnWall);

        return distance <= wallContactDistance;
    }
    private void TryWallJump()
    {
        Vector3 jumpDirection;

        if (touchingLeftWall)
        {
            jumpDirection = activeZone.GetRightDirection();
        }
        else if (touchingRightWall)
        {
            jumpDirection = activeZone.GetLeftDirection();
        }
        else
        {
            return;
        }

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        Vector3 jumpForce = Vector3.up * jumpUpForce + jumpDirection * jumpSideForce;

        rb.AddForce(jumpForce, ForceMode.Impulse);
    }
    private void MoveSideways()
    {
        if (pm != null && pm.grounded){ return; }
        float input = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            input -= 1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            input += 1f;
        }

        Vector3 direction = activeZone.GetRightDirection();

        rb.AddForce(direction * input * lateralAcceleration, ForceMode.Acceleration);

        Vector3 lateralVelocity = Vector3.Project(rb.linearVelocity, direction);

        if (lateralVelocity.magnitude > maxLateralSpeed)
        {
            Vector3 limitedVelocity = lateralVelocity.normalized * maxLateralSpeed;

            rb.linearVelocity += limitedVelocity - lateralVelocity;
        }
    }

}
