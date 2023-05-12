using UnityEngine;

namespace Src.Scripts.AI.States.Trooper
{
    public class TargetSighted : BaseState<TrooperStateMachine>
    {
        private Animator _animator;
        private int stateHash;

        public TargetSighted(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _animator = StateMachine.trooper.animator;
            stateHash = Animator.StringToHash("Attacking");
        }
        
        public override StateId GetId() => StateId.TargetSighted;

        public override void Enter()
        {
            base.Enter();
            StateMachine.trooper.TargetSighted();
        }

        public override void Execute()
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).tagHash == stateHash)
            {
                SwitchState(StateId.Attacking);
            }
        }

        public override void InitializeSubState()
        {
            
        }
    }
}