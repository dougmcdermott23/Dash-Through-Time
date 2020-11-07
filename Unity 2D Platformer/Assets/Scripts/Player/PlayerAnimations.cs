using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class PlayerAnimations : MonoBehaviour
{
    Player player;

    Animator animator;

    SpriteRenderer spriteRenderer;
    TrailRenderer trail;

    PreviousFrameParameters previousFrameParameters;
    [HideInInspector] public bool facingRight = true;

    public Dictionary<string, PlayerAnimationStartIndices> playerAnimationFrameTranslator = new Dictionary<string, PlayerAnimationStartIndices>()
    {
        { "Player_Idle", PlayerAnimationStartIndices.IDLE },
        { "Player_Run", PlayerAnimationStartIndices.RUN },
        { "Player_Jump_Vertical", PlayerAnimationStartIndices.JUMP_VERTICAL },
        { "Player_Jump_Forward", PlayerAnimationStartIndices.JUMP_FORWARD },
        { "Player_Invert_Jump_To_Fall", PlayerAnimationStartIndices.INVERT_JUMP_TO_FALL },
        { "Player_Fall", PlayerAnimationStartIndices.FALL },
        { "Player_Recover", PlayerAnimationStartIndices.RECOVER },
        { "Player_Wall_Slide", PlayerAnimationStartIndices.WALL_SLIDE },
        { "Player_Dash", PlayerAnimationStartIndices.DASH },
        { "Player_Dash_Recover_To_Ground", PlayerAnimationStartIndices.DASH_RECOVER_TO_GROUND },
        { "Player_Dash_Recover_To_Air", PlayerAnimationStartIndices.DASH_RECOVER_TO_AIR }
    };

    private void Start()
    {
        player = GetComponentInParent<Player>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
    }

    public int GetPlayerAnimation()
    {
        string animationName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        float frameOffset = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length * (animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1) * animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;

        if (playerAnimationFrameTranslator.ContainsKey(animationName))
            return (int)playerAnimationFrameTranslator[animationName] + (int)frameOffset;
        else
            return -1;
    }

    public void Flip()
    {
        if (player.WallSlide)
            facingRight = !player.PlayerController.collisions.left;
        else
            facingRight = !facingRight;

        spriteRenderer.flipX = !facingRight;
    }

    public void SetAnimationParameters()
    {
        // Set Player Sprite Direction
        if ((facingRight && player.PlayerInput.x < 0) ||
            (!facingRight && player.PlayerInput.x > 0) ||
            (previousFrameParameters.wallSlide && player.PlayerInput.x == 0) ||
            (player.WallSlide))
        {
            Flip();
        }

        // Horizontal Input
        animator.SetBool("isRunning", player.PlayerInput.x != 0);

        // Grounded
        animator.SetBool("isGrounded", player.PlayerController.collisions.below);

        // Wall Sliding
        int wallDirX = (previousFrameParameters.collisionInfo.left) ? -1 : 1;
        bool wallKick = wallDirX != player.PlayerInput.x || player.PlayerInput.x == 0;

        animator.SetBool("isWallSliding", player.WallSlide);
        animator.SetBool("wasWallSliding", previousFrameParameters.wallSlide);
        animator.SetBool("isWallKick", wallKick);

        // Jump
        if (player.Jump)
            animator.SetTrigger("jump");

        // Vertical Velocity
        animator.SetBool("isDescending", player.PlayerVelocity.y < 0);

        if (player.PlayerVelocity.y < 0 && previousFrameParameters.velocity.y > 0)
            animator.SetTrigger("verticalDirectionChanged");

        // Dashing
        animator.SetBool("isDashing", player.Dash);

        if (player.Dash && !previousFrameParameters.dash)
            animator.SetTrigger("startDash");

        // Rewind
        if (player.Rewind && !previousFrameParameters.rewind)
            animator.SetTrigger("startRewind");
        else if (!player.Rewind && previousFrameParameters.rewind)
            animator.SetTrigger("endRewind");

        previousFrameParameters.SetPreviousParameters(player);
    }

    public void SetSpriteEnabled(bool enabled)
    {
        spriteRenderer.enabled = enabled;
    }

    public void InitiateTrailRenderer(float trailTime)
    {
        SetTrailRendererEmitting(true);
        trail.time = trailTime;		// The trail should not start fading out before the player has finished the rewind
    }

    public void ResetTrailRenderer()
    {
        trail.Clear();
    }

    public void SetTrailRendererEmitting(bool emitting)
    {
        trail.emitting = emitting;
    }
}
