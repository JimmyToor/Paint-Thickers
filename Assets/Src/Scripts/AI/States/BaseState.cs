namespace Src.Scripts.AI.States
{
    public abstract class BaseState<T> where T : StateMachine<T>
    {
        protected T StateMachine;
        public BaseState<T> CurrentSuperState { get; private set; }
        public BaseState<T> CurrentSubState { get; private set; }

        protected BaseState(T stateMachine)
        {
            StateMachine = stateMachine;
        }
        
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

        /// <summary>
        /// Call Execute on each state in a top-down fashion.
        /// </summary>
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

        /// <summary>
        /// Exit the current substate and enter a new one. Also exits all subsequent sub-states.
        /// </summary>
        /// <param name="newSubState"></param>
        public void SetSubState(BaseState<T> newSubState)
        {
            CurrentSubState?.Exit();
            CurrentSubState = newSubState;
            CurrentSubState.SetSuperState(this);
            CurrentSubState.Enter();
        }

        /// <summary>
        /// Switch the current state to newState while maintaining the same parent state.
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
        /// Searches downwards in the state machine hierarchy for the first state with the passed StateId.
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns>A reference to the state if found, null otherwise.</returns>
        public BaseState<T> GetDescendantState(StateId stateId)
        {
            if (GetId() == stateId)
            {
                return this;
            }

            return CurrentSubState?.GetDescendantState(stateId);
        }

        /// <summary>
        /// Searches upwards in the state machine hierarchy for the first state with the passed StateId.
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns>A reference to the state if found, null otherwise.</returns>
        public BaseState<T> GetAncestorState(StateId stateId)
        {
            if (CurrentSuperState?.GetId() == stateId)
            {
                return CurrentSuperState;
            }
            return CurrentSuperState?.GetAncestorState(stateId);
        }
    }
}