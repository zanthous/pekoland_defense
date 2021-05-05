using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMovements : MonoBehaviour
{
    private enum State {
        stay,
        move
    };

    [SerializeField] private Transform m_EdgeDetection;                         // detect if the edge is reached and force speed to be 0
    [SerializeField] private Transform m_GroundCheck;                         // detect if the edge is reached and force speed to be 0
    [SerializeField] private bool FaceLeft = true;                              // true if the slime is facing left
    [SerializeField] private float speed = 10f;                                // horizontal speed slime moves in
    [SerializeField] private float minCooldown;                                 // minimum amount of time slime executes the action AI selected (in ms)
    [SerializeField] private float maxCooldown;                                 // maximum amount of time slime executes the action AI selected (in ms)
    [SerializeField] private float gravityScale = 10;                            // scaling done on the gravity
    [SerializeField] private int fps = 24;                                      // frame per second of the animation
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [Range(0, .3f)] private float m_MovementSmoothing = .5f;

    private int direction;                                                      // the direction in which the slime is facing. -1 is left, 1 is right.
    private float cooldown;                                                     // cooldowns after the AI decision
    private State state;                                                        // current state
    private bool grounded;                                                      // boolean denoting whether or not the value is grounded
    private Rigidbody2D m_Rigidbody2D;                                          // rigid body used for the physics
    private float targetSpeed;
    private Vector3 m_Velocity = Vector3.zero;


    // Start is called before the first frame update. Initialization
    void Start()
    {
        this.m_Rigidbody2D = GetComponent<Rigidbody2D>();

        this.cooldown = 0f;
        this.direction = this.FaceLeft ? -1 : 1;
        if (this.direction > 0) {
            transform.eulerAngles = new Vector3(0, -180, 0);
        } else {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GroundCheck();
        Move(this.targetSpeed, false);

        // if it's still in cooldown, do not go through AI decisions
        if (this.cooldown > 0f) {
            this.cooldown -= Time.fixedDeltaTime * 1000;
            return;
        }

        // check if player is detected
        bool playerFound = false; // TODO: set actual player detection logic

        // basic AI decisions
        // 1. player is not in its sight
        if (!playerFound) {
            
            // decide on three movement randomly
            int rand = Random.Range(0, 3);
            
            switch (rand) {
                // stay still
                case 0:
                    Move(0, true);
                    this.cooldown = Random.Range(minCooldown, maxCooldown);
                    return;
                // turn around
                case 1:
                    Turn();
                    Move(0, true);
                    this.cooldown = Random.Range(minCooldown, maxCooldown);
                    return;
                // move in one direction
                case 2:
                    Move(this.speed, true);
                    this.cooldown = Random.Range(minCooldown, maxCooldown);
                    break;
            }
        }
        // 2. player is in its sight
        else {
            // if the hop towards player will make it fall, stay
            // otherwise, turn to the direction the player is, and hop
        }
    }

    // Perform move action
    void Move(float targetSpeed, bool targetUpdate) {
        // check if the edge is detected below
        // offset the x value of raycast in order to take the amount of time required to break
        // extend the ray length so that no matter the y value, it detects current platform of slime
        float offsetX = 0f;
        RaycastHit2D groundInfoEdge = Physics2D.Raycast(m_EdgeDetection.position, Vector2.down, 1f);

        if (groundInfoEdge.collider == null) {
            this.targetSpeed = 0;
        } else if (targetUpdate) {
            this.targetSpeed = targetSpeed * this.direction * Time.fixedDeltaTime;
        }

        // Move the character by finding the target velocity
        Vector3 targetVelocity = new Vector2(this.targetSpeed, m_Rigidbody2D.velocity.y);
        // And then smoothing it out and applying it to the character
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
    }

    void Turn() {
        this.direction *= -1;
        if (this.direction < 0) {
            transform.eulerAngles = new Vector3(0, 0, 0);
        } else {
            transform.eulerAngles = new Vector3(0, -180, 0);
        }
    }

    //Is this used?
    void Jump() {
        if (this.grounded) {
            // base
            float second_base = 5f / 24f;
            float second_actual = 5f / this.fps;
            float force = 400f * (second_actual / second_base) * (m_Rigidbody2D.gravityScale / 3);
            m_Rigidbody2D.AddForce(new Vector2(0f, force));
        }
    }

    void GroundCheck() {
        bool wasGrounded = this.grounded;
		this.grounded = false;

		// The object is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, .2f, m_WhatIsGround);
		for(int i = 0; i < colliders.Length; i++)
		{
			if(colliders[i].gameObject != gameObject)
			{
				this.grounded = true;
			}
		}
    }
}
