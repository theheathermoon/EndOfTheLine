using UnityEditor;
using UnityEngine;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            string valueStr;

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    valueStr = prop.intValue.ToString();
                    break;
                case SerializedPropertyType.Boolean:
                    valueStr = prop.boolValue.ToString();
                    break;
                case SerializedPropertyType.Float:
                    valueStr = prop.floatValue.ToString("0.00000");
                    break;
                case SerializedPropertyType.String:
                    valueStr = prop.stringValue;
                    break;
                case SerializedPropertyType.Enum:
                    int index = prop.enumValueIndex;
                    valueStr = prop.enumNames[index].ToString();
                    break;
                default:
                    valueStr = "(not supported)";
                    break;
            }

            bool labeled = (attribute as ReadOnlyAttribute).IsLabel;

            GUI.enabled = false;
            if (!labeled)
            {
                EditorGUI.PropertyField(position, prop, label);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, valueStr);
            }
            GUI.enabled = true;
        }
    }
}