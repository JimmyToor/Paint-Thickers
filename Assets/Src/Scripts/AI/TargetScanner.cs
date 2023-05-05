using System.Collections;
using UnityEngine;

namespace Src.Scripts.AI
{
    public class TargetScanner : MonoBehaviour
    {
        public float sightDistance;
        public float timeBetweenScans;
        public bool searchWhenNoTarget;
        public LayerMask targetLayerMask;
        public LayerMask blockingLayerMask;
        private WaitForSeconds _scanDelay;
        [HideInInspector]public bool hasTarget;

        public Transform Target
        {
            get => _target;
            set
            {
                _target = value;
                hasTarget = value != null;
            }
        }
        
        private CharacterController _targetCharController;
        private Transform _target;

        private void Start()
        {
            _scanDelay = new WaitForSeconds(timeBetweenScans);
        }

        /// <summary>
        /// Searches for and targets the nearest player within line of sight
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
                if (targetHit.enabled == false)
                {
                    return false;
                }

                // Only care about seeing the body. Ignore things like the player's hands and weapons.
                if (!targetHit.TryGetComponent(out _targetCharController)) continue;
                if (!CheckLOS(targetHit.transform.position)) continue;
                
                SetNewTarget(targetHit.transform);
                return true;
            }

            return false;
        }

        void SetNewTarget(Transform newTarget)
        {
            Target = newTarget;
            Target.TryGetComponent(out _targetCharController);
            hasTarget = true;
        }

        public void TargetLost()
        {
            Target = null;
            _targetCharController = null;
            hasTarget = false;
            if (searchWhenNoTarget)
            {
                StartCoroutine(PeriodicSearch());
            }
        }
        
        
        /// <summary>
        /// Checks for line of sight between this object and the passed position.
        /// </summary>
        /// <param name="targetPos"></param>
        /// <returns>True if no obstacles are between us and the target. False otherwise.</returns>
        public bool CheckLOS(Vector3 targetPos)
        {
            // Check from closer to eye level, not object pivot
            Vector3 fromPos = transform.position;
            fromPos.y += 1f;

            if (!Physics.Raycast(fromPos,  targetPos - fromPos,
                    out _,Vector3.Distance(targetPos, fromPos),blockingLayerMask))
            {
                // Debug.DrawRay(eyePos, targetPos - eyePos, Color.blue);
                return true;
            }
            // Debug.DrawRay(eyePos, targetPos - eyePos, Color.red);

            return false;
        }

        public Vector3 GetTargetPos()
        {
            if (Target == null)
            {
                return Vector3.zero;
            }
            
            if (_targetCharController == null)
            {
                return Target.position;
            }
            
            return Target.TransformPoint(_targetCharController.center);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, sightDistance);
        }

        public IEnumerator PeriodicSearch()
        {
            while (!hasTarget)
            {
                TargetSearch();
                yield return _scanDelay;
            }
            
        }
    }
}