using Audio;
using Src.Scripts;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

// Handle swimming in paint and squid related movement
namespace Gameplay
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

        public bool InPaint // Tracks if the player is in paint of any colour
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
        public bool CanSwim // Tracks if the player is in paint they can swim in
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

        private void Update()
        {
            if (_player.isSquid)
            {
                locomotion.SlopeHandling = false;
                Swim();
            }
            else
            {
                locomotion.SlopeHandling = true;
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

            if (_player.isSquid && _belowHit.transform != null)
            {
                if (!normalSet) // Don't want to change the normal if it's been set by terrain ahead
                {
                    _orientationHandling.SetNewNormal(_belowHit, channel);
                }
            }
        }

        // Look for terrain changes under frontCheck
        private void CheckGroundAhead()
        {
            if (Physics.Raycast(frontCheck.position, -frontCheck.up, out RaycastHit frontHit, 1f, swimmableLayers))
            {
                if (_orientationHandling.SetNewNormal(frontHit, PaintTarget.RayChannel(frontHit)))
                {
                    normalSet = true;
                }
                else
                {
                    normalSet = false;
                }
            }
        }

        private void SetupEvents()
        {
            _playerEvents.Squid += HandleSwim;
            _playerEvents.Stand += HandleStand;
            _playerEvents.Move += HandleMove;
        }

        private void OnDisable() {
            _playerEvents.Squid -= HandleSwim;
            _playerEvents.Stand -= HandleStand;
        }

        private void HandleSwim()
        {
            if (_player.canSquid && !_player.isSquid)
            {
                _player.isSquid = true;
                gameObject.layer = _squidLayer;
                CheckTerrain();
            }
        }

        private void HandleStand()
        {
            gameObject.layer = _playerLayer;
            _player.isSquid = false;
            locomotion.moveSpeed = _player.walkSpeed;
            swimSound.Stop();
            CanSwim = false;
        
            // Rotating the character controller can result in clipping
            // Pop the player up a bit to prevent this when we reset orientation
            if (transform.up != Vector3.up)
            {
                Vector3 currPos = transform.localPosition;
                currPos.y += 0.5f;
                transform.position = currPos;
            }
        }

        // Adjust speed while swimming
        private void Swim()
        {
            if (CanSwim)
            {
                _orientationHandling.ToHeightWithoutOffset(OrientationHandling.SwimHeight);
                locomotion.moveSpeed = swimSpeed;
            
                if (!swimSound.isPlaying)
                {
                    if (_charController.velocity.sqrMagnitude > 1f)
                    {
                        swimSound.time = Random.Range(0f, swimSound.clip.length);
                        swimSound.Play();
                    }
                }
                else if (_charController.velocity.sqrMagnitude <= 1f)
                {
                    swimSound.Stop();
                }
                _player.EnableWeaponUI();
                _player.RefillWeaponAmmo();
            }
            else
            {
                _orientationHandling.ToHeightWithoutOffset(OrientationHandling.SquidHeight);

                if (swimSound.isPlaying)
                {
                    swimSound.Stop();
                }
                if (!InPaint)
                {
                    locomotion.moveSpeed = squidSpeed;
                }
                else
                {
                    locomotion.moveSpeed = enemyPaintSpeed;
                }
                _player.HideWeapon();
                _player.DisableWeaponUI();
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

            if (_player.isSquid)
            {
                // Player moved, so check the terrain again
                CheckTerrain();
                _orientationHandling.UpdateOrientation();
            }
        }
    }
}
