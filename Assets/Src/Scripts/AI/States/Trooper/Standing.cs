namespace AI.States.Trooper
{
    public class Standing : BaseState<TrooperStateMachine>
    {
        private AutoTrooper _trooper;

        public Standing(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _trooper = StateMachine.trooper;
        }
        
        public override StateId GetId() => StateId.Standing;

        public override void Execute()
        {
            if (_trooper.paintStatus == PaintStatus.EnemyPaint)
            {
                SwitchState(StateId.Sunk);
            }
        }

        public override void InitializeSubState()
        {
            if (_trooper.scanner.hasTarget)
            {
                SetSubState(StateMachine.GetState(StateId.TargetSighted));
            }
            else
            {
                SetSubState(StateMachine.GetState(StateId.Idle));
            }
            
        }
    }
}