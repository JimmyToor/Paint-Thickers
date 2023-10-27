using System;
using System.Collections.Generic;
using Src.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

// Based on https://github.com/roboryantron/Unite2017/tree/master by Ryan Hipple
namespace Src.Scripts.Utility
{
    public class GameEventListener : MonoBehaviour
    {
        [Serializable]
        public class GameEventResponse
        {
            [HideInInspector]
            public string name;
            
            [Tooltip("Event to register with.")]
            public GameEvent gameEvent;

            [Tooltip("Response to invoke when Event is raised.")]
            public UnityEvent response;
        }
        
        public List<GameEventResponse> gameEventResponses;
        private void OnEnable()
        {
            foreach (var eventResponse in gameEventResponses)
            {
                eventResponse.gameEvent.RegisterListener(eventResponse.response);
            }
        }

        private void OnDisable()
        {
            foreach (var eventResponse in gameEventResponses)
            {
                eventResponse.gameEvent.UnregisterListener(eventResponse.response);
            }
        }

        private void OnValidate()
        {
            if (gameEventResponses == null) return;
            
            foreach (var response in gameEventResponses)
            {
                response.name = response.gameEvent
                    ? response.gameEvent.name
                    : "No event";
            }
        }
    }
}
