using AI.States;
using AI.States.Trooper;

namespace AI
{
    public static class TrooperStateFactory
    {
        public static BaseState<TrooperStateMachine> GetState(StateId id, TrooperStateMachine trooperStateMachine)
        {
            switch (id)
            {
                case StateId.Idle:
                    return new Idle(trooperStateMachine);
                case StateId.Attacking:
                    return new Attacking(trooperStateMachine);
                case StateId.Wander:
                    return new States.Trooper.Wander(trooperStateMachine);
                case StateId.Sunk:
                    return new Sunk(trooperStateMachine);
                case StateId.Standing:
                    return new Standing(trooperStateMachine);
                case StateId.SunkStruggle:
                    return new SunkStruggle(trooperStateMachine);
                case StateId.SunkEscapePaint:
                    return new SunkEscapePaint(trooperStateMachine);
                case StateId.Scanning:
                    return new Scanning(trooperStateMachine);
                case StateId.TargetLost:
                    return new TargetLost(trooperStateMachine);
                case StateId.TargetSighted:
                    return new TargetSighted(trooperStateMachine);
                default:
                    return null;
            }
        }
    }
}