using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformController : RaycastController
{

	public LayerMask passengerMask;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller> passengerDictionary = new Dictionary<Transform, Controller>();

	public float speed;
	public float waitTime;
	[Range(0, 2)]
	public float easeAmount;

	public Vector3[] localWaypoints;
	Vector3[] globalWaypoints;
	public bool isCyclic;
	float nextMoveTime;
	int fromWaypointIndex;
	float percentBetweenWaypoints;

	public override void Start()
	{
		base.Start();

		// Set globalWaypoints for calculations
		globalWaypoints = new Vector3[localWaypoints.Length];
		for (int i = 0; i < localWaypoints.Length; i++)
			globalWaypoints[i] = localWaypoints[i] + transform.position;
	}

	void Update()
	{
		UpdateRayCastOrigins();

		Vector3 velocity = CalculatePlatformMovement();
		CalculatePassengerMovement(velocity);
		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
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

	// Function to create a gradual increase and decrease in speed from waypoints
	float Ease(float x)
	{
		float easeExp = 1 + easeAmount;
		return Mathf.Pow(x, easeExp) / (Mathf.Pow(x, easeExp) + Mathf.Pow((1 - x), easeExp));
	}

	// Platform moves between specified waypoints
	// Index values are clamped
	// The greater the distance, the slower the percentage will increase
	Vector3 CalculatePlatformMovement()
	{
		if (Time.time < nextMoveTime)
			return Vector3.zero;

		fromWaypointIndex %= globalWaypoints.Length;
		int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
		percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

		// Go to next waypoint
		if (percentBetweenWaypoints >= 1)
		{
			percentBetweenWaypoints = 0;
			fromWaypointIndex++;

			// Reset index to zero and flip array
			if (!isCyclic)
			{
				if (fromWaypointIndex >= globalWaypoints.Length - 1)
				{
					fromWaypointIndex = 0;
					System.Array.Reverse(globalWaypoints);
				}
			}

			nextMoveTime = Time.time + waitTime;
		}

		return (newPos - transform.position);
	}

	void CalculatePassengerMovement(Vector3 velocity)
	{
		HashSet<Transform>  movedPassengers = new HashSet<Transform>();
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

	// Draw waypoints for testing
	void OnDrawGizmos()
	{
		if (localWaypoints != null)
		{
			Gizmos.color = Color.red;
			float size = 0.3f;

			for (int i = 0; i < localWaypoints.Length; i++)
			{
				Vector3 globalWaypointPos = (Application.isPlaying) ? (globalWaypoints[i]) : (localWaypoints[i] + transform.position);
				Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
				Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
			}
		}
	}
}
