using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Trooper : Enemy
{
    [SerializeField]
    ParticleSystem paintSpray;
    Transform target;
    [SerializeField]
    Transform bodyRotate;
    [SerializeField]
    Transform aimRotate;
    [SerializeField]
    float turnSpeed = 10f;
    float aimSpeed = 25f;
    Tweener bodyTweener;
    Tweener aimTweener;
    public float fireInterval;
    private Animator animator;
    private int targetFoundHash = Animator.StringToHash("TargetFound");
    private int shootHash = Animator.StringToHash("Shoot");
    
    // Player in sight? ->Yes-> Attack
    //                  ->No-> Idle
    void Start()
    {
        InvokeRepeating("TargetSearch",1.0f, 0.2f);
        TryGetComponent(out animator);
    }

    // Searches for and targets the nearest player
    void TargetSearch()
    {
        Collider[] playersHit = new Collider[1];
        int size = Physics.OverlapSphereNonAlloc(transform.position, 10f, playersHit, LayerMask.GetMask("Players"));
        if (size > 0)
        {
            target = playersHit[0].transform;
            animator.SetBool(targetFoundHash, true);
            EnableSpray();
            EngageTarget(target);
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
        if (Vector3.Angle(transform.position,target.position) < 90)
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

}
