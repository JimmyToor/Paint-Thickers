using Gameplay;
using Src.Scripts.Gameplay;
using Src.Scripts.Preferences;
using Src.Scripts.Weapons;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utility;

namespace Src.Scripts
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
        public Weapon startingWeapon;

        private ActionBasedContinuousMoveProvider _locomotion;
        private float _oldSpeed;
        private Inventory _inventory = new Inventory();
        private Health _health;
        private Weapon _weapon;
        private CharacterController _charController;
        private Vector3 _resetPosition;
        private PaintColorMatcher _paintColorMatcher;

        private UserPreferences.MainHand _weaponHand;
        public UserPreferences.MainHand WeaponHand
        {
            get => _weaponHand;
            set => SetWeaponHand(value);
        }

        public Weapon Weapon
        {
            get => _weapon;
            set => SetWeapon(value);
        }

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
        
        public void DisableInputMovement()
        {
            playerEvents.inputs.FindAction("Squid").Disable();
            playerEvents.inputs.FindAction("Move").Disable();
            playerEvents.inputs.FindAction("Turn").Disable();
        }

        public void EnableInputMovement()
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
            _weapon = newWeapon;
            if (_weapon == null) return;
            
            Weapon.SetUIColor(GameManager.Instance.GetTeamColor(teamChannel));

            // Add the weapon's particle renderers to the color matcher list to ensure they are the correct color
            if (_paintColorMatcher)
            {
                foreach (var rend in Weapon.renderers)
                {
                    if (rend is ParticleSystemRenderer && rend.TryGetComponent(out PaintColorManager colorManager))
                    {
                        _paintColorMatcher.matchTeamColor.Add(colorManager);
                    }
                    _paintColorMatcher.UpdateTeamColor();
                }
            }
        }

        public void DisableHands()
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
            if (_paintColorMatcher && Weapon)
            {
                foreach (var rend in Weapon.renderers)
                {
                    if (rend is ParticleSystemRenderer && TryGetComponent(out PaintColorManager colorManager))
                    {
                        _paintColorMatcher.matchTeamColor.Remove(colorManager);
                    }
                }
            }
            Weapon = null;
        }

        public void DisableWeapon()
        {
            if (!Weapon) return;
            
            Weapon.gameObject.SetActive(false);
        }

        public void HideWeapon()
        {
            if (!Weapon) return;

            Weapon.HideWeapon();
        }

        public void ShowWeapon()
        {
            if (!Weapon) return;

            Weapon.ShowWeapon();
        }

        public void EnableWeaponUI()
        {
            if (!Weapon) return;
            
            Weapon.ShowUI();
            
        }

        public void DisableWeaponUI()
        {
            if (!Weapon) return;
            
            Weapon.HideUI();
        }
    
        public void RefillWeaponAmmo()
        {
            if (!Weapon) return;
            
            Weapon.RefillAmmo();
            
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
            if (Weapon != null)
            {
                Weapon.hideUIAboveThreshold = false;
            }
        }

        // Make any required changes when the player turns into a Human
        private void HumanMode()
        {
            EnableHands();
            ShowWeapon();
            if (Weapon != null)
            {
                Weapon.hideUIAboveThreshold = true;
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

        /// <summary>
        /// Used to drop the weapon from the current hand and place it into the new hand.
        /// </summary>
        /// <param name="newHand"></param>
        private void SetWeaponHand(UserPreferences.MainHand newHand)
        {
            if (newHand == WeaponHand) return;
            _weaponHand = newHand;
            
            // Switch our weapon to the new main hand
            if (Weapon != null)
            {
                ForceEquipWeapon(Weapon);
            }
        }

        public void ForceEquipWeapon(Weapon weapon)
        {
            XRGrabInteractable interactable = weapon.GetComponent<XRGrabInteractable>();

            interactable.selectable = true;
            
            if (interactable.selectingInteractor != null)
            {
                // Make the current interactor drop the weapon so we can grab it
                interactable.droppable = true;
                interactable.ForceDrop();
                interactable.droppable = false;
            }
                
            xrInteractionManager.ForceSelect(
                _weaponHand == UserPreferences.MainHand.Right
                    ? rightHand.GetComponent<XRBaseInteractor>()
                    : leftHand.GetComponent<XRBaseInteractor>(),
                interactable);
            Weapon = weapon;
            
            interactable.selectable = false;
        }
        
    }
}
