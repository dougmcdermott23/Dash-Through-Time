using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimations : MonoBehaviour
{
    Player player;
    Animator animator;
    SpriteRenderer spriteRenderer;
    DashEchoEffect dashEcho;

    public ParticleSystem moveDust;
    public ParticleSystem wallSlideDust;

    float wallSlideDustOffset;

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

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dashEcho = GetComponent<DashEchoEffect>();

        wallSlideDustOffset = wallSlideDust.transform.localPosition.x;
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

        // Create dust when player changes direction
        if (player.PlayerController.collisions.below)
            CreateDust(moveDust);
    }

    public void SetAnimationParameters()
    {
        #region Sprite Direction

        if ((facingRight && player.PlayerInput.x < 0) ||
            (!facingRight && player.PlayerInput.x > 0) ||
            (previousFrameParameters.wallSlide && player.PlayerInput.x == 0) ||
            (player.WallSlide))
        {
            Flip();
        }

        #endregion

        #region Horizontal Input

        animator.SetBool("isRunning", player.PlayerInput.x != 0);

        #endregion

        #region Grounded

        animator.SetBool("isGrounded", player.PlayerController.collisions.below);

        if (player.PlayerController.collisions.below && !previousFrameParameters.collisionInfo.below)
            CreateDust(moveDust);

        #endregion

        #region Wall Sliding

        int wallDirX = (previousFrameParameters.collisionInfo.left) ? -1 : 1;
        bool wallKick = wallDirX != player.PlayerInput.x || player.PlayerInput.x == 0;

        animator.SetBool("isWallSliding", player.WallSlide);
        animator.SetBool("wasWallSliding", previousFrameParameters.wallSlide);
        animator.SetBool("isWallKick", wallKick);

        if (player.WallSlide)
        {
            if (facingRight)
                wallSlideDust.transform.localPosition = new Vector3(wallSlideDustOffset, wallSlideDust.transform.localPosition.y, wallSlideDust.transform.localPosition.z);
            else
                wallSlideDust.transform.localPosition = new Vector3(-wallSlideDustOffset, wallSlideDust.transform.localPosition.y, wallSlideDust.transform.localPosition.z);
            
            CreateDust(wallSlideDust);
        }

        #endregion

        #region Jump

        if (player.Jump)
        {
            animator.SetTrigger("jump");

            CreateDust(moveDust);
        }

        #endregion

        #region Vertical Velocity

        animator.SetBool("isDescending", player.PlayerVelocity.y < 0);

        animator.ResetTrigger("verticalDirectionChanged");
        if (player.PlayerVelocity.y < 0 && previousFrameParameters.velocity.y >= 0)
            animator.SetTrigger("verticalDirectionChanged");

        #endregion

        #region Dashing

        animator.SetBool("isDashing", player.Dash);

        if (player.Dash && !previousFrameParameters.dash)
        {
            animator.SetTrigger("startDash");
            dashEcho.SpawnDashEcho(facingRight);
        }

        #endregion

        #region Rewind

        if (player.Rewind && !previousFrameParameters.rewind)
            animator.SetTrigger("startRewind");
        else if (!player.Rewind && previousFrameParameters.rewind)
            animator.SetTrigger("endRewind");

        #endregion

        previousFrameParameters.SetPreviousParameters(player);
    }

    public void SetSpriteEnabled(bool enabled)
    {
        spriteRenderer.enabled = enabled;
    }

    void CreateDust(ParticleSystem dust)
    {
        dust.Play();
    }
}
