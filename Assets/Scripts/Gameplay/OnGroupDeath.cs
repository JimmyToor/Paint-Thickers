using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility;

// Do something when an enemy group dies
public class OnGroupDeath : MonoBehaviour
{
    private static EnemyManager _enemyManager;
    public int groupId;
    public UnityEvent onGroupDeath;
    
    private void Start()
    {
        _enemyManager = FindObjectOfType<EnemyManager>();
        if (onGroupDeath != null)
        {
            _enemyManager.SubscribeGroupDefeatedEvent(groupId, onGroupDeath.Invoke);
        }
    }
    
}
