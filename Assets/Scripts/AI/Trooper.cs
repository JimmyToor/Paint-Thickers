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
        ParticleSystem paintSpray;
        [SerializeField]
        Transform bodyRotate;
        [SerializeField]
        Transform aimRotate;
        [SerializeField]
        public float turnSpeed;
        public float aimSpeed;
        Tweener bodyTweener;
        Tweener aimTweener;
        private Animator animator;
        private int targetFoundHash = Animator.StringToHash("TargetFound");
        private int shootHash = Animator.StringToHash("Shoot");
        private int offsetHash = Animator.StringToHash("Offset");

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
        }


        // Searches for and targets the nearest player
        // Ignores obstacles
        void TargetSearch()
        {
            Collider[] playersHit = new Collider[1];
            int size = Physics.OverlapSphereNonAlloc(transform.position, sightDistance, playersHit, LayerMask.GetMask("Players"));
            if (size > 0)
            {
                animator.SetBool(targetFoundHash, true);
                EnableSpray();
                EngageTarget(playersHit[0].transform);
            }
            else
            {
                animator.SetBool(targetFoundHash, false);
                DisableSpray();
            }
        }

        // Aim at the target
        void EngageTarget(Transform target)
        {
            if (!bodyTweener.IsActive())
                bodyTweener = bodyRotate.DOLookAt(target.position,turnSpeed,AxisConstraint.Y).SetSpeedBased(true);
            else
                bodyTweener.ChangeEndValue(target.position, true); // Don't make a new tween if one is active, just change the current one
        
            // Only move the nozzle if the target is in front us
            if (Vector3.Angle(transform.position,target.position) < 70)
            {
                if (!aimTweener.IsActive())
                    aimTweener = aimRotate.DOLookAt(target.position,aimSpeed,AxisConstraint.None,Vector3.up).SetSpeedBased(true);
                else
                    aimTweener.ChangeEndValue(target.position, true);
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
            }
        }

        private void OnDisable()
        {
            DisableSpray();
            CancelInvoke();
        }
    }
}
