using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*AttackData - info like knockback, damage etc for if the player walks into this enemy.
Projectiles have this too.I'm adding this in case attacks should sometimes do something special like freezing the player, 
it could have a bool FreezePlayer in it set to true for example. Just lets me do more in the future if I needed  
compared to every damage interaction being the same across the whole game */

public class AttackData : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private int knockbackSpeed = 4;

    public int Damage { get { return damage; } }
    public int KnockbackSpeed { get { return knockbackSpeed;  } }
}
