using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Player),typeof(PlayerEvents))]
// Handle height and matching terrain orientation in squid form
public class OrientationHandling : MonoBehaviour
{
    const float squidHeight = 0.5f;
    const float humanHeight = 0f;
    public float rotationSpeed = 90f;
    public float sinkSpeed = 2; // speed of squid transformation
    public bool slopeHandling = true;
    public AltMove locomotion;

    Player player;
    PlayerEvents playerEvents;
    Transform camOffset;
    Transform playerHead;
    Vector3 direction;
    LayerMask mask;
    RaycastHit directionHit;
    private RaycastHit slopeHit;
    Vector3 newOrientation;
    float targetHeight;

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
        playerEvents.Move += HandleMove;
        playerEvents.Swim += HandleSwim;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.isSquid)
        {
            ToSquidHeight();
            CheckForOrientationChange();
        }
        else
        {
            if (camOffset.transform.position.y != humanHeight)
                ToHeight(humanHeight); // Remove the squid height offset
            if (transform.up != Vector3.up)
                ToOrientation(Vector3.up);
            if (slopeHandling) // Prevent bouncing down slopes
            {
                Physics.Raycast(playerHead.position, -camOffset.transform.up, out slopeHit, 1f, mask);
                locomotion.slopeHandling = slopeHit.normal != transform.up;
            }
        }    
    }

    void HandleMove(Vector3 newDirection)
    {
        direction = newDirection;
    }

    void HandleSwim()
    {
        locomotion.slopeHandling = false;
    }

    // Check for an upcoming terrain orientation change so we can rotate accordingly
    private void CheckForOrientationChange()
    {
        if (CheckForPaintAhead()) // Check for walls
            ToOrientation(directionHit.normal);
        else if (Physics.Raycast(playerHead.position, -camOffset.transform.up, out directionHit, 1f, mask)) // Check for slope changes
            ToOrientation(directionHit.normal);
        else if (transform.up != Vector3.up) // Reset orientation
            ToOrientation(Vector3.up);
    }
    
    // Rotate the rig normal towards the newUp normal
    private void ToOrientation(Vector3 newUp)
    {
        Quaternion currRot = transform.rotation;
        var newRotation = Quaternion.FromToRotation(transform.up, newUp)*currRot; // FromToRotation may cause movement issues, might need alternative method
        transform.rotation = Quaternion.RotateTowards(currRot, newRotation, Time.deltaTime * rotationSpeed);
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

    // Look for paint straight ahead (i.e. on a wall)
    private bool CheckForPaintAhead()
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
