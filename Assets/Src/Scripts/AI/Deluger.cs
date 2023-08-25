using System;
using System.Collections.Generic;
using DG.Tweening;
using Src.Scripts.AI.States;
using UnityEngine;

namespace Src.Scripts.AI
{
    public class Deluger : Enemy
    {
        public float moveSpeed;
        public float turnSpeed;
        Animator _animator;
        List<Transform> _patrolNodes = new List<Transform>();
        public Transform patrolNodeGroup;
        public int initialPatrolNode;
        public Transform head;
        public Transform feet;
        [HideInInspector] public DelugerStateMachine stateMachine;

        private int _nextNode;

        private static readonly int MovingHash = Animator.StringToHash("Moving");

        protected void Start()
        {
            _animator = gameObject.GetComponent<Animator>();
            if (feet == null )
            {
                feet = transform.Find("wheel_constraint").transform;
                if (feet == null)
                {
                    Debug.LogErrorFormat("{0} cannot find feet under name 'wheel_constraint'", transform.name);
                }
            }
            if (head == null )
            {
                head = transform.Find("Armature/root/top").transform;
                if (head == null)
                {
                    Debug.LogErrorFormat("{0} cannot find head under name 'Armature/root/top'", transform.name);
                }
            }
            
            _nextNode = initialPatrolNode;
            
            if (patrolNodeGroup.childCount != 0)
            {
                foreach (Transform node in patrolNodeGroup)
                    _patrolNodes.Add(node);
            }
            stateMachine = new DelugerStateMachine(this, statesData.stateList);
            stateMachine.SetRootState(StateId.Patrol);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnDisable()
        {
            feet.DOKill();
            head.DOKill();
            transform.DOKill();
        }

        public void StartPatrol()
        { 
            if (_patrolNodes.Count < 1)
            {
                Debug.Log(transform.name + " has nowhere to patrol to.");
                return;
            }
            MovePrep();
        }
        
        // Deluger movement is restricted to a set path of preset nodes
        private void Move()
        {
            Vector3 nextNodePos = _patrolNodes[_nextNode].position;
            Vector3 position = transform.position;
            nextNodePos.y = position.y;
            transform.DOMove(nextNodePos, moveSpeed).SetSpeedBased(true).SetRecyclable(true).OnComplete(OnMoveComplete);
            _animator.SetBool(MovingHash, true);
        }

        private void OnMoveComplete()
        {
            _animator.SetBool(MovingHash, false);
            _nextNode++;
            _nextNode %= _patrolNodes.Count; // loops back to start of the list if we've reached the end 
            MovePrep();
        }

        // Rotates head and feet to point at the next node
        public void MovePrep()
        {
            Vector3 nextLookPos = _patrolNodes[_nextNode].position;
            nextLookPos.y = head.position.y;

            feet.DOLookAt(nextLookPos, turnSpeed);
            head.DOLookAt(nextLookPos,turnSpeed).OnComplete(Move);
        }
    }
}
