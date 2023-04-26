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
                    return new States.Trooper.Idle(trooperStateMachine);
                case StateId.Attacking:
                    return new States.Trooper.Attacking(trooperStateMachine);
                case StateId.Wander:
                    return new States.Trooper.Wander(trooperStateMachine);
                case StateId.Sunk:
                    return new States.Trooper.Sunk(trooperStateMachine);
                case StateId.Standing:
                    return new States.Trooper.Standing(trooperStateMachine);
                case StateId.SunkStruggle:
                    return new States.Trooper.SunkStruggle(trooperStateMachine);
                case StateId.SunkEscapePaint:
                    return new States.Trooper.SunkEscapePaint(trooperStateMachine);
                case StateId.Scanning:
                    return new States.Trooper.Scanning(trooperStateMachine);
                case StateId.TargetLost:
                    return new States.Trooper.TargetLost(trooperStateMachine);
                case StateId.TargetSighted:
                    return new States.Trooper.TargetSighted(trooperStateMachine);
                default:
                    return null;
            }
        }
    }
}