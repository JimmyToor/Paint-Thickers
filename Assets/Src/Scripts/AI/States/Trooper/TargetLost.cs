using UnityEngine;

namespace Src.Scripts.AI.States.Trooper
{
    public class TargetLost : BaseState<TrooperStateMachine>
    {
        private Animator _animator;
        private int _stateHash;
        private AutoTrooper _trooper;
        private TargetScanner _scanner;
        public TargetLost(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _animator = StateMachine.trooper.animator;
            _stateHash = Animator.StringToHash("Attacking");
            _trooper = StateMachine.trooper;
            _scanner = _trooper.scanner;
        }
        
        public override StateId GetId() => StateId.TargetLost;

        public override void Enter()
        {
            base.Enter();
            _trooper.TargetLost();
            _scanner.TargetLost();
        }

        public override void Execute()
        {
            if (_scanner.hasTarget)
            {
                SwitchState(StateId.TargetSighted);
            }

            // Let the attacking animation finish playing
            if (_animator.GetCurrentAnimatorStateInfo(0).tagHash == _stateHash) return;
            
            if (GetAncestorState(StateId.Sunk) != null)
            {
                SwitchState(StateId.SunkStruggle);
            }
            else
            {
                SwitchState(_trooper.idleBehaviour);
            }
        }

        public override void InitializeSubState()
        {
            
        }
    }
}