using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using ThunderWire.Utility;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomEditor(typeof(UIObjectInfo), true), CanEditMultipleObjects]
    public class UIObjectInfoEditor : Editor
    {
        private UIObjectInfo m_ObjectInfo;

        private SerializedProperty m_ObjectTitle;
        private SerializedProperty m_DynamicTitle;
        private SerializedProperty m_UseText;
        private SerializedProperty m_DynamicTrue;
        private SerializedProperty m_DynamicFalse;
        private SerializedProperty m_isUppercased;
        private SerializedProperty m_dynamicUseText;
        private SerializedProperty m_overrideDoorText;

        private SerializedProperty m_TitleKey;
        private SerializedProperty m_UseKey;
        private SerializedProperty m_DynamicTrueKey;
        private SerializedProperty m_DynamicFalseKey;

        private int selectedID;

        void OnEnable()
        {
            m_ObjectInfo = target as UIObjectInfo;

            m_ObjectTitle = serializedObject.FindProperty("ObjectTitle");
            m_DynamicTitle = serializedObject.FindProperty("DynamicTitle");
            m_UseText = serializedObject.FindProperty("UseText");
            m_DynamicTrue = serializedObject.FindProperty("DynamicTrue");
            m_DynamicFalse = serializedObject.FindProperty("DynamicFalse");
            m_isUppercased = serializedObject.FindProperty("isUppercased");
            m_dynamicUseText = serializedObject.FindProperty("dynamicUseText");
            m_overrideDoorText = serializedObject.FindProperty("overrideDoorText");

            m_TitleKey = serializedObject.FindProperty("titleKey");
            m_UseKey = serializedObject.FindProperty("useKey");
            m_DynamicTrueKey = serializedObject.FindProperty("dTrueKey");
            m_DynamicFalseKey = serializedObject.FindProperty("dFalseKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            string[] getFields = new string[1] { "Null" };

            if (m_ObjectInfo.DynamicTitle.Instance != null)
            {
                if (m_ObjectInfo.DynamicTitle.ReflectType == ReflectionUtil.ReflectType.Field)
                {
                    getFields = (from field in m_ObjectInfo.DynamicTitle.Instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                                 where field.FieldType == typeof(bool)
                                 select field.Name).ToArray();
                }
                else if (m_ObjectInfo.DynamicTitle.ReflectType == ReflectionUtil.ReflectType.Property)
                {
                    getFields = (from prop in m_ObjectInfo.DynamicTitle.Instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                                 where prop.PropertyType == typeof(bool)
                                 select prop.Name).ToArray();
                }
                else
                {
                    getFields = (from method in m_ObjectInfo.DynamicTitle.Instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                 where method.ReturnType == typeof(bool)
                                 select $"{method.ReturnType.Name} {method.Name}").ToArray();
                }

                if (getFields.Length > 0)
                {
                    for (int i = 0; i < getFields.Length; i++)
                    {
                        if (getFields[i].Equals(m_ObjectInfo.DynamicTitle.ReflectName))
                        {
                            selectedID = i;
                            break;
                        }
                    }
                }
            }

            EditorGUILayout.LabelField("Default Title", EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                if (!HFPS_GameManager.LocalizationEnabled)
                {
                    EditorGUILayout.PropertyField(m_ObjectTitle);
                }
                else
                {
                    Rect useRect = GUILayoutUtility.GetRect(1, 20);
                    EditorUtils.DrawLocaleSelector(useRect, m_TitleKey, new GUIContent("Title Key"));
                }
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Dynamic Title", EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorUtils.TrHelpIconText("Title will be changed depending on the true/false state of the reflected type.", MessageType.Info);

                EditorGUILayout.PropertyField(m_DynamicTitle.FindPropertyRelative("ReflectType"));
                EditorGUILayout.PropertyField(m_DynamicTitle.FindPropertyRelative("Instance"));
                using (new EditorGUI.DisabledGroupScope(m_ObjectInfo.DynamicTitle.Instance == null))
                {
                    selectedID = EditorGUILayout.Popup(new GUIContent("Reflect Name"), selectedID, getFields);
                }
            }
            m_ObjectInfo.DynamicTitle.ReflectName = getFields[selectedID];
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Titles", EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                if (!HFPS_GameManager.LocalizationEnabled)
                {
                    EditorGUILayout.PropertyField(m_UseText);
                    EditorGUILayout.PropertyField(m_DynamicTrue);
                    EditorGUILayout.PropertyField(m_DynamicFalse);
                }
                else
                {
                    Rect useRect = GUILayoutUtility.GetRect(1, 20);
                    EditorUtils.DrawLocaleSelector(useRect, m_UseKey, new GUIContent("Use Key"));
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    Rect dTrueRect = GUILayoutUtility.GetRect(1, 20);
                    EditorUtils.DrawLocaleSelector(dTrueRect, m_DynamicTrueKey, new GUIContent("Dynamic True Key"));
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    Rect dFalseRect = GUILayoutUtility.GetRect(1, 20);
                    EditorUtils.DrawLocaleSelector(dFalseRect, m_DynamicFalseKey, new GUIContent("Dynamic False Key"));
                }
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_isUppercased);
                EditorGUILayout.PropertyField(m_dynamicUseText);
                EditorGUILayout.PropertyField(m_overrideDoorText);
            }
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}