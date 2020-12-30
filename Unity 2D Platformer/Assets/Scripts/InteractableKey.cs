using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class InteractableKey : MonoBehaviour
{
    Vector3 keyStartPosition;
    bool isTriggered;

    Player player;

    public AnimationCurve animationCurve;
    public float animationHeight;
    public float animationSpeed;
    public float moveSpeed;

    void Start()
    {
        keyStartPosition = transform.position;
        isTriggered = false;
    }

    void Update()
    {
        if (isTriggered)
        {
            int index = player.listOfKeys.FindIndex(key => key == gameObject);
            GameObject followObject;

            if (index > 0)
                followObject = player.listOfKeys[index - 1];
            else
                followObject = player.gameObject;

            transform.position = Vector3.Lerp(transform.position, followObject.transform.position + new Vector3(0, 0.5f, 0), moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, animationCurve.Evaluate((Time.time * animationSpeed % animationCurve.length)) * animationHeight + keyStartPosition.y, transform.position.z);
        }
    }

    public void OnReset()
    {
        transform.position = keyStartPosition;
        gameObject.SetActive(true);
        isTriggered = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTriggered)
        {
            player = collision.gameObject.GetComponent<Player>();
            player.listOfKeys.Add(gameObject);
            isTriggered = true;
        }
    }
}
