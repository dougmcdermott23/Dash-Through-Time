using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public float cloudSpeed;

    void Update()
    {
        transform.position += new Vector3(cloudSpeed, 0, 0) * Time.deltaTime;
    }
}
