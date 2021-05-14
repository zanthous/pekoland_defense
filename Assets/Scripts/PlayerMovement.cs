using System;
using UnityEngine;

public enum AttackOrigin
{
    Right,
    Left,
    LowRight,
    LowLeft,
    Up,
    Down
}

//I want to rename this to something else
public class PlayerMovement : MonoBehaviour
{
    public static Action<WeaponInfo> WeaponChangedEvent;
    public static Action<AttackOrigin> AttackEvent;
    public static Action<int> ChangeHealth;
    public static Action PlayerDeath;

    [SerializeField] private float moveSpeed = 40.0f;
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

    private CharacterController2D controller;
    private PlayerAnimationController animationController;
    private CircleCollider2D feetCollider;
    private Rigidbody2D rigidbody;
    
    private float horizontalMove = 0.0f;
    private float verticalMove = 0.0f;
    private bool jump = false;
    private float attackTimer = float.MaxValue;
    private float aimThreshold = 0.1f; //How far in the y direction should you have to press to aim down/up?
    private Vector2 gravity;
    private float gravityScale = 3.0f;
    
    private int health = 1;
    private Transform currentMovingPlatform;
    private const int MaxHealth = 3;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController2D>();
        animationController = GetComponent<PlayerAnimationController>();
        feetCollider = GetComponent<CircleCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        
        AttackEvent += OnAttack;
        ChangeHealth += OnChangeHealth;
        
        //manually apply gravity force
        rigidbody.gravityScale = 0.0f;
    }

    //probably entirely unecessary
    private void OnDestroy()
    {
        ChangeHealth -= OnChangeHealth;
    }

    void Update()
    {
        attackTimer += Time.deltaTime;

        horizontalMove = Input.GetAxisRaw("Horizontal");
        verticalMove = Input.GetAxisRaw("Vertical");

        jump = Input.GetKey(KeyCode.Space);

        HandleAttack();

        animationController.Speed = Mathf.Abs(horizontalMove);

        if(jump || Mathf.Abs(horizontalMove) > 0.001f)
        {
            transform.parent = null;
        }

        if(jump)
        {
            animationController.Jumping = true;
        }
    }

    private void HandleAttack()
    {
        if(attackTimer > currentWeaponInfo.attackCooldown && Input.GetButtonDown("Fire1"))
        {
            attackTimer = 0.0f;

            AttackOrigin origin = AttackOrigin.Left;

            if(controller.Grounded)
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

    // Update is called once per frame
    void FixedUpdate()
    {
        AddGravity();
        controller.Move(horizontalMove * Time.fixedDeltaTime * moveSpeed, jump);
    }

    //https://gamedev.stackexchange.com/questions/146738/how-to-disable-gravity-for-a-rigidbody-moving-on-a-slope/169102#169102
    private void AddGravity()
    {
        if(controller.Grounded)
        {
            var hit = Physics2D.Raycast(transform.position, Vector2.down, -.4f, terrainMask);
            gravity = -hit.normal * gravityScale;
        }
        else
        {
            gravity = Physics2D.gravity * gravityScale;
        }
        rigidbody.AddForce(gravity, ForceMode2D.Force);
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

    void OnCollisionStay2D(Collision2D collider)
    {
        if(Mathf.Abs(horizontalMove) < 0.001f &&  collider.transform.CompareTag("MovingSurfaceCollider"))
        {
            currentMovingPlatform = collider.gameObject.transform;
            transform.SetParent(currentMovingPlatform);
        }
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
