namespace AI.States.Trooper
{
    public class TargetLost : BaseState<TrooperStateMachine>
    {

        public TargetLost(TrooperStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }
        
        public override StateId GetId() => StateId.TargetLost;

        public override void Enter()
        {
            base.Enter();
            StateMachine.trooper.TargetLost();
            StateMachine.trooper.scanner.TargetLost();
        }

        public override void Execute()
        {
            throw new System.NotImplementedException();
        }

        public override void InitializeSubState()
        {
            throw new System.NotImplementedException();
        }
    }
}