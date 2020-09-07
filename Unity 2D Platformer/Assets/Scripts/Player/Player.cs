using System.Collections;
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
	public float coytoteTimeJumpMaxTime = 0.1f;
	public float jumpBufferMaxTime = 0.1f;
	float maxJumpVelocity;
	float minJumpVelocity = 0;
	float gravity;
	float coyoteTimeJumpTimer;
	float jumpBufferTimer;

	// Spring
	bool springJump;
	bool onSpringPlatform;

	// Wallsliding
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallJumpLeap;
	public float wallSlideSpeedMax = 3;
	public float wallStickTime = 0.1f;
	float timeToWallUnstick;
	bool wallSliding;
	int wallDirX;

	// Rewind
	List<PointInTime> pointsInTime;
	public float maxRewindTime = 5;
	bool isRewinding = false;

	void Start()
	{
		controller = GetComponent<Controller>();

		// From Kinematic Equations
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs(gravity) * minJumpHeight);

		pointsInTime = new List<PointInTime>();
	}

	void Update ()
	{
		if (isRewinding)
		{
			Rewind();
		}
		else
		{
			CalculatePlayerVelocity();

			CheckGroundControlTimers();
			CheckWallSliding();
			CheckJumpBuffer();

			controller.Move(velocity * Time.deltaTime, input);

			// Store if player is on a spring platform here because if the platform is moving the controller.collisions is reset
			onSpringPlatform = controller.collisions.onSpringPlatform;

			if (controller.collisions.above || (controller.collisions.below && !controller.collisions.slidingDownSlope && !onSpringPlatform))
			{
				velocity.y = 0;
				springJump = false;
			}

			Record();
		}
	}

	void CalculatePlayerVelocity()
	{
		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}

	// Handle player sliding against a vertical wall
	// If directional input is in opposite direction of the wall while sliding, move after a slight delay to allow for player jump input
	void CheckWallSliding()
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

	void CheckJumpBuffer()
	{
		if (controller.collisions.below && !onSpringPlatform)
		{
			if (jumpBufferTimer > 0)
			{
				HandleGroundedJump(true);
				jumpBufferTimer = 0;
			}
		}
		else
		{
			jumpBufferTimer -= Time.deltaTime;
		}
	}

	void CheckGroundControlTimers()
	{
		if (controller.collisions.below)
		{
			coyoteTimeJumpTimer = coytoteTimeJumpMaxTime;
		}
		else
		{
			coyoteTimeJumpTimer -= Time.deltaTime;
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
			HandleWallSlideJump();
		}
		else if (controller.collisions.below && !onSpringPlatform)
		{
			HandleGroundedJump();
		}
		else
		{
			if (coyoteTimeJumpTimer > 0 && !springJump)
			{
				HandleCoyoteTimeJump();
			}
			else
			{
				HandleBufferJump();
			}
		}
	}

	public void OnJumpInputUp()
	{
		if (velocity.y > minJumpVelocity && !springJump)
		{
			velocity.y = minJumpVelocity;
		}
	}

	public void HandleSpringPlatform(float maxSpringVelocity, float minSpringVelocity)
	{
		if (jumpBufferTimer > 0)
		{
			velocity.y = maxSpringVelocity;
			jumpBufferTimer = 0;
		}
		else
		{
			velocity.y = minSpringVelocity;
		}
		
		springJump = true;
	}

	void HandleWallSlideJump()
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

		coyoteTimeJumpTimer = 0;
	}

	void HandleGroundedJump(bool bufferedJump = false)
	{
		if (controller.collisions.slidingDownSlope)
		{
			if (input.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
			{
				velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
				velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
			}
		}
		else if (bufferedJump)
		{
			if (Input.GetKey(KeyCode.Space))
				velocity.y = maxJumpVelocity;
			else
				velocity.y = minJumpVelocity;
		}
		else
		{
			velocity.y = maxJumpVelocity;
		}

		coyoteTimeJumpTimer = 0;
	}

	void HandleCoyoteTimeJump()
	{
		velocity.y = maxJumpVelocity;

		coyoteTimeJumpTimer = 0;
	}

	void HandleBufferJump()
	{
		jumpBufferTimer = jumpBufferMaxTime;
	}

	public void StartRewind()
	{
		isRewinding = true;
	}

	public void StopRewind()
	{
		isRewinding = false;
	}

	void Rewind()
	{
		if (pointsInTime.Count > 0)
		{
			transform.position = pointsInTime[0].position;
			velocity = pointsInTime[0].velocity;
			pointsInTime.RemoveAt(0);
		}
		else
		{
			StopRewind();
		}
	}

	void Record()
	{
		if (pointsInTime.Count > Mathf.Round(maxRewindTime / Time.fixedDeltaTime))
		{
			pointsInTime.RemoveAt(pointsInTime.Count - 1);
		}

		PointInTime pointInTime = new PointInTime(transform.position, velocity);

		pointsInTime.Insert(0, pointInTime);
	}
}
