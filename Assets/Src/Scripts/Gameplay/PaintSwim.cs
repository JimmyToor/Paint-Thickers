using Src.Scripts.Audio;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;


namespace Src.Scripts.Gameplay
{
    /// <summary>
    /// Handle swimming in paint and squid related movement
    /// </summary>
    [RequireComponent(typeof(OrientationHandling))]
    public class PaintSwim : MonoBehaviour
    {
        public PlayerEvents playerEvents;
        public TeamMember teamMember;
        [Header("Checkers")]
        public PaintChecker paintCheckerBelow;
        public PaintChecker paintCheckerAhead;
        [Tooltip("Used to check for terrain changes in the direction we're moving.")]
        public Transform frontCheckTransform;
        [Header("Movement")]
        public ActionBasedContinuousMoveProvider locomotion;
        public SpeedController speedController;
        [Tooltip("Speed of squid out of paint")]
        public float squidSpeed; 
        [Tooltip("Speed of squid in paint")]
        public float swimSpeed;
        [Tooltip("Speed in enemy paint as human or squid")]
        public float enemyPaintSpeed;
        
        
        [Header("SFX")]
        public AudioSource swimSound;
        public SFXSource sinkSounds;
        
        /// <summary>
        /// Tracks if the player is in paint of any colour.
        /// <remarks>Can affect <see cref="CanSwim"/> when set to prevent contradicting values.</remarks>
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
        /// <remarks>Can affect <see cref="InPaint"/> when set to prevent contradicting values.</remarks>
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

        private Transform _playerHead;
        private Transform _frontCheckAxis;
        private Vector3 _direction;
        private LayerMask _squidLayer;
        private LayerMask _playerLayer;
        private OrientationHandling _orientationHandling;
        private bool _inPaint;
        private bool _canSwim;
        private bool _isSquid;
        private bool _speedTransitioning;
        private CharacterController _charController;
        private float _goalSpeed;
        private float _standSpeed;
        private float _sign;
        
        private void OnEnable()
        {
            SetupEvents();
        }

        private void Awake()
        {
            if (locomotion == null && TryGetComponent(out locomotion))
            {
                Debug.LogError("No Locomotion Provider on PaintSwim!", this);
            }
            
            _orientationHandling = GetComponent<OrientationHandling>();
            _charController = GetComponent<CharacterController>();
            _squidLayer = LayerMask.NameToLayer("Squid");
            _playerLayer = LayerMask.NameToLayer("Players");
        }

        private void SetupEvents()
        {
            playerEvents.Squid += HandleSwimActivation;
            playerEvents.Stand += HandleStand;
            playerEvents.Move += HandleMove;
        }

        private void OnDisable() 
        {
            playerEvents.Squid -= HandleSwimActivation;
            playerEvents.Stand -= HandleStand;
        }
        
        void Start()
        {
            _playerHead = GameObject.Find("Main Camera").transform;
            _frontCheckAxis = frontCheckTransform.parent;
        }

        private void FixedUpdate()
        {
            if (!_isSquid)
            {
                return;
            }
            
            CheckTerrain();
            Swim();
        }

        private void CheckTerrain()
        {
            if (!CheckGround())
            {
                _orientationHandling.ResetOrientation();
            }
        }

        /// <summary>
        /// Look for terrain orientation and paint changes below us.
        /// </summary>
        /// <remarks>Prioritizes terrain orientation ahead.</remarks>
        /// <returns>True if something is below us to match orientation with, false otherwise.</returns>
        private bool CheckGround()
        {
            int channel = paintCheckerBelow.currChannel;
            if (channel == teamMember.teamChannel)
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

            // Check ahead first so we can adjust to slopes and walls
            if (paintCheckerAhead.currNormal != Vector3.zero && 
                _orientationHandling.SetNewTargetNormal(paintCheckerAhead.currNormal,
                    paintCheckerAhead.currChannel != teamMember.teamChannel, paintCheckerAhead.hitPosition)) 
            {
                return true; 
            }

            return paintCheckerBelow.currNormal != Vector3.zero &&
                   _orientationHandling.SetNewTargetNormal(paintCheckerBelow.currNormal, !CanSwim, paintCheckerBelow.hitPosition);
        }
        

        private void HandleSwimActivation()
        {
            _isSquid = true;
            locomotion.SlopeHandling = false;
            gameObject.layer = _squidLayer;
            paintCheckerAhead.keepUpdated = true;
            _orientationHandling.SetNewTargetHeight(OrientationHandling.SquidHeight, false);
        }

        private void HandleStand()
        {
            _isSquid = false;
            locomotion.SlopeHandling = true;
            gameObject.layer = _playerLayer;
            swimSound.Stop();
            CanSwim = false;
            _orientationHandling.ResetOrientation();
            _orientationHandling.ResetHeight();
            paintCheckerAhead.keepUpdated = false;
        }

        // Adjust speed, height, and SFX while swimming
        private void Swim()
        {
            if (CanSwim) // Indicates we are swimming in friendly paint
            {
                _orientationHandling.SetNewTargetHeight(OrientationHandling.SwimHeight, false);
                speedController.GoalSpeed = swimSpeed;

                playerEvents.OnSwim();
            
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
                playerEvents.OnStopSwim();
                _orientationHandling.SetNewTargetHeight(OrientationHandling.SquidHeight, false);

                if (swimSound.isPlaying)
                {
                    swimSound.Stop();
                }
                
                speedController.GoalSpeed = !InPaint ? squidSpeed : enemyPaintSpeed;
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
