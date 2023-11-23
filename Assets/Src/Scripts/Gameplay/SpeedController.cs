using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Gameplay
{
    public class SpeedController : MonoBehaviour
    {
        public ActionBasedContinuousMoveProvider locomotion;
        [Tooltip("Controls how much move speed will increase or decrease over a second.")]
        public float speedTransitionStep;
        
        /// <summary>
        /// The target speed to be transitioned to over time.
        /// </summary>
        public float GoalSpeed
        {
            get => _goalSpeed;
            set
            {
                _goalSpeed = value;
                _sign = Mathf.Sign(_goalSpeed - locomotion.moveSpeed);
            } 
        }
        
        private float _goalSpeed;
        private float _sign;


        // Start is called before the first frame update
        void Start()
        {
            GoalSpeed = locomotion.moveSpeed;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (Mathf.Approximately(locomotion.moveSpeed,
                    GoalSpeed)) return; 
            
            // Move the current speed closer to the goal speed
            UpdateSpeed();
        }
        
        private void UpdateSpeed()
        {
            var speedRemaining = Mathf.Abs(GoalSpeed - locomotion.moveSpeed);
            var delta = _sign * Mathf.Min(Time.deltaTime * speedTransitionStep, speedRemaining);
            locomotion.moveSpeed += delta;
        }
    }
}
