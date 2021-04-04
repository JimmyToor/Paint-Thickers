using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Will trigger SFX through the SFXPlayer when the object on which this is added trigger a collision enter event
/// </summary>
public class SFXSource : MonoBehaviour
{
    static int s_IDMax = 0;

    public AudioClip[] Clips;
    public float volume = 1f;
    public float minPitch = 1.2f;
    public float maxPitch = 0.8f;

    int m_ID;

    void Awake()
    {
        m_ID = s_IDMax;
        s_IDMax++;
    }

    public void TriggerPlay(Vector3 pos)
    {   
        AudioClip randomClip = Clips[Random.Range(0, Clips.Length)];
        
        SFXPlayer.Instance.PlaySFX(randomClip, pos, new SFXPlayer.PlayParameters()
        {
            Volume = volume,
            Pitch = Random.Range(minPitch, maxPitch),
            SourceID = m_ID
        }, 0);
    }
}
