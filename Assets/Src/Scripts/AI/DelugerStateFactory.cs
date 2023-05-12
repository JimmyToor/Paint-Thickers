using Src.Scripts.AI.States;
using Src.Scripts.AI.States.Deluger;

namespace Src.Scripts.AI
{
    public static class DelugerStateFactory
    {
        public static BaseState<DelugerStateMachine> GetState(StateId id, DelugerStateMachine delugerStateMachine)
        {
            switch (id)
            {
                case StateId.Patrol:
                    return new Patrol(delugerStateMachine);
                default:
                    return null;
            }
        }
    }
}