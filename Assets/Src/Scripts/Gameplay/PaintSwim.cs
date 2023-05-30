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
        public float squidSpeed; // Speed of squid out of paint
        public float swimSpeed; // Speed of squid in paint
        public float enemyPaintSpeed; // Speed in enemy paint as human or squid
        public Transform frontCheck; // Used to check for terrain changes in the direction we're moving
        [HideInInspector]
        public bool normalSet;
        public AudioSource swimSound; // Swimming sound determined by AudioSource's AudioClip
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
                    CanSwim = false; // Can't swim if not in paint
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
                    InPaint = true; // Must be in paint if we can swim
                }
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
        private bool _inPaint;
        private bool _canSwim;
        private CharacterController _charController;

        void Start()
        {
            _player = GetComponent<Player>();
            locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
            _orientationHandling = GetComponent<OrientationHandling>();
            _charController = GetComponent<CharacterController>();
            _squidLayer = LayerMask.NameToLayer("Squid");
            _playerLayer = LayerMask.NameToLayer("Players");
            _camOffset = transform.GetChild(0);
            _playerHead = _camOffset.GetChild(0);
            _frontCheckAxis = frontCheck.parent;
        }

        private void OnEnable()
        {
            _playerEvents = GetComponent<PlayerEvents>();
            SetupEvents();
        }

        private void FixedUpdate()
        {
            if (_player.isSquid)
            {
                Swim();
            }
            else
            {
                _orientationHandling.ResetHeight();
                _orientationHandling.ResetOrientation();
            }
        }

        private void CheckTerrain()
        {
            // Check ahead first so we can adjust to slopes and walls
            CheckGroundAhead();
            CheckGroundBelow();
        }

        // Look for terrain changes under the main camera
        private void CheckGroundBelow()
        {
            Physics.Raycast(_playerHead.position, -_camOffset.up, out _belowHit, 1f, swimmableLayers);
            //Debug.DrawRay(_playerHead.position,-_camOffset.up,Color.red,2f);
            int channel = PaintTarget.RayChannel(_belowHit);

            // Figure out what colour paint, if any, is underneath the player
            if (channel == _player.teamChannel)
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

            if (!_player.isSquid || _belowHit.transform == null) return;
            
            if (!normalSet) // Don't want to change the normal if it's been set by terrain ahead
            {
                _orientationHandling.SetNewNormal(_belowHit, channel);
            }
        }

        // Look for terrain changes under frontCheck
        private void CheckGroundAhead()
        {
            if (!Physics.Raycast(frontCheck.position, -frontCheck.up, out RaycastHit frontHit, 1f,
                    swimmableLayers)) return;

            normalSet = _orientationHandling.SetNewNormal(frontHit, PaintTarget.RayChannel(frontHit));
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
            _player.isSquid = true;
            gameObject.layer = _squidLayer;
            CheckTerrain();
            _orientationHandling.UpdateOrientation();
        }

        private void HandleStand()
        {
            locomotion.SlopeHandling = true;
            gameObject.layer = _playerLayer;
            _player.isSquid = false;
            locomotion.moveSpeed = _player.walkSpeed;
            swimSound.Stop();
            CanSwim = false;
        
            // Rotating the character controller can result in clipping
            // Pop the player up a bit to prevent this when we reset orientation
            if (transform.up == Vector3.up) return;
            //
            // Vector3 currPos = transform.localPosition;
            // currPos.y += 0.5f;
            // transform.position = currPos;
        }

        // Adjust speed while swimming
        private void Swim()
        {
            if (CanSwim)
            {
                _orientationHandling.ToHeightWithoutOffset(OrientationHandling.SwimHeight);
                locomotion.moveSpeed = swimSpeed;
                _player.EnableWeaponUI();
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
                _orientationHandling.ToHeightWithoutOffset(OrientationHandling.SquidHeight);

                if (swimSound.isPlaying)
                {
                    swimSound.Stop();
                }
                
                locomotion.moveSpeed = !InPaint ? squidSpeed : enemyPaintSpeed;
                _player.HideWeapon();
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

            if (!_player.isSquid) return;
            
            // Player moved, so check the terrain again
            CheckTerrain();
            _orientationHandling.UpdateOrientation();
        }
    }
}
