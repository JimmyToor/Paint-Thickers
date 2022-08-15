namespace AI.States.Trooper
{
    public class SunkStruggle : BaseState<TrooperStateMachine>
    {
        private AutoTrooper _trooper;

        public SunkStruggle(TrooperStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            _trooper = StateMachine.trooper;
        }
        
        public override StateId GetId() => StateId.SunkStruggle;

        public override void Enter()
        {
            base.Enter();
            StateMachine.trooper.StartCoroutine(_trooper.StartPaintEscapeAfterTimer());
        }

        public override void Execute()
        {
        }

        public override void InitializeSubState()
        {
        }

    }
}