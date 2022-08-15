using UnityEngine;

namespace AI.States.Trooper
{
    public class Wander : BaseState<TrooperStateMachine>
    {
        private AI.Wander _wander;
        private TargetScanner _scanner;

        public Wander(TrooperStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            _wander = stateMachine.trooper.GetComponent<AI.Wander>();
            _scanner = stateMachine.trooper.GetComponent<TargetScanner>();
        }
        
        public override StateId GetId()
        {
            return StateId.Wander;
        }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("wander state entered");
            _wander.StartWander();
        }

        public override void Execute()
        {
            _wander.WanderUpdate();
            if (!_wander.wanderQueued && _wander.IsWanderDone())
            {
                _wander.StartWanderAfterDelay();
            }
            if (_scanner.hasTarget)
            {
                SwitchState(StateId.Attacking);
            }
        }

        public override void Exit()
        {
            base.Exit();
            Debug.Log("wander state exited");
            _wander.StopWander();
        }

        public override void InitializeSubState()
        {
            
        }
    }
}
