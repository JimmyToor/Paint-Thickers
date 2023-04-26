using Gameplay;
using Src.Scripts.Preferences;
using Src.Scripts.Weapons;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utility;

namespace Src.Scripts.Gameplay
{
    [RequireComponent(typeof(PlayerEvents),typeof(CharacterController),typeof(PaintSwim))]
    public class Player : MonoBehaviour
    {
        public PlayerEvents playerEvents;
        [HideInInspector]public int teamChannel;
        public bool canSquid = true;
        public bool isSquid;
        public float walkSpeed;
        public GameObject leftHand;
        public GameObject rightHand;
        public GameObject leftUIHand;
        public GameObject rightUIHand;
        public GameObject overlayUICam;
        public XRInteractionManager xrInteractionManager;

        private ActionBasedContinuousMoveProvider _locomotion;
        private float _oldSpeed;
        private Inventory _inventory = new Inventory();
        private Health _health;
        private CharacterController _charController;
        private Vector3 _resetPosition;
        private PaintColorMatcher _paintColorMatcher;
        [SerializeField]
        private WeaponHandler weaponHandler;

        public UserPreferences.MainHand MainHand { get; set; }
        

        private void Awake()
        {
            InvokeRepeating(nameof(NewResetPosition),2f,5f);
            _locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
            _charController = GetComponent<CharacterController>();
            
            TryGetComponent(out TeamMember member);
            teamChannel = member.teamChannel;
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
        }

        private void Start()
        {
            playerEvents = GetComponent<PlayerEvents>();
            SetupEvents();
        }

        private void SetupEvents()
        {
            playerEvents.TakeHit += TakeHit;
            playerEvents.Launch += DisableInputMovement;
            playerEvents.Land += EnableInputMovement;
            playerEvents.Squid += SquidMode;
            playerEvents.Stand += HumanMode;
        }
        
        public void EnableUIHands()
        {
            leftHand.SetActive(false);
            rightHand.SetActive(false);
            leftUIHand.SetActive(true);
            rightUIHand.SetActive(true);
        }
    
        public void EnableGameHands()
        {
            leftHand.SetActive(true);
            rightHand.SetActive(true);
            leftUIHand.SetActive(false);
            rightUIHand.SetActive(false);
        }

        private void DisableInputMovement()
        {
            playerEvents.inputs.FindAction("Squid").Disable();
            playerEvents.inputs.FindAction("Move").Disable();
            playerEvents.inputs.FindAction("Turn").Disable();
        }

        private void EnableInputMovement()
        {
            playerEvents.inputs.FindAction("Squid").Enable();
            playerEvents.inputs.FindAction("Move").Enable();
            playerEvents.inputs.FindAction("Turn").Enable();
        }
    
        public void AddItem(ItemType item)
        {
            _inventory.AddItem(item);
        } 

        public bool ConsumeItem(ItemType item)
        {
            return _inventory.ConsumeItem(item);
        }
    
        private void TakeHit(float damage)
        {
            if (_health != null)
            {
                _health.TakeHit(damage);
            }
        }

        private void SetWeapon(Weapon newWeapon)
        {
            if (!weaponHandler.SetWeapon(newWeapon))
            {
                return;
            }

            weaponHandler.ShowUI();
            weaponHandler.SetUIColor(GameManager.Instance.GetTeamColor(teamChannel));

            // Add the weapon's particle renderers to the color matcher list to ensure they are the correct color
            if (_paintColorMatcher)
            {
                foreach (var rend in weaponHandler.Weapon.renderers)
                {
                    if (rend is ParticleSystemRenderer && rend.TryGetComponent(out PaintColorManager colorManager))
                    {
                        _paintColorMatcher.matchTeamColor.Add(colorManager);
                    }
                    _paintColorMatcher.UpdateTeamColor();
                }
            }
        }

        private void DisableHands()
        {
            leftHand.SetActive(false);
            rightHand.SetActive(false);
        }

        public void EnableHands()
        {
            leftHand.SetActive(true);
            rightHand.SetActive(true);
        }
    
        public void RemoveWeapon()
        {
            if (_paintColorMatcher && weaponHandler.Weapon)
            {
                foreach (var rend in weaponHandler.Weapon.renderers)
                {
                    if (rend is ParticleSystemRenderer && TryGetComponent(out PaintColorManager colorManager))
                    {
                        _paintColorMatcher.matchTeamColor.Remove(colorManager);
                    }
                }
            }
            weaponHandler.RemoveWeapon();
        }

        public void DisableWeapon()
        {
            weaponHandler.DisableWeapon();
        }
        
        public void EnableWeapon()
        {
            weaponHandler.EnableWeaponUI();
        }

        public void HideWeapon()
        {
            weaponHandler.Weapon.HideWeapon();
        }

        private void ShowWeapon()
        {
            weaponHandler.ShowWeapon();
        }

        public void EnableWeaponUI()
        {
            weaponHandler.ShowUI();
        }

        public void DisableWeaponUI()
        {
            weaponHandler.DisableWeaponUI();
        }
    
        public void RefillWeaponAmmo()
        {
            weaponHandler.RefillWeaponAmmo();
        }

        private void NewResetPosition()
        {
            if (_charController.isGrounded)
            {
                _resetPosition = transform.position;
                _resetPosition.y += 0.5f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("OOBVolume"))
            {
                transform.position = _resetPosition;
            }
        }

        // Make any required changes when the player turns into a squid
        private void SquidMode()
        {
            HideWeapon();
            DisableHands();
            if (weaponHandler.Weapon != null)
            {
                weaponHandler.Weapon.hideUIAboveThreshold = false;
            }
        }

        // Make any required changes when the player turns into a Human
        private void HumanMode()
        {
            EnableHands();
            ShowWeapon();
            if (weaponHandler.Weapon != null)
            {
                weaponHandler.Weapon.hideUIAboveThreshold = true;
            }
        }

        public void DisableOverlayUI()
        {
            overlayUICam.SetActive(false);
        }

        public void EnableOverlayUI()
        {
            overlayUICam.SetActive(true);
        }

        public void ToggleUIRays()
        {
            if (rightUIHand != null)
            {
                rightUIHand.SetActive(!rightUIHand.activeSelf);
                rightHand.SetActive(!rightHand.activeSelf);
            }

            if (leftUIHand != null)
            {
                leftUIHand.SetActive(!leftUIHand.activeSelf);
                leftHand.SetActive(!leftHand.activeSelf);
            }
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
                MainHand == UserPreferences.MainHand.Right
                    ? rightHand.GetComponent<XRBaseInteractor>()
                    : leftHand.GetComponent<XRBaseInteractor>(),
                interactable);
            weaponHandler.SetWeapon(weapon);
        }
        
    }
}
