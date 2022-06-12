using System;
using DG.Tweening;
using UnityEngine;
using Random = System.Random;

namespace AI
{
    [RequireComponent(typeof(Health))]
    public class Trooper : Enemy
    {
        [SerializeField]
        public ParticleSystem paintSpray;
        [SerializeField]
        public Transform bodyRotate;
        [SerializeField]
        public Transform aimRotate;
        public Tweener bodyTweener;
        public Tweener nozzleTweener;
        [SerializeField]
        public float bodyTurnSpeed;
        public float nozzleAimSpeed;
        public AudioClip shootSFX;

        private Animator animator;
        private int targetFoundHash = Animator.StringToHash("TargetFound");
        private int shootHash = Animator.StringToHash("Shoot");
        private int offsetHash = Animator.StringToHash("Offset");
        private SFXSource sfxSource;
        private LayerMask playerLayerMask;
        private LayerMask terrainLayerMask;

        public float sightDistance;

        // Player in sight? ->Yes-> Attack
        //                  ->No-> Idle
        protected override void Start()
        {
            base.Start();
            InvokeRepeating("TargetSearch",1.0f, 0.2f);
            if (TryGetComponent(out animator))
            {
                animator.SetFloat(offsetHash, UnityEngine.Random.Range(0f,1f));
            }

            TryGetComponent(out sfxSource);
            playerLayerMask = LayerMask.GetMask("Players");
            terrainLayerMask = LayerMask.GetMask("Terrain");
        }


        // Searches for and targets the nearest player within line of sight
        void TargetSearch()
        {
            Collider[] playersHit = new Collider[1];
            int size = Physics.OverlapSphereNonAlloc(transform.position, sightDistance, playersHit, playerLayerMask);
            
            if (size > 0)
            {
                foreach (var playerHit in playersHit)
                {
                    if (CheckLOS(playerHit))
                    { // Only care about the first valid target
                        return;
                    }
                }
            }
            
            // No valid target found
            animator.SetBool(targetFoundHash, false);
            DisableSpray();
        }

        // TODO : something's broken, debug this, they never shoot, probably not "seeing" player
        // Checks for line of sight between this object and the passed collider
        bool CheckLOS(Collider playerCollider)
        {
            // Check from eye level, not object pivot
            Vector3 eyePos = transform.position;
            eyePos.y += 1f;

            if (!playerCollider.TryGetComponent(out CharacterController charController))
            {
                return false;
            }
            
            if (!Physics.Raycast(eyePos, playerCollider.transform.TransformPoint(charController.center) - eyePos,
                    out RaycastHit hit,Vector3.Distance(playerCollider.transform.TransformPoint(charController.center), eyePos),terrainLayerMask ))
            {
                animator.SetBool(targetFoundHash, true);
                EnableSpray();
                EngageTarget(playerCollider.transform.TransformPoint(charController.center));
                return true;
            }
            else
            {
                Debug.Log("Hit " + hit.transform.name);
            }
            Debug.DrawRay(eyePos, hit.point - eyePos, Color.blue);

            return false;
        }

        // Aim at the target located at the passed Vector3
        // TODO: Too slow and seems to be ignoring speed values, also nozzle not turning, only body.
        void EngageTarget(Vector3 targetPosition)
        {
            if (!bodyTweener.IsActive())
                bodyTweener = bodyRotate.DOLookAt(targetPosition,bodyTurnSpeed,AxisConstraint.Y).SetSpeedBased(true);
            else
                bodyTweener.ChangeEndValue(targetPosition, true); // Don't make a new tween if one is active, just change the current one
        
            // Only move the nozzle if the target is in front us
            if (Vector3.Angle(transform.position,targetPosition) < 70)
            {
                if (!nozzleTweener.IsActive())
                    nozzleTweener = aimRotate.DOLookAt(targetPosition,nozzleAimSpeed,AxisConstraint.None,Vector3.up).SetSpeedBased(true);
                else
                    nozzleTweener.ChangeEndValue(targetPosition, true);
            }
        }

        void DisableSpray()
        {
            paintSpray.Stop();
        }

        void EnableSpray()
        {
            if (!paintSpray.isPlaying)
            {
                paintSpray.Play();
                animator.SetTrigger(shootHash);
                sfxSource.TriggerPlayOneShot(transform.position, shootSFX);
            }
        }

        private void OnDisable()
        {
            DisableSpray();
            CancelInvoke();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, sightDistance);
        }
    }
}
