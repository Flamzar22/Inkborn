using UnityEngine;

public class HoneyZone : MonoBehaviour
{
    public PlayerMovement playerMovement;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerObj"))
        {
            playerMovement = other.GetComponentInParent<PlayerMovement>();
            playerMovement.inHoney = true;
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        playerMovement.inHoney = false;
    }
}
