namespace AI.States.Trooper
{
    public class SunkEscapePaint : BaseState<TrooperStateMachine>
    {
        private AutoTrooper _trooper;

        public SunkEscapePaint(TrooperStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            _trooper = StateMachine.trooper;
        }
        
        public override StateId GetId() => StateId.SunkEscapePaint;

        public override void Enter()
        {
            base.Enter();
            _trooper.EscapePaint();
        }

        public override void Execute()
        {
            
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void InitializeSubState()
        {
        }
    }
}