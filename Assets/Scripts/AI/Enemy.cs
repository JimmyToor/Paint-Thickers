using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private static GameManager _gameManager;
    
    protected Health health;
    
    public int groupId; // Associate this enemy with a group of enemies

    protected virtual void Start()
    {
        SetupManager();
    }
    
    private void SetupManager()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _gameManager.AddEnemy(groupId,this);
        
        TryGetComponent(out health);
        if (health)
        {
            health.onDeath.AddListener(RemoveFromManager);
        }
    }

    private void RemoveFromManager()
    {
        _gameManager.RemoveEnemy(groupId,this);
    }
}
