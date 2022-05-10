using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Handle swimming in ink and squid related movement
[RequireComponent(typeof(Player),typeof(OrientationHandling))]
public class InkSwim : MonoBehaviour
{
    public ActionBasedContinuousMoveProvider locomotion;
    public float squidSpeed; // Speed of squid out of ink
    public float swimSpeed; // Speed of squid in ink
    public float enemyInkSpeed; // Speed in enemy ink as human or squid
    public bool slopeHandling = true;
    public Transform frontCheck; // Used to check for terrain changes in the direction we're moving
    [HideInInspector]
    public bool normalSet;

    public bool InInk // Tracks if the player is in ink of any colour
    {
        get => _inInk;
        set
        {
            _inInk = value;
            if (!value)
            {
                CanSwim = false; // Can't swim if not in ink
            }
        }
    }
    public bool CanSwim // Tracks if the player is in ink they can swim in
    {
        get => _canSwim;
        set
        {
            _canSwim = value;
            if (value)
            {
                InInk = true; // Must be in ink if we can swim
            }
        }
    }

    private Player player;
    private PlayerEvents playerEvents;
    private Transform playerHead;
    private Transform camOffset;
    private Transform frontCheckAxis;
    private Vector3 direction;
    private LayerMask terrainMask;
    private LayerMask squidLayer;
    private LayerMask playerLayer;
    private float standSpeed;
    private float frontAngle;
    private OrientationHandling orientationHandling;
    private RaycastHit belowHit;
    private bool _inInk;
    private bool _canSwim;

    void Start()
    {
        player = GetComponent<Player>();
        playerEvents = GetComponent<PlayerEvents>();
        locomotion = GetComponent<ActionBasedContinuousMoveProvider>();
        orientationHandling = GetComponent<OrientationHandling>();
        terrainMask = LayerMask.GetMask("Terrain");
        squidLayer = LayerMask.NameToLayer("Squid");
        playerLayer = LayerMask.NameToLayer("Players");
        camOffset = transform.GetChild(0);
        playerHead = camOffset.GetChild(0);
        frontCheckAxis = frontCheck.parent;
        SetupEvents();
    }

    void Update()
    {
        if (player.isSquid)
        {
            Swim();
        }
        else
        {
            orientationHandling.ResetHeight();
            orientationHandling.ResetOrientation();
        }
    }

    private void CheckTerrain()
    {
        // Check ahead first so we can adjust to slopes and walls
        CheckGroundAhead();
        CheckGroundBelow();
    }
    
    // Look for terrain changes under the main camera
    private void CheckGroundBelow()
    {
        Physics.Raycast(playerHead.position, -camOffset.up, out belowHit, 1f, terrainMask);

        int channel = PaintTarget.RayChannel(belowHit);
        
        // Figure out what colour ink, if any, is underneath the player
        if (channel == player.teamChannel)
        {
            CanSwim = true;
        }
        else if (channel != -1)
        {
            InInk = true;
            CanSwim = false;
        }
        else
        {
            InInk = false;
        }

        if (player.isSquid && belowHit.transform != null)
        {
            if (normalSet) // Don't want to change the normal since it's been set by terrain ahead
            {
                orientationHandling.SetNewNormal(belowHit, channel);
            }
        }
    }
    
    // Look for terrain changes under frontCheck
    private void CheckGroundAhead()
    {
        if (Physics.Raycast(frontCheck.position, -frontCheck.up, out RaycastHit frontHit, 1f, terrainMask))
        {
            if (orientationHandling.SetNewNormal(frontHit, PaintTarget.RayChannel(frontHit)))
            {
                normalSet = true;
            }
        }
    }

    private void SetupEvents()
    {
        playerEvents.Squid += HandleSwim;
        playerEvents.Stand += HandleStand;
        playerEvents.Move += HandleMove;
    }

    private void OnDisable() {
        playerEvents.Squid -= HandleSwim;
        playerEvents.Stand -= HandleStand;
    }

    private void HandleSwim()
    {
        if (player.canSquid)
        {
            player.isSquid = true;
            gameObject.layer = squidLayer;
        }
    }

    private void HandleStand()
    {
        gameObject.layer = playerLayer;
        player.isSquid = false;
        locomotion.moveSpeed = player.walkSpeed;
    }

    // Adjust speed while swimming
    private void Swim()
    {
        if (CanSwim)
        {
            orientationHandling.ToHeightWithoutOffset(OrientationHandling.swimHeight);
            locomotion.moveSpeed = swimSpeed;
        }
        else
        {
            orientationHandling.ToHeightWithoutOffset(OrientationHandling.squidHeight);
            if (!InInk)
            {
                locomotion.moveSpeed = squidSpeed;
            }
            else
            {
                locomotion.moveSpeed = enemyInkSpeed;
            }
        }
    }
    
    // Keep frontCheck in the direction the player moving
    private void HandleMove(Vector3 newDirection)
    {
        direction = playerHead.InverseTransformDirection(locomotion.LatestRelativeInput);

        float newAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        Vector3 currAngles = frontCheckAxis.localEulerAngles;
        currAngles.y = newAngle;
        frontCheckAxis.localEulerAngles = currAngles;
        
        // Player moved, so check the terrain again
        CheckTerrain();
        orientationHandling.UpdateOrientation();
        normalSet = false;
    }
}
