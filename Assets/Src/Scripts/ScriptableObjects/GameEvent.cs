using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Based on https://github.com/roboryantron/Unite2017/tree/master by Ryan Hipple
namespace Src.Scripts.ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Game Event Data", menuName = "Data Objects/Game Event")]
    public class GameEvent : ScriptableObject
    {
        // List is fine for a small number of listeners
        private readonly List<UnityEvent> _eventListeners = new List<UnityEvent>();

        public void Raise()
        {
            for(int i = _eventListeners.Count -1; i >= 0; i--)
                _eventListeners[i]?.Invoke();
        }

        public void RegisterListener(UnityEvent listener)
        {
            if (!_eventListeners.Contains(listener))
                _eventListeners.Add(listener);
            else
            {
                UnityEvent dupResponse = _eventListeners.Find(eventListener => eventListener == listener);
                Debug.Log("Duplicate listener for " + name +
                          " on " + listener +
                          " and " + dupResponse);
            }
        }

        public void UnregisterListener(UnityEvent listener)
        {
            if (_eventListeners.Contains(listener))
                _eventListeners.Remove(listener);
        }
        
        public int GetListenerCount()
        {
            return _eventListeners.Count;
        }
    }
}
