using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Src.Scripts.AI.States;
using Src.Scripts.Attributes;
using Src.Scripts.Audio;
using Src.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Random = UnityEngine.Random;

namespace Src.Scripts.AI
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
        public ParticleSystem groundPaintSplash;
        public ParticleSystem centerPaintSpray;
        public ParticleSystem sunkPaintSpray;
        public PaintColorMatcher paintColorMatcher;
        public MultiRotationConstraint bodyRotationConstraint;
        public MultiAimConstraint hoseAimConstraint;
        [Tooltip("The trooper will check for paint straight down from this transform.")]
        public Transform paintCheckPosition;
        public float bodyTurnSpeed;
        public float hoseAimSpeed;
        public AudioClip alertSFX;
        public AudioClip shootSFX;
        public Animator animator;
        [Tooltip("Colliders to disable when sunk.")]
        public List<Collider> lowerColliders;

        #region Animation Hashes

        private readonly int _targetFoundHash = Animator.StringToHash("Target Found");
        private readonly int _hasTargetHash = Animator.StringToHash("Has Target");
        private readonly int _shootHash = Animator.StringToHash("Shoot");
        private readonly int _offsetHash = Animator.StringToHash("Offset");
        private readonly int _sinkHash = Animator.StringToHash("Sink");
        private readonly int _sunkHash = Animator.StringToHash("Sunk");
        private readonly int _riseHash = Animator.StringToHash("Rise");
        private readonly int _shootGroundHash = Animator.StringToHash("Shoot Ground");
        private readonly int _resetAimHash = Animator.StringToHash("Reset Aim");

        #endregion

        protected SFXSource SfxSource;
        protected LayerMask PaintTerrainLayerMask;
        protected float PaintCheckDistance = 2.3f;

        private bool _onCooldown;
        private WaitForSeconds _attackDelay;
        private WaitForSeconds _sunkDelay;
        private Color _groundColor;
        private Tweener _bodyTweener;
        private Tweener _hoseTweener;
        private Vector3 _origHosePos;
        private Rigidbody _rigidbody;
        private Transform _hoseAimTarget;
        private Transform _bodyRotationTarget;
        private ParticlePainter _sunkSprayParticlePainter;
        protected virtual void Start()
        {
            _sunkDelay = new WaitForSeconds(timeSunk);
            _attackDelay = new WaitForSeconds(attackCooldown);
            
            scanner = GetComponent<TargetScanner>();
            
            _hoseAimTarget = hoseAimConstraint.data.sourceObjects.GetTransform(0);
            _origHosePos = _hoseAimTarget.localPosition;
            _bodyRotationTarget = bodyRotationConstraint.data.sourceObjects.GetTransform(0);
            
            if (TryGetComponent(out animator))
            {
                animator.SetFloat(_offsetHash, Random.Range(0f,1f));
            }

            if (!TryGetComponent(out SfxSource))
            {
                Debug.LogFormat("{0} has no SfxSource", gameObject);
            }
            
            if (!TryGetComponent(out _rigidbody))
            {
                Debug.LogWarningFormat("{0} has no RigidBody", gameObject);
            }

            if (sunkPaintSpray.TryGetComponent(out _sunkSprayParticlePainter))
            {
                Debug.LogWarningFormat("{0}'s {1} has no ParticlePainter", gameObject, sunkPaintSpray);
            }
            
            PaintTerrainLayerMask = LayerMask.GetMask("Terrain");
            
            stateMachine = new TrooperStateMachine(this, statesData.stateList);
            stateMachine.SetRootState(initialRootState);
        }
        
        protected virtual void FixedUpdate()
        {
            stateMachine.Update();
            CheckForPaint();
        }

        // Figure out what colour paint, if any, is underneath
        private void CheckForPaint()
        {
            Vector3 currPos = paintCheckPosition.position;

            var up = transform.up;
            Physics.Raycast(currPos, -up, out RaycastHit hit, PaintCheckDistance, PaintTerrainLayerMask);
            //Debug.DrawRay(currPos, -up * PaintCheckDistance, Color.red);
            int channel = PaintTarget.RayChannel(hit);

            if (channel == -1)
            {
                paintStatus = PaintStatus.NoPaint;
            }
            else
            {
                if (paintColorMatcher.EnvironmentChannel != channel)
                {
                    paintColorMatcher.UpdateEnvironmentColor(channel);
                }

                paintStatus = channel == team.teamChannel ? PaintStatus.FriendlyPaint : PaintStatus.EnemyPaint;
            }
            
            PaintEffects();
        }

        protected virtual void PaintEffects()
        {
            switch (paintStatus)
            {
                case PaintStatus.EnemyPaint:
                    StartGroundSplash();
                    break;
                case PaintStatus.FriendlyPaint:
                    StopGroundSplash();
                    break;
                case PaintStatus.NoPaint:
                    StopGroundSplash();
                    break;
            }
        }

        public virtual void TargetSighted()
        {
            SfxSource.TriggerPlayOneShot(transform.position, alertSFX);
            animator.SetTrigger(_targetFoundHash);
            animator.SetBool(_hasTargetHash, true);
            DOTween.To(() => hoseAimConstraint.weight, x => hoseAimConstraint.weight = x, 1, 1.5f);
        }

        public virtual void EngageTarget()
        {
            AimAtTarget(scanner.GetTargetPos());
            if (!_onCooldown)
            {
                Attack();
            }
        }

        
        /// <summary>
        /// Tween bodyRotate and aimRotate to look at targetPosition.
        /// hoseAimRotate will not rotate if the angle to targetPosition is > 90.
        /// </summary>
        /// <param name="targetPosition"></param>
        protected virtual void AimAtTarget(Vector3 targetPosition)
        {
            AimBody(targetPosition);
            AimHose(targetPosition);
        }

        private void AimBody(Vector3 targetPosition)
        {
            if (!_bodyTweener.IsActive())
            {
                _bodyTweener = _bodyRotationTarget.DOLookAt(targetPosition, bodyTurnSpeed, AxisConstraint.Y)
                    .SetSpeedBased(true);
            }
            else
            {
                _bodyTweener.ChangeEndValue(targetPosition,
                    true);
            }
        }

        private void AimHose(Vector3 targetPosition)
        {
            if (!_hoseTweener.IsActive())
            {
                _hoseTweener = _hoseAimTarget.DOMove(targetPosition, hoseAimSpeed).SetSpeedBased(true);
            }
            else
            {
                _hoseTweener.ChangeEndValue(targetPosition, false);
            }
        }

        public void ResetAim()
        {
            animator.SetTrigger(_resetAimHash);
            
            _hoseAimTarget.DOLocalMove(_origHosePos,
                         hoseAimSpeed).SetSpeedBased(true);
            DOTween.To(() => hoseAimConstraint.weight, x => hoseAimConstraint.weight = x, 1, 1.5f);
            
            _bodyRotationTarget.DOLocalRotate(_bodyRotationTarget.InverseTransformDirection(transform.forward),
                    bodyTurnSpeed).SetSpeedBased(true);
        }

        public virtual void TargetLost()
        {
            animator.SetBool(_hasTargetHash, false);
        }

        protected virtual void Attack()
        {
            Fire();
            StartCoroutine(StartCooldown());
        }

        protected void Fire()
        {
            weaponPaintSpray.Emit(1);
            animator.SetTrigger(_shootHash);
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
            animator.SetTrigger(_sinkHash);
            animator.SetBool(_sunkHash, true);
        }

        public virtual void Rise()
        {
            animator.SetTrigger(_riseHash);
            animator.SetBool(_sunkHash, false);
        }
        
        public IEnumerator StartPaintEscapeAfterTimer()
        {
            yield return _sunkDelay;
            
            BaseState<TrooperStateMachine> sunkState = stateMachine.CurrentRootState.GetDescendantState(StateId.SunkStruggle);
            sunkState?.SwitchState(StateId.SunkEscapePaint);
        }
        
        public virtual void EscapePaint()
        {
            animator.SetTrigger(_shootGroundHash);
        }

        public void StartSunkSpray()
        {
            sunkPaintSpray.Play();
        }

        public void StopSunkSpray()
        {
            sunkPaintSpray.Stop();
        }
        
        public void StartGroundSpray()
        {
            groundPaintSpray.Play();
        }

        public void StopGroundSpray()
        {
            if (groundPaintSpray.isPlaying)
            {
                groundPaintSpray.Stop();
                ResetAim();
            }
        }
        
        public void StartCenterSpray()
        {
            centerPaintSpray.Play();
        }

        public void StopCenterSpray()
        {
            centerPaintSpray.Stop();
        }

        public void StartGroundSplash()
        {
            groundPaintSplash.gameObject.SetActive(true);
        }
        
        public void StopGroundSplash()
        {
            groundPaintSplash.gameObject.SetActive(false);
        }

        public void MakeKinematic()
        {
            _rigidbody.isKinematic = true;
        }
        
        public void MakeNonKinematic()
        {
            _rigidbody.isKinematic = false;
        }

        public void DisableLowerColliders()
        {
            foreach (var col in lowerColliders)
            {
                col.enabled = false;
            }
        }
        
        public void EnableLowerColliders()
        {
            foreach (var col in lowerColliders)
            {
                col.enabled = true;
            }
        }
        
        [ContextMenu("Trigger Attack")]
        public void DebugAttack()
        {
            Attack();
        }
    }
}