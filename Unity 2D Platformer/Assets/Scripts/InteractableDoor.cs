using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class InteractableDoor : MonoBehaviour
{
    public Sprite doorClosedSprite;
    public Sprite doorOpenSprite;

    public LayerMask layerMask;
    public float searchRadius = 2f;
    public float doorOpeningSpeed = 2f;
    float doorOpeningDistance;

    Player player;
    Vector3 doorStartPosition;
    SpriteRenderer spriteRenderer;
    bool isTriggered;

    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        doorStartPosition = transform.position;
        isTriggered = false;

        doorOpeningDistance = gameObject.GetComponent<BoxCollider2D>().bounds.size.y;
    }

    void Update()
    {
        Collider2D collision = Physics2D.OverlapCircle(transform.position, searchRadius, layerMask);

        if (collision != null && !isTriggered)
        {
            player = collision.gameObject.GetComponent<Player>();

            if (player.listOfKeys.Count > 0)
            {
                GameObject key = player.listOfKeys[player.listOfKeys.Count - 1];
                key.SetActive(false);
                player.listOfKeys.RemoveAt(player.listOfKeys.Count - 1);

                spriteRenderer.sprite = doorOpenSprite;
                isTriggered = true;
            }
        }
        else if (isTriggered && transform.position.y < doorStartPosition.y + doorOpeningDistance)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + doorOpeningSpeed * Time.deltaTime, transform.position.z);
        }
    }

    public void OnReset()
    {
        spriteRenderer.sprite = doorClosedSprite;
        transform.position = doorStartPosition;
        isTriggered = false;
    }
}
