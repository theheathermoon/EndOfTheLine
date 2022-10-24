using UnityEditor;
using HFPS.Systems;
using UnityEngine;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomPropertyDrawer(typeof(SceneLoader.SceneInfo))]
    public class SceneInfoDrawer : PropertyDrawer
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
                if (Localization)
                    return SPACING * 5;

                return SPACING * 6;
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

                Rect sceneNameRect = position;
                sceneNameRect.height = EditorGUIUtility.singleLineHeight;
                sceneNameRect.y += SPACING;
                SerializedProperty m_SceneBuildName = property.FindPropertyRelative("SceneBuildName");
                EditorGUI.PropertyField(sceneNameRect, m_SceneBuildName);

                Rect levelNameRect = position;
                levelNameRect.height = EditorGUIUtility.singleLineHeight;
                levelNameRect.y += SPACING * 2;

                if (!Localization)
                {
                    SerializedProperty m_LevelName = property.FindPropertyRelative("LevelName");
                    EditorGUI.PropertyField(levelNameRect, m_LevelName);
                }
                else
                {
                    SerializedProperty m_LevelNameKey = property.FindPropertyRelative("LevelNameKey");
                    EditorUtils.DrawLocaleSelector(levelNameRect, m_LevelNameKey, new GUIContent("Level Name Key"));
                }

                Rect levelDescRect = position;
                levelDescRect.y += SPACING * 3;

                int space;
                if (!Localization)
                {
                    space = 2;
                    levelDescRect.height = EditorGUIUtility.singleLineHeight * 2;
                    SerializedProperty m_LevelDesc = property.FindPropertyRelative("LevelDescription");
                    EditorGUI.PropertyField(levelDescRect, m_LevelDesc);
                }
                else
                {
                    space = 1;
                    levelDescRect.height = EditorGUIUtility.singleLineHeight;
                    SerializedProperty m_LevelDescKey = property.FindPropertyRelative("LevelDescriptionKey");
                    EditorUtils.DrawLocaleSelector(levelDescRect, m_LevelDescKey, new GUIContent("Level Description Key"));
                }

                Rect bgRect = position;
                bgRect.height = EditorGUIUtility.singleLineHeight;
                bgRect.y += SPACING * (3 + space);
                SerializedProperty m_Background = property.FindPropertyRelative("Background");
                EditorGUI.PropertyField(bgRect, m_Background);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}