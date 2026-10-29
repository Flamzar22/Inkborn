using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Vector3 actualCheckpoint;
    public static Checkpoint instance;
    public GameObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            instance = this;
            actualCheckpoint = Checkpoint.instance.transform.position;
        }
    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        actualCheckpoint = player.transform.position;
    }
}
