using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launchable : MonoBehaviour
{
    public float speed = 1; // Units to travel per step (second)
    public float arcHeight;
    
    PlayerEvents playerEvents;
    Vector3 endPos;
    Vector3 startPos;
    float stepScale;
    float progress;
    Transform target;
    
    public bool IsLaunched {get; set;}

    void Start()
    {
        playerEvents = GetComponent<PlayerEvents>();
        target = transform;
        SetupEvents();
    }

    private void SetupEvents()
    {
        playerEvents.Launch += Launch;
        playerEvents.Land += Land;
    }

    private void OnDisable() 
    {
        playerEvents.Launch -= Launch;
        playerEvents.Land -= Land;
    }

    private void FixedUpdate() {
        if (IsLaunched)
        {
            // Increment our progress from 0 at the start, to 1 when we arrive.
            progress = Mathf.Min(progress + Time.deltaTime * stepScale, 1.0f);

            // Turn this 0-1 value into a parabola that goes from 0 to 1, then back to 0.
            float x = progress - 0.5f;
            float parabola = 1.0f - 4.0f * Mathf.Pow(x,2);

            // Travel in a straight line from our start position to the target.        
            Vector3 nextPos = Vector3.Lerp(startPos, endPos, progress);

            // Then add a vertical arc in excess of this.
            nextPos.y += parabola * arcHeight;

            // Continue as before.
            target.position = nextPos;

            if (progress == 1) // We've landed, resume normal movement
                playerEvents.OnLand();
        }
    }

    public void Launch(Vector3 endPosition)
    {
        endPos = endPosition;
        startPos = transform.position;

        float distance = Vector3.Distance(startPos, endPos);
        
        // Determines duration of launch (larger stepScale = shorter launch)
        stepScale = speed / distance; 

        IsLaunched = true;
    }

    public void Land()
    {
        progress = 0;
        IsLaunched = false;  
    }
    
}
