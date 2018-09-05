using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCenterController : MonoBehaviour
{
    PlayerController pc;

    private void Start()
    {
        pc = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (  collision.gameObject.tag==
            "SpriteMask")
        {
            Debug.Log("player center collided with sprite mask");
            pc.StartBackgroundSnake();
        }
    }
}
