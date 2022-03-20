using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

// Handle swimming in ink
[RequireComponent(typeof(Player),typeof(PlayerEvents))]
public class InkSwim : MonoBehaviour
{
    Player player;
    PlayerEvents playerEvents;
    Transform playerHead; // used to get distance from floor to player head
    Transform camOffset;
    public AltMove locomotion;
    public float squidSpeed = 0.7f; // speed of squid out of ink
    public float swimSpeed = 2; // speed of squid in ink
    float rayDist = 4.5f;
    bool inInk;
    RaycastHit downHit;
    RaycastHit directionHit;
    LayerMask mask;


    void Start()
    {
        player = GetComponent<Player>();
        playerEvents = GetComponent<PlayerEvents>();
        locomotion = GetComponent<AltMove>();
        SetupEvents();
        mask = LayerMask.GetMask("Terrain");   
        camOffset = transform.GetChild(0);
        playerHead = camOffset.GetChild(0);
    }

    void FixedUpdate()
    {
        if (CheckForPaintBelow())
            inInk = true;
        else
            inInk = false;

        if (player.isSquid)
            Swim();               
    }

    private void SetupEvents()
    {
        playerEvents.Swim += HandleSwim;
        playerEvents.Stand += HandleStand;
    }

    private void OnDisable() {
        playerEvents.Swim -= HandleSwim;
        playerEvents.Stand -= HandleStand;
    }

    void HandleSwim()
    {
        if (player.canSwim)
            player.isSquid = true;
    }

    void HandleStand()
    {
        player.isSquid = false;
        locomotion.moveSpeed = player.walkSpeed;
    }

    void Swim()
    {
        if (inInk)
            locomotion.moveSpeed = swimSpeed;
        else
            locomotion.moveSpeed = squidSpeed;
    }

    private bool CheckForPaintBelow()
    {   
        if (Physics.Raycast(playerHead.position, -transform.up, out downHit, camOffset.localPosition.y+rayDist, mask))
        {
            if (PaintTarget.RayChannel(downHit) == player.teamChannel)
            {
                return true;
            }
        }
        
        return false;
    }
}
