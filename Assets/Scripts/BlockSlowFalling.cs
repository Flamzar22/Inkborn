using UnityEngine;

public class BlockSlowFalling : MonoBehaviour
{
    private PlayerMovement pm;
    public GameObject player;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerObj"))
        {
            pm = player.GetComponent<PlayerMovement>();
            pm.slowfallingUnlocked = false;
        }
    }
}
