using Src.Scripts.AI.States;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Src.Scripts.AI
{
    // Idle 2s -> Wander -> Repeat
    // Spot Target -> Attack
    // Lose Target -> Return to Idle loop
    // Sink -> be sunk for some seconds, still shoot if we have target but don't move
    //      -> rise after 2 seconds with no target
    public class TrooperTuber : AutoTrooper
    {
        [FormerlySerializedAs("wander")] [Header("Movement")]
        public WanderBehaviour wanderBehaviour;
        public float normalSpeed;
        [Tooltip("Speed when slowed by enemy paint")]
        public float slowedSpeed;
        public NavMeshAgent navAgent;
        
        private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
        
        private const float GroundDistance = 1.4f;

        
        protected override void Start() 
        {
            PaintCheckDistance = GroundDistance;
            navAgent = GetComponent<NavMeshAgent>();
            base.Start();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateSpeed();
            
            BaseState<TrooperStateMachine> idleState = stateMachine.CurrentRootState?.GetDescendantState(StateId.Idle);
            if (idleState != null && idleBehaviour == StateId.Wander)
            {
                idleState.SwitchState(StateId.Wander);
            }
        }
        
        private void UpdateSpeed()
        {
            float speed = navAgent.velocity.magnitude / 3f;
            if (animator != null)
            {
                animator.SetFloat(_moveSpeedHash, speed);
            }
        }

        protected override void PaintEffects()
        {
            switch (paintStatus)
            {
                case PaintStatus.EnemyPaint:
                    navAgent.speed = slowedSpeed;
                    StartGroundSplash();
                    break;
                case PaintStatus.FriendlyPaint:
                    navAgent.speed = normalSpeed;
                    StartGroundSplash();
                    break;
                case PaintStatus.NoPaint:
                    navAgent.speed = normalSpeed;
                    StopGroundSplash();
                    break;
                
            }
        }
        
        public override void Sink()
        {
            StopMovement();
            base.Sink();
        }

        void StopMovement()
        {
            navAgent.isStopped = true;
            if (wanderBehaviour != null)
            {
                wanderBehaviour.StopWander();
            }
        }
    }
}