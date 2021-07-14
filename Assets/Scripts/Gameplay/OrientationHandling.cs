using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Handle height and matching terrain orientation in squid form
public class OrientationHandling : MonoBehaviour
{
    Player player;
    PlayerEvents playerEvents;
    Transform camOffset;
    Transform playerHead;
    const float squidHeight = 0.5f;
    const float humanHeight = 0f;
    public float rotationSpeed = 90f;
    public float sinkSpeed = 2; // speed of squid transformation
    Vector3 direction;
    bool squidForm = false;
    LayerMask mask;
    RaycastHit directionHit;
    Vector3 newOrientation;
    float targetHeight;
    public ActionBasedContinuousMoveProvider locomotion;
    public CharacterController character;

    void Start()
    {
        player = GetComponent<Player>();
        mask = LayerMask.GetMask("Terrain");
        camOffset = transform.GetChild(0);
        playerHead = camOffset.GetChild(0);
        playerEvents = GetComponent<PlayerEvents>();
        setupEvents(); 
    }

    private void setupEvents()
    {
        playerEvents.OnSwim += handleSwim;
        playerEvents.OnStand += handleStand;
        playerEvents.OnMove += handleMove;
    }

    // Update is called once per frame
    void Update()
    {
        if (squidForm)
        {
            toSquidHeight();
            checkForOrientationChange();
        }
        else
        {
            if (camOffset.transform.position.y != 0)
                toHeight(0); // Remove the squid height offset
            if (transform.up != Vector3.up)
                toOrientation(Vector3.up); 
        }    
    }

    void handleSwim()
    {
        squidForm = true;
    }

    void handleStand()
    {
        squidForm = false;
    }

    void handleMove(Vector3 newDirection)
    {
        direction = newDirection;
    }

    // Check for an upcoming terrain orientation change so we can rotate accordingly
    private void checkForOrientationChange()
    {
        if (checkForPaintAhead()) // Check for walls
            toOrientation(directionHit.normal);
        else if (character.isGrounded && Physics.Raycast(playerHead.position, -camOffset.transform.up, out directionHit, 1f, mask)) // Check for slope changes
            toOrientation(directionHit.normal);
        else if (transform.up != Vector3.up) // Reset orientation
            toOrientation(Vector3.up);
    }
    private void toOrientation(Vector3 newUp) 
    {
        var newRotation = Quaternion.FromToRotation(transform.up, newUp)*transform.rotation; // FromToRotation may cause movement issues, might need alternative method
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
    }

    // Lower view and negate vertical head movement
    void toSquidHeight()
    {
        float newHeight = -playerHead.localPosition.y + squidHeight; 
        toHeight(newHeight);
    }

    // Moves the camera closer to the passed height
    void toHeight(float newHeight)
    {
        Vector3 newPos = camOffset.transform.localPosition;
        newPos.y = Mathf.MoveTowards(newPos.y, newHeight, Time.deltaTime * sinkSpeed);
        camOffset.transform.localPosition = newPos;
    }

    private bool checkForPaintAhead()
    {
        Vector3 movementDir = playerHead.TransformDirection(direction);
        movementDir.y = 0f;

        if (Physics.Raycast(playerHead.position,movementDir, out directionHit, 1.5f, mask))
        {
            if (PaintTarget.RayChannel(directionHit) == player.teamChannel)
                return true;
        }
        return false;
    }
}
