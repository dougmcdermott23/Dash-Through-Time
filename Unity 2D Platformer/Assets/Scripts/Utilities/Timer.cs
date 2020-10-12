using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    Action timerCallback;
    float timer;
    
    public void SetTimer(float timer, Action timerCallback = null)
    {
        this.timer = timer;
        this.timerCallback = timerCallback;
    }

    private void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (IsTimerComplete())
            {
                if (timerCallback != null)
                    timerCallback();
            }
        }
    }

    public bool IsTimerComplete()
    {
        return timer <= 0f;
    }

    public void CancelTimer()
    {
        timer = 0;
    }
}
