using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PlayerEvents))]
public class Player : MonoBehaviour
{
    PlayerEvents playerEvents;
    public int teamChannel; // colour channel of the player's team
    public bool canSwim = true;
    public bool isSquid;
    public float walkSpeed = 5f;
    private Health health;
    AltMove locomotion;
    float oldSpeed;

    private void Start() {
        GetComponent<CharacterController>().tag = "Player";
        playerEvents = GetComponent<PlayerEvents>();
        locomotion = GetComponent<AltMove>();
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
        oldSpeed = walkSpeed;
        locomotion.moveSpeed = 0;
        canSwim = false;
        walkSpeed = 0;
        locomotion.useGravity = false;
    }

    public void EnableInputMovement()
    {
        walkSpeed = oldSpeed;
        locomotion.moveSpeed = walkSpeed;
        locomotion.useGravity = true;
        canSwim = true;
        Debug.LogError("speed " + walkSpeed);
    }
    
    private void TakeHit(float damage)
    {
        if (health != null)
        {
            health.TakeHit(damage);
        }
    }
}
