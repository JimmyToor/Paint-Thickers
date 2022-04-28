using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launchable : MonoBehaviour
{
    [Serializable]
    public struct LaunchableParams
    {
        [HideInInspector] 
        public Vector3 endPos;
        public AnimationCurve animCurve;
        [HideInInspector]
        public float stepScale;
        public float curveScale;
    }
    
    [HideInInspector]
    public bool isLaunched;
    public bool canLaunch;
    
    PlayerEvents playerEvents;
    Vector3 startPos;
    float progress;

    private LaunchableParams launchParams;
    

    void Start()
    {
        if (TryGetComponent(out playerEvents))
            SetupEvents();
    }

    private void SetupEvents()
    {
        playerEvents.Land += Land;
        playerEvents.Swim += () => canLaunch = true;
        playerEvents.Stand += () => canLaunch = false;
    }

    private void OnDisable()
    {
        DisableEvents();
    }

    private void DisableEvents()
    {
        if (playerEvents != null)
        {
            playerEvents.Land -= Land;
        }
    }

    private void Update()
    {
        if (isLaunched)
        {
            // Increment our progress from 0 at the start, to 1 when we arrive.
            progress = Mathf.Min(progress + Time.deltaTime * launchParams.stepScale, 1.0f);

            Vector3 nextPos = Vector3.Lerp(startPos, launchParams.endPos, progress);
            
            // Add vertical arc
            nextPos.y += launchParams.animCurve.Evaluate(progress)*launchParams.curveScale;
            transform.position = nextPos;

            if (progress >= 1) // We've landed, resume normal movement
            {
                Land();
            }
        }
    }

    public void Launch(LaunchableParams launchParameters)
    {
        launchParams = launchParameters;
        startPos = transform.position;
        
        if (playerEvents != null)
        {
            playerEvents.OnLaunch();
        }

        progress = 0;
        isLaunched = true;
    }

    public void Land()
    {
        isLaunched = false;
        if (playerEvents != null)
        {
            playerEvents.OnLand();
        }
    }
    
}
