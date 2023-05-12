using System;
using System.Collections.Generic;
using Src.Scripts.AI.States;
using UnityEngine;

namespace Src.Scripts.AI
{
    [Serializable]
    public class DelugerStateMachine : StateMachine<DelugerStateMachine>
    {
        [HideInInspector] public Deluger deluger;
        
        public DelugerStateMachine(Deluger deluger, IEnumerable<StateId> stateIds)
        {
            this.deluger = deluger;
            
            States = new BaseState<DelugerStateMachine>[Enum.GetNames(typeof(StateId)).Length];

            foreach (var stateId in stateIds)
            {
                States[(int)stateId] = DelugerStateFactory.GetState(stateId, this);
            }
        }
        
    }
}