using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 40.0f;

    private Animator animator; 
    private CharacterController2D controller;

    private float horizontalMove = 0.0f;
    private bool jump = false;
    
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController2D>();
        animator = GetComponent<Animator>();
        CharacterController2D.OnLandEvent += OnLand;
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
}
