using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Health health;

    private void Awake()
    {
        TryGetComponent(out health);
    }
    

    public virtual void OnHit(int damage)
    {
        if (health)
        {
            health.ReduceHP(damage);
        }
    }
}
