﻿namespace Src.Scripts.AI.States.Trooper
{
    public class SunkEscapePaint : BaseState<TrooperStateMachine>
    {
        private AutoTrooper _trooper;

        public SunkEscapePaint(TrooperStateMachine trooperStateMachine) : base(trooperStateMachine)
        {
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