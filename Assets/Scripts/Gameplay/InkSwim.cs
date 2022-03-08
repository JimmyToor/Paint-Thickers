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
    public UnityEngine.XR.Interaction.Toolkit.AltMove locomotion;
    const float squidHeight = 0.5f;
    const float humanHeight = 0f;
    public float SquidSpeed {get; set;} = 0.7f; // speed of squid out of ink
    public float SwimSpeed {get; set;} = 2; // speed of squid in ink
    float rayDist = 0.5f;
    bool inInk = false;
    Vector3 direction = Vector3.zero;
    RaycastHit downHit;
    RaycastHit directionHit;
    LayerMask mask;


    void Start()
    {
        player = GetComponent<Player>();
        playerEvents = GetComponent<PlayerEvents>();
        locomotion = GetComponent<UnityEngine.XR.Interaction.Toolkit.AltMove>();
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

        if (player.IsSquid)
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
        if (player.CanSwim)
            player.IsSquid = true;
    }

    void HandleStand()
    {
        player.IsSquid = false;
        locomotion.moveSpeed = player.WalkSpeed;
    }

    void Swim()
    {
        if (inInk)
            locomotion.moveSpeed = SwimSpeed;
        else
            locomotion.moveSpeed = SquidSpeed;
    }

    private bool CheckForPaintBelow()
    {   
        if (Physics.Raycast(playerHead.position, -transform.up, out downHit, camOffset.localPosition.y+1f, mask))
        {
            if (PaintTarget.RayChannel(downHit) == player.teamChannel)
                return true;
        }
        return false;
    }
}
