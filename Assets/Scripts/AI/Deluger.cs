using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;


public class Deluger : Enemy
{
    [SerializeField] 
    ParticleSystem paintSpray;
    Animator animator;
    List<Transform> patrolNodes = new List<Transform>();
    public Transform patrolNodeGroup;
    public Transform head;
    Transform feet;
    int nextNode = 0;
    [SerializeField]
    float moveSpeed = 2f;
    [SerializeField]
    float turnSpeed = 1f;
    private void Start() 
    {
        animator = gameObject.GetComponent<Animator>();
        feet = transform.Find("wheel_constraint").transform;
        if (patrolNodeGroup.childCount != 0)
        {
            foreach (Transform node in patrolNodeGroup)
                patrolNodes.Add(node);
            MovePrep();
        }
    }

    void DisableSpray()
    {
        paintSpray.Stop();
    }

    void EnableSpray()
    {
        paintSpray.Play();
    }

    // Flooder movement is restricted to a set path of preset nodes
    void Move()
    {
        Vector3 nextNodePos = patrolNodes[nextNode].position;
        nextNodePos.y = transform.position.y;
        transform.DOMove(nextNodePos, Vector3.Distance(transform.position,nextNodePos)/moveSpeed).SetRecyclable(true).OnComplete(OnMoveComplete);
        animator.SetBool("Moving", true);
    }

    void OnMoveComplete()
    {
        animator.SetBool("Moving", false);
        nextNode++;
        nextNode %= patrolNodes.Count; // loops back to start of the list if we've reached the end 
        MovePrep();
    }

    // Rotates head and feet to point at the next node
    void MovePrep()
    {
        Vector3 nextLookPos = patrolNodes[nextNode].position;
        nextLookPos.y = head.position.y;

        feet.DOLookAt(nextLookPos, turnSpeed);

        head.DOLookAt(nextLookPos,turnSpeed).OnComplete(Move);
    }
}
