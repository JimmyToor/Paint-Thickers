namespace AI.States.Trooper
{
    public class Wander : BaseState<TrooperStateMachine>
    {
        private AI.Wander _wander;
        private TargetScanner _scanner;

        public Wander(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _wander = StateMachine.trooper.GetComponent<AI.Wander>();
            _scanner = StateMachine.trooper.GetComponent<TargetScanner>();
        }
        
        public override StateId GetId()
        {
            return StateId.Wander;
        }

        public override void Enter()
        {
            base.Enter();
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
                SwitchState(StateId.TargetSighted);
            }
        }

        public override void Exit()
        {
            base.Exit();
            _wander.StopWander();
        }

        public override void InitializeSubState()
        {
            
        }
    }
}
