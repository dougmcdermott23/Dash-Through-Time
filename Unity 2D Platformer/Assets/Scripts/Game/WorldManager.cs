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

    public Animator transition;

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

        transition.gameObject.SetActive(false);
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
        StartCoroutine(ScreenWipe());
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

    IEnumerator ScreenWipe(float transitionTime = 1)
    {
        var start = Time.time;

        transition.gameObject.SetActive(true);
        player.OnReset(true, currentLevel.playerSpawnLocations);
        transition.SetTrigger("wipeScreen");

        yield return new WaitForSeconds(transitionTime);

        // Reset level when screen is covered
        currentLevel.ResetLevel();

        yield return new WaitForSeconds(transitionTime);

        transition.gameObject.SetActive(false);

        Debug.Log(Time.time - start);
    }
}
