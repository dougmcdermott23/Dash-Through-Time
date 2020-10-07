using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Platform))]
public class SpringPlatform : MonoBehaviour
{
    public float maxJumpHeight = 8;
    public float minJumpHeight = 3;
    public float timeToJumpApex = 0.4f;

    [HideInInspector]
    public float maxJumpVelocity;
    [HideInInspector]
    public float minJumpVelocity = 0;
    float gravity;

    void Start()
    {
        // From Kinematic Equations
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }
}
