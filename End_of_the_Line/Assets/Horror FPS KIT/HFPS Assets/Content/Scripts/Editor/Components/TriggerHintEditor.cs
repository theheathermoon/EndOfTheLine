using UnityEngine;
using UnityEditor;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomEditor(typeof(TriggerHint)), CanEditMultipleObjects]
    public class TriggerHintEditor : Editor
    {
        private SerializedProperty m_Hint;
        private SerializedProperty m_HintKey;
        private SerializedProperty m_TimeShow;
        private SerializedProperty m_ShowAfter;
        private SerializedProperty m_HintSound;

        private void OnEnable()
        {
            m_Hint = serializedObject.FindProperty("Hint");
            m_HintKey = serializedObject.FindProperty("HintKey");
            m_TimeShow = serializedObject.FindProperty("TimeShow");
            m_ShowAfter = serializedObject.FindProperty("ShowAfter");
            m_HintSound = serializedObject.FindProperty("HintSound");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (HFPS_GameManager.LocalizationEnabled)
            {
                Rect rect = GUILayoutUtility.GetRect(1, 20);
                EditorUtils.DrawLocaleSelector(rect, m_HintKey, new GUIContent("Hint Text Key"));
            }
            else
            {
                EditorGUILayout.PropertyField(m_Hint, new GUIContent("Hint Text"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_TimeShow);
            EditorGUILayout.PropertyField(m_ShowAfter);
            EditorGUILayout.PropertyField(m_HintSound);

            serializedObject.ApplyModifiedProperties();
        }
    }
}