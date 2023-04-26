using System.Collections;
using System.Collections.Generic;
using AI.States;
using Attributes;
using Audio;
using DG.Tweening;
using Gameplay;
using Src.Scripts;
using Src.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;
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
        public ParticleSystem groundPaintSplash;
        [FormerlySerializedAs("paintColorHandler")] public PaintColorMatcher paintColorMatcher;
        public MultiRotationConstraint bodyRotationConstraint;
        public MultiAimConstraint hoseAimConstraint;
        [Tooltip("The trooper will check for paint straight down from this transform.")]
        public Transform paintCheckPosition;
        public float bodyTurnSpeed;
        public float hoseAimSpeed;
        public AudioClip shootSFX;
        public Animator animator;
        [Tooltip("Colliders to disable when sunk.")]
        public List<Collider> lowerColliders;

        #region Animation Hashes

        public readonly int TargetFoundHash = Animator.StringToHash("Target Found");
        public readonly int HasTargetHash = Animator.StringToHash("Has Target");
        public readonly int ShootHash = Animator.StringToHash("Shoot");
        public readonly int OffsetHash = Animator.StringToHash("Offset");
        public readonly int SinkHash = Animator.StringToHash("Sink");
        public readonly int SunkHash = Animator.StringToHash("Sunk");
        public readonly int RiseHash = Animator.StringToHash("Rise");
        public readonly int ShootGroundHash = Animator.StringToHash("Shoot Ground");
        public readonly int ResetAimHash = Animator.StringToHash("Reset Aim");

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
                animator.SetFloat(OffsetHash, Random.Range(0f,1f));
            }

            TryGetComponent(out SfxSource);
            TryGetComponent(out _rigidbody);
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

            Physics.Raycast(currPos, -transform.up, out RaycastHit hit, PaintCheckDistance, PaintTerrainLayerMask);
            Debug.DrawRay(currPos, -transform.up * PaintCheckDistance, Color.red);
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
                
                if (channel == team.teamChannel)
                {
                    paintStatus = PaintStatus.FriendlyPaint;
                }
                else
                {
                    paintStatus = PaintStatus.EnemyPaint;
                }
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
            animator.SetTrigger(TargetFoundHash);
            animator.SetBool(HasTargetHash, true);
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
            animator.SetTrigger(ResetAimHash);
            
            _hoseAimTarget.DOLocalMove(_origHosePos,
                         hoseAimSpeed).SetSpeedBased(true);
            DOTween.To(() => hoseAimConstraint.weight, x => hoseAimConstraint.weight = x, 1, 1.5f);
            
            _bodyRotationTarget.DOLocalRotate(_bodyRotationTarget.InverseTransformDirection(transform.forward),
                    bodyTurnSpeed).SetSpeedBased(true);
        }

        public virtual void TargetLost()
        {
            animator.SetBool(HasTargetHash, false);
        }

        public virtual void TargetForgotten()
        {
            BaseState<TrooperStateMachine> lostState = stateMachine.CurrentRootState.GetDescendantState(StateId.TargetLost);
            if (lostState == null)
            {
                return;
            }
            if (lostState.GetAncestorState(StateId.Sunk) != null)
            {
                lostState.SwitchState(StateId.SunkStruggle);
            }
            else
            {
                lostState.SwitchState(idleBehaviour);
            }
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
            animator.SetBool(SunkHash, true);
        }

        public virtual void Rise()
        {
            animator.SetTrigger(RiseHash);
            animator.SetBool(SunkHash, false);
        }
        
        public IEnumerator StartPaintEscapeAfterTimer()
        {
            yield return _sunkDelay;
            
            BaseState<TrooperStateMachine> sunkState = stateMachine.CurrentRootState.GetDescendantState(StateId.SunkStruggle);
            sunkState?.SwitchState(StateId.SunkEscapePaint);
        }
        
        public virtual void EscapePaint()
        {
            animator.SetTrigger(ShootGroundHash);
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