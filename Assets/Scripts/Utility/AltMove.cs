using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    public class AltMove : ActionBasedContinuousMoveProvider
    {
        public bool slopeHandling = true;
        // Alternative method of calculating movement translation that works better with X and Z rotations, which we need for swimming
        protected override Vector3 ComputeDesiredMove(Vector2 input)
        {
            if (input == Vector2.zero)
                return Vector3.zero;

            var xrRig = system.xrRig;
            if (xrRig == null)
                return Vector3.zero;

            // Assumes that the input axes are in the range [-1, 1].
            // Clamps the magnitude of the input direction to prevent faster speed when moving diagonally,
            // while still allowing for analog input to move slower (which would be lost if simply normalizing).
            var inputMove = Vector3.ClampMagnitude(new Vector3(enableStrafe ? input.x : 0f, 0f, input.y), 1f);

            var rigTransform = xrRig.rig.transform;
            var rigUp = rigTransform.up;

            // Determine frame of reference for what the input direction is relative to
            var forwardSourceTransform = forwardSource == null ? xrRig.cameraGameObject.transform : forwardSource;
            var inputForwardInWorldSpace = forwardSourceTransform.forward;
            
            if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace, rigUp)), 1f))
            {
                // When the input forward direction is parallel with the rig normal,
                // it will probably feel better for the player to move along the same direction
                // as if they tilted forward or up some rather than moving in the rig forward direction.
                // It also will probably be a better experience to at least move in a direction
                // rather than stopping if the head/controller is oriented such that it is perpendicular with the rig.
                inputForwardInWorldSpace = -forwardSourceTransform.up;
            }

            var inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(inputForwardInWorldSpace, rigUp);
            var forwardRotation = Quaternion.AngleAxis(Vector3.Angle(rigTransform.forward,inputForwardProjectedInWorldSpace), rigUp);
            
            if (slopeHandling) // Prevent bouncing down slopes
            {
                inputMove += Vector3.down;
            }
            
            var translationInRigSpace = forwardRotation  * inputMove * (moveSpeed * Time.deltaTime); 
            var translationInWorldSpace = rigTransform.TransformDirection(translationInRigSpace);
            
            
            
            return translationInWorldSpace;
        }
    }
}
