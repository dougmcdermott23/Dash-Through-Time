using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerAnimations : MonoBehaviour
{
    SpriteRenderer sprite;
    TrailRenderer trail;
    Animator animator;

    [HideInInspector]
    public bool jump;

    public float maxMoveRotation = 10;
    public float rotationSpeed = 10;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
        animator = GetComponent<Animator>();

        Reset();
    }

    public void Reset()
    {
        jump = false;
    }

    public void SetAnimationParameters()
    {
        animator.SetBool("Jump", jump);
    }

    public void SetSpriteEnabled(bool enabled)
    {
        sprite.enabled = enabled;
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

    public void RotateInDirectionOfMovement(Vector2 input)
    {
        float targetZ = input.x * -maxMoveRotation;

        Vector3 euler = transform.localEulerAngles;
        float currentZ = euler.z;

        if (currentZ >= 360 - maxMoveRotation)
        {
            currentZ -= 360;
        }

        euler.z = Mathf.Lerp(currentZ, targetZ, rotationSpeed * Time.deltaTime);
        transform.localEulerAngles = euler;
    }
}
