using Cinemachine.Editor;
using Src.Scripts;
using UnityEditor;
using UnityEditor.ShaderGraph.Drawing.Inspector;
using UnityEngine;

// Based on 'PaintTargetEditor.cs' from https://assetstore.unity.com/packages/tools/paintz-free-145977
namespace Editor
{
    [CustomEditor(typeof(PaintTarget))]
    public class PaintTargetEditor : UnityEditor.Editor
    {
        private static Texture2D logo;
        private GUIStyle guiStyle = new GUIStyle(); //create a new variable

        public override void OnInspectorGUI()
        {
            PaintTarget script = (PaintTarget)target;
            GameObject go = (GameObject)script.gameObject;
            Renderer render = go.GetComponent<Renderer>();

            if (Application.isPlaying)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                
                script.paintAllSplats = GUILayout.Toggle(script.paintAllSplats, "Paint All Splats");

                if (GUILayout.Button("Clear Paint"))
                {
                    script.ClearPaint();
                }
                if (GUILayout.Button("Clear All Paint"))
                {
                    PaintTarget.ClearAllPaint();
                }
                if (script.useBaked)
                    script.bakedTex = (Texture2D)EditorGUILayout.ObjectField("Baked Texture",
                        script.bakedTex, typeof(Texture2D), true);

                
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical(GUI.skin.box);

                script.paintTextureSize = (TextureSize)EditorGUILayout.EnumPopup("Paint Texture", script.paintTextureSize);
                script.renderTextureSize = (TextureSize)EditorGUILayout.EnumPopup("Render Texture", script.renderTextureSize);
                script.setupOnStart = GUILayout.Toggle(script.setupOnStart, "Setup On Start");
                script.paintAllSplats = GUILayout.Toggle(script.paintAllSplats, "Paint All Splats");
                script.useBaked = GUILayout.Toggle(script.useBaked, "Use Baked Texture");
                if (script.useBaked)
                    script.bakedTex = (Texture2D)EditorGUILayout.ObjectField("Baked Texture", script.bakedTex, typeof(Texture2D), true);
                else
                    script.bakedTex = null;
       
       
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
                    EditorGUILayout.HelpBox("WARNING: Color Pick only works with Mesh Collider", MessageType.Warning);
                }

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
}