using System;
using UnityEngine;
using UnityEngine.Serialization;
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
        public float gravityAngleLimit;
        public ActionBasedContinuousMoveProvider locomotion;

        public bool rotating { get; set; } // Represents whether or not a rotation is in-progress
        public bool transforming { get; set; } // Represents whether or not a height change is in-progress
        
        private Vector3 _targetNormal;
        private Player _player;
        private Transform _camOffset;
        private Transform _playerHead;
        private RaycastHit _directionHit;
        private Vector3 _newOrientation;
        private float _targetHeight;
        private XRRig _xrRig;
        private float _wallGravityScale = 0.05f;
        private bool _useFloorOffset = true;
        
        private void Awake()
        {
            if (!locomotion)
            {
                locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
            }
        }
        
        void Start()
        {
            _xrRig = GameObject.Find("XR Rig").GetComponent<XRRig>();
            _player = GetComponent<Player>();
            _camOffset = transform.GetChild(0);
            _playerHead = _camOffset.GetChild(0);
            _targetNormal = transform.up;
        }

        /// <summary>
        /// Set the new normal that the XRRig will rotate to match based on the hit surface's normal.
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="channel"></param>
        /// <returns>True if goal normal is successfully set, false otherwise.</returns>
        public bool SetNewTargetNormal(RaycastHit hit, int channel)
        {
            if (hit.transform == null) return false;
            
            // Only match orientation with this surface if it has friendly paint or is a shallow enough slope.
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (channel != _player.TeamChannel && angle >= gravityAngleLimit) return false;
            
            _targetNormal = hit.normal;
            return true;
        }
        
        /// <summary>
        /// Set the new height that the main camera will be moved towards.
        /// </summary>
        /// <param name="height">The target height.</param>
        /// <param name="useOffset">Determines if the height passed is relative to the floor offset or not.</param>
        /// <returns>True if goal height is successfully set, false otherwise.</returns>
        public bool SetNewTargetHeight(float height, bool useOffset = true)
        {
            _targetHeight = height;
            _useFloorOffset = useOffset;
            return true;
        }

        private void FixedUpdate()
        {
            UpdateOrientation();
            UpdateHeight();
        }

        public void ResetHeight()
        {
            SetNewTargetHeight(0f); // Remove the squid height offset
        }
        
        public void ResetOrientation()
        {
            _targetNormal = Vector3.up;
            locomotion.GravityScale = 1f;
        }

        private void UpdateOrientation()
        {
            ToOrientation(_targetNormal.normalized);
        }

        private void UpdateHeight()
        {
            if (_useFloorOffset)
            {
                ToHeight(_targetHeight);
            }
            else
            {
                ToHeightWithoutOffset(_targetHeight);
            }
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
            
            // Reduce gravity for the rig enough to be able to move up the wall, but still slide down if not moving
            locomotion.GravityScale = newAngle >= gravityAngleLimit ? _wallGravityScale : 1f;
            rotating = transform.up != _targetNormal;
        }
    
        /// <summary>
        /// Set player's head height from the floor, ignoring floor offset and vertical head position.
        /// </summary>
        /// <param name="height"></param>
        public void ToHeightWithoutOffset(float height)
        {
            Vector3 localPosition = _playerHead.localPosition;
            float newHeight = -localPosition.y + height; 
            ToHeight(newHeight);
        }

        /// <summary>
        /// Move the camera offset's y-position towards the passed float to change the height of the player's view
        /// </summary>
        /// <param name="newHeight"></param>
        private void ToHeight(float newHeight)
        {
            Vector3 newPos = _camOffset.localPosition;
            
            newPos.y = Mathf.MoveTowards(newPos.y, newHeight, Time.deltaTime * sinkSpeed);
            _camOffset.localPosition = newPos;
            transforming = !Mathf.Approximately(newPos.y, newHeight);
        }
    }
}
