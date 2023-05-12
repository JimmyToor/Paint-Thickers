namespace Src.Scripts.AI.States.Trooper
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
            SetSubState(_trooper.scanner.hasTarget
                ? StateMachine.GetState(StateId.TargetSighted)
                : StateMachine.GetState(StateId.Idle));
        }
    }
}