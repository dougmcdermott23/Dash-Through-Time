﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller))]
public class Player : MonoBehaviour {

	Controller controller;

	Vector2 input;
	Vector3 velocity;

	// Ground movement
	public float moveSpeed = 6;
	float velocityXSmoothing;
	float accelerationTimeAirborne = 0.2f;
	float accelerationTimeGrounded = 0.1f;

	// Jumping
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = 0.4f;
	float maxJumpVelocity;
	float minJumpVelocity = 0;
	float gravity;

	// Wallsliding
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallJumpLeap;
	public float wallSlideSpeedMax = 3;
	public float wallStickTime = 0.1f;
	float timeToWallUnstick;
	bool wallSliding;
	int wallDirX;

	void Start()
	{
		controller = GetComponent<Controller>();

		// From Kinematic Equations
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpHeight = Mathf.Sqrt (2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	void Update ()
	{
		CalculatePlayerVelocity();
		CheckWallsliding();
		
		controller.Move (velocity * Time.deltaTime, input);

		if (controller.collisions.above || controller.collisions.below && !controller.collisions.slidingDownSlope)
			velocity.y = 0;
	}

	void CalculatePlayerVelocity()
	{
		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}

	// Handle player sliding against a vertical wall
	// If directional input is in opposite direction of the wall while sliding, move after a slight delay to allow for player jump input
	void CheckWallsliding()
	{
		wallDirX = (controller.collisions.left) ? -1 : 1;

		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
		{
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax)
				velocity.y = -wallSlideSpeedMax;

			if (timeToWallUnstick > 0)
			{
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (input.x != wallDirX && input.x != 0)
					timeToWallUnstick -= Time.deltaTime;
				else
					timeToWallUnstick = wallStickTime;
			}
			else
			{
				timeToWallUnstick = wallStickTime;
			}
		}
	}

	public void SetDirectionalInput(Vector2 directionInput)
	{
		input = directionInput;
	}

	// Handle player jumping
	public void OnJumpInputDown()
	{
		if (wallSliding)
		{
			if (wallDirX == input.x)
			{
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
			}
			else if (input.x == 0)
			{
				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;
			}
			else
			{
				velocity.x = -wallDirX * wallJumpLeap.x;
				velocity.y = wallJumpLeap.y;
			}
		}
		if (controller.collisions.below)
		{
			if (controller.collisions.slidingDownSlope)
			{
				if (input.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
				{
					velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
					velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			}
			else
			{
				velocity.y = maxJumpVelocity;
			}
		}
	}

	public void OnJumpInputUp()
	{
		if (velocity.y > minJumpVelocity)
			velocity.y = minJumpVelocity;
	}
}