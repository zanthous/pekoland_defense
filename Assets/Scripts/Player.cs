using System;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public enum AttackOrigin
{
    Right,
    Left,
    LowRight,
    LowLeft,
    Up,
    Down
}

//[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(PlayerAnimationController))]

public class Player : MonoBehaviour
{
    public static Action<WeaponInfo> WeaponChangedEvent;
    public static Action<AttackOrigin> AttackEvent;
    public static Action<int> ChangeHealth;
    public static Action PlayerDeath;

    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private WeaponInfo currentWeaponInfo;
    [SerializeField] private GameObject daggerPrefab;
    [SerializeField] private LayerMask terrainMask;
    
    //projectile spawn locations
    [SerializeField] private GameObject downSpawn;
    [SerializeField] private GameObject upSpawn;
    [SerializeField] private GameObject leftSpawn;
    [SerializeField] private GameObject lowLeftSpawn;
    [SerializeField] private GameObject rightSpawn;
    [SerializeField] private GameObject lowRightSpawn;

    //private CharacterController2D controller;
    private Controller2D controller;
    private PlayerAnimationController animationController;
    
    private float horizontalMove = 0.0f;
    private float verticalMove = 0.0f;

    private bool jump = false;

    [SerializeField] private float timeToJumpApex = .4f;
    [SerializeField] private float jumpHeight = 3.5f;

    private float jumpVelocity;

    private float attackTimer = float.MaxValue;
    private float aimThreshold = 0.5f; //How far in the y direction should you have to press to aim down/up?

    private float lastJumpPressTime = 0.0f;
    private bool bufferJump = false;
    private float jumpBufferTime = 0.1f;

    private float gravity = -20.0f;
    private Vector2 velocity;
    new Rigidbody2D rigidbody;
    float crouchThreshold = -.5f;

    public Vector2 Velocity
    {
        get { return velocity; }
    }
    
    private int health = 1;
    private Transform currentMovingPlatform;
    private const int MaxHealth = 3;

    private Vector2 tookDamageDirection; //for now only doing left or right but might do more directions later
    [SerializeField] private AnimationCurve hitStunVelocityCurve;
    private float tookDamageTimer = 0;
    private float hitStunDuration = 0.5f;
    private float invincibilityTime = 2.0f;
    private bool invincible = false;
    private bool hitStunned = false;

    private AttackData lastAttackData;
    private float damageColorCycleSpeed = 12.0f;
    private Material material;

    private LayerMask defaultMask;
    [SerializeField] private LayerMask invincibilityMask; //What to collide with while invincible after taking damage

    // Start is called before the first frame update
    void Start()
    {
        //controller = GetComponent<CharacterController2D>();
        animationController = GetComponent<PlayerAnimationController>();
        controller = GetComponent<Controller2D>();
        material = GetComponent<SpriteRenderer>().material;
        rigidbody = GetComponent<Rigidbody2D>();

        AttackEvent += OnAttack;
        ChangeHealth += OnChangeHealth; 

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        defaultMask = controller.collisionMask;
    }

    //probably entirely unecessary
    private void OnDestroy()
    {
        ChangeHealth -= OnChangeHealth;
    }

    private void Update()
    {
      
        //if(Input.GetKey(KeyCode.S))
        //{
        //    controller.Crouch = true;
        //}
        //else
        //{
        //    controller.Crouch = false;
        //}
    }

    void FixedUpdate()
    {
        if(controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        if(tookDamageTimer > 0)
        {
            material.SetFloat("_FlashAmount", Mathf.Abs(Mathf.Cos((invincibilityTime - tookDamageTimer) * damageColorCycleSpeed)));
        }
        if(invincibilityTime - tookDamageTimer > hitStunDuration)
        {
            //todo clean too tired
            animationController.HitStun = false;
            controller.HitStunned = false;
            hitStunned = false;

            if(tookDamageTimer <= 0 && invincible == true)
            {
                invincible = false;
                material.SetFloat("_FlashAmount", 0);
                controller.collisionMask = defaultMask;
            }
        }

        if(hitStunned)
        {
            horizontalMove = 0.0f;
            verticalMove = 0.0f;
        }

        if((bufferJump && controller.collisions.below)) //Jump queued and grounded? Jump
        {
            velocity.y = jumpVelocity;
            bufferJump = false;
            animationController.Jumping = true;
        }
        else if(controller.collisions.below) //Land
        {
            animationController.Jumping = false;
        }

        velocity.y += gravity * Time.fixedDeltaTime;

        if(tookDamageTimer > invincibilityTime - hitStunDuration) //if hitstunned
        {
            velocity.x = hitStunVelocityCurve.Evaluate((hitStunVelocityCurve.length / hitStunDuration) //scale to duration of the curve (it's 1 second duration)
                * (invincibilityTime - tookDamageTimer)) //time elapsed since hit)
                * lastAttackData.KnockbackSpeed 
                * -tookDamageDirection.x; 
        }
        else
        {
            velocity.x = horizontalMove * moveSpeed;
        }
        
        attackTimer += Time.fixedDeltaTime;
        tookDamageTimer -= Time.fixedDeltaTime;
        
        controller.Move(velocity * Time.fixedDeltaTime);

        animationController.Speed = Mathf.Abs(horizontalMove);

        if(Time.time > lastJumpPressTime + jumpBufferTime)
        {
            bufferJump = false;
        }
    }

    public void Move(CallbackContext context)
	{
        horizontalMove = context.ReadValue<Vector2>().x;
        verticalMove = context.ReadValue<Vector2>().y;
        //use aimthreshold so the player can either move or throw a dagger upward
        //but not both
        if(Mathf.Abs(horizontalMove) > aimThreshold) 
		{
            horizontalMove = Mathf.Sign(horizontalMove);
		}
        else
		{
            horizontalMove = 0.0f;
		}

        //less than aimtrheshold horizontal move means that the player is not moving
        //this is to prevent walking while crouching
        if(!controller.Crouch && controller.collisions.below && verticalMove < crouchThreshold && horizontalMove <= aimThreshold)
		{
            controller.Crouch = true;
            transform.localScale = new Vector3(1,.5f);
            transform.position = new Vector2(rigidbody.position.x, rigidbody.position.y - .5f);
            //rigidbody.MovePosition(new Vector2(rigidbody.position.x, rigidbody.position.y - .5f));
            
		}
        else if (controller.Crouch && verticalMove > crouchThreshold)
		{
            controller.Crouch = false;
            transform.localScale = new Vector3(1, 1);
            transform.position = new Vector2(rigidbody.position.x, rigidbody.position.y + .5f);
            //rigidbody.MovePosition(new Vector2(rigidbody.position.x, rigidbody.position.y + .5f));
        }
    }

    public void Jump(CallbackContext context)
	{
        bufferJump = true;
        lastJumpPressTime = Time.time;
    }

    public void Attack(CallbackContext context)
	{
        if(!context.started) return;
        if(attackTimer > currentWeaponInfo.attackCooldown)
		{
            attackTimer = 0.0f;

            AttackOrigin origin = AttackOrigin.Left;

            if(controller.collisions.below)
            {
                if(controller.Crouch)
                {
                    if(controller.FacingRight)
                    {
                        origin = AttackOrigin.LowRight;
                    }
                    else
                    {
                        origin = AttackOrigin.LowLeft;
                    }
                }
                else //not crouching
                {
                    //not moving and aiming up
                    if(horizontalMove == 0.0f && verticalMove > aimThreshold)
                    {
                        origin = AttackOrigin.Up;
                    }
                    else if(controller.FacingRight)
                    {
                        origin = AttackOrigin.Right;
                    }
                    else
                    {
                        origin = AttackOrigin.Left;
                    }
                }
            }
            else //in air
            {
                if(verticalMove > aimThreshold)
                {
                    origin = AttackOrigin.Up;
                }
                else if(verticalMove < -aimThreshold)
                {
                    origin = AttackOrigin.Down;
                }
                else
                {
                    origin = controller.FacingRight ? AttackOrigin.Right : AttackOrigin.Left;
                }
            }

            AttackEvent?.Invoke(origin);
        }
	}

    private void OnWeaponChanged(WeaponInfo weaponInfo)
    {
        currentWeaponInfo = weaponInfo;
    }

    private void OnAttack(AttackOrigin origin)
    {
        switch(currentWeaponInfo.weaponName)
        {
            case WeaponName.dagger:
                SpawnProjectile(origin,daggerPrefab);
                break;
        }
    }

    private void SpawnProjectile(AttackOrigin origin, GameObject prefab)
    {
        var projectile = Instantiate(prefab);

        switch(origin)
        {
            case AttackOrigin.Right:
                projectile.transform.position = rightSpawn.transform.position;
                projectile.transform.rotation = rightSpawn.transform.rotation;
                break;
            case AttackOrigin.Left:
                projectile.transform.position = leftSpawn.transform.position;
                projectile.transform.rotation = leftSpawn.transform.rotation;
                break;
            case AttackOrigin.LowRight:
                projectile.transform.position = lowRightSpawn.transform.position;
                projectile.transform.rotation = lowRightSpawn.transform.rotation;
                break;
            case AttackOrigin.LowLeft:
                projectile.transform.position = lowLeftSpawn.transform.position;
                projectile.transform.rotation = lowLeftSpawn.transform.rotation;
                break;
            case AttackOrigin.Up:
                projectile.transform.position = upSpawn.transform.position;
                projectile.transform.rotation = upSpawn.transform.rotation;
                break;
            case AttackOrigin.Down:
                projectile.transform.position = downSpawn.transform.position;
                projectile.transform.rotation = downSpawn.transform.rotation;
                break;
        }

        projectile.GetComponent<BoxCollider2D>().enabled = true;
    }
    
    private void OnChangeHealth(int amount)
    {
        health += amount;
        health = health > MaxHealth ? MaxHealth : health;
        if(health < 0)
        {
            PlayerDeath?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if((collider.transform.CompareTag("Enemy") || collider.transform.CompareTag("EnemyAttack")) && tookDamageTimer <= 0)
        {
            animationController.HitStun = true;
            invincible = true;
            controller.HitStunned = true;
            controller.collisionMask = invincibilityMask;
            lastAttackData = collider.transform.GetComponent<AttackData>();
            tookDamageTimer = invincibilityTime;
            if(collider.transform.position.x > transform.position.x)
            {
                tookDamageDirection = Vector2.right;
            }
            else
            {
                tookDamageDirection = Vector2.left;
            }

            ClearBufferedActions();
        }
    }

    void OnCollisionEnter2D(Collision2D collider)
    {
        if(collider.transform.CompareTag("MovingSurfaceCollider"))
        {
            currentMovingPlatform = collider.gameObject.transform;
            transform.SetParent(currentMovingPlatform);
        }
    }

    private void ClearBufferedActions()
    {
        bufferJump = false;
    }

    void OnCollisionExit2D(Collision2D collider)
    {
        if(collider.transform.CompareTag("MovingSurfaceCollider"))
        {
            currentMovingPlatform = null;
            transform.parent = null;
        } 
    }
}
