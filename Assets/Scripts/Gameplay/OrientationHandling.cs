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
        SetupEvents(); 
    }

    private void SetupEvents()
    {
        playerEvents.OnMove += HandleMove;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.IsSquid)
        {
            ToSquidHeight();
            CheckForOrientationChange();
        }
        else
        {
            if (camOffset.transform.position.y != 0)
                ToHeight(0); // Remove the squid height offset
            if (transform.up != Vector3.up)
                ToOrientation(Vector3.up); 
        }    
    }

    void HandleMove(Vector3 newDirection)
    {
        direction = newDirection;
    }

    // Check for an upcoming terrain orientation change so we can rotate accordingly
    private void CheckForOrientationChange()
    {
        if (checkForPaintAhead()) // Check for walls
            ToOrientation(directionHit.normal);
        else if (character.isGrounded && Physics.Raycast(playerHead.position, -camOffset.transform.up, out directionHit, 1f, mask)) // Check for slope changes
            ToOrientation(directionHit.normal);
        else if (transform.up != Vector3.up) // Reset orientation
            ToOrientation(Vector3.up);
    }
    private void ToOrientation(Vector3 newUp) 
    {
        var newRotation = Quaternion.FromToRotation(transform.up, newUp)*transform.rotation; // FromToRotation may cause movement issues, might need alternative method
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
    }

    // Lower view and negate vertical head movement
    void ToSquidHeight()
    {
        float newHeight = -playerHead.localPosition.y + squidHeight; 
        ToHeight(newHeight);
    }

    // Move the camera closer to the passed height
    void ToHeight(float newHeight)
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
