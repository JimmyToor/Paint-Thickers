using UnityEngine;

namespace AI.States.Trooper
{
    public class Standing : BaseState<TrooperStateMachine>
    {
        private AutoTrooper _trooper;

        public Standing(TrooperStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            _trooper = StateMachine.trooper;
        }
        
        public override StateId GetId() => StateId.Standing;

        public override void Execute()
        {
            if (_trooper.paintStatus == PaintStatus.EnemyPaint)
            {
                SwitchState(StateId.Sunk);
                Debug.Log("standing switched to sunk, so wander should no longer be a substate");
            }
        }

        public override void InitializeSubState()
        {
            if (_trooper.scanner.hasTarget)
            {
                SetSubState(StateMachine.GetState(StateId.Attacking));
            }
            else
            {
                SetSubState(StateMachine.GetState(StateId.Idle));
            }
            
        }
    }
}