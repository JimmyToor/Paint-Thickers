using Gameplay;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Utility;

[RequireComponent(typeof(PlayerEvents))]
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
    AltMove locomotion;
    float oldSpeed;

    private void Start()
    {
        GetComponent<CharacterController>().tag = "Player";
        playerEvents = GetComponent<PlayerEvents>();
        locomotion = GetComponent<AltMove>();
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
    
}
