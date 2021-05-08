using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static Action<int> ChangeHealth;
    public static Action PlayerDeath;

    [SerializeField] private float moveSpeed = 40.0f;

    private Animator animator; 
    private CharacterController2D controller;

    private float horizontalMove = 0.0f;
    private bool jump = false;
    
    private int health = 1;
    private const int MaxHealth = 3;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController2D>();
        animator = GetComponent<Animator>();
        CharacterController2D.OnLandEvent += OnLand;
        ChangeHealth += OnChangeHealth;
    }

    //probably entirely unecessary
    private void OnDestroy()
    {
        CharacterController2D.OnLandEvent -= OnLand;
        ChangeHealth -= OnChangeHealth;
    }

    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal");

        jump = Input.GetKey(KeyCode.Space);

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if(jump)
        {
            animator.SetBool("Jumping", true);
        }
    }

    private void OnLand()
    {
        animator.SetBool("Jumping", false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime * moveSpeed, jump);
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
}
