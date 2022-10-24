using UnityEngine;
using UnityEditor;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomEditor(typeof(InventoryFixedContainer)), CanEditMultipleObjects]
    public class FixedContainerEditor : Editor
    {
        SerializedProperty m_ContainerName;
        SerializedProperty m_ContainerNameKey;
        SerializedProperty m_OpenSound;
        SerializedProperty m_Volume;

        private void OnEnable()
        {
            m_ContainerName = serializedObject.FindProperty("ContainerName");
            m_ContainerNameKey = serializedObject.FindProperty("ContainerNameKey");
            m_OpenSound = serializedObject.FindProperty("OpenSound");
            m_Volume = serializedObject.FindProperty("Volume");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!HFPS_GameManager.LocalizationEnabled)
            {
                EditorGUILayout.PropertyField(m_ContainerName);
            }
            else
            {
                Rect rect = GUILayoutUtility.GetRect(1, 20);
                EditorUtils.DrawLocaleSelector(rect, m_ContainerNameKey, new GUIContent("Container Name Key"));

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Obtained Name: " + m_ContainerName.stringValue, EditorStyles.miniBoldLabel);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sounds", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_OpenSound);
            EditorGUILayout.PropertyField(m_Volume);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
