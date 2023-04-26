using UnityEngine;

namespace AI.States.Trooper
{
    public class SunkStruggle : BaseState<TrooperStateMachine>
    {
        private AutoTrooper _trooper;
        private Coroutine escapeTimerCoroutine;

        public SunkStruggle(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
            _trooper = StateMachine.trooper;
        }
        
        public override StateId GetId() => StateId.SunkStruggle;

        public override void Enter()
        {
            base.Enter();
            escapeTimerCoroutine = _trooper.StartCoroutine(_trooper.StartPaintEscapeAfterTimer());
        }

        public override void Execute()
        {
            if (StateMachine.trooper.scanner.hasTarget)
            {
                StateMachine.trooper.StopCoroutine(escapeTimerCoroutine);
                SwitchState(StateId.TargetSighted);
            }
        }

        public override void InitializeSubState()
        {
        }

    }
}