using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashEchoEffect : MonoBehaviour
{
    public GameObject dashEcho;
    SpriteRenderer echoSpriteRenderer;

    public float timeBetweenSpawns;
    Timer betweenSpawnsTimer;

    public int numSpawns;
    int spawnCount = 0;

    void Start()
    {
        echoSpriteRenderer = dashEcho.GetComponent<SpriteRenderer>();
        betweenSpawnsTimer = gameObject.AddComponent(typeof(Timer)) as Timer;
    }

    public void SpawnDashEcho(bool facingRight)
    {
        if (spawnCount < numSpawns)
        {
            echoSpriteRenderer.flipX = !facingRight;
            Instantiate(dashEcho, transform.position, Quaternion.identity);
            betweenSpawnsTimer.SetTimer(timeBetweenSpawns, delegate () { SpawnDashEcho(facingRight); });
            spawnCount++;
        }
        else
        {
            spawnCount = 0;
        }
    }
}
