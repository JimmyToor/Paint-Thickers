using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

[CustomEditor(typeof(DirectionTween))]
public class DirectionTweenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DirectionTween tweenScriptTarget = (DirectionTween) target;

        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Start Tween"))
            {
                tweenScriptTarget.StartTween();
            }
            if (GUILayout.Button("Reset Position"))
            {
                tweenScriptTarget.ResetPosition();
            }
        }
    }
}
