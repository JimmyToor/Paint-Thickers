using System;
using System.Collections.Generic;
using System.IO.Ports;
using Gameplay;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utility;

[RequireComponent(typeof(PlayerEvents),typeof(CharacterController),typeof(InkSwim))]
public class Player : MonoBehaviour
{
    public PlayerEvents playerEvents;
    public int teamChannel; // colour channel of the player's team
    public bool canSquid = true;
    public bool isSquid;
    public float walkSpeed;
    public GameObject leftHand;
    public GameObject rightHand;
    
    private ActionBasedContinuousMoveProvider locomotion;
    private float oldSpeed;
    private Inventory inventory = new Inventory(true);
    private Health health;
    private Weapon.Weapon weapon;
    private CharacterController charController;
    private Vector3 resetPosition;

    private void Start()
    {
        InvokeRepeating(nameof(NewResetPosition),2f,5f);
        locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
        charController = GetComponent<CharacterController>();
        locomotion.moveSpeed = walkSpeed;
        TryGetComponent(out health);
        locomotion.moveSpeed = walkSpeed;
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
        locomotion.enabled = false;
        canSquid = false;
    }

    public void EnableInputMovement()
    {
        locomotion.enabled = true;
        canSquid = true;
    }
    
    public void AddItem(ItemType item)
    {
        inventory.AddItem(item);
    } 

    public bool ConsumeItem(ItemType item)
    {
        return inventory.ConsumeItem(item);
    }
    
    private void TakeHit(float damage)
    {
        if (health != null)
        {
            health.TakeHit(damage);
        }
    }

    public void SetWeapon(Weapon.Weapon newWeapon)
    {
        weapon = newWeapon;
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
        weapon = null;
    }

    public void DisableWeapon()
    {
        if (weapon == null)
        {
            return;
        }

        weapon.HideWeapon();
    }

    public void EnableWeapon()
    {
        if (weapon == null)
        {
            return;
        }
        
        weapon.ShowWeapon();
    }

    public void EnableWeaponUI()
    {
        if (weapon != null)
        {
            weapon.ShowUI();
        }
    }

    public void DisableWeaponUI()
    {
        if (weapon != null)
        {
            weapon.HideUI();
        }
    }
    
    public void RefillWeaponAmmo()
    {
        if (weapon != null)
        {
            weapon.RefillAmmo();
        }
    }

    private void NewResetPosition()
    {
        if (charController.isGrounded)
        {
            resetPosition = transform.position;
            resetPosition.y += 0.5f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OOBVolume"))
        {
            transform.position = resetPosition;
        }
    }

    // Make any required changes when the player turns into a squid
    private void SquidMode()
    {
        DisableWeapon();
        DisableHands();
    }

    // Make any required changes when the player turns into a Human
    private void HumanMode()
    {
        EnableWeapon();
        EnableHands();
        DisableWeaponUI();
    }
}
