using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour
{

    Player player;

    void Start()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
        // Handle Player Movement
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(input);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.OnJumpInputDown();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            player.OnJumpInputUp();
        }

        // Handle Time Rewind
        if (Input.GetKeyDown(KeyCode.R))
        {
            player.StartRewind();
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            player.StopRewind();
        }
    }

    public void SpringJump(float maxSpringVelocity, float minSpringVelocity)
    {
        player.HandleSpringPlatform(maxSpringVelocity, minSpringVelocity);
    }
}
