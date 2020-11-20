using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// TO DO:
/// This needs to be changed so that it will talk to the GameManager.
/// The Game Manager will hold the current room manager and send the reset command to the correct room

[RequireComponent(typeof(Collider2D))]
public class DeathTrigger : MonoBehaviour
{
	Collider2D deathCollider;

	RoomManager roomManager;

	void Start()
	{
		deathCollider = gameObject.GetComponent<Collider2D>();
		deathCollider.isTrigger = true;

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
