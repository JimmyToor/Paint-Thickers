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
        public AnimationCurve launchArc;
        [HideInInspector]
        public float stepScale;
        public float arcScale;
        public float buildupTime;
    }

    [HideInInspector]
    public bool isLaunched;
    public bool canLaunch;
    public AudioClip landingAudioClip;
    public GameObject landingVFX;
    public Transform fxPosition;

    private PlayerEvents playerEvents;
    private Vector3 startPos;
    private float progress;
    private LaunchableParams launchParams;
    

    private void OnEnable()
    {
        if (TryGetComponent(out playerEvents))
            SetupEvents();
    }

    private void SetupEvents()
    {
        playerEvents.Land += Land;
        playerEvents.Squid += () => canLaunch = true;
        playerEvents.Stand += () => canLaunch = false;
    }

    private void OnDisable()
    {
        if (playerEvents != null)
        {
            DisableEvents();
        }
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
            nextPos.y += launchParams.launchArc.Evaluate(progress)*launchParams.arcScale;
            transform.position = nextPos;

            if (progress >= 1) // We've landed, resume normal movement
            {
                if (playerEvents != null)
                {
                    playerEvents.OnLand();
                }
            }
        }
    }

    public void Launch(LaunchableParams launchParameters)
    {
        launchParams = launchParameters;
        startPos = transform.position;
        progress = 0;
        canLaunch = false;

        if (playerEvents != null)
        {
            playerEvents.OnLaunch();
        }
        StartCoroutine(BuildUp());
    }

    // Wait while the launcher plays effects
    private IEnumerator BuildUp()
    {
        yield return new WaitForSeconds(launchParams.buildupTime);
        isLaunched = true;
    }

    public void Land()
    {
        isLaunched = false;
        SpawnFXAtFeet();
    }

    private void SpawnFXAtFeet()
    {
        Vector3 fxPos = fxPosition.position;
        fxPos.y = transform.position.y;
        AudioSource.PlayClipAtPoint(landingAudioClip, fxPos);
        Instantiate(landingVFX, fxPos, Quaternion.identity);
    }

}
