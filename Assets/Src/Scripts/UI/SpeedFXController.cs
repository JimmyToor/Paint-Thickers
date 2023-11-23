using System;
using Src.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.VFX;

namespace Src.Scripts.UI
{
    public class SpeedFXController : MonoBehaviour
    {
        [Tooltip("The transform whose speed will be tracked.")]
        public Transform speedDriver;
        public float speedThreshold; //Speed threshold where speed lines activate
        public VisualEffect linesVFX;
        
        private PlayerEvents _playerEvents;
        private Vector3 _oldPos;
        private float _velocity;
        
        private void Awake()
        {
            if (speedDriver == null)
            {
                Debug.Log("No driver found for SpeedFXController, defaulting to parent transform.");
                speedDriver = transform;
            }
        }

        private void Start()
        {
            _oldPos = speedDriver.position;
            linesVFX.enabled = true;
        }

        private void Update()
        {
            CalcVelocity();
            if (_velocity >= speedThreshold)
            {
                linesVFX.Play();
            }
            else if (_velocity < speedThreshold)
            {
                linesVFX.Stop();
            }
        }

        private void CalcVelocity()
        {
            Vector3 newPos = speedDriver.position;
            _velocity = Vector3.Distance(_oldPos, newPos) / Time.deltaTime;
            _oldPos = newPos;
        }
        
    }
}
