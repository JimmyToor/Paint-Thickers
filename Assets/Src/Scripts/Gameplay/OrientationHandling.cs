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
        public float gravityAngleLimit;
        public ActionBasedContinuousMoveProvider locomotion;
        
        private Vector3 _targetNormal;
        private Transform _camOffset;
        private Transform _playerHead;
        private RaycastHit _directionHit;
        private Vector3 _newOrientation;
        private float _targetHeight;
        private XRRig _xrRig;
        private float _wallGravityScale = 0.05f;
        private bool _useFloorOffset = true;
        private Vector3 _rotatePos;
        private float gravityAngleLimitDot; // Pre-calculated dot product of the angle limit

        
        private void Awake()
        {
            if (!locomotion)
            {
                locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
            }
            // Pre calculate the dot product of the angle limit for performance
            gravityAngleLimitDot = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        }
        
        void Start()
        {
            _xrRig = GameObject.Find("XR Rig").GetComponent<XRRig>();
            _camOffset = GameObject.Find("Camera Offset").transform;
            _playerHead = GameObject.Find("Main Camera").transform;
            _targetNormal = transform.up;
        }

        /// <summary>
        /// Set the new normal that the XRRig will rotate to match based on the hit surface's normal.
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="useAngleLimit">When true, the target normal will only be set if it is below the angle limit.</param>
        /// <param name="targetPosition">The position to rotate around when matching this normal. Defaults to head.</param>
        /// <returns>True if goal normal is successfully set, false otherwise.</returns>
        public bool SetNewTargetNormal(Vector3 normal, bool useAngleLimit = false, Vector3 targetPosition = default)
        {
            // Only match orientation with this surface if it has friendly paint or is a shallow enough slope.
            if (useAngleLimit && normal.y <= gravityAngleLimitDot)
            {
                return false;
            }
            _targetNormal = normal;
            _rotatePos = targetPosition;
            return true;
        }
        
        /// <summary>
        /// Set the new height that the main camera will be moved towards.
        /// </summary>
        /// <param name="height">The target height.</param>
        /// <param name="useOffset">Determines if the height passed is relative to the floor offset or not.</param>
        /// <returns>True if goal height is successfully set, false otherwise.</returns>
        public void SetNewTargetHeight(float height, bool useOffset = true)
        {
            _targetHeight = height;
            _useFloorOffset = useOffset;
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

        public void UpdateOrientation()
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
            float absAngle = Mathf.Abs(angle);
            
            float rotationAmount;
            if (absAngle <= rotationSpeed)
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

            if (_rotatePos == default)
            {
                _xrRig.RotateAroundCameraPosition(axis, rotationAmount);
            }
            else
            {
                _xrRig.RotateAroundPosition(_rotatePos, axis, rotationAmount);
            }
            
            
            // Reduce gravity for the rig enough to be able to move up the wall, but still slide down if not moving
            locomotion.GravityScale = newUp.y <= gravityAngleLimitDot ? _wallGravityScale : 1f;
        }
    
        /// <summary>
        /// Set player's head height from the floor, ignoring floor offset and vertical head position.
        /// </summary>
        public void ToHeightWithoutOffset(float height)
        {
            Vector3 localPosition = _playerHead.localPosition;
            float newHeight = -localPosition.y + height; 
            ToHeight(newHeight);
        }

        /// <summary>
        /// Move the camera offset's y-position towards the provided <paramref name="newHeight"/>
        /// to change the height of the player's view
        /// </summary>
        private void ToHeight(float newHeight)
        {
            Vector3 newPos = _camOffset.localPosition;
            
            newPos.y = Mathf.MoveTowards(newPos.y, newHeight, Time.deltaTime * sinkSpeed);
            _camOffset.localPosition = newPos;
        }
    }
}
