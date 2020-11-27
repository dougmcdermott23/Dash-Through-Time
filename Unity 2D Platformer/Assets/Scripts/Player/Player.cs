using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Controller))]
public class Player : MonoBehaviour {

	public Controller PlayerController { get { return _playerController; } private set { _playerController = value; } }
	private Controller _playerController;

	public Vector2 PlayerInput { get { return _input; } private set { _input = value; } }
	private Vector2 _input;

	public Vector3 PlayerVelocity { get { return _velocity; } private set { _velocity = value; } }
	private Vector3 _velocity;

	public bool Jump { get { return _jump; } private set { _jump = value; } }
	private bool _jump;

	public bool WallSlide { get { return _wallSlide; } private set { _wallSlide = value; } }
	private bool _wallSlide;

	public bool Dash { get { return _dash; } private set { _dash = value; } }
	private bool _dash;

	public bool Rewind { get { return _rewind; } private set { _rewind = value; } }
	private bool _rewind;

	public bool Dead { get { return _dead; } private set { _dead = value; } }
	private bool _dead;

	PlayerAnimations playerAnimations;
	PlayerTrail playerTrail;

	CinemachineImpulseSource cinemachineImpulseSource;

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
	Timer coyoteTimeJumpTimer;
	Timer jumpBufferTimer;
	float maxJumpVelocity;
	float minJumpVelocity = 0;
	float gravity;

	// Spring
	[Header("Spring")]
	public float maxSpringJumpHeight = 8;
	public float minSpringJumpHeight = 3;
	public float timeToSpringJumpApex = 0.4f;
	float maxSpringJumpVelocity;
	float minSpringJumpVelocity;
	float springGravity;
	bool springJump;

	// Wall sliding
	[Header("Wall Sliding")]
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallJumpLeap;
	public float wallSlideSpeedMax = 3;
	public float wallStickTime = 0.1f;
	Timer wallStickTimer;
	int wallDirX;

	// Dash
	[Header("Dash")]
	public float maxDashDelayTime = 0.1f;
	public float maxTimeBetweenDash = 0.1f;
	public float dashDistance = 4;
	public float maxDashTime = 0.1f;
	Timer dashDelayTimer;
	Timer betweenDashTimer;
	Timer dashTimer;
	float dashSpeed;
	PlayerDashStates dashState = PlayerDashStates.DASH_AVAILABLE;

	// Rewind
	[Header("Rewind")]
	public GameObject playerGhostPreFab;
	public GameObject rewindStartPreFab;
	public GameObject rewindEndPreFab;
	GameObject playerRewindGhost;
	AnimationFramePickerSystem rewindGhostFramePicker;
	public float maxRecordTime = 5;
	public float maxRewindTime = 0.5f;
	Timer rewindTimer;
	List<PointInTime> pointsInTime;
	List<Vector3> trailPositions;
	PlayerRewindStates rewindState = PlayerRewindStates.REWIND_NOT_AVAILABLE;

	// Level Transition
	[Header("Level Transition")]
	public GameObject playerDeathPrefab;
	public GameObject ghostDeathPrefab;
	public float levelTransitionTime = 0.5f;
	public float deathTransitionTime = 2f;
	bool pausePlayerControl;

	void Start()
	{
		_playerController = GetComponent<Controller>();

		cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();

		coyoteTimeJumpTimer = gameObject.AddComponent(typeof(Timer)) as Timer;
		jumpBufferTimer = gameObject.AddComponent(typeof(Timer)) as Timer;
		wallStickTimer = gameObject.AddComponent(typeof(Timer)) as Timer;
		dashDelayTimer = gameObject.AddComponent(typeof(Timer)) as Timer;
		betweenDashTimer = gameObject.AddComponent(typeof(Timer)) as Timer;
		dashTimer = gameObject.AddComponent(typeof(Timer)) as Timer;
		rewindTimer = gameObject.AddComponent(typeof(Timer)) as Timer;

		playerAnimations = gameObject.GetComponentInChildren<PlayerAnimations>();
		playerTrail = gameObject.GetComponentInChildren<PlayerTrail>();

		if (playerTrail)
		{
			playerTrail.SetTrailRendererEmitting(true);
		}
		else
		{
			Debug.LogError("Child object is missing PlayerAnimations script!");
		}

		pointsInTime = new List<PointInTime>();
		trailPositions = new List<Vector3>();

		// Grounded Jump from kinematic equations
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs(gravity) * minJumpHeight);

		// Spring Jump from kinematic equations
		springGravity = -(2 * maxSpringJumpHeight) / Mathf.Pow(timeToSpringJumpApex, 2);
		maxSpringJumpVelocity = Mathf.Abs(springGravity) * timeToSpringJumpApex;
		minSpringJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(springGravity) * minSpringJumpHeight);

		// Calculate dash speed
		dashSpeed = dashDistance / maxDashTime;
	}

	void Update ()
	{
		if (pausePlayerControl)
			return;

		if (rewindState != PlayerRewindStates.REWINDING)
		{
			CalculatePlayerVelocity();

			CheckDashSettings();
			CheckGroundControlTimers();
			CheckWallSliding();
			CheckJumpBuffer();

			_playerController.Move(_velocity * Time.deltaTime, _input);

			if (_playerController.collisions.above || (_playerController.collisions.below && !_playerController.collisions.slidingDownSlope))
			{
				_velocity.y = 0;
				springJump = false;
			}

			Record();
		}

		// Controll player animations
		playerAnimations.SetAnimationParameters();
		_jump = false;
	}

	void ResetStateVariables(bool resetAll = false)
	{
		_jump = false;
		_wallSlide = false;

		if (resetAll)
		{
			_dash = false;
			_rewind = false;
			_dead = false;
		}
	}

	// If player dies, play animation and reset player
	// If player transitions to a new room, determine spawn position and reset player
	public void OnReset(bool isPlayerDead, Vector3[] playerSpawnLocations)
	{
		ResetStateVariables(true);

		if (isPlayerDead)
		{
			// Set for state controller
			_dead = true;

			playerAnimations.SetSpriteEnabled(false);
			Instantiate(playerDeathPrefab, transform.position + new Vector3(0, 0.5f, 0), transform.rotation);

			Instantiate(ghostDeathPrefab, playerRewindGhost.transform.position + new Vector3(0, 0.5f, 0), transform.rotation);
			Destroy(playerRewindGhost);

			playerTrail.FadeOut(deathTransitionTime);

			StartCoroutine(ResetPlayer(deathTransitionTime, isPlayerDead, playerSpawnLocations[0], Vector3.zero));
		}
		else
		{
			StartCoroutine(ResetPlayer(levelTransitionTime, isPlayerDead, Vector3.zero, Vector3.zero));
		}
	}

	void CalculatePlayerVelocity()
	{
		bool setDir = true;

		if (dashState == PlayerDashStates.DASHING)
		{
			HandlePlayerDash();

			setDir = !dashDelayTimer.IsTimerComplete();
		}
		else
		{
			float targetVelocityX = _input.x * moveSpeed;
			_velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref velocityXSmoothing, (_playerController.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
			_velocity.y += gravity * Time.deltaTime;
		}

		if (_input.x != 0 && setDir)
			facingRight = _input.x > 0;
	}

	public void SetDirectionalInput(Vector2 directionInput)
	{
		_input = directionInput;
	}

	// Handle player sliding against a vertical wall
	// If directional input is in opposite direction of the wall while sliding, move after a slight delay to allow for player jump input
	void CheckWallSliding()
	{
		wallDirX = (_playerController.collisions.left) ? -1 : 1;

		if ((_playerController.collisions.left || _playerController.collisions.right) && !_playerController.collisions.below && _velocity.y < 0)
		{
			_wallSlide = true;

			if (_velocity.y < -wallSlideSpeedMax && _input.x != 0)
				_velocity.y = -wallSlideSpeedMax;

			if (!wallStickTimer.IsTimerComplete())
			{
				velocityXSmoothing = 0;
				_velocity.x = 0;

				if (_input.x == wallDirX || _input.x == 0)
					wallStickTimer.SetTimer(wallStickTime);
			}
			else
			{
				wallStickTimer.SetTimer(wallStickTime);
			}
		}
		else
		{
			_wallSlide = false;
		}
	}

	#region Dash

	// Player cannot dash if already dashing
	// Player regains ability to dash when grounded
	// There is a small delay between the player being able to dash
	public void OnDashInputDown()
	{
		if (dashState == PlayerDashStates.DASH_AVAILABLE && betweenDashTimer.IsTimerComplete())
		{
			dashDelayTimer.SetTimer(maxDashDelayTime, delegate () { dashTimer.SetTimer(maxDashTime); });
			dashState = PlayerDashStates.DASHING;
		}
	}

	// There is a small delay between button press and player dash where the user can buffer a direction
	// The player dashes in the set x direction for a set period
	// On the first frame after the dash is complete, set player speed and timer between player dashes
	void HandlePlayerDash()
	{
		if (!dashDelayTimer.IsTimerComplete())
		{
			_velocity = Vector2.zero;
		}
		else
		{
			dashState = PlayerDashStates.DASHING;

			// Set for state controller
			_dash = true;

			if (!dashTimer.IsTimerComplete())
			{
				float direction = facingRight ? 1 : -1;

				_velocity.x = direction * dashSpeed;
				_velocity.y = 0;
			}
			else
			{
				// Player has finished dashing, reset velocity and set delay timer
				float direction = facingRight ? 1 : -1;

				_velocity.x = direction * moveSpeed;
				_velocity.y = gravity * Time.deltaTime;

				betweenDashTimer.SetTimer(maxTimeBetweenDash);

				dashState = PlayerDashStates.DASH_NOT_AVAILABLE;

				// Set for state controller
				_dash = false;
			}
		}
	}

	void CheckDashSettings()
	{
		if (_playerController.collisions.below && dashState != PlayerDashStates.DASHING)
		{
			dashState = PlayerDashStates.DASH_AVAILABLE;
		}
	}

	void ResetDash()
	{
		dashState = PlayerDashStates.DASH_AVAILABLE;
	}

	#endregion

    #region Jump

    // Handle player jumping
    public void OnJumpInputDown()
	{
		if (_wallSlide)
		{
			HandleWallSlideJump();
		}
		else if (_playerController.collisions.below)
		{
			HandleGroundedJump();
		}
		else
		{
			if (!coyoteTimeJumpTimer.IsTimerComplete() && !springJump)
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
		if (_velocity.y > minJumpVelocity && !springJump)
		{
			_velocity.y = minJumpVelocity;
		}
	}

	void CheckJumpBuffer()
	{
		if (!jumpBufferTimer.IsTimerComplete())
		{
			if (_playerController.collisions.below)
			{
				HandleGroundedJump(true);
				jumpBufferTimer.CancelTimer();
			}
			else if (_wallSlide)
			{
				HandleWallSlideJump();
				jumpBufferTimer.CancelTimer();
			}
		}
	}

	void CheckGroundControlTimers()
	{
		if (_playerController.collisions.below)
		{
			coyoteTimeJumpTimer.SetTimer(coytoteTimeJumpMaxTime);
		}
	}

	void HandleWallSlideJump()
	{
		if (wallDirX == _input.x)
		{
			_velocity.x = -wallDirX * wallJumpClimb.x;
			_velocity.y = wallJumpClimb.y;
		}
		else if (_input.x == 0)
		{
			_input.x = wallDirX * -1;	// Set input direction for dashes after wall jumps with no input
			_velocity.x = -wallDirX * wallJumpOff.x;
			_velocity.y = wallJumpOff.y;
		}
		else
		{
			_velocity.x = -wallDirX * wallJumpLeap.x;
			_velocity.y = wallJumpLeap.y;
		}

		// Set for state controller
		_jump = true;

		coyoteTimeJumpTimer.CancelTimer();
	}

	void HandleGroundedJump(bool bufferedJump = false)
	{
		if (_playerController.collisions.slidingDownSlope)
		{
			if (_input.x != -Mathf.Sign(_playerController.collisions.slopeNormal.x))
			{
				_velocity.y = maxJumpVelocity * _playerController.collisions.slopeNormal.y;
				_velocity.x = maxJumpVelocity * _playerController.collisions.slopeNormal.x;

				// Set for state controller
				_jump = true;
			}
		}
		else if (bufferedJump)
		{
  			if (Input.GetKey(KeyCode.Space))
				_velocity.y = maxJumpVelocity;
			else
				_velocity.y = minJumpVelocity;

			// Set for state controller
			_jump = true;
		}
		else
		{
			_velocity.y = maxJumpVelocity;

			// Set for state controller
			_jump = true;
		}

		coyoteTimeJumpTimer.CancelTimer();
	}

	void HandleCoyoteTimeJump()
	{
		_velocity.y = maxJumpVelocity;

		// Set for state controller
		_jump = true;

		coyoteTimeJumpTimer.CancelTimer();
	}

	void HandleBufferJump()
	{
		jumpBufferTimer.SetTimer(jumpBufferMaxTime);
	}

	public void HandleSpringPlatform(SpringVelocityVector springVelocity)
	{
		if (springVelocity.AssignSpringVelocity)
		{
			_velocity = springVelocity.SpringVelocity;
		}
		else
		{
			if (!jumpBufferTimer.IsTimerComplete())
			{
				_velocity.y = maxSpringJumpVelocity;

				jumpBufferTimer.CancelTimer();
			}
			else
			{
				_velocity.y = minSpringJumpVelocity;
			}
		}

		ResetDash();

		// Set for state controller
		_jump = true;
		springJump = true;
	}

	#endregion

	#region Rewind

	public void StartRewind()
	{
		if (rewindState == PlayerRewindStates.REWIND_AVAILABLE && !_dead)
		{
			rewindState = PlayerRewindStates.REWINDING;
			rewindTimer.SetTimer(maxRewindTime, delegate() { StopRewind(); });

			playerAnimations.SetSpriteEnabled(false);
			playerTrail.SetTrailRendererEmitting(false);

			Instantiate(rewindStartPreFab, transform.position + new Vector3(0, 0.5f, 0), transform.rotation);

			// Set for state controller
			_rewind = true;
		}
	}

	void Record()
	{
		bool removedPointFromList = false;

		if (pointsInTime.Count > Mathf.Round(maxRecordTime / Time.deltaTime))
		{
			pointsInTime.RemoveAt(pointsInTime.Count - 1);
			trailPositions.RemoveAt(0);

			removedPointFromList = true;

			rewindState = PlayerRewindStates.REWIND_AVAILABLE;
		}

		try
		{
			// Player Ghost
			PointInTime pointInTime = new PointInTime(transform.position, playerAnimations.GetPlayerAnimation(), playerAnimations.facingRight ? 1 : -1);
			pointsInTime.Insert(0, pointInTime);

			// Player Trail
			trailPositions.Add(transform.position);
			playerTrail.SetTrailPositions(trailPositions.ToArray());

			if (!playerRewindGhost)
			{
				playerRewindGhost = InstantiateRewindGhost(pointsInTime[pointsInTime.Count - 1]);
				rewindGhostFramePicker = playerRewindGhost.GetComponent<AnimationFramePickerSystem>();
			}
			else if (removedPointFromList)
				UpdateRewindGhost(pointsInTime[pointsInTime.Count - 1]);
		}
		catch (Exception)
		{
			// This fails on the first frame after rewind finishes
		}
	}

	void StopRewind()
	{
		transform.position = pointsInTime[pointsInTime.Count - 1].position;
		_velocity = Vector3.zero;

		ResetRewind();
		Instantiate(rewindEndPreFab, transform.position + new Vector3(0, 0.5f, 0), transform.rotation);

		// Set for state controller
		_rewind = false;

		//cinemachineImpulseSource.GenerateImpulse();
	}

	void ResetRewind()
	{
		rewindState = PlayerRewindStates.REWIND_NOT_AVAILABLE;

		pointsInTime.Clear();
		trailPositions.Clear();

		Destroy(playerRewindGhost);

		playerAnimations.SetSpriteEnabled(true);
		playerTrail.SetTrailRendererEmitting(true);
		playerTrail.ResetTrailRenderer();
	}

	GameObject InstantiateRewindGhost(PointInTime point)
	{
		GameObject ghost = Instantiate(playerGhostPreFab, point.position + new Vector3(0, -0.5f, 0), Quaternion.identity);    // Need to shift the ghost by 0.5 in y dir to match the sprite shift as result from collider 2D shift

		if (rewindGhostFramePicker)
			rewindGhostFramePicker.ChangeSprite(point.spriteIndex);

		ghost.transform.localScale = new Vector3(point.facingRight, 1, 1);

		return ghost;
	}

	void UpdateRewindGhost(PointInTime point)
	{
		playerRewindGhost.transform.position = point.position + new Vector3(0, -0.5f, 0); // Need to shift the ghost by 0.5 in y dir to match the sprite shift as result from collider 2D shift
		
		if (rewindGhostFramePicker && point.spriteIndex >= 0)
			rewindGhostFramePicker.ChangeSprite(point.spriteIndex);

		playerRewindGhost.transform.localScale = new Vector3(point.facingRight, 1, 1);
	}

	#endregion

    IEnumerator ResetPlayer(float time, bool isPlayerDead, Vector3 playerSpawnLocation, Vector3 playerBoostVelocity)
	{
		pausePlayerControl = true;
		Vector3 playerPauseVelocity = _velocity;

		yield return new WaitForSeconds(time);

		// Set for state controller
		_dead = false;

		if (!isPlayerDead)
		{
			_velocity = playerPauseVelocity + playerBoostVelocity;
		}
		else
		{
			transform.position = playerSpawnLocation;
			_velocity = Vector3.zero;
			playerAnimations.SetSpriteEnabled(true);
		}

		ResetDash();
		ResetRewind();

		pausePlayerControl = false;
	}
}
