using UnityEngine;

public class WallJumpCameraTarget : MonoBehaviour
{
    public Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }
    void LateUpdate()
    {
        if (player == null){ return; }

        transform.position = new Vector3(transform.position.x, player.position.y, transform.position.z);
    }
}
