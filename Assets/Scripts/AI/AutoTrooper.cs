using System.Collections;
using AI.States;
using Attributes;
using Audio;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

namespace AI
{   
    public class AutoTrooper : Enemy
    {
        [Header("Behaviour")]
        [StateDataDropdown]public StateId initialRootState;
        [Tooltip("The state to be in when there's nothing important to do")]
        [StateDataDropdown]public StateId idleBehaviour;
        [HideInInspector] public TrooperStateMachine stateMachine;
        [HideInInspector] public PaintStatus paintStatus;
        public TargetScanner scanner;
        [Tooltip("The time in seconds spent in enemy ink until trying to escape")]
        public float timeSunk;
        [Tooltip("The time in seconds before another attack can begin")]
        public float attackCooldown;

        public ParticleSystem weaponPaintSpray;
        public ParticleSystem groundPaintSpray;
        public ParticleSystem centerPaintSpray;
        public Transform bodyRotate;
        public Transform aimRotate;
        public Transform groundTarget;
        public Tweener BodyTweener;
        public Tweener NozzleTweener;
        public float bodyTurnSpeed;
        public float nozzleAimSpeed;
        public AudioClip shootSFX;
        public Animator animator;
        public RotationConstraint hoseRotateConstraint;
        public RotationConstraint bodyRotateConstraint;
        
        #region Animation Hashes

        public readonly int TargetFoundHash = Animator.StringToHash("TargetFound");
        public readonly int HasTargetHash = Animator.StringToHash("HasTarget");
        public readonly int ShootHash = Animator.StringToHash("Shoot");
        public readonly int OffsetHash = Animator.StringToHash("Offset");
        public readonly int SinkHash = Animator.StringToHash("Sink");
        public readonly int RiseHash = Animator.StringToHash("Rise");
        public readonly int ShootGroundHash = Animator.StringToHash("ShootGround");

        #endregion

        protected SFXSource SfxSource;
        protected LayerMask PaintTerrainLayerMask;
        protected float paintCheckDistance = 1.4f;

        private bool _onCooldown;
        private WaitForSeconds _attackDelay;
        private WaitForSeconds _sunkDelay;

        protected override void Start()
        {
            base.Start();
            stateMachine = new TrooperStateMachine(this, statesData.stateList);
            stateMachine.SetRootState(initialRootState);
            _sunkDelay = new WaitForSeconds(timeSunk);
            _attackDelay = new WaitForSeconds(attackCooldown);
            scanner = GetComponent<TargetScanner>();
        }
        
        protected virtual void FixedUpdate()
        {
            stateMachine.Update();
            CheckForPaint();
        }

        protected virtual void Awake()
        {
            if (TryGetComponent(out animator))
            {
                animator.SetFloat(OffsetHash, Random.Range(0f,1f));
            }

            TryGetComponent(out SfxSource);
            PaintTerrainLayerMask = LayerMask.GetMask("Terrain");
        }

        // Figure out what colour ink, if any, is underneath
        private void CheckForPaint()
        {
            Vector3 currPos = transform.position;
            currPos.y = groundTarget.position.y;

            Physics.Raycast(currPos, -transform.up, out RaycastHit hit, paintCheckDistance, PaintTerrainLayerMask);
            Debug.DrawRay(currPos,-transform.up*paintCheckDistance,Color.red);
            
            int channel = PaintTarget.RayChannel(hit);
            if (channel == teamChannel)
            {
                paintStatus = PaintStatus.FriendlyPaint;
            }
            else if (channel == -1)
            {
                paintStatus = PaintStatus.NoPaint;
            }
            else
            {
                paintStatus = PaintStatus.EnemyPaint;
            }
        }

        public virtual void TargetSighted()
        {
            animator.SetTrigger(TargetFoundHash);
            animator.SetBool(HasTargetHash, true);
        }

        public virtual void EngageTarget()
        {
            AimAtTarget(scanner.GetTargetPos());
            if (!_onCooldown)
            {
                Attack();
            }
        }
        
        protected virtual void AimAtTarget(Vector3 targetPosition)
        {
            if (!BodyTweener.IsActive())
            {
                bodyRotateConstraint.locked = false;
                BodyTweener = bodyRotate.DOLookAt(targetPosition,bodyTurnSpeed,AxisConstraint.Y)
                                        .SetSpeedBased(true)
                                        .OnComplete(() => { bodyRotateConstraint.locked = true; });
            }
            else
            {
                BodyTweener.ChangeEndValue(targetPosition,
                    true); // Don't make a new tween if one is active, just change the current one
            }

            // Only move the nozzle if the target is in front us
            if (Vector3.Angle(transform.position,targetPosition) < 90)
            {
                if (!NozzleTweener.IsActive())
                {
                    hoseRotateConstraint.locked = false;
                    NozzleTweener = aimRotate.DOLookAt(targetPosition,nozzleAimSpeed,AxisConstraint.None,Vector3.up)
                                            .SetSpeedBased(true)
                                            .OnComplete(() => { hoseRotateConstraint.locked = true; });
                }
                else
                {
                    NozzleTweener.ChangeEndValue(targetPosition, true);
                }
            }
        }

        public virtual void TargetLost()
        {
            animator.SetBool(HasTargetHash, false);
            UnlockAimConstraint();
            UnlockBodyConstraint();
        }

        protected virtual void Attack()
        {
            Fire();
            StartCoroutine(StartCooldown());
        }

        protected void Fire()
        {
            weaponPaintSpray.Emit(1);
            animator.SetTrigger(ShootHash);
            SfxSource.TriggerPlayOneShot(transform.position, shootSFX);
        }
        
        private IEnumerator StartCooldown()
        {
            _onCooldown = true;
            yield return _attackDelay;
            _onCooldown = false;
        }
        
        private void StopSpray()
        {
            weaponPaintSpray.Stop();
            groundPaintSpray.Stop();
        }

        protected void OnDisable()
        {
            StopSpray();
            CancelInvoke();
        }

        public virtual void Sink()
        {
            animator.SetTrigger(SinkHash);
        }

        public virtual void Rise()
        {
            animator.SetTrigger(RiseHash);
        }
        
        public IEnumerator StartPaintEscapeAfterTimer()
        {
            yield return _sunkDelay;
            
            BaseState<TrooperStateMachine> sunkState = stateMachine.CurrentRootState.GetDescendantState(StateId.SunkStruggle);
            sunkState?.SwitchState(StateId.SunkEscapePaint);
        }
        
        public virtual void EscapePaint()
        {
            animator.SetBool(ShootGroundHash, true);
        }

        public void StartGroundSpray()
        {
            groundPaintSpray.Play();
        }

        public void StopGroundSpray()
        {
            groundPaintSpray.Stop();
            animator.SetBool(ShootGroundHash, false);
        }
        
        public void StartCenterSpray()
        {
            centerPaintSpray.Play();
        }

        public void StopCenterSpray()
        {
            centerPaintSpray.Stop();
        }
        
        [ContextMenu("Trigger Attack")]
        public void DebugShoot()
        {
            Attack();
        }

        public void LockAimConstraint()
        {
            hoseRotateConstraint.locked = true;
        }
        
        public void UnlockAimConstraint()
        {
            hoseRotateConstraint.locked = false;
        }
        
        public void LockBodyConstraint()
        {
            bodyRotateConstraint.locked = true;
        }
        
        public void UnlockBodyConstraint()
        {
            bodyRotateConstraint.locked = false;
        }
        
    }
}