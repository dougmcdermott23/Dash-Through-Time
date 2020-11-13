using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : Platform
{
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

		Vector3 velocity = new Vector3();

		velocity += CalculatePlatformMovement();

		CalculatePassengerMovement(velocity);
		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
	}

	public override void OnReset()
	{
		base.OnReset();

		nextMoveTime = 0;
		fromWaypointIndex = 0;
		percentBetweenWaypoints = 0;
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
