using DG.Tweening;
using UnityEngine;

namespace AI
{
    public class Trooper : Enemy
    {
        public ParticleSystem paintSpray;
        public Transform bodyRotate;
        public Transform aimRotate;
        public Tweener bodyTweener;
        public Tweener nozzleTweener;
        public float bodyTurnSpeed;
        public float nozzleAimSpeed;
        public float sightDistance;
        public AudioClip shootSFX;

        public Transform Target
        {
            set
            {
                target = value;
                hasTarget = value != null;
            }
            get => target;
        }
        
        protected Animator animator;
        protected int targetFoundHash = Animator.StringToHash("TargetFound");
        protected int shootHash = Animator.StringToHash("Shoot");
        protected int offsetHash = Animator.StringToHash("Offset");
        protected SFXSource sfxSource;
        protected LayerMask playerLayerMask;
        protected LayerMask terrainLayerMask;
        protected CharacterController targetCharController;
        protected bool hasTarget;
        protected Transform target;


        protected virtual void OnEnable()
        {
            InvokeRepeating("TargetSearch",1f, 0.2f);
        }

        protected virtual void Awake()
        {
            if (TryGetComponent(out animator))
            {
                animator.SetFloat(offsetHash, UnityEngine.Random.Range(0f,1f));
            }

            TryGetComponent(out sfxSource);
            playerLayerMask = LayerMask.GetMask("Players");
            terrainLayerMask = LayerMask.GetMask("Terrain");
            
        }

        protected void TargetSearch()
        {
            Collider[] playersHit = new Collider[1];
            int size = Physics.OverlapSphereNonAlloc(transform.position, sightDistance, playersHit, playerLayerMask);
            
            if (size > 0)
            {
                foreach (var playerHit in playersHit)
                {
                    if (playerHit.TryGetComponent(out targetCharController) && CheckLOS(playerHit.transform))
                    {
                        TargetSighted(playerHit.transform);
                        return; // Only care about the first valid target
                    }
                }
            }
        }

        protected virtual void TargetSighted(Transform newTarget)
        {
            Target = newTarget;
            animator.SetBool(targetFoundHash, true);
            
            CancelInvoke(); // Stop searching for targets
            InvokeRepeating("Fire",1.0f, 0.75f);
        }

        // Checks for line of sight between this object and the passed transform
        protected bool CheckLOS(Transform playerHit)
        {
            // Check from eye level, not object pivot
            Vector3 eyePos = transform.position;
            eyePos.y += 1f;

            Vector3 targetPos = playerHit.TransformPoint(targetCharController.center);
            
            if (!Physics.Raycast(eyePos,  targetPos - eyePos,
                    out RaycastHit hit,Vector3.Distance(targetPos, eyePos),terrainLayerMask) && playerHit.gameObject.activeSelf)
            {
                // Debug.DrawRay(eyePos, targetPos - eyePos, Color.blue);
                return true;
            }
            // Debug.DrawRay(eyePos, targetPos - eyePos, Color.red);

            return false;
        }

        protected virtual void EngageTarget(Vector3 targetPosition)
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

        protected override void Attack()
        {
            Fire();
        }

        protected virtual void Fire()
        {
            paintSpray.Emit(1);
            animator.SetTrigger(shootHash);
            sfxSource.TriggerPlayOneShot(transform.position, shootSFX);
        }

        private void DisableSpray()
        {
            paintSpray.Stop();
        }

        protected void OnDisable()
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