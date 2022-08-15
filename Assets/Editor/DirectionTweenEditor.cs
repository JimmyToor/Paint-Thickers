using Gameplay;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(DirectionTween))]
    public class DirectionTweenEditor : UnityEditor.Editor
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
}
