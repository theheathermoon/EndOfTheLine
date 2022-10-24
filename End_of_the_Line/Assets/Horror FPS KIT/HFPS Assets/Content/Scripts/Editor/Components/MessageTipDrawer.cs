using UnityEngine;
using UnityEditor;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomPropertyDrawer(typeof(InteractiveItem.MessageTip))]
    public class MessageTipDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lines = 0;
            if (HFPS_GameManager.LocalizationEnabled)
                lines += 2;

            return (EditorGUIUtility.singleLineHeight * (2 + lines)) + (EditorGUIUtility.standardVerticalSpacing * (3 + lines));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.indentLevel++;

            Rect inputActionRect = position;
            inputActionRect.y += EditorGUIUtility.standardVerticalSpacing;
            inputActionRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(inputActionRect, property.FindPropertyRelative("InputAction"));

            Rect messageOrKeyRect = position;
            messageOrKeyRect.y += EditorGUIUtility.singleLineHeight + (EditorGUIUtility.standardVerticalSpacing * 2);
            messageOrKeyRect.height = EditorGUIUtility.singleLineHeight;

            if (!HFPS_GameManager.LocalizationEnabled)
            {
                EditorGUI.PropertyField(messageOrKeyRect, property.FindPropertyRelative("Message"));
            }
            else
            {
                EditorUtils.DrawLocaleSelector(messageOrKeyRect, property.FindPropertyRelative("MessageKey"), new GUIContent("Message Key"));
                string obtained = property.FindPropertyRelative("Message").stringValue;
                if (string.IsNullOrEmpty(obtained))
                    obtained = "Null";

                Rect uppercaseRect = position;
                uppercaseRect.y += (EditorGUIUtility.singleLineHeight * 2) + (EditorGUIUtility.standardVerticalSpacing * 3);
                uppercaseRect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(uppercaseRect, property.FindPropertyRelative("TextUppercased"));

                Rect obtainedRect = position;
                obtainedRect.y += (EditorGUIUtility.singleLineHeight * 3) + (EditorGUIUtility.standardVerticalSpacing * 4);
                obtainedRect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(obtainedRect, "Obtained: " + obtained, EditorStyles.boldLabel);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
}