using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerDashStates
{
    DASH_AVAILABLE,
    DASH_NOT_AVAILABLE,
    DASHING
}

public enum PlayerRewindStates
{
    RECORDING,
    REWINDING
}

public enum PlayerAnimationStartIndices
{
	IDLE						= 0,
	RUN							= 3,
	JUMP_VERTICAL				= 11,
	JUMP_FORWARD				= 12,
	INVERT_JUMP_TO_FALL			= 13,
	FALL						= 16,
	RECOVER						= 20,
	WALL_SLIDE					= 23,
	DASH						= 27,
	DASH_RECOVER_TO_GROUND		= 28,
	DASH_RECOVER_TO_AIR			= 32
}

public struct CollisionInfo
{
	public bool above, below;
	public bool left, right;
	public int facingRight;

	public bool fallingThroughPlatform;

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

		climbingSlope = false;
		descendingSlope = false;
		slidingDownSlope = false;
		slopeAngleOld = slopeAngle;
		slopeAngle = 0;
	}
};

public struct PreviousFrameParameters
{
    public CollisionInfo collisionInfo;
    public Vector2 input;
    public Vector3 velocity;
    public bool jump;
    public bool wallSlide;
    public bool dash;
    public bool rewind;
    public bool dead;

    public void SetPreviousParameters(Player player)
    {
        collisionInfo = player.PlayerController.collisions;
        input = player.PlayerInput;
        velocity = player.PlayerVelocity;
        jump = player.Jump;
        wallSlide = player.WallSlide;
        dash = player.Dash;
        rewind = player.Rewind;
        dead = player.Dead;
    }
}