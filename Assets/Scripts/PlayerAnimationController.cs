using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://www.youtube.com/watch?v=nBkiSJ5z-hE
//decided to go the code route for simplicity
public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private int currentState;

    //Use hashes to select animations faster apparently
    private static readonly int AirAttackLeft = Animator.StringToHash("Pekora_Air_Attack_Left");
    private static readonly int AirAttackRight = Animator.StringToHash("Pekora_Air_Attack_Right");

    private static readonly int AttackLeft = Animator.StringToHash("Pekora_Attack_Left");
    private static readonly int AttackRight = Animator.StringToHash("Pekora_Attack_Right");

    private static readonly int IdleLeft = Animator.StringToHash("Pekora_Idle_Left");
    private static readonly int IdleRight = Animator.StringToHash("Pekora_Idle_Right");

    private static readonly int JumpLeft = Animator.StringToHash("Pekora_Jump_Left");
    private static readonly int JumpRight = Animator.StringToHash("Pekora_Jump_Right");

    private static readonly int RunLeft = Animator.StringToHash("Pekora_Run_Left");
    private static readonly int RunRight = Animator.StringToHash("Pekora_Run_Right");

    public float Speed { get; set; }
    public bool FacingRight { get; set; } = true;
    public bool Jumping { get; set; } = false;

    //If jumping right transitions to jumping left, I want to continue the animation where it left off
    //This is probably not necessary if aircontrol is off, but I am leaving the option open
    private float lastTime = 0;

    //These animations should transition seemlessly, continuing from where the previous left off
    private Dictionary<int, int> animationPairs = new Dictionary<int, int>();

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        //I was thinking of making a bidirectional dictionary class but not bothering
        animationPairs.Add(JumpLeft, JumpRight);
        animationPairs.Add(JumpRight, JumpLeft);

        animationPairs.Add(AirAttackLeft, AirAttackRight);
        animationPairs.Add(AirAttackRight, AirAttackLeft);
    }

    void FixedUpdate()
    {
        if(Speed > 0.1f && !Jumping) 
        {
            if(FacingRight)
            {
                ChangeAnimationState(RunRight);
            }
            else
            {
                ChangeAnimationState(RunLeft);
            }
        }
        else if(Jumping)
        {
            if(FacingRight)
            {
                ChangeAnimationState(JumpRight);
            }
            else
            {
                ChangeAnimationState(JumpLeft);
            }
        }
        else //not jumping, and not running on the ground
        {
            if(FacingRight)
            {
                ChangeAnimationState(IdleRight);
            }
            else
            {
                ChangeAnimationState(IdleLeft);
            }
        }
    }

    private void ChangeAnimationState(int newStateHash)
    {
        lastTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        if(currentState == newStateHash) return;

        //Continue new animation from current time of last animation
        if( animationPairs.ContainsKey(currentState) && animationPairs[currentState] == newStateHash)
        {
            animator.Play(newStateHash,0,lastTime);
        }
        else
        {
            animator.Play(newStateHash);
        }

        currentState = newStateHash;
    }
}
