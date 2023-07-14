﻿using UnityEngine.Assertions;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to smoothly move their rig continuously over time.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    public abstract class ContinuousMoveProviderBase : LocomotionProvider
    {
        public Vector3 LatestRelativeInput { get; private set; }
        
        /// <summary>
        /// Controls whether gravity is relative to the rig or the world.
        /// </summary>
        public bool UseRigRelativeGravity { get; set; }
        
        /// <summary>
        /// Multiply the force gravity applies to the rig.
        /// </summary>
        public float GravityScale { get; set; } = 1;
        
        /// <summary>
        /// Apply extra downward force to reduce jitter on slopes.
        /// </summary>
        public bool SlopeHandling { get; set; } = true;
        
        /// <summary>
        /// Defines when gravity will begin to take effect.
        /// </summary>
        /// <seealso cref="gravityApplicationMode"/>
        public enum GravityApplicationMode
        {
            /// <summary>
            /// Only begin to apply gravity and apply locomotion when a move input occurs.
            /// When using gravity, will then continue applying each frame, even if input is stopped, until touching ground.
            /// </summary>
            /// <remarks>
            /// Use this style when you don't want gravity to apply when the player physically walks away and off a ground surface.
            /// Gravity will only begin to move the player back down to the ground when they try to use input to move.
            /// </remarks>
            AttemptingMove,

            /// <summary>
            /// Apply gravity and apply locomotion every frame, even without move input.
            /// </summary>
            /// <remarks>
            /// Use this style when you want gravity to apply when the player physically walks away and off a ground surface,
            /// even when there is no input to move.
            /// </remarks>
            Immediately,
        }

        [SerializeField]
        [Tooltip("The speed, in units per second, to move forward.")]
        float m_MoveSpeed = 1f;
        /// <summary>
        /// The speed, in units per second, to move forward.
        /// </summary>
        public float moveSpeed
        {
            get => m_MoveSpeed;
            set => m_MoveSpeed = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to enable strafing (sideways movement).")]
        bool m_EnableStrafe = true;
        /// <summary>
        /// Controls whether to enable strafing (sideways movement).
        /// </summary>
        public bool enableStrafe
        {
            get => m_EnableStrafe;
            set => m_EnableStrafe = value;
        }

        [SerializeField]
        [Tooltip("Controls whether gravity affects this provider when a Character Controller is used.")]
        bool m_UseGravity = true;
        /// <summary>
        /// Controls whether gravity affects this provider when a <see cref="CharacterController"/> is used.
        /// </summary>
        public bool useGravity
        {
            get => m_UseGravity;
            set => m_UseGravity = value;
        }

        [SerializeField]
        [Tooltip("Controls when gravity begins to take effect.")]
        GravityApplicationMode m_GravityApplicationMode;
        /// <summary>
        /// Controls when gravity begins to take effect.
        /// </summary>
        /// <seealso cref="GravityApplicationMode"/>
        public GravityApplicationMode gravityApplicationMode
        {
            get => m_GravityApplicationMode;
            set => m_GravityApplicationMode = value;
        }

        [SerializeField]
        [Tooltip("The source Transform to define the forward direction.")]
        Transform m_ForwardSource;
        /// <summary>
        /// The source <see cref="Transform"/> to define the forward direction.
        /// </summary>
        public Transform forwardSource
        {
            get => m_ForwardSource;
            set => m_ForwardSource = value;
        }

        CharacterController m_CharacterController;

        bool m_AttemptedGetCharacterController;

        Vector3 m_VerticalVelocity;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            var xrRig = system.xrRig;
            if (xrRig == null)
                return;

            var input = ReadInput();
            var translationInWorldSpace = ComputeDesiredMove(input);

            switch (m_GravityApplicationMode)
            {
                case GravityApplicationMode.Immediately:
                    MoveRig(translationInWorldSpace);
                    break;
                case GravityApplicationMode.AttemptingMove:
                    if (input != Vector2.zero || m_VerticalVelocity != Vector3.zero)
                    {
                        MoveRig(translationInWorldSpace);
                    }

                    break;
                default:
                    Assert.IsTrue(false, $"{nameof(m_GravityApplicationMode)}={m_GravityApplicationMode} outside expected range.");
                    break;
            }
        }

        /// <summary>
        /// Reads the current value of the move input.
        /// </summary>
        /// <returns>Returns the input vector, such as from a thumbstick.</returns>
        protected abstract Vector2 ReadInput();

        /// <summary>
        /// Determines how much to slide the rig due to <paramref name="input"/> vector.
        /// </summary>
        /// <param name="input">Input vector, such as from a thumbstick.</param>
        /// <returns>Returns the translation amount in world space to move the rig.</returns>
        protected virtual Vector3 ComputeDesiredMove(Vector2 input)
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
            var forwardRotation = Quaternion.FromToRotation(rigTransform.forward, inputForwardProjectedInWorldSpace);

            LatestRelativeInput = forwardRotation*rigTransform.TransformVector(inputMove);
            
            var translationInWorldSpace = LatestRelativeInput * (moveSpeed * Time.deltaTime);

            return translationInWorldSpace;
        }

        /// <summary>
        /// Creates a locomotion event to move the rig by <paramref name="translationInWorldSpace"/>,
        /// and optionally applies gravity.
        /// </summary>
        /// <param name="translationInWorldSpace">The translation amount in world space to move the rig (pre-gravity).</param>
        protected virtual void MoveRig(Vector3 translationInWorldSpace)
        {
            var xrRig = system.xrRig;
            if (xrRig == null)
                return;

            FindCharacterController();

            var motion = translationInWorldSpace;

            if (m_CharacterController != null && m_CharacterController.enabled)
            {
                // Step vertical velocity from gravity
                if (m_CharacterController.isGrounded || !m_UseGravity)
                {
                    m_VerticalVelocity = Vector3.zero;
                }
                else if (m_UseGravity)
                {
                    if (UseRigRelativeGravity)
                    {
                        m_VerticalVelocity += xrRig.transform.TransformVector(Physics.gravity * Time.deltaTime);
                    }
                    else
                    {
                        Vector3 scaledGravity = GravityScale * Physics.gravity;
                        m_VerticalVelocity += scaledGravity * Time.deltaTime;
                    }
                }

                if (SlopeHandling)
                {
                    m_VerticalVelocity += Vector3.down*2;
                }
                motion += m_VerticalVelocity * Time.deltaTime;
                
                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    // Note that calling Move even with Vector3.zero will have an effect by causing isGrounded to update
                    m_CharacterController.Move(motion);
    
                    EndLocomotion();
                }
            }
            else if (CanBeginLocomotion() && BeginLocomotion())
            {
                xrRig.rig.transform.position += motion;
                EndLocomotion();
            }
        }

        void FindCharacterController()
        {
            var xrRig = system.xrRig;
            if (xrRig == null)
                return;

            // Save a reference to the optional CharacterController on the rig GameObject
            // that will be used to move instead of modifying the Transform directly.
            if (m_CharacterController == null && !m_AttemptedGetCharacterController)
            {
                m_CharacterController = xrRig.rig.GetComponent<CharacterController>();
                m_AttemptedGetCharacterController = true;
            }
        }
    }
}
