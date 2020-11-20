using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPlatform : Platform
{
    Animator animator;

    public float maxJumpHeight = 8;
    public float minJumpHeight = 3;
    public float timeToJumpApex = 0.4f;

    [HideInInspector]
    public float maxJumpVelocity;
    [HideInInspector]
    public float minJumpVelocity = 0;
    float gravity;

    public override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();

        // From Kinematic Equations
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    void Update()
    {
        UpdateRayCastOrigins();

        if (DetectPassenger())
        {
            animator.SetTrigger("activateSpring");

            var listOfPassengers = FindPassengers();
            foreach (GameObject passenger in listOfPassengers)
            {
                passenger.SendMessage("HandleSpringPlatform", new List<float>() { maxJumpVelocity, minJumpVelocity });
            }
        }
    }

    List<GameObject> FindPassengers()
    {
        List<GameObject> passengers = new List<GameObject>();
        float rayLength = skinWidth * 2;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = raycastOrigins.topLeft;

            rayOrigin += Vector2.right * (verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

            if (hit && !passengers.Contains(hit.transform.gameObject))
            {
                passengers.Add(hit.transform.gameObject);
            }
        }

        return passengers;
    }
}
