using System;
using Src.Scripts.AI;
using Src.Scripts.Audio;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

// Handle swimming in paint and squid related movement
namespace Src.Scripts.Gameplay
{
    [RequireComponent(typeof(Player),typeof(OrientationHandling))]
    public class PaintSwim : MonoBehaviour
    {
        public ActionBasedContinuousMoveProvider locomotion; 
        [Tooltip("Mask for swimmable layers")]
        public LayerMask swimmableLayers;
        [Tooltip("Speed of squid out of paint")]
        public float squidSpeed; 
        [Tooltip("Speed of squid in paint")]
        public float swimSpeed;
        [Tooltip("Speed in enemy paint as human or squid")]
        public float enemyPaintSpeed;
        [Tooltip("Controls how much move speed will increase or decrease over a second.")]
        public float speedTransitionStep;
        [Tooltip("Used to check for terrain changes in the direction we're moving")]
        public Transform frontCheck;
        [HideInInspector]
        public bool normalSet;
        public AudioSource swimSound;
        public SFXSource sinkSounds;

        /// <summary>
        /// Tracks if the player is in paint of any colour.
        /// <remarks>Can affect <paramref name="CanSwim"/> when set to prevent contradicting values.</remarks>
        /// </summary>
        public bool InPaint
        {
            get => _inPaint;
            set
            {
                _inPaint = value;
                if (!value)
                {
                    _canSwim = false; // Can't swim if not in paint
                }
            }
        }
        
        /// <summary>
        /// Tracks if the player is in paint they can swim in.
        /// <remarks>Can affect <paramref name="InPaint"/> when set to prevent contradicting values.</remarks>
        /// </summary>
        public bool CanSwim
        {
            get => _canSwim;
            set
            {
                _canSwim = value;
                if (value)
                {
                    _inPaint = true; // Must be in paint if we can swim
                }
            }
        }
        
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

        private Player _player;
        private PlayerEvents _playerEvents;
        private Transform _playerHead;
        private Transform _camOffset;
        private Transform _frontCheckAxis;
        private Vector3 _direction;
        private LayerMask _squidLayer;
        private LayerMask _playerLayer;
        private float _standSpeed;
        private float _frontAngle;
        private OrientationHandling _orientationHandling;
        private RaycastHit _belowHit;
        private RaycastHit _frontHit;
        private bool _inPaint;
        private bool _canSwim;
        private bool _speedTransitioning;
        private CharacterController _charController;
        private float _goalSpeed;
        private float _sign;

        private void OnEnable()
        {
            _player = GetComponent<Player>();
            _playerEvents = GetComponent<PlayerEvents>();
            SetupEvents();
        }

        private void Awake()
        {
            if (locomotion == null && TryGetComponent<ActionBasedContinuousMoveProvider>(out locomotion))
            {
                Debug.LogError("No Locomotion Provider on PaintSwim!", this);
            }
            
            _orientationHandling = GetComponent<OrientationHandling>();
            _charController = GetComponent<CharacterController>();
            _squidLayer = LayerMask.NameToLayer("Squid");
            _playerLayer = LayerMask.NameToLayer("Players");
        }

        void Start()
        {
            _camOffset = transform.GetChild(0);
            _playerHead = _camOffset.GetChild(0);
            _frontCheckAxis = frontCheck.parent;
            GoalSpeed = locomotion.moveSpeed;
        }

        private void Update()
        {
            if (!Mathf.Approximately(locomotion.moveSpeed, GoalSpeed))
            {   // Move the current speed closer to the goal speed
                float speedRemaining = Mathf.Abs(GoalSpeed - locomotion.moveSpeed);
                float delta = _sign * Mathf.Min(Time.deltaTime * speedTransitionStep,speedRemaining);
                locomotion.moveSpeed += delta;
            }
        }

        private void FixedUpdate()
        {
            if (!_player.isSquid)
            {
                return;
            }
            
            CheckTerrain();
            Swim();
        }

        private void CheckTerrain()
        {
            // Check ahead first so we can adjust to slopes and walls
            if (!CheckGroundAhead() & !CheckGroundBelow())
            {
                _orientationHandling.ResetOrientation();
            }
        }

        /// <summary>
        /// Look for terrain changes under the main camera
        /// </summary>
        /// <returns>True if something is below us, false otherwise</returns>
        private bool CheckGroundBelow()
        {
            //Debug.DrawRay(_playerHead.position,-_camOffset.up,Color.blue,2f);
            if (!Physics.Raycast(_playerHead.position, -_camOffset.up, out _belowHit, 1f, swimmableLayers) 
                || _belowHit.transform == null) return false;
            
            int channel = PaintTarget.RayChannel(_belowHit);

            // Figure out what colour paint, if any, is underneath the player
            if (channel == _player.TeamChannel)
            {
                if (!CanSwim) // Player was previously not in swimmable paint
                {
                    sinkSounds.TriggerPlay(_playerHead.position);
                }
                CanSwim = true;
            }
            else if (channel != -1)
            {
                InPaint = true;
                CanSwim = false;
            }
            else
            {
                InPaint = false;
            }
            
            if (!normalSet) // Don't want to change the normal if it's been set by terrain ahead
            {
                _orientationHandling.SetNewTargetNormal(_belowHit, channel);
            }

            return true;
        }

        /// <summary>
        /// Look for terrain changes under frontCheck
        /// </summary>
        /// <returns>True if something is ahead of us, false otherwise</returns>
        private bool CheckGroundAhead()
        {
            //Debug.DrawRay(_frontCheckAxis.position,-frontCheck.up,Color.green,1f);

            if (!Physics.Raycast(frontCheck.position, -frontCheck.up, out _frontHit, 1f, swimmableLayers)
                || _frontHit.transform == null)
            {
                normalSet = false;
                return false;
            }

            normalSet = _orientationHandling.SetNewTargetNormal(_frontHit, PaintTarget.RayChannel(_frontHit));
            return true;
        }

        private void SetupEvents()
        {
            _playerEvents.Squid += HandleSwimActivation;
            _playerEvents.Stand += HandleStand;
            _playerEvents.Move += HandleMove;
        }

        private void OnDisable() {
            _playerEvents.Squid -= HandleSwimActivation;
            _playerEvents.Stand -= HandleStand;
        }

        private void HandleSwimActivation()
        {
            if (!_player.canSquid || _player.isSquid) return;
            
            locomotion.SlopeHandling = false;
            gameObject.layer = _squidLayer;
        }

        private void HandleStand()
        {
            locomotion.SlopeHandling = true;
            gameObject.layer = _playerLayer;
            GoalSpeed = _player.walkSpeed;
            swimSound.Stop();
            CanSwim = false;
            _orientationHandling.ResetOrientation();
            _orientationHandling.ResetHeight();
        }

        // Adjust speed, height, and SFX while swimming
        private void Swim()
        {
            if (CanSwim) // Indicates we are swimming in friendly paint
            {
                _orientationHandling.SetNewTargetHeight(OrientationHandling.SwimHeight, false);
                GoalSpeed = swimSpeed;

                _player.RefillWeaponAmmo();
            
                if (!swimSound.isPlaying)
                {
                    if (!(_charController.velocity.sqrMagnitude > 1f)) return;
                    
                    swimSound.time = Random.Range(0f, swimSound.clip.length);
                    swimSound.Play();
                }
                else if (_charController.velocity.sqrMagnitude <= 1f)
                {
                    swimSound.Stop();
                }
            }
            else
            {
                _orientationHandling.SetNewTargetHeight(OrientationHandling.SquidHeight, false);

                if (swimSound.isPlaying)
                {
                    swimSound.Stop();
                }
                
                GoalSpeed = !InPaint ? squidSpeed : enemyPaintSpeed;
            }
        }

        // Keep frontCheck in the direction the player moving
        private void HandleMove(Vector3 newDirection)
        {
            _direction = _playerHead.InverseTransformDirection(locomotion.LatestRelativeInput);
            float newAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
            Vector3 currAngles = _frontCheckAxis.localEulerAngles;
            currAngles.y = newAngle;
            _frontCheckAxis.localEulerAngles = currAngles;
        }
    }
}
