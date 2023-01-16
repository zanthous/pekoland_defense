using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://www.youtube.com/watch?v=nBkiSJ5z-hE
//decided to go the code route for simplicity
public class PlayerAnimationController : MonoBehaviour, IAnimationController
{
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

    //These animations should transition seamlessly, continuing from where the previous left off
    private Dictionary<int, List<int>> smoothTransitions = new();
    private Animator animator;
    private WeaponInfo currentWeaponInfo;
    private Coroutine stopAttackCoroutine;
    private Player playerMovement;
    private AttackOrigin attackOrigin;
    private bool attackStarting = false;
    private float lastAttack = 0.0f;
    private int currentState;
    //If jumping right transitions to jumping left, I want to continue the animation where it left off
    //This is probably not necessary if aircontrol is off, but I am leaving the option open
    private float lastTime = 0;

    public float Speed { get; set; }
    public bool FacingRight { get; set; } = true;
    public bool Jumping { get; set; } = false;
    public bool Attacking { get; set; } = false;
    public bool HitStun { get; set; } = false;
    public bool hitDirection { get; set; } 


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<Player>();

        smoothTransitions.Add(JumpLeft, new List<int>() { JumpRight });
        smoothTransitions.Add(JumpRight, new List<int>() { JumpLeft });

        smoothTransitions.Add(AirAttackLeft, new List<int>() { AirAttackRight, AttackLeft });
        smoothTransitions.Add(AirAttackRight, new List<int>() { AirAttackLeft, AttackRight });

        smoothTransitions.Add(AttackRight, new List<int>() { AirAttackRight, AttackLeft });
        smoothTransitions.Add(AttackLeft, new List<int>() { AirAttackLeft, AttackRight });

        Player.AttackEvent += OnAttack;
        Player.WeaponChangedEvent += OnWeaponChanged;
    }

    private void OnWeaponChanged(WeaponInfo weaponInfo)
    {
        currentWeaponInfo = weaponInfo;
    }

    private void OnAttack(AttackOrigin origin)
    {
        attackStarting = true;
        Attacking = true;
        attackOrigin = origin;
    }

    void Update()
    {
        if(HitStun)
        {
            //im reusing this as the hurt animation for now
            if(FacingRight)
            {
                ChangeAnimationState(JumpRight, 0.5f);
            }
            else //facing left
            {
                ChangeAnimationState(JumpLeft, 0.5f);
            }
        }
        else
        { 
            if(Speed > 0.1f && !Jumping) //on ground running
            {
                if(Attacking)
                {
                    if(FacingRight)
                    {
                        ChangeAnimationState(AttackRight);
                    }
                    else //facing left
                    {
                        ChangeAnimationState(AttackLeft);
                    }
                }
                else
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
            }
            else if(Jumping) //in air animations
            { //todo check velocity
                if(Attacking)
                {
                    if(FacingRight)
                    {
                        ChangeAnimationState(AirAttackRight);
                    }
                    else //facing left
                    {
                        ChangeAnimationState(AirAttackLeft);
                    }
                }
                else //not attacking
                {
                    //TODO replace
                    //start at falling animation if character is falling (halfway through jump)
                    float delay = playerMovement.Velocity.y < 0.0f ? 0.5f : 0.0f;
                    if(FacingRight)
                    {
                        ChangeAnimationState(JumpRight, delay);
                    }
                    else //facing left
                    {
                        ChangeAnimationState(JumpLeft, delay);
                    }
                }
            }
            else //not jumping, and not running on the ground
        {
            if(Attacking)
            {
                if(FacingRight)
                {
                    ChangeAnimationState(AttackRight);
                }
                else //facing left
                {
                    ChangeAnimationState(AttackLeft);
                }
            }
            else //not attacking
            {
                if(FacingRight)
                {
                    ChangeAnimationState(IdleRight);
                }
                else //facing left
                {
                    ChangeAnimationState(IdleLeft);
                }
            }
        }
        }

        if(attackStarting) //essentially set a timer to turn off attacking when the animation finishes
        {
            attackStarting = false;
            lastAttack = Time.time;
            //unity bug causing animator stateinfo not to update instantly, have to do something weird
            if(stopAttackCoroutine == null)
            {
                stopAttackCoroutine = StartCoroutine(AttackComplete());
            }
        }
    }

    IEnumerator AttackComplete()
    {
        //delay one frame because for some reason Animator.Play does not update the state instantly in the current version..
        yield return null;
        //wait for attack animation to end
        while(Time.time < animator.GetCurrentAnimatorStateInfo(0).length + lastAttack)
        {
            yield return null;
        }
        Attacking = false;
        stopAttackCoroutine = null;
    }

    //TODO smooth transitions between states of different time lengths. Blocked due to unity bug of stateinfo
    //not updating after a new animation is played until a frame late
    //because of this is "landing" part of the attack_right and attack_left animations have been temporarily removed
    //so the animations have the same length. I submitted a bug report and will see if they respond
    private void ChangeAnimationState(int newStateHash, float delay = 0.0f)
    {
        lastTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        if(currentState == newStateHash) return;

        //Continue new animation from current time of last animation
        if(smoothTransitions.ContainsKey(currentState) && smoothTransitions[currentState].Contains(newStateHash))
        {
            animator.Play(newStateHash,0, lastTime);
        }
        else
        {
            animator.Play(newStateHash,0,delay);
        }
        currentState = newStateHash;
    }
}
