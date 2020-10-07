using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller))]
public class Player : MonoBehaviour {

	Controller controller;

	PlayerAnimations playerAnimations;

	Vector2 input;
	Vector3 velocity;

	// Ground movement
	[Header("Ground Movement")]
	public float moveSpeed = 6;
	float velocityXSmoothing;
	float accelerationTimeAirborne = 0.2f;
	float accelerationTimeGrounded = 0.1f;
	bool facingRight = true;

	// Jumping
	[Header("Jumping")]
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

	// Wall sliding
	[Header("Wall Sliding")]
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallJumpLeap;
	public float wallSlideSpeedMax = 3;
	public float wallStickTime = 0.1f;
	float timeToWallUnstick;
	bool wallSliding;
	int wallDirX;

	// Dash
	[Header("Dash")]
	public float maxDashDelayTime = 0.1f;
	public float maxTimeBetweenDash = 0.1f;
	public float dashDistance = 4;
	public float maxDashTime = 0.1f;
	float dashDelayTime;
	float timeBetweenDash;
	float dashTime;
	float dashSpeed;
	bool isDashing;
	bool isDelayedDashing;
	bool canDash;

	// Rewind
	[Header("Rewind")]
	public GameObject playerGhostPreFab;
	GameObject playerRewindGhost;
	public float maxRecordTime = 5;
	public float minRecordTime = 1;
	public float maxRecordIntervalTime = 0.25f;
	public float maxRewindTime = 0.5f;
	List<PointInTime> pointsInTime;
	float rewindTime;
	bool isRewinding = false;

	// Level Transition
	[Header("Level Transition")]
	public float levelTransitionTime = 0.5f;
	[SerializeField] bool pausePlayerControl;

	void Start()
	{
		controller = GetComponent<Controller>();

		playerAnimations = gameObject.GetComponentInChildren<PlayerAnimations>();
		if (playerAnimations)
		{
			playerAnimations.InitiateTrailRenderer(maxRecordTime);
		}
		else
		{
			Debug.LogError("Child object is missing PlayerAnimations script!");
		}

		pointsInTime = new List<PointInTime>();

		// From Kinematic Equations
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs(gravity) * minJumpHeight);

		// Calculate dash speed
		dashSpeed = dashDistance / maxDashTime;
	}

	void Update ()
	{
		if (pausePlayerControl)
			return;

		if (isRewinding)
		{
			Rewind();
		}
		else
		{
			playerAnimations.RotateInDirectionOfMovement(input);
			CalculatePlayerVelocity();

			CheckDashSettings();
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

		playerAnimations.SetAnimationParameters();
		playerAnimations.Reset();
	}

	public void OnLevelReset(bool playerDied, Vector3[] playerSpawnLocations)
	{
		StartCoroutine(PlayerContolPause(levelTransitionTime));

		velocity = Vector2.zero;

		int minIndex = 0;
		float minDistance = float.PositiveInfinity;

		for (int i = 0; i < playerSpawnLocations.Length; i++)
		{
			float distance = Vector3.Distance(transform.position, playerSpawnLocations[i]);
			if (distance < minDistance)
			{
				minIndex = i;
				minDistance = distance;
			}
		}

		transform.position = playerSpawnLocations[minIndex];

		ResetRewind();
	}

	void CalculatePlayerVelocity()
	{
		bool setDir = true;

		if (isDashing)
		{
			HandlePlayerDash();

			setDir = isDelayedDashing;
		}
		else
		{
			float targetVelocityX = input.x * moveSpeed;
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
			velocity.y += gravity * Time.deltaTime;
		}

		if (input.x != 0 && setDir)
			facingRight = input.x > 0;
	}

	// There is a small delay between button press and player dash where the user can buffer a direction
	// The player dashes in the set x direction for a set period
	// On the first frame after the dash is complete, set player speed and timer between player dashes
	void HandlePlayerDash()
	{
		if (dashDelayTime > 0)
		{
			velocity = Vector2.zero;
			dashDelayTime -= Time.deltaTime;
		}
		else
		{
			isDelayedDashing = false;

			if (dashTime > 0)
			{
				float direction = facingRight ? 1 : -1;

				velocity.x = direction * dashSpeed;
				velocity.y = 0;

				dashTime -= Time.deltaTime;
			}
			else
			{
				// Player has finished dashing, reset velocity and set delay timer
				float direction = facingRight ? 1 : -1;

				velocity.x = direction * moveSpeed;
				velocity.y = 0;

				timeBetweenDash = maxTimeBetweenDash;

				isDashing = false;
			}
		}
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

	void CheckDashSettings()
	{
		if (controller.collisions.below)
		{
			canDash = true;
		}

		if (!isDashing)
		{
			timeBetweenDash -= Time.deltaTime;
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

	// Player cannot dash if already dashing
	// Player regains ability to dash when grounded
	// There is a small delay between the player being able to dash
	public void OnDashInputDown()
	{
		if (!isDashing && canDash && timeBetweenDash < 0)
		{
			dashDelayTime = maxDashDelayTime;
			dashTime = maxDashTime;
			isDashing = true;
			isDelayedDashing = true;
			canDash = false;
		}
	}

	public void HandleSpringPlatform(float maxSpringVelocity, float minSpringVelocity)
	{
		if (jumpBufferTimer > 0)
		{
			velocity.y = maxSpringVelocity;

			playerAnimations.jump = true;

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

		playerAnimations.jump = true;

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

				playerAnimations.jump = true;
			}
		}
		else if (bufferedJump)
		{
  			if (Input.GetKey(KeyCode.Space))
				velocity.y = maxJumpVelocity;
			else
				velocity.y = minJumpVelocity;

			playerAnimations.jump = true;
		}
		else
		{
			velocity.y = maxJumpVelocity;

			playerAnimations.jump = true;
		}

		coyoteTimeJumpTimer = 0;
	}

	void HandleCoyoteTimeJump()
	{
		velocity.y = maxJumpVelocity;

		playerAnimations.jump = true;

		coyoteTimeJumpTimer = 0;
	}

	void HandleBufferJump()
	{
		jumpBufferTimer = jumpBufferMaxTime;
	}

	public void StartRewind()
	{
		if (!isRewinding)
		{
			isRewinding = true;
			rewindTime = maxRewindTime;

			playerAnimations.SetSpriteEnabled(false);
			playerAnimations.SetTrailRendererEmitting(false);
		}
	}

	void Record()
	{
		bool removedPointFromList = false;

		if (pointsInTime.Count > Mathf.Round(maxRecordTime / Time.deltaTime))
		{
			pointsInTime.RemoveAt(pointsInTime.Count - 1);

			removedPointFromList = true;
		}

		PointInTime pointInTime = new PointInTime(transform.position, playerAnimations.transform.localRotation, playerAnimations.transform.localScale);

		pointsInTime.Insert(0, pointInTime);

		if (!playerRewindGhost)
			playerRewindGhost = InstantiateRewindGhost(pointsInTime[pointsInTime.Count - 1]);
		else if (removedPointFromList)
			UpdateRewindGhost(pointsInTime[pointsInTime.Count - 1]);
	}

	void Rewind()
	{
		if (rewindTime > 0)
		{
			rewindTime -= Time.deltaTime;
		}
		else
		{
			StopRewind();
		}
	}

	void StopRewind()
	{
		transform.position = pointsInTime[pointsInTime.Count - 1].position;
		velocity = Vector3.zero;

		ResetRewind();
	}

	void ResetRewind()
	{
		isRewinding = false;

		pointsInTime.Clear();

		Destroy(playerRewindGhost);

		playerAnimations.SetSpriteEnabled(true);
		playerAnimations.SetTrailRendererEmitting(true);
		playerAnimations.ResetTrailRenderer();
	}

	GameObject InstantiateRewindGhost(PointInTime point)
	{
		GameObject ghost = Instantiate(playerGhostPreFab, point.position + new Vector3(0, 0.5f, 0), point.rotation);    // Need to shift the ghost by 0.5 in y dir to match the sprite shift as result from collider 2D shift
		ghost.transform.localScale = point.scale;

		return ghost;
	}

	void UpdateRewindGhost(PointInTime point)
	{
		playerRewindGhost.transform.position = point.position + new Vector3(0, 0.5f, 0); // Need to shift the ghost by 0.5 in y dir to match the sprite shift as result from collider 2D shift
		playerRewindGhost.transform.rotation = point.rotation;
		playerRewindGhost.transform.localScale = point.scale;
	}

	IEnumerator PlayerContolPause(float time)
	{
		pausePlayerControl = true;

		yield return new WaitForSeconds(time);

		pausePlayerControl = false;
	}
}
