using System;
using System.Collections.Generic;
using System.IO.Ports;
using Gameplay;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utility;

[RequireComponent(typeof(PlayerEvents),typeof(CharacterController))]
public class Player : MonoBehaviour
{
    public PlayerEvents playerEvents;
    public int teamChannel; // colour channel of the player's team
    public bool canSwim = true;
    public bool isSquid;
    public float walkSpeed;
    public GameObject leftHand;
    public GameObject rightHand;
    
    private Inventory inventory = new Inventory(true);
    private Health health;
    private GameObject weapon;
    private List<GameObject> weaponParts = new List<GameObject>();
    private CharacterController charController;
    private Vector3 resetPosition;
    ActionBasedContinuousMoveProvider locomotion;
    float oldSpeed;

    private void Start()
    {
        charController = GetComponent<CharacterController>();
        InvokeRepeating(nameof(NewResetPosition),2f,5f);
        playerEvents = GetComponent<PlayerEvents>();
        locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
        locomotion.moveSpeed = walkSpeed;
        TryGetComponent(out health);
        SetupEvents();
    }

    private void SetupEvents()
    {
        playerEvents.TakeHit += TakeHit;
        playerEvents.Launch += DisableInputMovement;
        playerEvents.Land += EnableInputMovement;
        playerEvents.Swim += DisableHands;
        playerEvents.Swim += DisableWeapon;
        playerEvents.Stand += EnableHands;
        playerEvents.Stand += EnableWeapon;
    }

    // Disable input-driven player movement
    public void DisableInputMovement()
    {
        locomotion.moveSpeed = 0;
        canSwim = false;
        locomotion.useGravity = false;
    }

    public void EnableInputMovement()
    {
        locomotion.moveSpeed = walkSpeed;
        locomotion.useGravity = true;
        canSwim = true;
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

    public void SetWeapon(GameObject newWeapon)
    {
        weapon = newWeapon;
        foreach (Transform weaponPart in weapon.transform)
        {
            weaponParts.Add(weaponPart.gameObject);
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
        if (weapon != null)
        {
            weapon = null;
            weaponParts.Clear();
        }
    }

    public void DisableWeapon()
    {
        if (weapon == null)
        {
            return;
        }

        foreach (GameObject weaponPart in weaponParts)
        {
            weaponPart.SetActive(false);
        }
    }

    public void EnableWeapon()
    {
        if (weapon == null)
        {
            return;
        }
        
        foreach (GameObject weaponPart in weaponParts)
        {
            weaponPart.SetActive(true);
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
}
