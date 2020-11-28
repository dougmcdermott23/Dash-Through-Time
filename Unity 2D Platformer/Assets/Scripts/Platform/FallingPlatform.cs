using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : Platform
{
	SpriteRenderer spriteRenderer;
	Animator animator;
	ParticleSystem particles;
	int startLayer;

	Vector3 velocity;
	public float delayBeforeFall = 1;
	public float platformFallTime = 1;
	public float platformDisabledTime = 1;
	public float gravity = 1;
	bool prevPlatformTriggered;
	bool isPlatformTriggered;
	bool isPlatformFalling;
	bool isPlatformDisabled;

	public override void Start()
	{
		base.Start();

		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		animator = gameObject.GetComponent<Animator>();
		particles = gameObject.GetComponent<ParticleSystem>();
		startLayer = gameObject.layer;

		prevPlatformTriggered = false;
		isPlatformTriggered = false;

		velocity = Vector3.zero;
	}

	void Update()
	{
		UpdateRayCastOrigins();

		Vector3 velocity = new Vector3();

		prevPlatformTriggered = isPlatformTriggered;
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
		prevPlatformTriggered = false;
		isPlatformTriggered = false;
		isPlatformFalling = false;
		isPlatformDisabled = false;

		spriteRenderer.enabled = true;
		gameObject.layer = startLayer;
	}

	// Platform moves after something triggers it
	// There is a specified delay before the platform can move
	Vector3 CalculatePlatformMovement()
	{
		if (isPlatformTriggered && !prevPlatformTriggered)
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

	// Called from animation event to instantiate particle system
	public void PlayParticles()
	{
		particles.Play();
	}

	IEnumerator PlatformFall(float time)
	{
		animator.SetTrigger("playerOnPlatform");

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
			spriteRenderer.enabled = false;
			gameObject.layer = 0;
		}

		isPlatformDisabled = !enable;
	}
}
