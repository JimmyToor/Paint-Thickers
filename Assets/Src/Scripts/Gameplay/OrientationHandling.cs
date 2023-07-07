using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Gameplay
{
    [RequireComponent(typeof(Player))]
// Handle height and matching terrain orientation in squid form
    public class OrientationHandling : MonoBehaviour
    {
        public const float SquidHeight = 0.6f;
        public const float SwimHeight = 0.4f;

        public float rotationSpeed;
        public float sinkSpeed; // speed of squid transformation
        [Tooltip("The angle at which rig-relative gravity is enabled.")]
        public float _gravityAngleLimit;
        public ActionBasedContinuousMoveProvider locomotion;

        public bool Rotating { get; set; } // Represents whether or not a rotation is in-progress
        public bool Transforming { get; set; } // Represents whether or not a height change is in-progress
        
        private Vector3 _goalNormal;
        private Player _player;
        private Transform _camOffset;
        private Transform _playerHead;
        private RaycastHit _directionHit;
        private Vector3 _newOrientation;
        private float _targetHeight;
        private XRRig _xrRig;
        private float _wallGravityScale = 0.05f;
        private CharacterController _charController;

        void Start()
        {
            _xrRig = GameObject.Find("XR Rig").GetComponent<XRRig>();
            _player = GetComponent<Player>();
            _camOffset = transform.GetChild(0);
            _playerHead = _camOffset.GetChild(0);
            _charController = _xrRig.GetComponent<CharacterController>();
            _goalNormal = transform.up;
        }

        private void Awake()
        {
            if (!locomotion)
            {
                locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
            }
        }

        public void UpdateOrientation()
        {
            ToOrientation(_goalNormal.normalized);
        }

        /// <summary>
        /// Set the new normal that the XRRig will rotate to match based on the hit surface's normal.
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="channel"></param>
        /// <returns>True if goal normal is successfully set, false otherwise.</returns>
        public bool SetNewGoalNormal(RaycastHit hit, int channel)
        {
            if (hit.transform == null) return false;
            
            // Only match orientation with this surface if it has friendly paint or is a small enough slope.
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (channel != _player.TeamChannel && angle >= _gravityAngleLimit) return false;
            
            _goalNormal = hit.normal;
            return true;
        }

        public void ResetOrientation()
        {
            _goalNormal = Vector3.up;
            UpdateOrientation();
        }

        /// <summary>
        /// Rotate the rig normal towards the passed normal vector
        /// </summary>
        /// <param name="newUp"></param>
        private void ToOrientation(Vector3 newUp)
        {
            if (newUp == transform.up) return; // Already at this orientation, nothing to do
            
            // Get the tangent axis on which the rig will rotate
            Vector3 axis = Vector3.Cross(newUp, transform.up);
            if (axis == Vector3.zero)
            {
                axis = Vector3.Cross(newUp, transform.forward);
            }

            float angle = Vector3.SignedAngle(transform.up, newUp, axis);
            float rotationAmount;
            
            if (Mathf.Abs(angle) <= rotationSpeed)
            {   // Rotate only the remaining degrees to prevent over-rotation
                rotationAmount = angle;
            }
            else if (angle > 0)
            {
                rotationAmount = rotationSpeed;
            }
            else
            {
                rotationAmount = -rotationSpeed;
            }

            _xrRig.RotateAroundCameraPosition(axis, rotationAmount);
            
            float newAngle = Vector3.Angle(newUp, Vector3.up);
        
            if (_charController.isGrounded && newAngle >= _gravityAngleLimit)
            {
                // Reduce gravity for the rig enough to be able to move up the wall, but still slide down if not moving
                locomotion.GravityScale = _wallGravityScale;
            }
            else
            {
                locomotion.UseRigRelativeGravity = false;
                locomotion.GravityScale = 1f;
            }

            Rotating = transform.up != _goalNormal;
        }

        public void ResetHeight()
        {
            TowardsHeight(0); // Remove the squid height offset
        }
    
        /// <summary>
        /// Set player's head height from the floor, ignoring floor offset and vertical head position
        /// </summary>
        /// <param name="height"></param>
        public void ToHeightWithoutOffset(float height)
        {
            Vector3 localPosition = _playerHead.localPosition;
            float newHeight = -localPosition.y + height; 
            TowardsHeight(newHeight);
            Transforming = !Mathf.Approximately(-localPosition.y, newHeight);
        }

        /// <summary>
        /// Move the camera offset's y-position towards the passed float to change the height of the player's view
        /// </summary>
        /// <param name="newHeight"></param>
        private void TowardsHeight(float newHeight)
        {
            Vector3 newPos = _camOffset.localPosition;
            if (Mathf.Approximately(newPos.y, newHeight))
                return;
            
            Debug.Log("moving " + newPos.y + " to " + newHeight);
            newPos.y = Mathf.MoveTowards(newPos.y, newHeight, Time.deltaTime * sinkSpeed);
            _camOffset.localPosition = newPos;
        }
    }
}
