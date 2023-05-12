namespace Src.Scripts.AI.States.Trooper
{
    public class Wander : BaseState<TrooperStateMachine>
    {
        private WanderBehaviour _wanderBehaviour;
        private TargetScanner _scanner;

        public Wander(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _wanderBehaviour = StateMachine.trooper.GetComponent<WanderBehaviour>();
            _scanner = StateMachine.trooper.GetComponent<TargetScanner>();
        }
        
        public override StateId GetId()
        {
            return StateId.Wander;
        }

        public override void Enter()
        {
            base.Enter();
            _wanderBehaviour.StartWander();
        }

        public override void Execute()
        {
            _wanderBehaviour.WanderUpdate();
            if (!_wanderBehaviour.wanderQueued && _wanderBehaviour.IsWanderDone())
            {
                _wanderBehaviour.StartWanderAfterDelay();
            }
            if (_scanner.hasTarget)
            {
                SwitchState(StateId.TargetSighted);
            }
        }

        public override void Exit()
        {
            base.Exit();
            _wanderBehaviour.StopWander();
        }

        public override void InitializeSubState()
        {
            
        }
    }
}
