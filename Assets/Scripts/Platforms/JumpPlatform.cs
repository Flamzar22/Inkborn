using System.Collections;
using UnityEngine;

public class JumpPlatform : MonoBehaviour
{
    private bool isMoving = false;
    public Rigidbody playerRb;
    public PlayerMovement pm;
    private Rigidbody rb;
    public float impulseForce;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isMoving && collision.contacts[0].normal.y < -0.5f)
        {
            isMoving = true;
            StartCoroutine(MovePlatform());
        }
    }
    private IEnumerator MovePlatform()
    {
        Vector3 initialPosition = transform.position;
        Vector3 desiredPosition = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        float velocidad = 2f;

        while (Vector3.Distance(rb.position, desiredPosition) > 0.001f)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb.position, desiredPosition, velocidad * Time.fixedDeltaTime);

            Vector3 platformMovement = newPosition - rb.position;

            rb.MovePosition(newPosition);

            if (pm.grounded)
            {
                playerRb.MovePosition(playerRb.position + platformMovement);
            }

            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(0.2f);

        while (Vector3.Distance(transform.position, initialPosition) > 0.001f)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb.position, initialPosition, velocidad * Time.fixedDeltaTime);

            rb.MovePosition(newPosition);

            yield return new WaitForFixedUpdate();
        }

        if (pm.grounded)
        {
            ApplyForce();
        }

        isMoving = false;
    }
    private void ApplyForce()
    {
        playerRb.AddForce(Vector3.up * impulseForce, ForceMode.Impulse);
    }
}
