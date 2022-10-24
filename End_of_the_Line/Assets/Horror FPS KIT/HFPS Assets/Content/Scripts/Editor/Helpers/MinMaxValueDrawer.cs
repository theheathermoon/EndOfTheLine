using UnityEngine;
using UnityEditor;
using ThunderWire.Helpers;

namespace ThunderWire.Editors
{
    [CustomPropertyDrawer(typeof(MinMaxValue))]
    public class MinMaxValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty min = property.FindPropertyRelative("min");
            SerializedProperty max = property.FindPropertyRelative("max");
            SerializedProperty value = property.FindPropertyRelative("value");

            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, label);
                position.height = EditorGUIUtility.singleLineHeight;

                Rect minMaxRect = position;
                minMaxRect.xMax -= EditorGUIUtility.singleLineHeight + 2f;

                float[] values = new float[2];
                values[0] = min.floatValue;
                values[1] = max.floatValue;

                EditorGUI.MultiFloatField(minMaxRect, new GUIContent[]
                {
                new GUIContent("Min"),
                new GUIContent("Max"),
                }, values);

                min.floatValue = values[0];
                max.floatValue = values[1];

                Rect flipRect = minMaxRect;
                flipRect.width = EditorGUIUtility.singleLineHeight;
                flipRect.x = minMaxRect.xMax + 2f;

                Vector2 iconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));

                GUIContent flipIcon = EditorGUIUtility.TrIconContent("preAudioLoopOff", "Flip min/max values.");
                if (GUI.Button(flipRect, flipIcon, EditorStyles.iconButton))
                {
                    float _min = min.floatValue;
                    min.floatValue = max.floatValue;
                    max.floatValue = _min;

                    if (property.serializedObject != null)
                        property.serializedObject.ApplyModifiedProperties();
                }

                EditorGUIUtility.SetIconSize(iconSize);

                Rect minMaxValueRect = position;
                minMaxValueRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                value.floatValue = EditorGUI.Slider(minMaxValueRect, value.floatValue, min.floatValue, max.floatValue);
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}