using Gameplay;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utility;

[RequireComponent(typeof(PlayerEvents),typeof(CharacterController),typeof(PaintSwim))]
public class Player : MonoBehaviour
{
    public PlayerEvents playerEvents;
    public int teamChannel; // colour channel of the player's team
    public bool canSquid = true;
    public bool isSquid;
    public float walkSpeed;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject leftUIHand;
    public GameObject rightUIHand;
    public GameObject overlayUICam;

    private ActionBasedContinuousMoveProvider _locomotion;
    private float _oldSpeed;
    private Inventory _inventory = new Inventory(true);
    private Health _health;
    private Weapons.Weapon _weapon;
    private CharacterController _charController;
    private Vector3 _resetPosition;
    private void Start()
    {
        InvokeRepeating(nameof(NewResetPosition),2f,5f);
        _locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
        _charController = GetComponent<CharacterController>();
        _locomotion.moveSpeed = walkSpeed;
        TryGetComponent(out _health);
        _locomotion.moveSpeed = walkSpeed;
        if (leftUIHand == null)
        {
            leftUIHand = GameObject.Find("LeftHand Ray Controller");
        }
        if (rightUIHand == null)
        {
            rightUIHand = GameObject.Find("RightHand Ray Controller");
        }
    }

    private void OnEnable()
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

    // Disable input-driven player movement
    public void DisableInputMovement()
    {
        _locomotion.enabled = false;
        canSquid = false;
    }

    public void EnableInputMovement()
    {
        _locomotion.enabled = true;
        canSquid = true;
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

    public void SetWeapon(Weapons.Weapon newWeapon)
    {
        _weapon = newWeapon;
        _weapon.SetUIColor(GameManager.Instance.GetTeamColor(teamChannel));
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
        _weapon = null;
    }

    public void DisableWeapon()
    {
        if (_weapon != null)
        {
            _weapon.gameObject.SetActive(false);
        }
    }

    public void HideWeapon()
    {
        if (_weapon == null)
        {
            return;
        }

        _weapon.HideWeapon();
    }

    public void ShowWeapon()
    {
        if (_weapon == null)
        {
            return;
        }
        
        _weapon.ShowWeapon();
    }

    public void EnableWeaponUI()
    {
        if (_weapon != null)
        {
            _weapon.ShowUI();
        }
    }

    public void DisableWeaponUI()
    {
        if (_weapon != null)
        {
            _weapon.HideUI();
        }
    }
    
    public void RefillWeaponAmmo()
    {
        if (_weapon != null)
        {
            _weapon.RefillAmmo();
        }
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
        if (_weapon != null)
        {
            _weapon.hideUIAboveThreshold = false;
        }
    }

    // Make any required changes when the player turns into a Human
    private void HumanMode()
    {
        EnableHands();
        ShowWeapon();
        if (_weapon != null)
        {
            _weapon.hideUIAboveThreshold = true;
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
}
