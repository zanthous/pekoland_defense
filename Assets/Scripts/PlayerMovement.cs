using System;
using UnityEngine;

//I want to rename this to something else
public class PlayerMovement : MonoBehaviour
{
    public static Action<WeaponInfo> WeaponChanged;
    public static Action Attack;

    [SerializeField] private float moveSpeed = 40.0f;

    private CharacterController2D controller;
    private PlayerAnimationController animationController;

    private float horizontalMove = 0.0f;
    private bool jump = false;
    private float attackTimer = float.MaxValue;

    [SerializeField] private WeaponInfo currentWeaponInfo;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController2D>();
        animationController = GetComponent<PlayerAnimationController>();
    }

    void Update()
    {
        attackTimer += Time.deltaTime;

        horizontalMove = Input.GetAxisRaw("Horizontal");

        jump = Input.GetKey(KeyCode.Space);

        if(attackTimer > currentWeaponInfo.attackCooldown && Input.GetButtonDown("Fire1"))
        {
            Debug.Log("fired");
            attackTimer = 0.0f;
            Attack?.Invoke();
        }

        animationController.Speed = Mathf.Abs(horizontalMove);

        if(jump)
        {
            animationController.Jumping = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SlopeCheck();
        controller.Move(horizontalMove * Time.fixedDeltaTime * moveSpeed, jump);
    }

    private void SlopeCheck()
    {
        throw new NotImplementedException();
    }

    private void OnWeaponChanged(WeaponInfo weaponInfo)
    {
        currentWeaponInfo = weaponInfo;
    }
}
