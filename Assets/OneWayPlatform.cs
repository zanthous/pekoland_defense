using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    private GameObject player;
    private Vector2 playerBottom;

    private void Start()
    {
        player = FindObjectOfType<PlayerMovement>().gameObject;
        var feetCollider = player.GetComponent<CircleCollider2D>();
        playerBottom = (Vector2)player.transform.position -  feetCollider.offset;
        playerBottom.y -= feetCollider.radius;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
