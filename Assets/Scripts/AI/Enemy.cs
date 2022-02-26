using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private static GameManager _gameManager;
    private Health health; // Need health to die but not to get hit
    public int groupId; // Associate this enemy with a group of enemies

    private void Start()
    {
        SetupHealth();
        SetupManager();
    }
    
    private void SetupManager()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _gameManager.AddEnemy(groupId,this);
    }

    private void SetupHealth()
    {
        TryGetComponent(out health);
        if (health)
            health.onDeath.AddListener(OnDeath);
    }

    public virtual void OnHit(int damage)
    {
        if (health)
            health.ReduceHP(damage);
    }
    
    public virtual void OnDeath()
    {
        if (health)
        {
            _gameManager.RemoveEnemy(groupId,this);
        }
        Destroy(gameObject);
    }
}
