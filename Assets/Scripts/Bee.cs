using UnityEngine;

public class Bee : MonoBehaviour
{
    public float moveSpeed;
    public float moveDistance;
    public float rotationSpeed;
    private Vector3 startPosition;
    private Vector3 initialDirection;
    private float traveledDistance = 0f;
    private bool movingForward = true;
    private bool rotating = false;
    private Quaternion targetRotation;
    void Start()
    {
        startPosition = transform.position;
        initialDirection = transform.forward;
    }
    void Update()
    {
        if (rotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                rotating = false;
            }

            return;
        }

        float targetDistance = movingForward ? moveDistance : 0f;

        traveledDistance = Mathf.MoveTowards(traveledDistance, targetDistance, moveSpeed * Time.deltaTime);

        transform.position = startPosition + initialDirection * traveledDistance;

        if (movingForward && traveledDistance >= moveDistance)
        {
            movingForward = false;
            StartRotation();
        }
        else if (!movingForward && traveledDistance <= 0f)
        {
            movingForward = true;
            StartRotation();
        }
    }
    void StartRotation()
    {
        targetRotation = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
        rotating = true;
    }
}
