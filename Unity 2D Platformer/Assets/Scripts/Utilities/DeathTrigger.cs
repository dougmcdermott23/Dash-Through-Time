using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathTrigger : MonoBehaviour
{
	Collider2D deathCollider;

	WorldManager worldManager;

	void Start()
	{
		deathCollider = gameObject.GetComponent<Collider2D>();
		deathCollider.isTrigger = true;

		worldManager = FindObjectOfType<WorldManager>();
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			worldManager.OnPlayerDeath();
		}
	}
}
