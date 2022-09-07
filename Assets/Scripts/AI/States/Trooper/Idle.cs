namespace AI.States.Trooper
{
    public class Idle : BaseState<TrooperStateMachine>
    {
        private TargetScanner _scanner;

        public Idle(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _scanner = StateMachine.trooper.GetComponent<TargetScanner>();
        }
        
        public override StateId GetId() => StateId.Idle;
        
        public override void Execute()
        {
            if (_scanner.hasTarget)
            {
                SwitchState(StateId.TargetSighted);
            }
        }
        
        public override void InitializeSubState()
        {
            
        }
        
        
    }
}