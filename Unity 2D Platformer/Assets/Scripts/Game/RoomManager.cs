using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////////////////////
/// Room Manager
/// 
/// This script is responsible for:
/// - Room transitions
/// - Handling player death and spawing, as well as resetting the room after player death
//////////////////////////////////////////////////////////////////////////////////////////

[RequireComponent(typeof(BoxCollider2D))]
public class RoomManager : MonoBehaviour
{
    BoxCollider2D boxCollider2D;

    Player player;
    Platform[] levelPlatforms;

    public GameObject virtualCamera;
    public Vector3[] playerSpawnLocations;

    private void Start()
    {
        boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
        boxCollider2D.isTrigger = true;

        player = Object.FindObjectOfType<Player>();
        levelPlatforms = GetComponentsInChildren<Platform>(true);

        if (playerSpawnLocations == null)
            Debug.LogError("Room requires at least one spawn location!");
    }

    public void ResetLevel(bool isPlayerDead)
    {
        player.OnReset(isPlayerDead, playerSpawnLocations);

        foreach (Platform platform in levelPlatforms)
        {
            platform.OnReset();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            virtualCamera.SetActive(true);

            ResetLevel(false);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            virtualCamera.SetActive(false);
        }
    }

    // Draw player spawn location
    void OnDrawGizmos()
    {
        if (playerSpawnLocations != null)
        {
            for (int i = 0; i < playerSpawnLocations.Length; i++)
            {
                Gizmos.color = Color.green;
                float size = 0.3f;

                Gizmos.DrawLine(playerSpawnLocations[i] - Vector3.up * size, playerSpawnLocations[i] + Vector3.up * size);
                Gizmos.DrawLine(playerSpawnLocations[i] - Vector3.left * size, playerSpawnLocations[i] + Vector3.left * size);
            }
        }
    }
}
