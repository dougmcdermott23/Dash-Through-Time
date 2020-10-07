using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Platform))]
public class FallingPlatform : MonoBehaviour
{
	Vector3 velocity;
	public float delayBeforeFall = 1;
	public float platformFallTime = 1;
	public float gravity = 1;
	float timeToPlatformFall;
	float timeToPlatformDisable;
	bool platformTriggered;

	void Start()
	{
		platformTriggered = false;
		velocity = Vector3.zero;
	}

	public void OnLevelReset()
	{
		velocity = Vector3.zero;
		platformTriggered = false;
		timeToPlatformFall = 0;
		timeToPlatformDisable = 0;
		gameObject.SetActive(true);
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

			if (timeToPlatformDisable <= 0)
			{
				gameObject.SetActive(false);
			}
			else
			{
				timeToPlatformDisable -= Time.deltaTime;
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
			timeToPlatformDisable = platformFallTime + delayBeforeFall;
			timeToPlatformFall = delayBeforeFall;
		}

		platformTriggered = true;
	}
}
