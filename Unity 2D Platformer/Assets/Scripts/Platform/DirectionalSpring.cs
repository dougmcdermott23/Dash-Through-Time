using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalSpring : Platform
{
    public enum SpringDirection
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    Animator animator;

    public Vector3 springVelocity;
    public SpringDirection direction; 

    public override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateRayCastOrigins();

        if (DirectionalSpringDetectPassenger())
        {
            animator.SetTrigger("activateSpring");

            var listOfPassengers = FindPassengers();
            foreach (GameObject passenger in listOfPassengers)
            {
                passenger.SendMessage("HandleSpringPlatform", new SpringVelocityVector(springVelocity, true));
            }
        }
    }

    bool DirectionalSpringDetectPassenger()
    {
        float rayLength = skinWidth * 2;

        int rayCount;
        float raySpacing;
        Vector2 rayOrigin;
        Vector2 searchDirection;
        Vector2 rayDirection;

        if (direction == SpringDirection.UP || direction == SpringDirection.DOWN)
        {
            rayCount = verticalRayCount;
            raySpacing = verticalRaySpacing;
            rayOrigin = direction == SpringDirection.UP ? raycastOrigins.topLeft : raycastOrigins.botLeft;
            searchDirection = Vector2.right;
            rayDirection = direction == SpringDirection.UP ? Vector2.up : Vector2.down;
        }
        else
        {
            rayCount = horizontalRayCount;
            raySpacing = horizontalRaySpacing;
            rayOrigin = direction == SpringDirection.LEFT ? raycastOrigins.botLeft : raycastOrigins.botRight;
            searchDirection = Vector2.up;
            rayDirection = direction == SpringDirection.LEFT ? Vector2.left : Vector2.right;
        }

        for (int i = 0; i < rayCount; i++)
        {
            rayOrigin += searchDirection * (raySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, passengerMask);

            if (hit)
            {
                return true;
            }
        }

        return false;
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
