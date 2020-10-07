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

public class RoomManager : MonoBehaviour
{
    Player player;
    Platform[] levelPlatforms;

    public GameObject virtualCamera;
    public Vector3[] playerSpawnLocations;

    private void Start()
    {
        player = Object.FindObjectOfType<Player>();
        levelPlatforms = GetComponentsInChildren<Platform>(true);

        if (playerSpawnLocations == null)
            Debug.LogError("Room requires at least one spawn location!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {

        }
    }

    public void ResetLevel(bool playerDied)
    {
        player.OnLevelReset(false, playerSpawnLocations);

        foreach (Platform platform in levelPlatforms)
        {
            platform.OnLevelReset();
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
