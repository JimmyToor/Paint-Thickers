using System;
using Src.Scripts.Preferences;
using Src.Scripts.Utility;
using Src.Scripts.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Gameplay
{
    [RequireComponent(typeof(PlayerEvents),typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        public PlayerEvents playerEvents;
        public GameObject overlayUICam;
        public XRInteractionManager xrInteractionManager;
        [Header("Movement")]
        public bool canSquid = true;
        public bool isSquid;
        public float walkSpeed;
        [Header("Hands")]
        public GameObject leftHand;
        public GameObject rightHand;
        public GameObject leftUIHand;
        public GameObject rightUIHand;

        public int TeamChannel { get; set; }
        
        public UserPreferencesManager.MainHand MainHand { get; set; }
        public Inventory Inventory = new Inventory();
        
        private ActionBasedContinuousMoveProvider _locomotion;
        private float _oldSpeed;
        private Health _health;
        private CharacterController _charController;
        private Vector3 _resetPosition;
        private PaintColorMatcher _paintColorMatcher;
        private WeaponHandler _weaponHandler;

        // These are used to ensure we don't enable movement while we're launched
        private Action _disableResumeMovementOnUnpause;
        private Action _enableResumeMovementOnUnpause;
        
        private void OnEnable()
        {
            SetupEvents();
        }

        private void OnDisable()
        {
            UnsubEvents();
        }

        private void Awake()
        {
            _disableResumeMovementOnUnpause = () => GameManager.Instance.onResume.RemoveListener(playerEvents.EnableInputMovement);
            _enableResumeMovementOnUnpause = () => GameManager.Instance.onResume.AddListener(playerEvents.EnableInputMovement);
            _resetPosition = transform.position;
            InvokeRepeating(nameof(NewResetPosition),2f,5f);
            _locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
            _charController = GetComponent<CharacterController>();
            
            
            _locomotion.moveSpeed = walkSpeed;
            
            TryGetComponent(out _health);
            if (leftUIHand == null)
            {
                leftUIHand = GameObject.Find("LeftHand Ray Controller");
            }
            if (rightUIHand == null)
            {
                rightUIHand = GameObject.Find("RightHand Ray Controller");
            }
            
            if (xrInteractionManager == null)
            {
                xrInteractionManager = GameObject.Find("XR Interaction Manager").GetComponent<XRInteractionManager>();
            }

            TryGetComponent(out _paintColorMatcher);

            if (!TryGetComponent(out _weaponHandler))
            {
                Debug.LogErrorFormat("{0} has no WeaponHandler!", gameObject);
            }
            
            playerEvents = GetComponent<PlayerEvents>();
        }

        private void Start()
        {
            if (!TryGetComponent(out TeamMember member))
            {
                Debug.LogError("No team assigned to " + this + " because it has no TeamMember component!", this);
            }
            else
            {
                TeamChannel = member.teamChannel;
            }
        }

        private void SetupEvents()
        {
            GameManager.Instance.onPause?.AddListener(playerEvents.DisableInputMovement);
            GameManager.Instance.onPause?.AddListener(playerEvents.DisableSquid);
            GameManager.Instance.onResume?.AddListener(playerEvents.EnableInputMovement);
            GameManager.Instance.onResume?.AddListener(playerEvents.EnableSquid);
            playerEvents.TakeHit += TakeHit;
            playerEvents.LauncherActivated += playerEvents.DisableStand;
            playerEvents.LauncherActivated += playerEvents.DisableInputMovement;
            playerEvents.LauncherActivated += DisableGravity;
            playerEvents.Launch += _disableResumeMovementOnUnpause;
            playerEvents.Land += _enableResumeMovementOnUnpause;
            playerEvents.Land += playerEvents.EnableStand;
            playerEvents.Land += playerEvents.EnableInputMovement;
            playerEvents.Land += EnableGravity;
            playerEvents.Squid += SquidMode;
            playerEvents.Stand += HumanMode;
        }

        private void UnsubEvents()
        {
            GameManager.Instance.onPause?.RemoveListener(playerEvents.DisableInputMovement);
            GameManager.Instance.onPause?.RemoveListener(playerEvents.DisableSquid);
            GameManager.Instance.onResume?.RemoveListener(playerEvents.EnableInputMovement);
            GameManager.Instance.onResume?.RemoveListener(playerEvents.EnableSquid);
            playerEvents.TakeHit -= TakeHit;
            playerEvents.LauncherActivated -= playerEvents.DisableStand;
            playerEvents.LauncherActivated -= playerEvents.DisableInputMovement;
            playerEvents.LauncherActivated -= DisableGravity;
            playerEvents.Launch -= _disableResumeMovementOnUnpause;
            playerEvents.Land -= _enableResumeMovementOnUnpause;
            playerEvents.Land -= playerEvents.EnableStand;
            playerEvents.Land -= playerEvents.EnableInputMovement;
            playerEvents.Land -= EnableGravity;
            playerEvents.Squid -= SquidMode;
            playerEvents.Stand -= HumanMode;
        }        
        
        public void EnableUIHands()
        {
            leftUIHand.SetActive(true);
            rightUIHand.SetActive(true);
        }

        public void DisableUIHands()
        {
            leftUIHand.SetActive(false);
            rightUIHand.SetActive(false);
        }
    
        public void EnableGameHands()
        {
            leftHand.SetActive(true);
            rightHand.SetActive(true);
        }

        public void DisableGameHands()
        {
            leftHand.SetActive(false);
            rightHand.SetActive(false);
        }
        
        public void ShowGameHands()
        {
            leftHand.transform.localScale = Vector3.one;
            rightHand.transform.localScale = Vector3.one;
        }

        public void HideGameHands()
        {
            leftHand.transform.localScale = Vector3.zero;
            rightHand.transform.localScale = Vector3.zero;
        }
    
        public void AddItem(ItemType item)
        {
            Inventory.AddItem(item);
        } 

        public bool ConsumeItem(ItemType item)
        {
            return Inventory.ConsumeItem(item);
        }
    
        private void TakeHit(float damage)
        {
            if (_health != null)
            {
                _health.TakeHit(damage);
            }
        }

        public void SetWeapon(Weapon newWeapon)
        {
            if (!_weaponHandler.EquipWeapon(newWeapon))
            {
                return;
            }

            _weaponHandler.SetColor(GameManager.Instance.GetTeamColor(TeamChannel));
            if (_paintColorMatcher)
            {
                _weaponHandler.MatchColors(_paintColorMatcher);
            }
        }

        public void TryUnequipWeapon(Weapon weapon)
        {
            if (!_weaponHandler.IsUnequipValid(weapon) || isSquid) return;
            
            if (_paintColorMatcher != null && _weaponHandler.Weapon != null)
            {
                _weaponHandler.UnmatchColors(_paintColorMatcher);
            }
            _weaponHandler.UnequipWeapon();
        }

        public void DisableWeapon()
        {
            _weaponHandler.DisableWeapon();
        }
        
        public void EnableWeapon()
        {
            _weaponHandler.EnableWeapon();
        }

        public void HideWeapon()
        {
            _weaponHandler.HideWeapon();
        }

        private void ShowWeapon()
        {
            _weaponHandler.ShowWeapon();
        }
    
        public void RefillWeaponAmmo()
        {
            _weaponHandler.RefillWeaponAmmo();
        }

        private void NewResetPosition()
        {
            if (!_charController.isGrounded) return;
            
            _resetPosition = transform.position;
            _resetPosition.y += 0.5f;
        }

        public void ResetPosition()
        {
            transform.position = _resetPosition;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("OOBVolume"))
            {
                ResetPosition();
            }
        }

        // Make any required changes when the player turns into a squid
        private void SquidMode()
        {
            _weaponHandler.SquidMode();
            HideGameHands();
            isSquid = true;
        }

        // Make any required changes when the player turns into a Human
        private void HumanMode()
        {
            ShowGameHands();
            _weaponHandler.HumanMode();
            isSquid = false;
        }

        public void DisableGravity()
        {
            _locomotion.useGravity = false;
            _locomotion.SlopeHandling = false;
        }
        
        public void EnableGravity()
        {
            _locomotion.useGravity = true;
        }
        
        public void DisableOverlayUI()
        {
            overlayUICam.SetActive(false);
        }

        public void EnableOverlayUI()
        {
            overlayUICam.SetActive(true);
        }

        public void ForceEquipWeapon(Weapon weapon)
        {
            XRGrabInteractable interactable = weapon.GetComponent<XRGrabInteractable>();
            
            if (interactable.selectingInteractor != null)
            {
                // Make the current interactor drop the weapon so we can grab it
                interactable.ForceDrop();
            }
                
            xrInteractionManager.ForceSelect(
                MainHand == UserPreferencesManager.MainHand.Right
                    ? rightHand.GetComponent<XRBaseInteractor>()
                    : leftHand.GetComponent<XRBaseInteractor>(),
                interactable);
            _weaponHandler.EquipWeapon(weapon);
        }
        
    }
}
