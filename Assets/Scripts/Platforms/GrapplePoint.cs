using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    public static readonly List<GrapplePoint> allPoints = new List<GrapplePoint>();

    [Header("Grapple Point")]
    public Transform anchorPoint;
    public float grappleRange;

    public Vector3 AnchorPosition
    {
        get
        {
            return anchorPoint != null ? anchorPoint.position : transform.position;
        }
    }

    private void OnEnable()
    {
        if (!allPoints.Contains(this))
        {
            allPoints.Add(this);
        }
    }
    private void OnDisable()
    {
        allPoints.Remove(this);
    }
    private void OnDrawGizmos()
    {
        Vector3 center = anchorPoint != null ? anchorPoint.position : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, grappleRange);
        Gizmos.DrawSphere(center, 0.15f);
    }
}
