using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomManager : MonoBehaviour
{
    BoxCollider2D boxCollider2D;

    Platform[] levelPlatforms;

    public GameObject virtualCamera;
    public Vector3[] playerSpawnLocations;

    private void Start()
    {
        boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
        boxCollider2D.isTrigger = true;

        levelPlatforms = GetComponentsInChildren<Platform>(true);

        if (playerSpawnLocations == null)
            Debug.LogError("Room requires at least one spawn location!");
    }

    public void ResetLevel()
    {
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

            ResetLevel();
            collision.gameObject.GetComponent<Player>().OnReset(false, playerSpawnLocations);
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
