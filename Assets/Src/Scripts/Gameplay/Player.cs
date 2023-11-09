using System;
using Src.Scripts.Preferences;
using Src.Scripts.ScriptableObjects;
using Src.Scripts.Utility;
using Src.Scripts.Weapons;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Gameplay
{
    [RequireComponent(typeof(PlayerEvents),typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        public PlayerEvents playerEvents;
        public PauseHandler pauseHandler;
        public WeaponHandler weaponHandler;
        public TeamColorScriptableObject teamColorData;
        public XRRig xrRig;
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
        private ActionBasedSnapTurnProvider _snapTurnProvider;
        private float _oldSpeed;
        private Health _health;
        private CharacterController _charController;
        private Vector3 _resetPosition;
        private PaintColorMatcher _paintColorMatcher;
        private const float NormalDebounceTime = 0.2f;
        private const float PauseDebounceTime = 0f;
        
        
        // These are used to ensure we don't enable movement while we're launched
        private Action _disableResumeMovementOnUnpause;
        private Action _enableResumeMovementOnUnpause;
        
        private void OnEnable()
        {
            SetupEvents();
        }

        private void OnDisable()
        {
            if(!gameObject.scene.isLoaded) return;
            UnsubEvents();
        }

        private void Awake()
        {
            _disableResumeMovementOnUnpause = () => pauseHandler.onResume.RemoveListener(playerEvents.EnableInputMovement);
            _enableResumeMovementOnUnpause = () => pauseHandler.onResume.AddListener(playerEvents.EnableInputMovement);
            
            _locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
            _charController = GetComponent<CharacterController>();
            
            _locomotion.moveSpeed = walkSpeed;
            
            
            _snapTurnProvider = GetComponent<ActionBasedSnapTurnProvider>();
            TryGetComponent(out _health);
            if (leftUIHand == null)
            {
                leftUIHand = GameObject.Find("LeftHand Ray Controller");
            }
            if (rightUIHand == null)
            {
                rightUIHand = GameObject.Find("RightHand Ray Controller");
            }
            
            if (xrRig == null)
            {
                xrRig = GetComponent<XRRig>();
            }
            
            if (xrInteractionManager == null)
            {
                xrInteractionManager = FindObjectOfType<XRInteractionManager>();
            }

            TryGetComponent(out _paintColorMatcher);

            if (weaponHandler == null && !TryGetComponent(out weaponHandler))
            {
                Debug.LogErrorFormat("{0} has no WeaponHandler!", gameObject);
            }
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
            
            _resetPosition = SpawnPoint.Instance.transform.position;
            InvokeRepeating(nameof(NewResetPosition),2f,5f);
            Invoke(nameof(MoveToSpawn), 0.2f); // Slight delay to wait for main camera to line up with player head.
        }

        private void SetupEvents()
        {
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
        
        /// <summary>
        /// Matches player position and rotation to spawn point.
        /// </summary>
        private void MoveToSpawn()
        {
            if (this == null) return;
            float rotY = SpawnPoint.Instance.transform.rotation.eulerAngles.y - Camera.main!.transform.rotation.eulerAngles.y;
            transform.Rotate(0,rotY,0);
            xrRig.MoveCameraToWorldLocation(SpawnPoint.Instance.transform.position);
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
            if (!weaponHandler.EquipWeapon(newWeapon))
            {
                return;
            }

            weaponHandler.SetColor(teamColorData.GetTeamColor(TeamChannel));
            if (_paintColorMatcher)
            {
                weaponHandler.MatchColors(_paintColorMatcher);
            }
        }

        public void TryUnequipWeapon(Weapon weapon)
        {
            if (!weaponHandler.IsUnequipValid(weapon) || isSquid) return;
            
            if (_paintColorMatcher != null && weaponHandler.Weapon != null)
            {
                weaponHandler.UnmatchColors(_paintColorMatcher);
            }
            weaponHandler.UnequipWeapon();
        }

        public void DisableWeapon()
        {
            weaponHandler.DisableWeapon();
        }
        
        public void EnableWeapon()
        {
            weaponHandler.EnableWeapon();
        }

        public void HideWeapon()
        {
            weaponHandler.HideWeapon();
        }

        private void ShowWeapon()
        {
            weaponHandler.ShowWeapon();
        }
    
        public void RefillWeaponAmmo()
        {
            weaponHandler.RefillWeaponAmmo();
        }

        public void StopManualReload()
        {
            weaponHandler.StopManualReload();
        }
        
        private void NewResetPosition()
        {
            if (!_charController.isGrounded) return;
            
            _resetPosition = transform.position;
            _resetPosition.y += 1f;
        }

        [ContextMenu("Reset Position")]
        public void ResetPosition()
        {
            xrRig.MoveCameraToWorldLocation(_resetPosition);
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
            weaponHandler.SquidMode();
            HideGameHands();
            isSquid = true;
        }

        // Make any required changes when the player turns into a Human
        private void HumanMode()
        {
            ShowGameHands();
            weaponHandler.HumanMode();
            isSquid = false;
        }

        private void DisableGravity()
        {
            _locomotion.useGravity = false;
            _locomotion.SlopeHandling = false;
        }

        private void EnableGravity()
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
        }

        public void OnEnd()
        {
            DisableOverlayUI();
            DisableWeapon();
            DisableGameHands();
            EnableUIHands();
        }

        public void ToUIMode()
        {
            EnableUIHands();
            HideGameHands();
        }

        public void ToGameplayMode()
        {
            DisableUIHands();
            ShowGameHands();
        }
        
        public void OnPause()
        {
            ToUIMode();
            playerEvents.DisableInputMovement();
            playerEvents.DisableSquid();
            _snapTurnProvider.debounceTime = PauseDebounceTime;
        }

        public void OnResume()
        {
            ToGameplayMode();
            playerEvents.EnableInputMovement();
            playerEvents.EnableSquid();
            _snapTurnProvider.debounceTime = NormalDebounceTime;
        }
    }
}
