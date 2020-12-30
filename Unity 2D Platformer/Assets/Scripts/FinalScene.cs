using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalScene : MonoBehaviour
{
    WorldManager worldManager;
    bool isTriggered;
    
    void Start()
    {
        worldManager = FindObjectOfType<WorldManager>();
        isTriggered = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTriggered)
        {
            worldManager.PlayerWin();
            isTriggered = true;
        }
    }
}
