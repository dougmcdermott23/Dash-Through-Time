using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : Platform
{
	MeshRenderer meshRenderer;
	int startLayer;

	Vector3 velocity;
	public float delayBeforeFall = 1;
	public float platformFallTime = 1;
	public float platformDisabledTime = 1;
	public float gravity = 1;
	bool isPlatformTriggered;
	bool isPlatformFalling;
	bool isPlatformDisabled;

	public override void Start()
	{
		base.Start();

		meshRenderer = gameObject.GetComponent<MeshRenderer>();
		startLayer = gameObject.layer;

		isPlatformTriggered = false;
		velocity = Vector3.zero;
	}

	void Update()
	{
		UpdateRayCastOrigins();

		Vector3 velocity = new Vector3();

		isPlatformTriggered = isPlatformTriggered || DetectPassenger();

		velocity += CalculatePlatformMovement();

		CalculatePassengerMovement(velocity);
		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
	}

	public override void OnReset()
	{
		base.OnReset();

		StopAllCoroutines();

		velocity = Vector3.zero;
		isPlatformTriggered = false;
		isPlatformFalling = false;
		isPlatformDisabled = false;

		meshRenderer.enabled = true;
		gameObject.layer = startLayer;
	}

	// Platform moves after something triggers it
	// There is a specified delay before the platform can move
	Vector3 CalculatePlatformMovement()
	{
		if (isPlatformTriggered)
		{
			StartCoroutine(PlatformFall(delayBeforeFall));
		}

		if (isPlatformFalling)
		{
			StartCoroutine(SetPlatformEnabled(platformFallTime, false));
			velocity.y -= gravity * Time.deltaTime;
		}

		// if platformDisabledTime > 0 set a timer to reset the platform, otherwise disable the gameObject
		if (isPlatformDisabled)
		{
			if (platformDisabledTime > 0)
			{
				StartCoroutine(SetPlatformEnabled(platformDisabledTime, true));
			}
			else
			{
				gameObject.SetActive(false);
			}
		}

		return velocity;
	}

	IEnumerator PlatformFall(float time)
	{
		yield return new WaitForSeconds(time);

		isPlatformFalling = true;
	}

	IEnumerator SetPlatformEnabled(float time, bool enable)
	{
		yield return new WaitForSeconds(time);

		if (enable)
		{
			OnReset();
		}
		else
		{
			meshRenderer.enabled = false;
			gameObject.layer = 0;
		}

		isPlatformDisabled = !enable;
	}
}
