using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
	Vector3 velocity;
	public float delayBeforeFall = 1;
	public float platformFallTime = 1;
	public float gravity = 1;
	float timeToPlatformFall;
	bool platformTriggered;

	void Start()
	{
		platformTriggered = false;
		velocity = Vector3.zero;
	}

	// Platform moves after something triggers it
	// There is a specified delay before the platform can move
	public Vector3 CalculatePlatformMovement()
	{
		if (platformTriggered)
		{
			if (timeToPlatformFall <= 0)
			{
				velocity.y -= gravity * Time.deltaTime;
			}
			else
			{
				timeToPlatformFall -= Time.deltaTime;
			}
		}

		return velocity;
	}

	// Detect if a passenger is on the platform
	// If passenger is detected, enable the platform trigger
	public void PassengerDetected()
	{
		if (!platformTriggered)
		{
			Destroy(gameObject, platformFallTime + delayBeforeFall);
			timeToPlatformFall = delayBeforeFall;
		}

		platformTriggered = true;
	}
}
