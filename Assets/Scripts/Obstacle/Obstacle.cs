using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public GameObject player;
    private Animator animator;
    private PlayerMovement pm;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Die());
        }   
    }
    private IEnumerator Die()
    {
        animator = player.GetComponentInChildren<Animator>();
        pm = player.GetComponent<PlayerMovement>();
        animator.SetTrigger("Die");
        player.GetComponentInChildren<CapsuleCollider>().enabled = false;
        pm.freeze = true;
        yield return new WaitForSeconds(2f);
        pm.freeze = false;
        player.GetComponentInChildren<CapsuleCollider>().enabled = true;
        Vector3 previousPosition = player.transform.position;
        player.transform.position = Checkpoint.actualCheckpoint;
        Vector3 positionDelta = player.transform.position - previousPosition;
        CinemachineCore.OnTargetObjectWarped(player.transform, positionDelta);
    }
}
