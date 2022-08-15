using System.Collections;
using AI.States;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    // Idle 2s -> Wander -> Repeat      <-----  
    // Spot Target -> Target/Attack -> Repeat \
    // Lose Target ---------------------------|
    // Sink -> be sunk for seconds, still shoot if have target but can't move
    //      -> rise after 2 seconds
    public class TrooperTuber : AutoTrooper
    {
        [Header("Movement")]
        public Wander wander;
        public float normalSpeed;
        public float slowedSpeed;
        public NavMeshAgent navAgent;
        
        private int moveSpeedHash = Animator.StringToHash("MoveSpeed");
        private bool _wanderQueued;
        private readonly float groundDistance = 0.5f;

        protected override void Start() 
        {
            paintCheckDistance = groundDistance;
            base.Start();
            navAgent = GetComponent<NavMeshAgent>();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            animator.SetFloat(moveSpeedHash, navAgent.speed);
            if (idleBehaviour == StateId.Wander && !_wanderQueued && stateMachine?.CurrentRootState?.GetDescendantState(StateId.Idle)?.GetId() == StateId.Idle)
            {
                StartCoroutine(WanderAfterDelay());
            }
            PaintEffects();
            UpdateSpeed();
        }
        
        private void UpdateSpeed()
        {
            float speed = navAgent.velocity.magnitude / 3f;
            if (animator != null)
            {
                animator.SetFloat(moveSpeedHash, speed);
            }
        }
        
        protected void PaintEffects()
        {
            switch (paintStatus)
            {
                case PaintStatus.EnemyPaint:
                    navAgent.speed = slowedSpeed;
                    break;
                default:
                    navAgent.speed = normalSpeed;
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
            if (wander != null)
            {
                wander.StopWander();
            }
        }

        public IEnumerator WanderAfterDelay()
        {
            _wanderQueued = true;
            yield return wander.wanderDelay;
            BaseState<TrooperStateMachine> idleState = stateMachine.CurrentRootState.GetDescendantState(StateId.Idle);
            idleState?.SwitchState(StateId.Wander);
            
            _wanderQueued = false;
        }
    }
}