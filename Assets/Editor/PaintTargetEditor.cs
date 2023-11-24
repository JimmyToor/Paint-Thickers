using System;
using Src.Scripts.Gameplay;
using UnityEditor;
using UnityEngine;

// Based on 'PaintTargetEditor.cs' from https://assetstore.unity.com/packages/tools/paintz-free-145977
namespace Editor
{
    [CustomEditor(typeof(PaintTarget))]
    [CanEditMultipleObjects]
    public class PaintTargetEditor : UnityEditor.Editor
    {
        private static Texture2D logo;
        private const int MaxNearSplatValue = 50;
        private SerializedProperty nearSplats;
        private SerializedProperty paintTextureSize;
        private SerializedProperty renderTextureSize;
        private SerializedProperty setupOnStart;
        private SerializedProperty paintAllSplats;
        private SerializedProperty useBakedPaintMap;

        private void OnEnable()
        {
            nearSplats = serializedObject.FindProperty("maxNearSplats");
            paintTextureSize = serializedObject.FindProperty("paintTextureSize");
            renderTextureSize = serializedObject.FindProperty("renderTextureSize");
            setupOnStart = serializedObject.FindProperty("setupOnStart");
            useBakedPaintMap = serializedObject.FindProperty("useBakedPaintMap");
        }

        public override void OnInspectorGUI()
        {
            PaintTarget script = (PaintTarget)target;
            GameObject go = (GameObject)script.gameObject;
            Renderer render = go.GetComponent<Renderer>();
            serializedObject.Update();

            if (Application.isPlaying)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                
                if (GUILayout.Button("Clear Paint"))
                {
                    script.ClearPaint();
                }
                if (GUILayout.Button("Clear All Paint"))
                {
                    PaintTarget.ClearAllPaint();
                }
                if (script.useBakedPaintMap)
                    script.bakedPaintMap = (Texture2D)EditorGUILayout.ObjectField("Baked PaintMap",
                        script.bakedPaintMap, typeof(Texture2D), true);
                
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical(GUI.skin.box);

                EditorGUILayout.PropertyField(paintTextureSize, new GUIContent("Paint Texture",
                    "Affects paint resolution. Higher values result in less choppy edges."));
                
                EditorGUILayout.PropertyField(renderTextureSize, new GUIContent("Render Texture",
                    "Affects paint border quality. Higher values result in better visual depth."));
                
                nearSplats.intValue = EditorGUILayout.IntSlider(new GUIContent("Max Near Splats",
                    "Maximum number of nearby targets to paint in addition to the primary target."),
                    nearSplats.intValue, 0, MaxNearSplatValue);
                EditorGUILayout.Space(10);
                
                EditorGUILayout.PropertyField(setupOnStart, new GUIContent("Setup On Start"));
                EditorGUILayout.PropertyField(paintAllSplats, new GUIContent("Paint All Splats"));
                EditorGUILayout.PropertyField(useBakedPaintMap, new GUIContent("Use Baked PaintMap"));
                if (useBakedPaintMap.boolValue)
                {
                    script.bakedPaintMap = (Texture2D) EditorGUILayout.ObjectField("Baked PaintMap",
                        script.bakedPaintMap,
                        typeof(Texture2D), true);
                }
                else
                {
                    script.bakedPaintMap = null;
                }
                
                GUILayout.EndVertical();

                if (render == null)
                {
                    EditorGUILayout.HelpBox("Missing Render Component", MessageType.Error);
                }
                else
                {
                    foreach (Material mat in render.sharedMaterials)
                    {
                        if (!mat.shader.name.StartsWith("Shader Graphs/Paintable"))
                        {
                            EditorGUILayout.HelpBox(mat.name + "\nMissing Paint Shader", MessageType.Warning);
                        }
                    }
                }

                bool foundCollider = false;
                bool foundMeshCollider = false;
                if (go.GetComponent<MeshCollider>())
                {
                    foundCollider = true;
                    foundMeshCollider = true;
                }
                if (go.GetComponent<BoxCollider>()) foundCollider = true;
                if (go.GetComponent<SphereCollider>()) foundCollider = true;
                if (go.GetComponent<CapsuleCollider>()) foundCollider = true;
                if (!foundCollider)
                {
                    EditorGUILayout.HelpBox("Missing Collider Component", MessageType.Warning);
                }
                if (!foundMeshCollider)
                {
                    EditorGUILayout.HelpBox("WARNING: Color detection only works with non-convex mesh collider", MessageType.Warning);
                }

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                }
                
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}