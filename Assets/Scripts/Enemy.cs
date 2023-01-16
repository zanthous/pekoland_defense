using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private int health;
    private int maxHealth;

    //carried item

    private void ChangeHealth(int amount)
    {
        health += amount;
        health = health > maxHealth ? maxHealth : health;
        if(health < 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {

    }
}

