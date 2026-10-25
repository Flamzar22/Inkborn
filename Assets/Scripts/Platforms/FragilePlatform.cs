using System.Collections;
using UnityEngine;

public class FragilePlatform : MonoBehaviour
{
    public float respawnTime;
    private MeshRenderer mr;
    private MeshCollider mc;
    private void Start()
    {
        mr = GetComponent<MeshRenderer>();
        mc = GetComponent<MeshCollider>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(StartBreaking());
        }
    }
    private IEnumerator StartBreaking()
    {
        //Start animation
        yield return new WaitForSeconds(2f);
        mc.enabled = false;
        mr.enabled = false;
        yield return new WaitForSeconds(respawnTime);
        mc.enabled = true;
        mr.enabled = true;
    }
}
