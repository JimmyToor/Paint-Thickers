using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

// Handle swimming in ink
[RequireComponent(typeof(Player),typeof(PlayerEvents))]
public class InkSwim : MonoBehaviour
{
    public AltMove locomotion;
    public float squidSpeed = 0.7f; // speed of squid out of ink
    public float swimSpeed = 2; // speed of squid in ink
    public bool inInk; // in ink (as either squid or human)
    float rayDist = 4.5f;
    
    Player player;
    PlayerEvents playerEvents;
    Transform playerHead; // used to get distance from floor to player head
    Transform camOffset;
    RaycastHit downHit;
    RaycastHit directionHit;
    
    private LayerMask terrainMask;
    private LayerMask squidLayer;
    private LayerMask playerLayer;
    private float standSpeed;


    void Start()
    {
        player = GetComponent<Player>();
        playerEvents = GetComponent<PlayerEvents>();
        locomotion = GetComponent<AltMove>();
        SetupEvents();
        terrainMask = LayerMask.GetMask("Terrain");
        squidLayer = LayerMask.NameToLayer("Squid");
        playerLayer = LayerMask.NameToLayer("Players");
        camOffset = transform.GetChild(0);
        playerHead = camOffset.GetChild(0);
        standSpeed = player.walkSpeed;
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
        {
            player.isSquid = true;
            player.DisableWeapon();
            gameObject.layer = squidLayer;
        }
    }

    void HandleStand()
    {
        gameObject.layer = playerLayer;
        player.isSquid = false;
        player.EnableWeapon();
        locomotion.moveSpeed = standSpeed;
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
        if (Physics.Raycast(playerHead.position, -transform.up, out downHit, camOffset.localPosition.y+rayDist, terrainMask))
        {
            //Debug.Log("channel: " + PaintTarget.RayChannel(downHit));
            //Debug.DrawRay(playerHead.position, -transform.up,Color.green,5f);
            if (PaintTarget.RayChannel(downHit) == player.teamChannel)
            {
                return true;
            }
        }
        
        return false;
    }
}
