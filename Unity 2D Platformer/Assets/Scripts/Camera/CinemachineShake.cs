using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance { get; private set; }

    CinemachineVirtualCamera cinemachineVirtualCamera;
    CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    Timer shakeTimer;

    void Awake()
    {
        Instance = this;

        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        shakeTimer = gameObject.AddComponent(typeof(Timer)) as Timer;
    }

    void Update()
    {
        Instance = this;
    }

    public void ShakeCamera(float intensity, float time)
    {
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
        shakeTimer.SetTimer(time, delegate () { EndCameraShake(); });
    }

    public void EndCameraShake()
    {
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0f;
    }
}
