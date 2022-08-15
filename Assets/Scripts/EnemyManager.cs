using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : Singleton<MonoBehaviour>
{
    // Pairs of (groupId, Enemies of that group) 
    public Dictionary<int, HashSet<Enemy>> EnemyGroupDictionary = new Dictionary<int, HashSet<Enemy>>();

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
            Debug.LogFormat("Tried to raise event for defeating enemy group {0} but there is no event.", groupId);
        }
    }

    public void AddEnemy(int groupId, Enemy enemy)
    {
        if(!EnemyGroupDictionary.TryGetValue(groupId, out HashSet<Enemy> groupHashSet))
        {
            // Make a new HashSet for this id
            groupHashSet = new HashSet<Enemy>();
            EnemyGroupDictionary.Add(groupId,groupHashSet);
        }
        groupHashSet.Add(enemy);
        //Debug.LogFormat("Enemy {0} added to group {1}.",enemy.name, groupId);
    }
    
    public void RemoveEnemy(int groupId, Enemy enemy)
    {
        if(!EnemyGroupDictionary.TryGetValue(groupId, out HashSet<Enemy> groupHashSet))
        {
            Debug.LogErrorFormat("{0} wants to be removed from group {1} but is not in that group!", enemy.name, groupId);
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
}