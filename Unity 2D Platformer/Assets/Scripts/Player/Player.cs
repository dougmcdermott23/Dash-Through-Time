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

	// Rewind
	[Header("Rewind")]
	public GameObject playerGhostPreFab;
	public float maxRecordTime = 5;
	public float minRecordTime = 1;
	public float maxRecordIntervalTime = 0.25f;
	public float maxRewindTime = 0.5f;
	float maxGhostInstantiateInterval;
	List<PointInTime> pointsInTime;
	List<GameObject> playerRewindGhosts;
	float recordTime;
	float recordIntervalTime;
	float rewindTime;
	float ghostInstantiateInterval;
	bool isRewindInit = false;
	bool isRewinding = false;

	void Start()
	{
		controller = GetComponent<Controller>();

		playerAnimations = gameObject.GetComponentInChildren<PlayerAnimations>();
		if (playerAnimations)
		{
			playerAnimations.InitiateTrailRenderer(maxRecordTime + maxRewindTime);
		}
		else
		{
			Debug.LogError("Child object is missing PlayerAnimations script!");
		}

		pointsInTime = new List<PointInTime>();
		playerRewindGhosts = new List<GameObject>();

		// From Kinematic Equations
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	void Update ()
	{
		if (isRewinding)
		{
			Rewind();
		}
		else
		{
			playerAnimations.RotateInDirectionOfMovement(input);
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

			CheckRecordTimer();
		}

		playerAnimations.SetAnimationParameters();
		playerAnimations.Reset();
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

	public void InitiateRecordAndStartRewind()
	{
		if (!isRewinding)
		{
			if (!isRewindInit)
			{
				InitiateRecord();
			}
			else if (maxRecordTime - recordTime > minRecordTime)
			{
				StartRewind();
			}
		}
	}

	void InitiateRecord()
	{
		isRewindInit = true;
		recordTime = maxRecordTime;
		recordIntervalTime = maxRecordIntervalTime;

		playerAnimations.SetTrailRendererEmitting(true);

		pointsInTime.Clear();
		PointInTime pointInTime = new PointInTime(transform.position, playerAnimations.transform.localRotation, playerAnimations.transform.localScale);
		pointsInTime.Insert(0, pointInTime);

		InstantiateRewindGhost(pointInTime);
	}

	void StartRewind()
	{
		isRewindInit = false;
		isRewinding = true;
		rewindTime = maxRewindTime;

		maxGhostInstantiateInterval = maxRewindTime / pointsInTime.Count;
		playerAnimations.SetSpriteEnabled(false);
	}

	void CheckRecordTimer()
	{
		if (isRewindInit)
		{
			if (recordTime > 0)
			{
				CheckRecordIntervalTimer();

				recordTime -= Time.deltaTime;
			}
			else
			{
				StartRewind();
			}
		}
	}

	void CheckRecordIntervalTimer()
	{

		if (recordIntervalTime > 0)
		{
			recordIntervalTime -= Time.deltaTime;
		}
		else
		{
			recordIntervalTime = maxRecordIntervalTime;

			PointInTime pointInTime = new PointInTime(transform.position, playerAnimations.transform.localRotation, playerAnimations.transform.localScale);

			pointsInTime.Insert(0, pointInTime);
		}
	}

	void Rewind()
	{
		if (rewindTime > 0)
		{
			if (ghostInstantiateInterval > 0)
			{
				ghostInstantiateInterval -= Time.deltaTime;
			}
			else
			{
				if (pointsInTime.Count > 1)
				{
					InstantiateRewindGhost(pointsInTime[0]);
					pointsInTime.RemoveAt(0);
					ghostInstantiateInterval = maxGhostInstantiateInterval;
				}
			}

			rewindTime -= Time.deltaTime;
		}
		else
		{
			StopRewind();
		}
	}

	void StopRewind()
	{
		isRewindInit = false;
		isRewinding = false;

		playerAnimations.SetSpriteEnabled(true);
		playerAnimations.SetTrailRendererEmitting(false);

		transform.position = pointsInTime[pointsInTime.Count - 1].position;
		velocity = Vector3.zero;

		pointsInTime.Clear();
		DestroyRewindGhosts();
	}

	void InstantiateRewindGhost(PointInTime point)
	{
		GameObject ghost = Instantiate(playerGhostPreFab, point.position + new Vector3(0, 0.5f, 0), point.rotation);    // Need to shift the ghost by 0.5 in y dir to match the sprite shift as result from collider 2D shift
		ghost.transform.localScale = point.scale;
		playerRewindGhosts.Add(ghost);
	}

	void DestroyRewindGhosts()
	{
		foreach (GameObject ghost in playerRewindGhosts)
		{
			Destroy(ghost);
		}

		playerRewindGhosts.Clear();
	}
}
