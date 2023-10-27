using UnityEditor;
using UnityEngine;

// Based on https://github.com/roboryantron/Unite2017/tree/master by Ryan Hipple
namespace Src.Scripts.ScriptableObjects.Editor
{
    [CustomEditor(typeof(GameEvent), editorForChildClasses: true)]
    public class EventEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            GameEvent gameEvent = target as GameEvent;
            if (gameEvent.GetListenerCount() > 0)
            {
                if (GUILayout.Button("Raise"))
                    gameEvent.Raise();
            }
            else
            {
                GUILayout.Label("No Listeners");
            }
        }
    }
}