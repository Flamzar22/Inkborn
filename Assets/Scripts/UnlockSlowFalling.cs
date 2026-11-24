using UnityEngine;

public class UnlockSlowFalling : MonoBehaviour
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
            pm.slowfallingUnlocked = true;
        }
    }
}
