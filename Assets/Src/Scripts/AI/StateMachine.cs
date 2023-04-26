using System;
using AI.States;

namespace AI
{
    public class StateMachine<T> where T : StateMachine<T>
    {
        public BaseState<T> CurrentRootState { get; protected set; }
        protected BaseState<T>[] States; // Stores all possible states indexed by their enum value
        
        public BaseState<T> GetState(StateId stateId)
        {
            try
            {
                return States[(int) stateId];
            }
            catch (Exception e)
            {
                Console.WriteLine(e + " State " + stateId + " was not in the States ScriptableObject.");
                throw;
            }
        }
        
        public void SetRootState(StateId newStateId)
        {
            BaseState<T> newState = GetState(newStateId);
            CurrentRootState?.Exit();
            CurrentRootState = newState;
            CurrentRootState?.CurrentSuperState?.SetSubState(newState);
            CurrentRootState?.Enter();
        }

        public void Update()
        {
            CurrentRootState?.UpdateStates();
        }
    }
}