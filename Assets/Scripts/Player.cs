using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    PlayerEvents playerEvents;
    public int teamChannel = 0; // colour channel of the player's team
    public bool CanSwim { get; set; } = true;
    public bool IsSquid {get; set;} = false;
    public float WalkSpeed {get; set;} = 5;
    UnityEngine.XR.Interaction.Toolkit.AltMove locomotion;
    float oldSpeed;

    private void Start() {
        GetComponent<CharacterController>().tag = "Player";
        playerEvents = GetComponent<PlayerEvents>();
        locomotion = GetComponent<UnityEngine.XR.Interaction.Toolkit.AltMove>();
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
        locomotion.useGravity = false;
        CanSwim = true;
    }
    
}
