using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Do something when an enemy group dies
public class OnGroupDeath : MonoBehaviour
{
    private static GameManager _gameManager;
    public int groupId;
    public UnityEvent onGroupDeath;
    
    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _gameManager.SubscribeGroupDefeatedEvent(groupId, onGroupDeath.Invoke);
    }
    
}
