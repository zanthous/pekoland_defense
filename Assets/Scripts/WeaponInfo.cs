using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    melee,
    throwable
}

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon", order = 1)]
public class WeaponInfo : ScriptableObject
{
    public float attackCooldown;
    public WeaponType weaponType;              //in case different animations should play
    public int damage;
}
