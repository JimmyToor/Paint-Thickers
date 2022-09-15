using AI.States;

namespace AI
{
    public static class DelugerStateFactory
    {
        public static BaseState<DelugerStateMachine> GetState(StateId id, DelugerStateMachine delugerStateMachine)
        {
            switch (id)
            {
                case StateId.Patrol:
                    return new States.Deluger.Patrol(delugerStateMachine);
                default:
                    return null;
            }
        }
    }
}