using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Src.Scripts.AI
{
    public class TargetScanner : MonoBehaviour
    {
        public float sightDistance;
        public float timeBetweenScans;
        [Tooltip("Time between losing a target and beginning another scan.")]
        public float targetLossCooldown;
        public bool searchWhenNoTarget;
        public LayerMask targetLayerMask;
        public LayerMask blockingLayerMask;
        public Transform originTransform;
        public UnityEvent<Transform> onTargetSighted;
        public UnityEvent onTargetLost;
        
        private WaitForSeconds _scanDelay;
        private CharacterController _targetCharController;
        private TargetModifier _targetModifier;
        [HideInInspector]public bool hasTarget;
        [HideInInspector]public bool hasLOS;
        
        private Transform _target;
        public Transform Target
        {
            get => _target;
            set
            {
                _target = value;
                if (value == null)
                {
                    hasTarget = false;
                    hasLOS = false;
                }
                else
                {
                    hasTarget = true;
                }
            }
        }
        
        private void Start()
        {
            _scanDelay = new WaitForSeconds(timeBetweenScans);
        }

        /// <summary>
        /// Searches for and targets the nearest valid target within line of sight.
        /// </summary>
        /// <returns></returns>
        public bool TargetSearch()
        {
            Collider[] targetsHit = new Collider[1];
            int size = Physics.OverlapSphereNonAlloc(transform.position, sightDistance, targetsHit, targetLayerMask);

            if (size <= 0)
            {
                return false;
            }

            foreach (var targetHit in targetsHit)
            {
                if (!IsValidTarget(targetHit))
                {
                    continue;
                }
                
                SetNewTarget(targetHit.transform);
                return true;
            }
            return false;
        }

        private bool IsValidTarget(Collider newTarget)
        {
            if (newTarget.gameObject.activeSelf && newTarget.TryGetComponent(out _targetCharController))
            {
                return newTarget.TryGetComponent(out TargetModifier targetModifier)
                    ? CheckLOS(targetModifier.targetTransform.position)
                    : CheckLOS(newTarget.transform.TransformPoint(_targetCharController.center));
            }
            return false;
        }
        
        private void SetNewTarget(Transform newTarget)
        {
            Target = newTarget;
            if (_targetCharController == null)
            {
                Target.TryGetComponent(out _targetCharController);
            }

            if (_targetModifier == null)
            {
                Target.TryGetComponent(out _targetModifier);
            }
           
            hasTarget = true;
            
            onTargetSighted.Invoke(Target);
        }

        public void TargetLost()
        {
            StopCoroutine(PeriodicLOSCheck());
            Target = null;
            _targetCharController = null;
            _targetModifier = null;
            hasTarget = false;
            if (searchWhenNoTarget)
            {
                StartCoroutine(PeriodicSearch());
            }
            onTargetLost.Invoke();
        }
        
        
        /// <summary>
        /// Checks for line of sight between this object's transform and the passed position.
        /// </summary>
        /// <param name="targetPos"></param>
        /// <remarks>Modifies <paramref name="hasLOS">hasLOS.</paramref></remarks>
        /// <returns>True if no obstacles are between our transform position and the target. False otherwise.</returns>
        public bool CheckLOS(Vector3 targetPos)
        {
            Vector3 fromPos = originTransform.position;
            
            if (!Physics.Raycast(fromPos,  targetPos - fromPos, out RaycastHit _hit,Vector3.Distance(targetPos, fromPos),blockingLayerMask))
            {
                // Debug.DrawRay(fromPos, targetPos - fromPos, Color.blue);
                hasLOS = true;
                return true;
            }
            // Debug.DrawRay(fromPos, targetPos - fromPos, Color.red);
            hasLOS = false;

            return false;
        }

        public Vector3 GetTargetPos()
        {
            if (Target == null)
            {
                return Vector3.zero;
            }
            
            if (_targetModifier != null)
            {
                return _targetModifier.targetTransform.position;
            }
            
            if (_targetCharController != null)
            {
                return Target.TransformPoint(_targetCharController.center);
            }
            
            return Target.position;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, sightDistance);
        }

        public IEnumerator PeriodicSearch()
        {
            float cooldown = targetLossCooldown;
            while (cooldown > 0)
            {
                cooldown -= Time.deltaTime;
            }
            while (!hasTarget)
            {
                TargetSearch();
                yield return _scanDelay;
            }
        }
        
        public IEnumerator PeriodicLOSCheck()
        {
            while (hasTarget)
            {
                if (Target.gameObject.activeSelf == false)
                {
                    hasLOS = false;
                }
                else
                {
                    CheckLOS(GetTargetPos());
                }
                yield return _scanDelay;
            }
            
        }
    }
}