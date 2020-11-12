using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class PlayerTrail : MonoBehaviour
{
    TrailRenderer trail;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

    public void ResetTrailRenderer()
    {
        trail.Clear();
    }

    public void SetTrailRendererEmitting(bool emitting)
    {
        trail.emitting = emitting;
    }

    public void SetTrailPositions(Vector3[] trailPositions)
    {
        ResetTrailRenderer();
        trail.AddPositions(trailPositions);
    }
}
