using AI;
using Attributes;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(StateDataDropdownAttribute))]
    public class StateDataDropdownAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var enemy = property.serializedObject.targetObject as Enemy;
            if (enemy == null || enemy.statesData == null || enemy.statesData.stateList.Length == 0)
            {
                GUI.Label(position, "This field requires non-empty States Data and must be within the Enemy class");
                return;
            }

            StateId[] states = enemy.statesData.stateList;
            int numStates = states.Length;
            string[] enumStrings = new string[numStates];
            for (int i = 0; i < numStates; i++)
            {
                int currEnumValue = (int)states[i];
                enumStrings[i] = states[i].ToString();
                if (property.enumValueIndex == currEnumValue)
                {
                    property.enumValueIndex = i;
                }
            }
            
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            int enumIndex = EditorGUI.Popup(position,property.enumValueIndex, enumStrings);
            
            property.enumValueIndex = (int)states[enumIndex];
            EditorGUI.EndProperty();
            //property.serializedObject.ApplyModifiedProperties();
        }
    }
    
}
