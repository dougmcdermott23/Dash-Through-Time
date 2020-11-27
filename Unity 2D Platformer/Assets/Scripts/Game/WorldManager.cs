using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class WorldManager : MonoBehaviour
{
    Cinemachine.CinemachineBrain cinemachineBrain;
    Cinemachine.ICinemachineCamera currentCinemachineCamera;

    Player player;

    List<RoomManager> worldLevels;
    RoomManager currentLevel;

    void Start()
    {
        cinemachineBrain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();

        player = FindObjectOfType<Player>();

        worldLevels = new List<RoomManager>();

        foreach (RoomManager child in gameObject.GetComponentsInChildren<RoomManager>())
        {
            worldLevels.Add(child);
        }

        worldLevels.Sort(CompareWorldLevels);

        currentLevel = worldLevels[0];
    }

    void Update()
    {
        if (currentCinemachineCamera != cinemachineBrain.ActiveVirtualCamera)
        {
            currentCinemachineCamera = cinemachineBrain.ActiveVirtualCamera;
            currentLevel = currentCinemachineCamera.VirtualCameraGameObject.GetComponentInParent<RoomManager>();
        }
    }

    public void OnPlayerDeath()
    {
        currentLevel.ResetLevel();
        player.OnReset(true, currentLevel.playerSpawnLocations);
    }

    private static int CompareWorldLevels(RoomManager x, RoomManager y)
    {
        if (x == null)
        {
            if (y == null)
            {
                // If x is null and y is null, they're
                // equal.
                return 0;
            }
            else
            {
                // If x is null and y is not null, y
                // is greater.
                return -1;
            }
        }
        else
        {
            // If x is not null...
            //
            if (y == null)
            // ...and y is null, x is greater.
            {
                return 1;
            }
            else
            {
                // ...and y is not null, compare the
                // lengths of the two strings.
                //
                int retval = x.transform.name.CompareTo(y.transform.name);

                if (retval != 0)
                {
                    // If the strings are not of equal length,
                    // the longer string is greater.
                    //
                    return retval;
                }
                else
                {
                    // If the strings are of equal length,
                    // sort them with ordinary string comparison.
                    //
                    return x.transform.name.CompareTo(y.transform.name);
                }
            }
        }
    }
}
