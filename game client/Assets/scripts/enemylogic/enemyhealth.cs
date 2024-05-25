using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyhealth : MonoBehaviour
{
    public int id;
    public bool active = true;
    public Type type;

    public float currenthealth;

    void ToggleActive()
    {
        active = !active;
    }

    public void TakeDamage(float damage, int AP, int STP)
    {
        if(active)
        {
            damage = damage * MathF.Min((5f - ((float)type.armor - (float)AP)) * .2f, 1f);

            currenthealth -= damage;

            if (currenthealth <= 0)
            {
                Die();
            }
        }
    }
    public virtual void Die()
    {
        transform.position = new Vector3(0f, 200f, 0f);

        active = false;
    }
}
