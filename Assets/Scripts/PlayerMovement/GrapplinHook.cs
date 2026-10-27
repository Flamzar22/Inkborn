using UnityEngine;

public class GrapplinHook : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public Transform orientation;
    public LineRenderer rope;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.F;
    public KeyCode releaseKey = KeyCode.Space;

    [Header("Rope")]
    public float ropeLength;
    public float reelSpeed;

    [Header("Swing")]
    public float swingAcceleration;
    public float angularDamping;
    public float maxSwingAngle;
    public float maxSwingSpeed;
    [Range(0f, 1f)]
    public float bounceStrength;

    [Header("Launch")]
    public float launchForwardBoost;
    public float launchUpBoost;

    public bool isGrappling {  get; private set; }

    private Rigidbody rb;
    private GrapplePoint currentPoint;

    private Vector3 anchorPosition;
    private Vector3 swingForward;

    private float swingAngle;
    private float swingVelocity;
    private float currentRopeLength;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if(playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        if(orientation == null && playerMovement != null)
        {
            orientation = playerMovement.orientation;
        }
        if(rope != null)
        {
            rope.positionCount = 2;
            rope.enabled = false;
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(grappleKey) && !isGrappling)
        {
            TryGrapple();
        }

        if (isGrappling && Input.GetKeyDown(releaseKey))
        {
            ReleaseGrapple();
        }
    }
    private void FixedUpdate()
    {
        if (!isGrappling) { return; }

        Swing();
        UpdateRope();
    }
    private void TryGrapple()
    {
        if(playerMovement == null || orientation == null) { return; }

        if(playerMovement.freeze || playerMovement.dashing || playerMovement.climbing || playerMovement.climbingScript != null && playerMovement.climbingScript.exitingWall) { return; }

        GrapplePoint bestPoint = null;
        float closestDistance = float.MaxValue;

        foreach(GrapplePoint point in GrapplePoint.allPoints)
        {
            if(point == null) continue;

            float distance = Vector3.Distance(transform.position, point.AnchorPosition);

            if (distance <= point.grappleRange && distance <  closestDistance)
            {
                closestDistance = distance;
                bestPoint = point;
            }
        }
        
        if(bestPoint == null) { return; }

        AttachToPoint(bestPoint);
    }
    private void AttachToPoint(GrapplePoint point)
    {
        currentPoint = point;
        anchorPosition = point.AnchorPosition;

        Vector3 toPlayer = transform.position - anchorPosition;

        currentRopeLength = toPlayer.magnitude;

        swingForward = Vector3.ProjectOnPlane(orientation.forward, Vector3.up.normalized);

        if(swingForward.sqrMagnitude < 0.001f)
        {
            swingForward = Vector3.forward;
        }

        Vector3 planeRight = Vector3.Cross(Vector3.up, swingForward.normalized);

        toPlayer = Vector3.ProjectOnPlane(toPlayer, planeRight);

        if (toPlayer.sqrMagnitude < 0.001f) { return; }

        swingAngle = Mathf.Atan2(Vector3.Dot(toPlayer, swingForward), Vector3.Dot(toPlayer, Vector3.up));

        Vector3 tangent = GetTangent();

        swingVelocity = Vector3.Dot(rb.linearVelocity, tangent) / Mathf.Max(currentRopeLength, 0.1f);

        isGrappling = true;

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if(rope != null)
        {
            rope.enabled = true;
        }
        if(playerMovement != null)
        {
            playerMovement.grappling = true;
        }
    }
    private void Swing()
    {
        if(currentPoint == null)
        {
            ReleaseGrapple();
            return;
        }

        anchorPosition = currentPoint.AnchorPosition;

        float input = 0f;

        Vector3 cameraForward = Vector3.ProjectOnPlane(orientation.forward, Vector3.up).normalized;

        Vector3 swingDirection = GetTangent().normalized;

        float directionDot = Vector3.Dot(cameraForward, swingDirection);

        if (Input.GetKey(KeyCode.W))
        {
            input += Mathf.Sign(directionDot);
        }

        if (Input.GetKey(KeyCode.S))
        {
            input -= Mathf.Sign(directionDot);
        }

        currentRopeLength = Mathf.MoveTowards(currentRopeLength, ropeLength, reelSpeed * Time.fixedDeltaTime);

        float gravityAcceleration = -(Physics.gravity.magnitude / currentRopeLength) * Mathf.Sin(swingAngle);

        swingVelocity += (gravityAcceleration + input * swingAcceleration) * Time.fixedDeltaTime;

        swingVelocity = Mathf.Clamp(swingVelocity, -maxSwingSpeed, maxSwingSpeed);

        swingVelocity *= Mathf.Clamp01(1f - angularDamping * Time.fixedDeltaTime);

        swingAngle += swingVelocity * Time.fixedDeltaTime;

        float maxAngleRad = maxSwingAngle * Mathf.Deg2Rad;

        if (swingAngle >= maxAngleRad)
        {
            swingAngle = maxAngleRad;

            if (swingVelocity > 0f)
            {
                swingVelocity = -swingVelocity * bounceStrength;
            }
        }
        else if (swingAngle <= -maxAngleRad)
        {
            swingAngle = -maxAngleRad;

            if (swingVelocity < 0f)
            {
                swingVelocity = -swingVelocity * bounceStrength;
            }
        }

        Vector3 radialDirection = swingForward * Mathf.Sin(swingAngle) - Vector3.up * Mathf.Cos(swingAngle);

        Vector3 targetPosition = anchorPosition + radialDirection * currentRopeLength;

        rb.MovePosition(targetPosition);

        rb.linearVelocity = Vector3.zero;
    }
    private Vector3 GetTangent()
    {
        return swingForward * Mathf.Cos(swingAngle) + Vector3.up * Mathf.Sin(swingAngle);
    }
    private void UpdateRope()
    {
        if(rope == null) return;

        rope.SetPosition(0, anchorPosition);
        rope.SetPosition(1, transform.position);
    }
    private void ReleaseGrapple()
    {
        if(!isGrappling) return;

        float direction = swingAngle >= 0f ? 1f : -1f;

        Vector3 launchVelocity = swingForward * direction * launchForwardBoost + Vector3.up * launchUpBoost;

        isGrappling = false;
        currentPoint = null;

        if(rope != null)
        {
            rope.enabled = false;
        }

        if(playerMovement != null)
        {
            playerMovement.grappling = false;
        }

        rb.useGravity = true;
        rb.linearVelocity = launchVelocity;
    }
}
