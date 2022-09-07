namespace AI.States.Trooper
{
    public class Attacking : BaseState<TrooperStateMachine>
    {
        private AutoTrooper _trooper;
        private TargetScanner _scanner;

        public Attacking(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _trooper = trooperStateMachine.trooper;
            _scanner = _trooper.scanner;
        }
        
        public override StateId GetId() => StateId.Attacking;

        public override void Enter()
        {
            base.Enter();
            if (_scanner.hasTarget)
            {
                UnityEngine.Animator animator = StateMachine.trooper.animator;
                animator.SetBool(StateMachine.trooper.HasTargetHash, true);
            }
        }

        public override void Execute()
        {
            if (_scanner.hasTarget && _scanner.Target.gameObject.activeSelf && _scanner.CheckLOS(_scanner.GetTargetPos()))
            {
                _trooper.EngageTarget();
            }
            else
            {
                SwitchState(StateId.TargetLost);
            }
        }

        public override void InitializeSubState()
        {
            
        }
    }
}