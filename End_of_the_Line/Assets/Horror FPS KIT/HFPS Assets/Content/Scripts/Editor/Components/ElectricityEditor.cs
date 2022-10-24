using UnityEngine;
using UnityEditor;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomEditor(typeof(Electricity)), CanEditMultipleObjects]
    public class ElectricityEditor : Editor
    {
        private SerializedProperty m_offHint;
        private SerializedProperty m_offHintKey;
        private SerializedProperty m_hintTime;

        private SerializedProperty m_LampIndicator;
        private SerializedProperty m_isPoweredOn;

        private void OnEnable()
        {
            m_offHint = serializedObject.FindProperty("offHint");
            m_offHintKey = serializedObject.FindProperty("offHintKey");
            m_hintTime = serializedObject.FindProperty("hintTime");
            m_LampIndicator = serializedObject.FindProperty("LampIndicator");
            m_isPoweredOn = serializedObject.FindProperty("isPoweredOn");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (HFPS_GameManager.LocalizationEnabled)
            {
                Rect rect = GUILayoutUtility.GetRect(1, 20);
                EditorUtils.DrawLocaleSelector(rect, m_offHintKey, new GUIContent("Electricity Off Key"));
            }
            else
            {
                EditorGUILayout.PropertyField(m_offHint, new GUIContent("Electricity Off Hint"));
            }

            EditorGUILayout.PropertyField(m_hintTime);

            EditorGUILayout.PropertyField(m_LampIndicator);
            EditorGUILayout.PropertyField(m_isPoweredOn);

            serializedObject.ApplyModifiedProperties();
        }
    }
}