using UnityEngine;

namespace Src.Scripts.AI.States.Trooper
{
    public class Attacking : BaseState<TrooperStateMachine>
    {
        private AutoTrooper _trooper;
        private TargetScanner _scanner;

        public Attacking(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _trooper = trooperStateMachine.trooper;
            _scanner = _trooper.scanner;
        }
        
        public override StateId GetId() => StateId.Attacking;

        public override void Enter()
        {
            base.Enter();
            _scanner.StartCoroutine(_scanner.PeriodicLOSCheck());
        }

        public override void Execute()
        {
            if (_scanner.hasTarget && _scanner.hasLOS)
            {
                _trooper.EngageTarget();
            }
            else
            {
                SwitchState(StateId.TargetLost);
            }
        }

        public override void InitializeSubState()
        {
            
        }
    }
}