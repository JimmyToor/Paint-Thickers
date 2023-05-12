using Src.Scripts.AI;
using UnityEditor;
using UnityEngine;

// Displays circles of various thickness in the scene view
namespace Editor
{
    [CustomEditor(typeof(WanderBehaviour))]
    public class ExampleEditor : UnityEditor.Editor
    {
        public void OnSceneGUI()
        {
            var t = target as WanderBehaviour;
            var tr = t.transform;
            var position = tr.position;
            Handles.color = Color.yellow;
            var radius = t.wanderDistance;
            Handles.DrawWireDisc(position, tr.up, radius, 1);
        }
    }
}
