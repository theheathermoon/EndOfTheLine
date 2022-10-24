using UnityEditor;
using HFPS.Systems;
using UnityEngine;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomPropertyDrawer(typeof(TipsManager.TipInfo))]
    public class TipInfoDrawer : PropertyDrawer
    {
        private float SPACING => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        private bool Localization
        {
            get
            {
#if TW_LOCALIZATION_PRESENT
                return ThunderWire.Localization.LocalizationSystem.HasReference;
#else
                return false;
#endif
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return SPACING * 2;
            }

            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutLabelRect = position;
            foldoutLabelRect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(foldoutLabelRect, property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                Rect tipMessageRect = position;
                tipMessageRect.height = EditorGUIUtility.singleLineHeight;
                tipMessageRect.y += SPACING;

                if (!Localization)
                {
                    SerializedProperty m_TipMessage = property.FindPropertyRelative("TipMessage");
                    EditorGUI.PropertyField(tipMessageRect, m_TipMessage);
                }
                else
                {
                    SerializedProperty m_TipMessageKey = property.FindPropertyRelative("TipKey");
                    EditorUtils.DrawLocaleSelector(tipMessageRect, m_TipMessageKey, new GUIContent("Tip Message Key"));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}