using System;
using System.Collections;
using System.Collections.Generic;
using Src.Scripts.Preferences;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class HandednessSwap : MonoBehaviour
{

    public Material leftHandedMaterial; 
    public Material rightHandedMaterial;
    public MeshRenderer meshRenderer;
    public int matIndex;
    public UserPreferencesManager userPreferencesManager;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (leftHandedMaterial == null)
        {
            Debug.Log("HandednessSwap is missing Left Handed Material!", this);
        }
        else if (rightHandedMaterial == null)
        {
            Debug.Log("HandednessSwap is missing Right Handed Material!", this);
        }
        else if (userPreferencesManager == null)
        {
            Debug.Log("HandednessSwap is missing User Preferences Manager!", this);
        }
        else if (meshRenderer == null)
        {
            Debug.Log("HandednessSwap is missing Mesh Renderer!", this);
        }
    }

    private void OnEnable()
    {
        userPreferencesManager.onHandChange.AddListener(SwapMaterial);
    }
    
    private void OnDisable()
    {
        userPreferencesManager.onHandChange.RemoveListener(SwapMaterial);
    }

    private void SwapMaterial(UserPreferencesManager.MainHand hand)
    {
        meshRenderer.materials[matIndex] = hand switch
        {
            UserPreferencesManager.MainHand.Left => leftHandedMaterial,
            UserPreferencesManager.MainHand.Right => rightHandedMaterial,
            _ => throw new ArgumentOutOfRangeException(nameof(hand), hand, null)
        };
    }
}
