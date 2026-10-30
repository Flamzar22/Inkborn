using Unity.Cinemachine;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public GameObject player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 previousPosition = player.transform.position;
            player.transform.position = Checkpoint.actualCheckpoint;
            Vector3 positionDelta = player.transform.position - previousPosition;
            CinemachineCore.OnTargetObjectWarped(player.transform, positionDelta);
        }
    }
}
