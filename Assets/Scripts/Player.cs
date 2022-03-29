using Gameplay;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PlayerEvents))]
public class Player : MonoBehaviour
{
    public PlayerEvents playerEvents;
    public int teamChannel; // colour channel of the player's team
    public bool canSwim = true;
    public bool isSquid;
    public float walkSpeed;
    public Inventory inventory = new Inventory();
    
    private Health health;
    AltMove locomotion;
    float oldSpeed;

    private void Start()
    {
        GetComponent<CharacterController>().tag = "Player";
        playerEvents = GetComponent<PlayerEvents>();
        locomotion = GetComponent<AltMove>();
        locomotion.moveSpeed = walkSpeed;
        inventory.Initialize();
        TryGetComponent(out health);
        SetupEvents();
    }

    private void SetupEvents()
    {
        playerEvents.TakeHit += TakeHit;
    }

    // Disable dynamic player movement
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
    
    private void TakeHit(float damage)
    {
        if (health != null)
        {
            health.TakeHit(damage);
        }
    }
}
