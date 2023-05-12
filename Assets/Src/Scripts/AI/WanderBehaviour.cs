using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Src.Scripts.AI
{
    public class WanderBehaviour : MonoBehaviour
    {
        public float wanderDistance;
        public float timeBetweenWanders;
        public WaitForSeconds wanderDelay;
        [HideInInspector]
        public bool wanderQueued;
        
        private Vector3 _initialPos;
        private NavMeshAgent _navAgent;
        private Coroutine wanderCoroutine;

        private void Start()
        {
            wanderDelay = new WaitForSeconds(timeBetweenWanders);
        }

        private void OnEnable()
        {
            _initialPos = transform.position;
            _navAgent = GetComponent<NavMeshAgent>();
        }

        public void StartWander()
        {
            if (SetWanderPos())
            {
                _navAgent.isStopped = false;
            }
        }
        
        public void WanderUpdate()
        {
            if (_navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                SetWanderPos();
            }
        }

        public bool IsWanderDone()
        {
            if (!_navAgent.pathPending && Mathf.Approximately(_navAgent.remainingDistance, 0f))
            {
                return true;
            }
            return false;
        }
        
        public bool SetWanderPos()
        {
            Vector3 randomPos = _initialPos + Random.insideUnitSphere * wanderDistance;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, wanderDistance, _navAgent.areaMask))
            {
                _navAgent.SetDestination(hit.position);
                return true;
            }
            return false;
        }

        public void StopWander()
        {
            if (wanderCoroutine != null)
            {
                StopCoroutine(wanderCoroutine);

                wanderCoroutine = null;
                wanderQueued = false;
            }
            _navAgent.ResetPath();
        }

        public void StartWanderAfterDelay()
        {
            if (wanderCoroutine != null)
            { // In case there's already one running, stop it.
                StopCoroutine(wanderCoroutine);
            }
            wanderCoroutine = StartCoroutine(WanderAfterDelay());
        }
        
        public IEnumerator WanderAfterDelay()
        {
            wanderQueued = true;
            yield return wanderDelay;
            wanderQueued = false;
            StartWander();
        }

    }
}