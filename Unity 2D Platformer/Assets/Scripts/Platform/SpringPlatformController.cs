using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPlatformController : RaycastController
{
	// ***** TO DO *****
	// - Send player in perpendicular direction to the platform
	// - Currently the script calls a method in Player. If this is to be compatible with all projects need to have the same "OnSpringPlatform" method for each
	//		- This means that we must make sure here that the passenger contains such a method. What is the best way to accomplish this?
	// - Update Player.cs to allow for jumping on the spring platform
	//		- The buffered jump may need to be slightly altered to allow for jumps slightly before they hit and slightly after they leave the platform

	public LayerMask passengerMask;

	List<PassengerMovement> passengerMovement;
	Dictionary<Transform, Controller> passengerDictionary = new Dictionary<Transform, Controller>();

	public float springVelocity;

	public override void Start()
	{
		base.Start();
	}

	void Update()
	{
		UpdateRayCastOrigins();

		DetectPassengers();
		MovePassengers();
	}

	void MovePassengers()
	{
		foreach (PassengerMovement passenger in passengerMovement)
		{
			if (!passengerDictionary.ContainsKey(passenger.transform))
				passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller>());

			try
			{
				var passengerController = passenger.transform.GetComponent<Player>();
				passengerController.OnSpringPlatform(passenger.velocity);
			}
			catch
			{
				Debug.LogError("Exception: passenger does not contain correct method for applying spring velocity" + passenger.transform.gameObject.name);
			}
		}
	}

	void DetectPassengers()
	{
		HashSet<Transform> movedPassengers = new HashSet<Transform>();
		passengerMovement = new List<PassengerMovement>();

		float rayLength = skinWidth * 2;

		for (int i = 0; i < verticalRayCount; i++)
		{
			Vector2 rayOrigin = raycastOrigins.topLeft;

			// Improve this to send player in perpendicular direction to the platform (ray is not just shot up)
			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

			if (hit)
			{
				if (!movedPassengers.Contains(hit.transform))
				{
					movedPassengers.Add(hit.transform);

					// Improve this to send player in perpendicular direction to the platform (apply force in perpendicular direction, not just +y)

					passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(0, springVelocity)));
				}
			}
		}
	}

	struct PassengerMovement
	{
		public Transform transform;
		public Vector3 velocity;

		public PassengerMovement(Transform _transform, Vector3 _velocity)
		{
			transform = _transform;
			velocity = _velocity;
		}
	};

}
