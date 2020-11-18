using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    public GameObject cloudPreFab;

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

    void Start()
    {
        cameraBrain = GetComponentInParent<Cinemachine.CinemachineBrain>();

        cloudSpawnTimer = gameObject.AddComponent(typeof(Timer)) as Timer;
        listOfClouds = new List<GameObject>();

        cloudSpawnTimer.SetTimer(Random.Range(minSpawnTime, maxSpawnTime), SpawnCloud);
    }

    public void SpawnCloud()
    {
        var cloud = Instantiate(cloudPreFab, transform.position + new Vector3(-1 * spawnDelta, Random.Range(minCloudHeight, maxCloudHeight), 0), transform.rotation);

        listOfClouds.Add(cloud);

        cloud.transform.parent = transform.parent;

        CloudController cloudController = cloud.GetComponent<CloudController>();
        cloudController.cloudSpeed = new Vector3(Random.Range(minCloudSpeed, maxCloudSpeed), 0, 0);

        SpriteRenderer cloudSpriteRenderer = cloud.GetComponent<SpriteRenderer>();
        cloudSpriteRenderer.sortingLayerName = Random.Range(0f, 1f) > 0.5f ? "Background Infront" : "Background Behind";

        AnimationFramePickerSystem cloudFramePicker = cloud.GetComponent<AnimationFramePickerSystem>();
        cloudFramePicker.RandomSprite();

        cloudSpawnTimer.SetTimer(Random.Range(minSpawnTime, maxSpawnTime), SpawnCloud);

        if (!cameraBrain.IsBlending)
            DespawnClouds();
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
}
