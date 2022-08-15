using AI.States;

namespace AI
{
    public class StateMachine<T> where T : StateMachine<T>
    {
        public BaseState<T> CurrentRootState { get; protected set; }
        protected BaseState<T>[] States;
        
        public BaseState<T> GetState(StateId stateId)
        {
            return States[(int) stateId];
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