using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Gameplay
{
    [RequireComponent(typeof(Player))]
// Handle height and matching terrain orientation in squid form
    public class OrientationHandling : MonoBehaviour
    {
        public const float SquidHeight = 0.5f;
        public const float SwimHeight = 0.3f;
    
        public Vector3 NewNormal { get; set; }
        public float rotationSpeed;
        public float sinkSpeed; // speed of squid transformation
        public ActionBasedContinuousMoveProvider locomotion;

        private Player _player;
        private Transform _camOffset;
        private Transform _playerHead;
        private RaycastHit _directionHit;
        private Vector3 _newOrientation;
        private float _targetHeight;
        private float _slopeLimit;
        private bool _orienting;

        void Start()
        {
            _player = GetComponent<Player>();
            _camOffset = transform.GetChild(0);
            _playerHead = _camOffset.GetChild(0);
            NewNormal = Vector3.zero;
        }

        private void Awake()
        {
            _slopeLimit = GetComponent<CharacterController>().slopeLimit;
            if (!locomotion)
            {
                locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
            }
        }

        public void UpdateOrientation()
        {
            if (NewNormal == Vector3.zero) 
            { 
                NewNormal = Vector3.up; // reset our orientation since there was nothing to match orientation with
            }
        
            ToOrientation(NewNormal.normalized);
 
            NewNormal = Vector3.zero;
        }

        // Set the new normal that the XRRig will rotate to match
        public bool SetNewNormal(RaycastHit hit, int channel)
        {
            if (hit.transform != null) // Only factor in this surface if it has friendly paint or is a small enough slope
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                if (channel == _player.teamChannel || angle < _slopeLimit)
                {
                    NewNormal = hit.normal;
                    return true;
                }
            }

            return false;
        }

        public void ResetOrientation()
        {
            ToOrientation(Vector3.up);
        }

        // Rotate the rig normal towards the passed normal vector
        private void ToOrientation(Vector3 newUp)
        {
            if (newUp == transform.up)
            {
                return; // Already at this orientation, nothing to do
            }
        
            Quaternion currRot = transform.rotation;
            var newRotation = Quaternion.FromToRotation(transform.up, newUp) * currRot;

            transform.rotation = Quaternion.RotateTowards(currRot, newRotation, Time.deltaTime * rotationSpeed);

            float newAngle = Vector3.Angle(newUp, Vector3.up);
        
            if (newAngle > _slopeLimit)
            {
                // Reduce gravity for the rig enough to be able to move up the wall, but still slide down if not moving
                locomotion.UseRigRelativeGravity = true;
                locomotion.GravityScale = 0.05f;
            }
            else
            {
                locomotion.UseRigRelativeGravity = false;
                locomotion.GravityScale = 1f;
            }
        }

        public void ResetHeight()
        {
            ToHeight(0); // Remove the squid height offset
        }
    
        // Set player's head height from the floor, ignoring floor offset and vertical head position
        public void ToHeightWithoutOffset(float height)
        {
            float newHeight = -_playerHead.localPosition.y + height; 
            ToHeight(newHeight);
        }

        // Move the camera offset's y-position towards the passed float to change the height of the player's view
        public void ToHeight(float newHeight)
        {
            Vector3 newPos = _camOffset.localPosition;
            newPos.y = Mathf.MoveTowards(newPos.y, newHeight, Time.deltaTime * sinkSpeed);
            _camOffset.localPosition = newPos;
        }
    }
}
