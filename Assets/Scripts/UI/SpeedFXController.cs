using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.VFX;

public class SpeedFXController : MonoBehaviour
{
    private PlayerEvents playerEvents;
    private Vector3 oldPos;
    private float velocity;

    public float speedThreshold; //Speed threshold where speed lines activate
    public VisualEffect linesVFX;
    
    private void Start()
    {
        oldPos = transform.position;
        linesVFX.enabled = true;
    }

    private void FixedUpdate()
    {
        CalcVelocity();
        if (velocity >= speedThreshold)
        {
            linesVFX.Play();
        }
        else if (velocity < speedThreshold)
        {
            linesVFX.Stop();
        }
    }

    private void CalcVelocity()
    {
        Vector3 newPos = transform.position;
        velocity = Vector3.Distance(oldPos, newPos) / Time.deltaTime;
        oldPos = newPos;
    }
        
}
