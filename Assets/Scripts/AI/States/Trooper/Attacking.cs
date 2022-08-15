using UnityEngine;

namespace AI.States.Trooper
{
    public class Attacking : BaseState<TrooperStateMachine>
    {
        private AutoTrooper _trooper;
        private TargetScanner _scanner;

        public Attacking(TrooperStateMachine trooperStateMachine)
        {
            StateMachine = trooperStateMachine;
            _trooper = trooperStateMachine.trooper;
            _scanner = _trooper.scanner;
        }
        
        public override StateId GetId() => StateId.Attacking;

        public override void Enter()
        {
            base.Enter();
        }

        public override void Execute()
        {
            if (_scanner.hasTarget && _scanner.Target.gameObject.activeSelf && _scanner.CheckLOS(_scanner.GetTargetPos()))
            {
                _trooper.EngageTarget();
            }
            else
            {
                SwitchState(StateId.TargetLost);
            }
        }

        public override void Exit()
        {
            base.Exit();
            
        }

        public override void InitializeSubState()
        {
            
        }
    }
}