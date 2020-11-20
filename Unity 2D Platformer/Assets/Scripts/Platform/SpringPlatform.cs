using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPlatform : Platform
{
    Animator animator;

    public override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
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
                passenger.SendMessage("HandleSpringPlatform", new SpringVelocityVector(Vector3.zero, false));
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
