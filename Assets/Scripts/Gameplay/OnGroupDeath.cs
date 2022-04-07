using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility;

// Do something when an enemy group dies
public class OnGroupDeath : MonoBehaviour
{
    private static GameManager _gameManager;
    public int groupId;
    public UnityEvent onGroupDeath;
    
    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        if (onGroupDeath != null)
        {
            _gameManager.SubscribeGroupDefeatedEvent(groupId, onGroupDeath.Invoke);
        }
    }
    
}
