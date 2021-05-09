using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//To decide which animation to play
public enum WeaponType
{
    melee,
    throwable
}

public enum WeaponName
{
    dagger
}

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon", order = 1)]
public class WeaponInfo : ScriptableObject
{
    public float attackCooldown;
    public WeaponType weaponType;              //in case different animations should play
    public WeaponName weaponName;              
    public int damage;
}
