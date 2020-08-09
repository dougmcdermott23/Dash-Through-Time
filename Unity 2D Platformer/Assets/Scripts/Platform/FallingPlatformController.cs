using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatformController : RaycastController
{

	public LayerMask passengerMask;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller> passengerDictionary = new Dictionary<Transform, Controller>();

	Vector3 velocity;
	public float delayBeforeFall = 1;
	public float platformFallTime = 1;
	public float gravity = -1;
	float timeToPlatformFall;
	bool platformTriggered;
	bool platformFalling;

	public override void Start()
	{
		base.Start();

		platformTriggered = false;
		platformFalling = false;
		velocity = Vector3.zero;
	}

	void Update()
	{
		UpdateRayCastOrigins();

		DetectPassenger();

		CalculatePlatformMovement();
		if (platformFalling) { 
			CalculatePassengerMovement(velocity);
			MovePassengers(true);
			transform.Translate(velocity);
			MovePassengers(false);
		}
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

	// Platform moves after something triggers it
	// There is a specified delay before the platform can move
	void CalculatePlatformMovement()
	{
		if (platformTriggered)
		{
			if (timeToPlatformFall <= 0)
			{
				platformFalling = true;
				velocity.y -= gravity * Time.deltaTime;
			}
			else
			{
				timeToPlatformFall -= Time.deltaTime;
			}
		}
	}

	// Detect if a passenger is on the platform
	// If passenger is detected, enable the platform trigger
	void DetectPassenger()
	{
		float rayLength = skinWidth * 2;

		for (int i = 0; i < verticalRayCount; i++)
		{
			Vector2 rayOrigin = raycastOrigins.topLeft;

			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

			if (hit)
			{
				if (!platformTriggered)
				{
					Destroy(gameObject, platformFallTime + delayBeforeFall);
					timeToPlatformFall = delayBeforeFall;
				}
				platformTriggered = true;
				break;
			}
		}

	}

	void CalculatePassengerMovement(Vector3 velocity)
	{
		HashSet<Transform>  movedPassengers = new HashSet<Transform>();
		passengerMovement = new List<PassengerMovement>();

		float directionY = Mathf.Sign(velocity.y);

		// Passenger on top of downward moving platform
		if (velocity.y != 0)
		{
			float rayLength = Mathf.Abs(velocity.y) + skinWidth;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = raycastOrigins.topLeft;

				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

				if (hit)
				{
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
	}

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
}
