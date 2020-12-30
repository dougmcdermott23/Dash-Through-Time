using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class WorldManager : MonoBehaviour
{
    Cinemachine.CinemachineBrain cinemachineBrain;
    Cinemachine.ICinemachineCamera currentCinemachineCamera;

    public Animator transition;
    public TextMeshProUGUI playerDeathCounter;
    public TextMeshProUGUI finalTime;
    public TextMeshProUGUI finalDeathCount;

    Player player;
    public GameObject playerPrefab;

    List<RoomManager> worldLevels;
    RoomManager currentLevel;

    int playerDeaths = 0;
    public int startingLevel = 0;

    void Start()
    {
        cinemachineBrain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();

        worldLevels = new List<RoomManager>();

        foreach (RoomManager child in gameObject.GetComponentsInChildren<RoomManager>())
        {
            worldLevels.Add(child);
        }

        worldLevels.Sort(CompareWorldLevels);

        currentLevel = worldLevels[0];

        transition.gameObject.SetActive(false);

        if (Application.isEditor)
        {
            currentLevel = worldLevels[startingLevel];
        }

        var playerObj = Instantiate(playerPrefab, currentLevel.playerSpawnLocations[0], transform.rotation);
        player = playerObj.GetComponent<Player>();
    }

    void Update()
    {
        if (currentCinemachineCamera != cinemachineBrain.ActiveVirtualCamera)
        {
            currentCinemachineCamera = cinemachineBrain.ActiveVirtualCamera;
            if (currentCinemachineCamera != null)
                currentLevel = currentCinemachineCamera.VirtualCameraGameObject.GetComponentInParent<RoomManager>();
        }
    }

    public void OnPlayerDeath()
    {
        playerDeaths++;
        playerDeathCounter.SetText(playerDeaths.ToString("D3"));
        StartCoroutine(ScreenWipe());
    }

    public void PlayerWin()
    {
        string finalTimeString = string.Format("Time: {0}", Time.timeSinceLevelLoad.ToString("n2"));
        string finalDeathCountString = string.Format("Deaths: {0}", playerDeaths);

        finalTime.SetText(finalTimeString);
        finalDeathCount.SetText(finalDeathCountString);
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
        transition.gameObject.SetActive(true);
        player.OnReset(true, currentLevel.playerSpawnLocations);
        transition.SetTrigger("wipeScreen");

        yield return new WaitForSeconds(transitionTime);

        // Reset level when screen is covered
        currentLevel.ResetLevel();

        yield return new WaitForSeconds(transitionTime);

        transition.gameObject.SetActive(false);
    }
}
