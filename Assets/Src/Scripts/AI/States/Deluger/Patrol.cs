namespace AI.States.Deluger
{
    public class Patrol : BaseState<DelugerStateMachine>
    {
        private AI.Deluger _deluger;

        public Patrol(DelugerStateMachine delugerStateMachine) : base(delugerStateMachine)
        {
            _deluger = delugerStateMachine.deluger;
        }
        
        public override StateId GetId() => StateId.Attacking;

        public override void Enter()
        {
            base.Enter();
            _deluger.MovePrep();
        }

        public override void Execute()
        {
           
        }

        public override void InitializeSubState()
        {
            
        }
    }
}