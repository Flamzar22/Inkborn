using System.Collections;
using UnityEngine;

public class FragilePlatform : MonoBehaviour
{
    public float respawnTime;
    public float despawnTime;
    private MeshCollider mc;
    public Animator animator;
    private void Start()
    {
        mc = GetComponent<MeshCollider>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.contacts[0].normal.y < -0.5f)
        {
            StartCoroutine(StartBreaking());
        }
    }
    private IEnumerator StartBreaking()
    {
        animator.SetTrigger("Break");
        yield return new WaitForSeconds(despawnTime);
        mc.enabled = false;
        yield return new WaitForSeconds(respawnTime);
        mc.enabled = true;
    }
}
