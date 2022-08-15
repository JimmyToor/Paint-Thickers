using UnityEngine;

namespace AI.States
{
    public abstract class BaseState<T> where T : StateMachine<T>
    {
        protected T StateMachine;
        public BaseState<T> CurrentSuperState { get; private set; }
        public BaseState<T> CurrentSubState { get; private set; }
        
        public abstract StateId GetId();

        public virtual void Enter()
        {
            InitializeSubState();
        }
        
        public abstract void Execute();

        public virtual void Exit()
        {
            CurrentSubState?.Exit();
        }

        public void UpdateStates()
        {
            Execute();
            
            if (CurrentSuperState != null)
            {   // Check if the state was switched during execution
                if (CurrentSuperState.CurrentSubState.GetId() != GetId())
                {
                    // If this state was switched during execution, the new state will be the new sub-state of the super-state
                    CurrentSuperState?.CurrentSubState?.UpdateStates();
                }
                else
                {
                    CurrentSubState?.UpdateStates();
                }
            }
            else
            {
                CurrentSubState?.UpdateStates();
            }
        }

        public abstract void InitializeSubState();

        public void SetSuperState(BaseState<T> newSuperState)
        {            
            CurrentSuperState = newSuperState;
        }

        public void SetSubState(BaseState<T> newSubState)
        {
            CurrentSubState?.Exit();
            CurrentSubState = newSubState;
            CurrentSubState.SetSuperState(this);
            CurrentSubState.Enter();
        }

        /// <summary>
        /// Switch from the current state to newState while maintaining the parent state
        /// </summary>
        /// <param name="newState"></param>
        public void SwitchState(StateId newState)
        {
            if (CurrentSuperState == null)
            {   // this is the root state
                StateMachine.SetRootState(newState);
            }
            else
            {
                CurrentSuperState.SetSubState(StateMachine.GetState(newState));
            }
        }
        
        /// <summary>
        /// Returns a reference to the state with stateId if it is currently in the state machine hierarchy.
        ///  Returns null otherwise.
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public BaseState<T> GetDescendantState(StateId stateId)
        {
            if (GetId() == stateId)
            {
                return this;
            }

            return CurrentSubState?.GetDescendantState(stateId);
        }
    }
}