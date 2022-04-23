using System;
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
    
    private Inventory inventory = new Inventory(true);
    private Health health;
    private GameObject weapon;
    private Renderer[] weaponRenderers;
    private GameObject weaponParticles;
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
        weaponRenderers = weapon.GetComponentsInChildren<MeshRenderer>();
        
        // Assumes the main particle will always have this name
        weaponParticles = weapon.transform.Find("Main Visual Particle")?.gameObject;
    }

    public void RemoveWeapon()
    {
        if (weapon != null)
        {
            weapon = null;
        }
    }

    public void DisableWeapon()
    {
        if (weapon == null)
        {
            return;
        }

        foreach (MeshRenderer meshRenderer in weaponRenderers)
        {
            meshRenderer.enabled = false;
        }

        if (weaponParticles != null)
        {
            weaponParticles.SetActive(false);
        }
    }

    public void EnableWeapon()
    {
        if (weapon == null)
        {
            return;
        }
        
        foreach (MeshRenderer meshRenderer in weaponRenderers)
        {
            meshRenderer.enabled = true;
        }

        if (weaponParticles != null)
        {
            weaponParticles.SetActive(true);
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
