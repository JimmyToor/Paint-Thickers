using Src.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.VFX;

namespace Src.Scripts.UI
{
    public class SpeedFXController : MonoBehaviour
    {
        private PlayerEvents playerEvents;
        private Vector3 oldPos;
        private float velocity;

        public float speedThreshold; //Speed threshold where speed lines activate
        public VisualEffect linesVFX;
    
        private void Start()
        {
            oldPos = transform.position;
            linesVFX.enabled = true;
        }

        private void Update()
        {
            CalcVelocity();
            if (velocity >= speedThreshold)
            {
                linesVFX.Play();
            }
            else if (velocity < speedThreshold)
            {
                linesVFX.Stop();
            }
        }

        private void CalcVelocity()
        {
            Vector3 newPos = transform.position;
            velocity = Vector3.Distance(oldPos, newPos) / Time.deltaTime;
            oldPos = newPos;
        }
        
    }
}
