using Src.Scripts.AI;
using Src.Scripts.AI.States;
using Src.Scripts.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!CheckCondition(property)) return;
            EditorGUI.PropertyField(position, property, label, true);
        }

        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (CheckCondition(property))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return -EditorGUIUtility.standardVerticalSpacing;            
        }
        
        bool CheckCondition(SerializedProperty property) {
            string condPath = ((ShowIfAttribute)attribute).Condition;

            // If this property is defined inside a nested type 
            // (like a struct inside a MonoBehaviour), look for
            // our condition field inside the same nested instance.
            string propPath = property.propertyPath;
            int last = propPath.LastIndexOf('.');
            if (last > 0) {            
                string containerPath = propPath.Substring(0, last + 1);
                condPath = containerPath + condPath;
            }

            var conditionProperty = property.serializedObject.FindProperty(condPath);

            return !(conditionProperty is {type: "bool"}) || conditionProperty.boolValue;
        }
    }
    
}