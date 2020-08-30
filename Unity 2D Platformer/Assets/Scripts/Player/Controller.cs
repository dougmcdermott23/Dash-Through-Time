using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : RaycastController
{

	public CollisionInfo collisions;
	[HideInInspector]
	public Vector2 playerInput;

	public float maxSlopeAngle = 55;

	public override void Start()
	{
		base.Start();
		collisions.facingRight = 1;
	}

	public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
	{
		UpdateRayCastOrigins();
		collisions.Reset();
		playerInput = input;

		collisions.velocityOld = velocity;

		if (velocity.y < 0)
			DescendSlope(ref velocity);

		if (velocity.x != 0)
			collisions.facingRight = (int)Mathf.Sign(velocity.x);

		HorizontalCollisions(ref velocity);
		if (velocity.y != 0)
			VerticalCollisions(ref velocity, input);

		if (collisions.onPlatform)
			collisions.platform.PlayerInteraction(transform.gameObject);

		if (standingOnPlatform)
			collisions.below = true;

		transform.Translate(velocity);
	}

	// Checks for horizontal collisions by shooting rays horizontally
	void HorizontalCollisions(ref Vector3 velocity)
	{
		float directionX = collisions.facingRight;
		float rayLength = Mathf.Abs(velocity.x) + skinWidth;

		if (Mathf.Abs(velocity.x) < skinWidth)
			rayLength = 2 * skinWidth;

		for (int i = 0; i < horizontalRayCount; i++)
		{
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.botLeft : raycastOrigins.botRight;

			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			if (hit)
			{
				// Player is falling or jumping through an object
				if (hit.distance == 0 && !collisions.below)
					continue;

				Platform platform = hit.transform.GetComponent<Platform>();
				if (platform != null)
				{
					collisions.onPlatform = true;
					collisions.platform = platform;
				}

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxSlopeAngle)
				{
					// Handle scenario where player goes from descending to climbing a slope
					if (collisions.descendingSlope)
					{
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}

					// If ray detects slope, move player the rest of the distance to the slope before climbing
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld)
					{
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}

				// Collision detected, update variables and reset ray length to only detect other collisions that are closer
				if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
				{
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					if (collisions.climbingSlope)
						velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);

					if (directionX == -1)
						collisions.left = true;
					else
						collisions.right = true;
				}
			}
		}
	}

	// Checks for vertical collisions by shooting rays vertically
	void VerticalCollisions(ref Vector3 velocity, Vector2 input)
	{
		float directionY = Mathf.Sign(velocity.y);
		float rayLength = Mathf.Abs(velocity.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i++)
		{
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.botLeft : raycastOrigins.topLeft;

			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			if (hit)
			{
				// If obstacle has the "Through" tag, player may jump through the bottom or fall through the top of the object
				if (hit.collider.CompareTag("Through"))
				{
					if (directionY == 1 || hit.distance == 0)
					{
						continue;
					}
					if (collisions.fallingThroughPlatform)
					{
						continue;
					}
					if (input.y == -1)
					{
						collisions.fallingThroughPlatform = true;
						Invoke("ResetFallingThroughPlatform", 0.25f);
						continue;
					}
				}

				Platform platform = hit.transform.GetComponent<Platform>();
				if (platform != null)
				{
					collisions.onPlatform = true;
					collisions.platform = platform;
				}

				// Collision detected, update variables and reset ray length to only detect other collisions that are closer
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if (collisions.climbingSlope)
					velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);

				if (directionY == -1)
					collisions.below = true;
				else
					collisions.above = true;
			}
		}

		// Handle scenario where slope angle changes while climbing slope
		// Reset X velocity of the player so that they do not finish inside an object
		if (collisions.climbingSlope)
		{
			float directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.botLeft : raycastOrigins.botRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			if (hit)
			{
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (slopeAngle != collisions.slopeAngle)
				{
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	// Set player velocity while climbing a slope
	void ClimbSlope(ref Vector3 velocity, float slopeAngle)
	{
		float moveDistance = Mathf.Abs(velocity.x);
		float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelocityY)
		{
			velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
			velocity.y = climbVelocityY;
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}

	// Check if player is on a max slope and set player velocity while descending a slope
	void DescendSlope(ref Vector3 velocity)
	{
		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.botLeft, Vector2.down, Mathf.Abs(velocity.y) + skinWidth, collisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.botRight, Vector2.down, Mathf.Abs(velocity.y) + skinWidth, collisionMask);

		if (maxSlopeHitLeft ^ maxSlopeHitRight)
		{
			SlideDownMaxSlope(maxSlopeHitLeft, ref velocity);
			SlideDownMaxSlope(maxSlopeHitRight, ref velocity);
		}
		else if (maxSlopeHitLeft && maxSlopeHitRight)
		{
			if (maxSlopeHitLeft.distance < maxSlopeHitRight.distance)
				SlideDownMaxSlope(maxSlopeHitLeft, ref velocity);
			if (maxSlopeHitLeft.distance > maxSlopeHitRight.distance)
				SlideDownMaxSlope(maxSlopeHitRight, ref velocity);
		}

		if (!collisions.slidingDownSlope)
		{
			float directionX = Mathf.Sign(velocity.x);
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.botRight : raycastOrigins.botLeft;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);

			if (hit)
			{
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
				{
					if (Mathf.Sign(hit.normal.x) == directionX)
					{
						// Check if player is close enough to the slope before moving (ray was initially shot with inf distance)
						if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
						{
							float moveDistance = Mathf.Abs(velocity.x);
							float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
							velocity.y -= descendVelocityY;
							velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

							collisions.slopeAngle = slopeAngle;
							collisions.descendingSlope = true;
							collisions.below = true;
							collisions.slopeNormal = hit.normal;
						}
					}
				}
			}
		}
	}

	// Set player velocity while descending a max slope
	void SlideDownMaxSlope(RaycastHit2D hit, ref Vector3 velocity)
	{
		if (hit)
		{
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle > maxSlopeAngle)
			{
				velocity.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(velocity.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);
				collisions.slopeAngle = slopeAngle;
				collisions.slidingDownSlope = true;
				collisions.slopeNormal = hit.normal;
			}
		}
	}

	void ResetFallingThroughPlatform()
	{
		collisions.fallingThroughPlatform = false;
	}

	public struct CollisionInfo
	{
		public bool above, below;
		public bool left, right;
		public int facingRight;

		public bool fallingThroughPlatform;
		public bool onPlatform;
		public Platform platform;

		public bool climbingSlope;
		public bool descendingSlope;
		public bool slidingDownSlope;
		public float slopeAngle;
		public float slopeAngleOld;

		public Vector3 slopeNormal;
		public Vector3 velocityOld;

		public void Reset()
		{
			above = false;
			below = false;
			left = false;
			right = false;

			onPlatform = false;
			platform = null;

			climbingSlope = false;
			descendingSlope = false;
			slidingDownSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	};
}
