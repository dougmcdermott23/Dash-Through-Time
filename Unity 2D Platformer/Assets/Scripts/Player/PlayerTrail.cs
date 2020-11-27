using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class PlayerTrail : MonoBehaviour
{
    TrailRenderer trail;

    float fixedAplha = 155f / 255f;

    float fadeOutTime = 0;
    float currentFadeTime = 0;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();

        SetTrailAlpha(fixedAplha);
    }

    public void ResetTrailRenderer()
    {
        SetTrailAlpha(fixedAplha);
        trail.Clear();
    }

    public void SetTrailRendererEmitting(bool emitting)
    {
        trail.emitting = emitting;
    }

    public void FadeOut(float time)
    {
        fadeOutTime = time;
        currentFadeTime = time;
    }

    public void SetTrailPositions(Vector3[] trailPositions)
    {
        ResetTrailRenderer();
        trail.AddPositions(trailPositions);
    }

    void SetTrailAlpha(float alpha)
    {
        Gradient gradient = trail.colorGradient;
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[gradient.alphaKeys.Length];

        for (int i = 0; i < gradient.alphaKeys.Length; i++)
        {
            alphaKeys[i] = new GradientAlphaKey(alpha, gradient.alphaKeys[i].time);
        }

        gradient.SetKeys(gradient.colorKeys, alphaKeys);
        trail.colorGradient = gradient;
    }

    void Update()
    {
        if (currentFadeTime > 0)
        {
            float lerpTime = Mathf.Lerp(0, fadeOutTime, currentFadeTime);
            float alpha = Mathf.Lerp(0, fixedAplha, lerpTime);

            SetTrailAlpha(alpha);

            currentFadeTime -= Time.deltaTime;
        }
    }
}
