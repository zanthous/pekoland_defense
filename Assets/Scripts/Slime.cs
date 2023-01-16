using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Slime : Enemy
{
    private enum slimeState 
    { 
        jumpCooldown,
        crawl,
        idle,
        jump
    }

    private Controller2D controller;
    private bool FacingRight = false;
    private GameObject player;
    private float crawlTime = 1f;
    private float idleTime = 0.8f;
    private float jumpCooldownTime = 0.5f;
    private float timer = 0.0f;

    private bool jumped = false;

    private slimeState state = slimeState.jump;

    private float crawlMovespeed = 0.4f;
    private float jumpMoveSpeed = 4.5f;

    private float timeToJumpApex = 0.6f;
    private float jumpHeight = 4.0f;

    private float gravity = -20.0f;
    private Vector2 velocity;
    private float jumpVelocity;

    private BoxCollider2D collider;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();
        player = FindObjectOfType<Player>().gameObject;
        collider = GetComponent<BoxCollider2D>();

        FacingRight = player.transform.position.x > transform.position.x;
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void FixedUpdate()
    {
        switch(state)
        {
            case slimeState.jumpCooldown:
                timer += Time.fixedDeltaTime;
                if(timer > jumpCooldownTime)
                {
                    timer = 0.0f;
                    state = slimeState.crawl;
                }
                break;
            case slimeState.crawl:
                velocity = (FacingRight ? Vector2.right : Vector2.left) * crawlMovespeed * Time.fixedDeltaTime;
                timer += Time.fixedDeltaTime;
                if(timer > crawlTime)
                {
                    timer = 0.0f;
                    state = slimeState.idle;
                }
                break;
            case slimeState.idle: //using idle to make it look like it's charging a jump for a brief time. Optionally a different animation would play
                timer += Time.fixedDeltaTime;
                if(timer > idleTime)
                {
                    timer = 0.0f;
                    state = slimeState.jump;
                }
                break;
            case slimeState.jump:
                if(controller.collisions.above || controller.collisions.below)
                {
                    velocity.y = 0;
                }
                if(jumped && controller.collisions.below)
                {
                    state = slimeState.jumpCooldown;
                    jumped = false;
                    break;
                }
                else if(controller.collisions.below && !jumped)
                {
                    velocity.y = jumpVelocity;
                    jumped = true;
                }
                velocity.x = (FacingRight ? 1 : -1) * jumpMoveSpeed;
                break;
        }
        velocity.y += gravity * Time.fixedDeltaTime;
        controller.Move(velocity * Time.fixedDeltaTime);

        DespawnCheck();
    }

    private void DespawnCheck()
    {
        float halfWidth = collider.bounds.size.x / 2.0f;
        float cameraX = Camera.x;

        if((transform.position.x < cameraX - (Camera.width / 2.0f) - halfWidth) && !FacingRight)
        {
            //despawn if off left side of screen and heading left
            //Drop something if necessary here
            Destroy(gameObject);
        }
        else if(transform.position.x > cameraX + (Camera.width / 2.0f) + halfWidth && FacingRight) 
        {
            Destroy(gameObject);
        }
    }
}