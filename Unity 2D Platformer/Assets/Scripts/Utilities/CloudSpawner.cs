using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    public GameObject cloudPreFab;

    public int minStartingClouds;
    public int maxStartingClouds;
    public float minSpawnTime;
    public float maxSpawnTime;
    public float minCloudSpeed;
    public float maxCloudSpeed;
    public float minCloudHeight;
    public float maxCloudHeight;

    Cinemachine.CinemachineBrain cameraBrain;
    Timer cloudSpawnTimer;
    List<GameObject> listOfClouds;

    float spawnDelta = 30f;

    const string backgroundBehindSortingLayer = "Background Behind";
    const string backgroundInfrontSortingLayer = "Background Infront";

    void Start()
    {
        cameraBrain = GetComponentInParent<Cinemachine.CinemachineBrain>();

        cloudSpawnTimer = gameObject.AddComponent(typeof(Timer)) as Timer;
        listOfClouds = new List<GameObject>();

        InitializeClouds();

        cloudSpawnTimer.SetTimer(Random.Range(minSpawnTime, maxSpawnTime), SpawnCloudOnTimer);
    }

    // Initialization works best if the starting position of the main camera approximately matches up with the first room.
    public void InitializeClouds()
    {
        int numStartingClouds = Random.Range(minStartingClouds, maxStartingClouds);

        for (int i = 0; i < numStartingClouds; i++)
        {
            SpawnCloud(Random.Range(transform.position.x - spawnDelta, transform.position.x));
        }
    }

    void SpawnCloud(float cloudStartingX)
    {
        CloudSpawnValues cloudSpawnValues = SetCloudSpawnValues();

        var cloud = Instantiate(cloudPreFab, transform.position + new Vector3(cloudStartingX, cloudSpawnValues.cloudHeight, 0), transform.rotation);
        listOfClouds.Add(cloud);
        cloud.transform.parent = transform.parent;

        CloudController cloudController = cloud.GetComponent<CloudController>();
        SpriteRenderer cloudSpriteRenderer = cloud.GetComponent<SpriteRenderer>();
        AnimationFramePickerSystem cloudFramePicker = cloud.GetComponent<AnimationFramePickerSystem>();

        // Set remaining cloud spawn variables
        cloudController.cloudSpeed = cloudSpawnValues.cloudSpeed;
        cloudSpriteRenderer.sortingLayerName = cloudSpawnValues.isBackgroundBehind ? "Background Behind" : "Background Infront";
        cloudFramePicker.RandomSprite();
    }

    void SpawnCloudOnTimer()
    {
        SpawnCloud(-1 * spawnDelta);

        // Reset timer for next cloud spawn
        cloudSpawnTimer.SetTimer(Random.Range(minSpawnTime, maxSpawnTime), SpawnCloudOnTimer);

        // Check if clouds are out of view to be removed
        if (!cameraBrain.IsBlending)
            DespawnClouds();
    }

    CloudSpawnValues SetCloudSpawnValues()
    {
        float cloudSpeed;
        float cloudHeight;
        bool isBackgroundBehind;

        float cloudSpeedThreshold = (minCloudSpeed + maxCloudSpeed) / 2f;

        cloudSpeed = Random.Range(minCloudSpeed, maxCloudSpeed);
        cloudHeight = transform.position.y + Mathf.Lerp(minCloudHeight, maxCloudHeight, Mathf.InverseLerp(minCloudSpeed, maxCloudSpeed, cloudSpeed));
        isBackgroundBehind = (cloudSpeed > cloudSpeedThreshold);

        return new CloudSpawnValues(cloudSpeed, cloudHeight, isBackgroundBehind);
    }

    void DespawnClouds()
    {
        for (int i = listOfClouds.Count - 1; i >=0; i--)
        {
            if (listOfClouds[i].transform.position.x > transform.position.x + spawnDelta)
            {
                Destroy(listOfClouds[i]);
                listOfClouds.RemoveAt(i);
            }
        }
    }

    void DespawnAllClouds()
    {
        for (int i = listOfClouds.Count - 1; i >= 0; i--)
        {
            Destroy(listOfClouds[i]);
            listOfClouds.RemoveAt(i);
        }
    }

    // Draw waypoints for testing
    void OnDrawGizmos()
    {
        Vector3 startPos, endPos;

        Gizmos.color = Color.red;

        // Min Clound Line
        startPos = new Vector3(-10, minCloudHeight, 0) + transform.position;
        endPos = new Vector3(10, minCloudHeight, 0) + transform.position;
        Gizmos.DrawLine(startPos, endPos);

        // Max Cloud Line
        startPos = new Vector3(-10, maxCloudHeight, 0) + transform.position;
        endPos = new Vector3(10, maxCloudHeight, 0) + transform.position;
        Gizmos.DrawLine(startPos, endPos);
    }

    struct CloudSpawnValues
    {
        public float cloudSpeed;
        public float cloudHeight;
        public bool isBackgroundBehind;

        public CloudSpawnValues(float _cloudSpeed, float _cloudHeight, bool _isBackgroundBehind)
        {
            cloudSpeed = _cloudSpeed;
            cloudHeight = _cloudHeight;
            isBackgroundBehind = _isBackgroundBehind;
        }
    }
}
