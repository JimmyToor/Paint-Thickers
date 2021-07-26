using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;


public class Flooder : Enemy
{
    ParticleSystem spray;
    [SerializeField]
    List<Transform> patrolNodes = new List<Transform>();
    int currNode = 0;
    float speed = 2f;


    private void Update()
    {
        if (!DOTween.IsTweening(transform))
            Move();
    }
    void DisableSpray()
    {
        spray.Pause();
    }

    void EnableSpray()
    {
        spray.Play();
    }

    // Flooder movement is restricted to a set path of preset nodes
    void Move()
    {
        Vector3 nextNodePos = patrolNodes[currNode].position;
        nextNodePos.y = transform.position.y;
        transform.DOMove(nextNodePos, Vector3.Distance(transform.position,nextNodePos)/speed).SetRecyclable(true).OnComplete(OnMoveComplete);
        Debug.Log(currNode);
    }

    void OnMoveComplete()
    {
        currNode++;
        currNode %= patrolNodes.Count; // loops back to start of array if we've reached the end 
    }
}
