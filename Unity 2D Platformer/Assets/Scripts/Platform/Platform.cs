using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////////////////////
/// Platform Controller
/// 
/// Platform Types:
/// - Moving Platform: moves between waypoints.
/// - Falling Platform: moves vertically and is destroyed after player triggers.
/// - Spring Platform: bounces player
/// 
/// Combining various types of platforms may result in unexpected behaviour
//////////////////////////////////////////////////////////////////////////////////////////

public class Platform : RaycastController
{
	public LayerMask passengerMask;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller> passengerDictionary = new Dictionary<Transform, Controller>();

	MovingPlatform movingPlatform;
	FallingPlatform fallingPlatform;
	SpringPlatform springPlatform;

	Vector3 platformStartPosition;

	struct PassengerMovement
	{
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
		{
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	};

	public override void Start()
    {
        base.Start();

		platformStartPosition = transform.position;

		movingPlatform = gameObject.GetComponent<MovingPlatform>();
		fallingPlatform = gameObject.GetComponent<FallingPlatform>();
		springPlatform = gameObject.GetComponent<SpringPlatform>();

		if (movingPlatform != null && fallingPlatform != null)
			Debug.LogError("The moving platform controller and falling platform controller are not compatible!");
	}

	void Update()
	{
		UpdateRayCastOrigins();

		Vector3 velocity = new Vector3();

		DetectPassenger();

		if (movingPlatform != null && movingPlatform.enabled)
			velocity += movingPlatform.CalculatePlatformMovement();

		if (fallingPlatform != null && fallingPlatform.enabled)
			velocity += fallingPlatform.CalculatePlatformMovement();

		CalculatePassengerMovement(velocity);
		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
	}

	public void OnReset()
	{
		transform.position = platformStartPosition;

		if (movingPlatform != null && movingPlatform.enabled)
			movingPlatform.OnReset();

		if (fallingPlatform != null && fallingPlatform.enabled)
			fallingPlatform.OnReset();
	}

	void CalculatePassengerMovement(Vector3 velocity)
	{
		HashSet<Transform> movedPassengers = new HashSet<Transform>();
		passengerMovement = new List<PassengerMovement>();

		float directionX = Mathf.Sign(velocity.x);
		float directionY = Mathf.Sign(velocity.y);

		// Vertically moving platform
		if (velocity.y != 0)
		{
			float rayLength = Mathf.Abs(velocity.y) + skinWidth;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.botLeft : raycastOrigins.topLeft;

				rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				if (hit)
				{
					if (hit.distance == 0)
						continue;

					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);

						float pushX = (directionY == 1) ? velocity.x : 0;
						float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

						// Passenger is on platform if platform moving up, always want passenger to move first
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, directionY == 1));
					}
				}
			}
		}

		// Horizontally moving platform
		if (velocity.x != 0)
		{
			float rayLength = Mathf.Abs(velocity.x) + skinWidth;

			if (Mathf.Abs(velocity.x) < skinWidth)
				rayLength = 2 * skinWidth;

			for (int i = 0; i < horizontalRayCount; i++)
			{
				Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.botLeft : raycastOrigins.botRight;

				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				if (hit)
				{
					if (hit.distance == 0)
						continue;

					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);

						float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
						float pushY = -skinWidth;

						// Impossible for passenger to be on platform, always want passenger to move first
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
					}
				}
			}
		}

		// Passenger on top of horizontally or downward moving platform
		if (directionY == -1 || (velocity.y == 0 && velocity.x != 0))
		{
			float rayLength = skinWidth * 2;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.topLeft;

				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

				if (hit)
				{
					if (hit.distance == 0)
						continue;

					if (!movedPassengers.Contains(hit.transform))
					{
						movedPassengers.Add(hit.transform);

						float pushX = velocity.x;
						float pushY = velocity.y;

						// Passenger is on platform, always want platform to move first
						passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
					}
				}
			}
		}
	}

	// Detect if a passenger is on the platform
	// If passenger is detected, enable the platform trigger
	bool DetectPassenger()
	{
		float rayLength = skinWidth * 2;

		for (int i = 0; i < verticalRayCount; i++)
		{
			Vector2 rayOrigin = raycastOrigins.topLeft;

			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

			if (hit)
			{
				return true;
			}
		}

		return false;
	}

	// Move Passengers on the platform.
	// If the platform is moving up, move the passenger first
	// If the platform is moving down, move the platform first
	void MovePassengers(bool beforeMovePlatform)
	{
		foreach (PassengerMovement passenger in passengerMovement)
		{
			if (!passengerDictionary.ContainsKey(passenger.transform))
				passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller>());
			if (passenger.moveBeforePlatform == beforeMovePlatform)
				passengerDictionary[passenger.transform].Move(passenger.velocity, Vector2.zero, passenger.standingOnPlatform);
		}
	}

	// Called from player when they hit a platform
	public void PlayerInteraction(GameObject gameObject)
	{
		if (DetectPassenger())
		{
			if (fallingPlatform != null && fallingPlatform.enabled)
			{
				fallingPlatform.PassengerDetected();
			}

			if (springPlatform != null && springPlatform.enabled)
			{
				PlayerInput playerInput = gameObject.GetComponent<PlayerInput>();
				if (playerInput != null)
					playerInput.SpringJump(springPlatform.maxJumpVelocity, springPlatform.minJumpVelocity);
			}
		}
	}

	public bool IsSpringPlatform()
	{
		if (springPlatform != null)
			return true;
		else
			return false;
	}
}
