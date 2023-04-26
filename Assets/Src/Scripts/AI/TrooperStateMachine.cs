using System;
using System.Collections.Generic;
using AI.States;
using UnityEngine;

namespace AI
{
    [Serializable]
    public class TrooperStateMachine : StateMachine<TrooperStateMachine>
    {
        [HideInInspector] public AutoTrooper trooper;
        
        public TrooperStateMachine(AutoTrooper trooper, IEnumerable<StateId> stateIds)
        {
            this.trooper = trooper;
            
            States = new BaseState<TrooperStateMachine>[Enum.GetNames(typeof(StateId)).Length];

            foreach (var stateId in stateIds)
            {
                States[(int)stateId] = TrooperStateFactory.GetState(stateId, this);
            }
        }
        
    }
}