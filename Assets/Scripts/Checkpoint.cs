using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Vector3 actualCheckpoint;
    public static Checkpoint instance;
    public GameObject player;
    public Transform spawn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerObj"))
        {
            instance = this;
            actualCheckpoint = Checkpoint.instance.spawn.position;
        }
    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        actualCheckpoint = player.transform.position;
    }
}
