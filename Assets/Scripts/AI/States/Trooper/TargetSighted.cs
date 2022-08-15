namespace AI.States.Trooper
{
    public class TargetSighted : BaseState<TrooperStateMachine>
    {
        public TargetSighted(TrooperStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }
        
        public override StateId GetId() => StateId.TargetSighted;

        public override void Enter()
        {
            base.Enter();
            StateMachine.trooper.TargetSighted();
        }

        public override void Execute()
        {
            SwitchState(StateId.Attacking);
        }

        public override void InitializeSubState()
        {
            throw new System.NotImplementedException();
        }
    }
}