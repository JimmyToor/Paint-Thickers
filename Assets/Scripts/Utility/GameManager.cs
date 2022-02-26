using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<MonoBehaviour>
{
    // Pairs of (groupId, Enemies of that group) 
    public Dictionary<int, HashSet<Enemy>> enemyGroupDictionary = new Dictionary<int, HashSet<Enemy>>();

    private UnityAction[] onGroupDefeatedArray;
    
    public void SubscribeGroupDefeatedEvent(int groupId, UnityAction action)
    {
        onGroupDefeatedArray[groupId] += action;
    }
    
    public void RaiseGroupDefeatedEvent(int groupId)
    {
        onGroupDefeatedArray[groupId]();
    }

    public void AddEnemy(int groupId, Enemy enemy)
    {
        if(!enemyGroupDictionary.TryGetValue(groupId, out HashSet<Enemy> groupHashSet))
        {
            // Make a new HashSet for this id
            groupHashSet = new HashSet<Enemy>();
            enemyGroupDictionary.Add(groupId,groupHashSet);
        }
        groupHashSet.Add(enemy);
        enemyGroupDictionary.Add(groupId,groupHashSet);
    }
    
    public void RemoveEnemy(int groupId, Enemy enemy)
    {
        if(!enemyGroupDictionary.TryGetValue(groupId, out HashSet<Enemy> groupHashSet))
        {
            Debug.LogErrorFormat("{0} wants to be removed from group {1} but is not in that group!", enemy.name, groupId);
        }
        else
        {
            groupHashSet.Remove(enemy);
            if (groupHashSet.Count <= 0)
                RaiseGroupDefeatedEvent(groupId);
        }
    }
}
