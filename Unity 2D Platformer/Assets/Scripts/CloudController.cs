using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public Vector3 cloudSpeed;

    void Update()
    {
        transform.position += cloudSpeed * Time.deltaTime;
    }
}
