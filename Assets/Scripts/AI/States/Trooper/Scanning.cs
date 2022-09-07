namespace AI.States.Trooper
{
    public class Scanning : BaseState<TrooperStateMachine>
    {
        private TargetScanner _scanner;

        public Scanning(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _scanner = StateMachine.trooper.GetComponent<TargetScanner>();
        }
        
        public override StateId GetId() => StateId.Scanning;

        public override void Enter()
        {
            base.Enter();
            _scanner.searchWhenNoTarget = true;
            _scanner.StartCoroutine(_scanner.PeriodicSearch());
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
            _scanner.searchWhenNoTarget = false;
            _scanner.StopCoroutine(_scanner.PeriodicSearch());
        }

        public override void InitializeSubState()
        {
            SetSubState(StateMachine.GetState(StateId.Standing));
        }
    }
}