using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PlayerEvents))]
public class Player : MonoBehaviour
{
    PlayerEvents playerEvents;
    public int teamChannel; // colour channel of the player's team
    public bool CanSwim { get; set; } = true;
    public bool IsSquid {get; set;}
    public float WalkSpeed {get; set;} = 5f;
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
        oldSpeed = WalkSpeed;
        locomotion.moveSpeed = 0;
        CanSwim = false;
        WalkSpeed = 0;
        locomotion.useGravity = false;
    }

    public void EnableInputMovement()
    {
        WalkSpeed = oldSpeed;
        locomotion.moveSpeed = WalkSpeed;
        locomotion.useGravity = true;
        CanSwim = true;
    }
    
    private void TakeHit(float damage)
    {
        if (health != null)
        {
            health.TakeHit(damage);
        }
    }
}
