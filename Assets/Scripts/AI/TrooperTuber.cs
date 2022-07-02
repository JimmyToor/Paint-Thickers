using System.Collections;
using System.Runtime.Remoting.Channels;
using DG.Tweening;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    // Idle 2s -> Wander -> Repeat      <-----  
    // Spot Target -> Target/Attack -> Repeat \
    // Lose Target ---------------------------|
    // Sink -> be sunk for seconds, still shoot if have target but can't move
    //      -> rise after 2 seconds
    public class TrooperTuber : Trooper
    {
        public float wanderDistance;
        public float timeBetweenWanders;
        public float timeSunk;
        public bool wander;

        private Vector3 initialPos;
        private NavMeshAgent navAgent;
        private bool sunk;
        private int sinkHash = Animator.StringToHash("Sink");
        private int riseHash = Animator.StringToHash("Rise");
        private int movingHash = Animator.StringToHash("Moving");
        private int moveSpeedHash = Animator.StringToHash("MoveSpeed");
        private int shootGroundHash = Animator.StringToHash("ShootGround");
        private WaitForSeconds idleDelay;
        private WaitForSeconds sunkDelay;

        protected override void Start()
        {
            base.Start();
            navAgent = GetComponent<NavMeshAgent>();
            idleDelay = new WaitForSeconds(timeBetweenWanders);
            sunkDelay = new WaitForSeconds(timeSunk);
        }

        protected void FixedUpdate()
        {
            CheckForPaint();
            if (navAgent.remainingDistance < 0.1f)
            {
                StopMovement();
            }
            if (hasTarget)
            {
                if (CheckLOS(target))
                {
                    StopMovement();
                    EngageTarget(target.TransformPoint(targetCharController.center));
                }
                else
                {
                    TargetLost();
                }
            }
            else if (wander)
            {
                WanderUpdate();
            }
        }

        private void WanderUpdate()
        {
            if (navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                SetWanderPos();
            }
            else
            {
                float speed = navAgent.velocity.magnitude / 3f;
                animator.SetFloat(moveSpeedHash, speed);
                if (!navAgent.pathPending && Mathf.Approximately(navAgent.remainingDistance, 0f))
                {
                    StartCoroutine(Wander());
                }
            } 
        }

        // Figure out what colour ink, if any, is underneath
        private void CheckForPaint()
        {
            Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 1f, terrainLayerMask);
            
            int channel = PaintTarget.RayChannel(hit);
            Debug.Log("hit " + channel + " we are " + teamChannel);

            if (channel == teamChannel)
            {
                if (sunk)
                {
                    Rise();
                }
                else if (!navAgent.isStopped)
                {
                    // particle effect
                }
            }
            else if (!sunk && channel != -1)
            {
                // in ink but not our team's
                StopMovement();
                StartCoroutine(Sink());
            }
        }

        protected void TargetLost()
        {
            Target = null;
            
            CancelInvoke();
            InvokeRepeating("TargetSearch",1.0f, 0.2f);
            
            animator.SetBool(targetFoundHash, false);
            StartCoroutine(Wander());
        }

        protected override void OnEnable()
        {
            InvokeRepeating("TargetSearch",1f, 0.2f);
            initialPos = transform.position;
        }

        bool SetWanderPos()
        {
            Debug.Log("Getting new wander pos");
            Vector3 randomPos = initialPos + Random.insideUnitSphere * wanderDistance;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, wanderDistance, navAgent.areaMask))
            {
                navAgent.SetDestination(hit.position);
                return true;
            }

            return false;
        }

        IEnumerator Wander()
        {
            if (navAgent.isStopped && !hasTarget && !sunk)
            {
                if (SetWanderPos())
                {
                    Debug.Log("mvove");
                    navAgent.isStopped = true;
                    yield return idleDelay;
                    navAgent.isStopped = false;
                    animator.SetBool(movingHash, true);
                }
            }
        }

        IEnumerator Sink()
        {
            animator.SetTrigger(sinkHash);
            sunk = true;
            Debug.Log("shoot ground");
            yield return sunkDelay;
            animator.SetTrigger(shootGroundHash);
        }

        void Rise()
        {
            animator.SetTrigger(riseHash);
            sunk = false;
        }

        void ShootGround()
        {
            Vector3 ground = aimRotate.position;
            ground.y = transform.position.y - 5f;
            
            if (!nozzleTweener.IsActive())
            {
                nozzleTweener = aimRotate.DOLookAt(ground,nozzleAimSpeed,AxisConstraint.None,Vector3.up).SetSpeedBased(true).OnComplete(Fire);;
            }
            else
            {
                nozzleTweener.ChangeEndValue(ground, false).OnComplete(Fire);;
            }
        }

        void StopMovement()
        {
            navAgent.isStopped = true;
            animator.SetBool(movingHash, false);
        }
    }
}