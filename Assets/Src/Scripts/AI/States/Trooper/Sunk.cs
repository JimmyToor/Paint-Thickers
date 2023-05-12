namespace Src.Scripts.AI.States.Trooper
{
    public class Sunk : BaseState<TrooperStateMachine>
    {
        private AutoTrooper _trooper;
        public Sunk(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _trooper = StateMachine.trooper;
        }

        public override StateId GetId() => StateId.Sunk;

        public override void Enter()
        {
            base.Enter();
            _trooper.Sink();
        }

        public override void Execute()
        {
            if (_trooper.paintStatus != PaintStatus.EnemyPaint)
            {
                SwitchState(StateId.Standing);
            }
        }

        public override void Exit()
        {
            base.Exit();
            _trooper.Rise();
        }

        public override void InitializeSubState()
        {
            if (_trooper.scanner.hasTarget)
            {
                SetSubState(StateMachine.GetState(StateId.Attacking));
            }
            else
            {
                SetSubState(StateMachine.GetState(StateId.SunkStruggle));
            }
        }
    }
}