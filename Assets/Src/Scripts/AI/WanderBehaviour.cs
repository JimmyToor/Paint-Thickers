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
        public float wanderTimeout = 5;

        private Vector3 _initialPos;
        private NavMeshAgent _navAgent;
        private Coroutine _wanderCoroutine;

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
                StartCoroutine(WanderTimeout());
            }
        }
        
        public void WanderUpdate()
        {
            if (IsWanderDone())
            {
                StartWanderAfterDelay();
            }
        }

        public bool IsWanderDone()
        {
            // Wander is done if there's no current, queued, or pending path or the agent is close enough to the destination
            if (!_navAgent.hasPath && !wanderQueued && !_navAgent.pathPending
                && _navAgent.remainingDistance <= _navAgent.stoppingDistance)
            {
                StopCoroutine(WanderTimeout());
                Debug.Log(gameObject.name + " has finished it's wander.");
                return true;
            }
            return false;
        }

        private bool SetWanderPos()
        {
            Vector3 randomPos = _initialPos + Random.insideUnitSphere * wanderDistance;

            if (!NavMesh.SamplePosition(randomPos, out NavMeshHit hit, wanderDistance, _navAgent.areaMask))
                return false;
            
            return _navAgent.SetDestination(hit.position);
        }

        public void StopWander()
        {
            if (_wanderCoroutine != null)
            {
                StopAllCoroutines();

                _wanderCoroutine = null;
                wanderQueued = false;
            }
            _navAgent.ResetPath();
        }

        public void StartWanderAfterDelay()
        {
            if (_wanderCoroutine != null)
            { // In case there's already one running, stop it.
                StopCoroutine(_wanderCoroutine);
            }
            _wanderCoroutine = StartCoroutine(WanderAfterDelay());
            Debug.Log(gameObject.name + " will wander after " + timeBetweenWanders + " seconds.");
        }

        private IEnumerator WanderAfterDelay()
        {
            wanderQueued = true;
            yield return new WaitForSeconds(timeBetweenWanders);
            wanderQueued = false;
            StartWander();
        }
        
        private IEnumerator WanderTimeout()
        {
            yield return new WaitForSeconds(wanderTimeout);
            if (_navAgent.hasPath)
            {
                Debug.Log(gameObject.name + " has timed out.");
                StopWander();
            }
        }

    }
}