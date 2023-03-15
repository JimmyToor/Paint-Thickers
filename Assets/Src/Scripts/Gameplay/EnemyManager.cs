using System.Collections.Generic;
using AI;
using Src.Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : Singleton<MonoBehaviour>
{
    // Pairs of (groupId, Enemies of that group) 
    private Dictionary<int, HashSet<Enemy>> _enemyGroupDictionary = new Dictionary<int, HashSet<Enemy>>();

    // Holds Actions to invoke when group id [index] is defeated
    private Dictionary<int, UnityEvent> _onGroupDefeatedEvents = new Dictionary<int, UnityEvent>();
    
    public void SubscribeGroupDefeatedEvent(int groupId, UnityAction newAction)
    {
        if (!_onGroupDefeatedEvents.TryGetValue(groupId, out var groupDefeatedEvent))
        {
            groupDefeatedEvent = new UnityEvent();
            _onGroupDefeatedEvents.Add(groupId,groupDefeatedEvent);
        }
        groupDefeatedEvent.AddListener(newAction);
        Debug.LogFormat("GroupDefeatedEvent for group {0} has new action.", groupId);
    }
    
    public void RaiseGroupDefeatedEvent(int groupId)
    {
        if (_onGroupDefeatedEvents.TryGetValue(groupId, out var groupDefeatedEvent))
        {
            groupDefeatedEvent.Invoke();
            Debug.LogFormat("GroupDefeatedEvent for group {0} has been raised.", groupId);
        }
        else
        {
            Debug.LogErrorFormat("Tried to raise event for defeating enemy group {0} but there is no event.", groupId);
        }
    }

    public void AddEnemy(int groupId, Enemy enemy)
    {
        if(!_enemyGroupDictionary.TryGetValue(groupId, out HashSet<Enemy> groupHashSet))
        {
            // Make a new HashSet for this id
            groupHashSet = new HashSet<Enemy>();
            _enemyGroupDictionary.Add(groupId,groupHashSet);
        }
        groupHashSet.Add(enemy);
        //Debug.LogFormat("Enemy {0} added to group {1}.",enemy.name, groupId);
    }
    
    public void RemoveEnemy(int groupId, Enemy enemy)
    {
        if(!_enemyGroupDictionary.TryGetValue(groupId, out HashSet<Enemy> groupHashSet))
        {
            Debug.LogErrorFormat("{0} wants to be removed from group {1} but is not in that group!", enemy, groupId);
        }
        else
        {
            groupHashSet.Remove(enemy);
            //Debug.LogFormat("Enemy {0} removed from group {1}.",enemy.name, groupId);
            
            if (groupHashSet.Count <= 0)
            {
                //Debug.LogFormat("Group {0} has no more enemies.", groupId);
                RaiseGroupDefeatedEvent(groupId);
            }
        }
    }

    public void EnableGroup(int groupId)
    {
        if(!_enemyGroupDictionary.TryGetValue(groupId, out HashSet<Enemy> groupHashSet))
        {
            Debug.LogErrorFormat("Tried to enable enemy group {0} but it does not exist!", groupId);
        }
        else
        {
            foreach (var enemy in groupHashSet)
            {
                enemy.gameObject.SetActive(true);
            }
        }
    }
}