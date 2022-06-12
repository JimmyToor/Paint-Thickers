using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// Keeps team colors updated
[CustomEditor(typeof(GameManager))]
public class TeamColorsEditor : Editor
{
    public Object colors;

    private GameManager gameManager;

    private void OnEnable()
    {
        colors = serializedObject.FindProperty("teamColors").objectReferenceValue;
        gameManager = (GameManager) target;
        if (colors != null)
        {
            gameManager.UpdateTeamColors();
        }
    }

    public override void OnInspectorGUI()
    {
        Object oldColors = colors;
        base.OnInspectorGUI();
        serializedObject.Update();
        
        if (oldColors != colors && colors != null)
        {
            gameManager.UpdateTeamColors();
        }
    }
}
