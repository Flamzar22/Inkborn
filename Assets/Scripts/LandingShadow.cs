using UnityEngine;

public class LandingShadow : MonoBehaviour
{
    public Transform player;
    public LayerMask whatIsGround;

    public float raycastDistance = 100f;
    public float heightOffset = 0.05f;
    RaycastHit hit;
    void Start()
    {
        
    }
    private void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(player.position, Vector3.down, out hit, raycastDistance, whatIsGround))
        {
            transform.position = hit.point + Vector3.up * heightOffset;

            transform.gameObject.SetActive(true);
        }
        else
        {
            transform.gameObject.SetActive(false);
        }
    }
}
