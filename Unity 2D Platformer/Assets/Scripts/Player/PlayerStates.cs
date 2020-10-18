public enum PlayerAnimationStates
{
    IDLE,
    RUN,
    STANDING_JUMP,
    RUNNING_JUMP,
    FALL,
    WALL_SLIDE,
    WALL_JUMP_UP,
    WALL_JUMP_OFF,
    REWIND_START,
    REWIND_END,
    DASH,
    DASH_RECOVER,
    DEATH
}

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