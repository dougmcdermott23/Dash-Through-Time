using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DeathTrigger : MonoBehaviour
{
	BoxCollider2D boxCollider2D;

	RoomManager roomManager;

	void Start()
	{
		boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
		boxCollider2D.isTrigger = true;

		roomManager = gameObject.GetComponentInParent<RoomManager>();
	}

	// If more than one player spawn position set it to use the first in the list on player death
	// Disable the player pause on death in the player script, it isn't necessary
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			roomManager.ResetLevel(true);
		}
	}
}
