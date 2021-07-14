using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

// Handle swimming in ink
public class InkSwim : MonoBehaviour
{
    Player player;
    PlayerEvents playerEvents;
    Transform playerHead; // used to get distance from floor to player head
    Transform camOffset;
    public ActionBasedContinuousMoveProvider locomotion;
    
    const float squidHeight = 0.5f;
    const float humanHeight = 0f;
    public float squidSpeed = 0.7f; // speed of squid out of ink
    public float swimSpeed = 2; // speed of squid in ink
    
    public float rayDist = 0.5f;
    public bool canSwim = true;
    float humanSpeed;
    bool inInk = false;
    bool squidForm = false;
    Vector3 direction = Vector3.zero;
    RaycastHit downHit;
    RaycastHit directionHit;
    LayerMask mask;


    void Start()
    {
        player = GetComponent<Player>();
        playerEvents = GetComponent<PlayerEvents>();
        playerEvents.OnSwim += handleSwim;
        playerEvents.OnStand += handleStand;
        humanSpeed = locomotion.moveSpeed;
        mask = LayerMask.GetMask("Terrain");   
        camOffset = transform.GetChild(0);
        playerHead = camOffset.GetChild(0);
    }

    void FixedUpdate()
    {
        if (checkForPaintBelow())
            inInk = true;
        else
            inInk = false;

        if (squidForm)
            swim();               
    }

    void handleSwim()
    {
        squidForm = true;
    }

    void handleStand()
    {
        squidForm = false;
        locomotion.moveSpeed = humanSpeed;
    }

    void swim()
    {
        if (inInk)
            locomotion.moveSpeed = swimSpeed;
        else
            locomotion.moveSpeed = squidSpeed;
    }

    private bool checkForPaintBelow()
    {
        if (Physics.Raycast(playerHead.position, -transform.up, out downHit, camOffset.localPosition.y+1f, mask))
        {
            if (PaintTarget.RayChannel(downHit) == player.teamChannel)
                return true;
        }
        return false;
    }
}
