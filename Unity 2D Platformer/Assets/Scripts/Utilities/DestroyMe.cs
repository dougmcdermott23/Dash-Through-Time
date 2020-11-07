using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMe : MonoBehaviour
{
    public float timeToDestroy = 0.5f;

    private void Awake()
    {
        Destroy(gameObject, timeToDestroy);
    }
}
