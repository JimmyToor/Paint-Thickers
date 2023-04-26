using UnityEngine;

namespace AI.States.Trooper
{
    public class TargetLost : BaseState<TrooperStateMachine>
    {
        private Animator _animator;
        private int stateHash;
        public TargetLost(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _animator = StateMachine.trooper.animator;
            stateHash = Animator.StringToHash("Attacking");
        }
        
        public override StateId GetId() => StateId.TargetLost;

        public override void Enter()
        {
            base.Enter();
            StateMachine.trooper.TargetLost();
            StateMachine.trooper.scanner.TargetLost();
        }

        public override void Execute()
        {
            if (StateMachine.trooper.scanner.hasTarget)
            {
                SwitchState(StateId.TargetSighted);
            }
            if (_animator.GetCurrentAnimatorStateInfo(0).tagHash != stateHash)
            {
                if (GetAncestorState(StateId.Sunk) != null)
                {
                    SwitchState(StateId.SunkStruggle);
                }
                else
                {
                    SwitchState(StateMachine.trooper.idleBehaviour);
                }
            }
        }

        public override void InitializeSubState()
        {
            
        }
    }
}