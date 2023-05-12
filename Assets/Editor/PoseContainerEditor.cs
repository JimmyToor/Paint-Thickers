using Src.Scripts.Pose;
using UnityEditor;
using UnityEngine;

// From: https://github.com/C-Through/XR-HandPoser/tree/main/Assets/_HandPoser
namespace Editor
{
    [CustomEditor(typeof(PoseContainer))]
    public class PoseContainerEditor : UnityEditor.Editor
    {
        private PoseContainer poseContainer = null;

        private void OnEnable()
        {
            poseContainer = (PoseContainer)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Open Pose Editor"))
                PoseWindow.Open(poseContainer.pose);
        }
    }
}
