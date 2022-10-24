using UnityEngine;
using UnityEditor;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomEditor(typeof(InventoryContainer)), CanEditMultipleObjects]
    public class InventoryContainerEditor : Editor
    {
        SerializedProperty m_startingItems;
        SerializedProperty m_randomItems;
        SerializedProperty m_randomCount;

        SerializedProperty m_containerName;
        SerializedProperty m_containerNameKey;
        SerializedProperty m_containerSpace;
        SerializedProperty m_canStore;

        SerializedProperty m_OpenSound;
        SerializedProperty m_Volume;

        private void OnEnable()
        {
            m_startingItems = serializedObject.FindProperty("startingItems");
            m_randomItems = serializedObject.FindProperty("randomItems");
            m_randomCount = serializedObject.FindProperty("randomCount");

            m_containerName = serializedObject.FindProperty("containerName");
            m_containerNameKey = serializedObject.FindProperty("containerNameKey");
            m_containerSpace = serializedObject.FindProperty("containerSpace");
            m_canStore = serializedObject.FindProperty("canStore");

            m_OpenSound = serializedObject.FindProperty("OpenSound");
            m_Volume = serializedObject.FindProperty("Volume");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Starting Items", EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_startingItems);
                EditorGUILayout.PropertyField(m_randomItems);
                EditorGUILayout.PropertyField(m_randomCount);
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                if (!HFPS_GameManager.LocalizationEnabled)
                {
                    EditorGUILayout.PropertyField(m_containerName);
                }
                else
                {
                    Rect rect = GUILayoutUtility.GetRect(1, 20);
                    EditorUtils.DrawLocaleSelector(rect, m_containerNameKey, new GUIContent("Container Name Key"));
                }

                EditorGUILayout.PropertyField(m_containerSpace);
                EditorGUILayout.PropertyField(m_canStore);
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Sounds", EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_OpenSound);
                EditorGUILayout.PropertyField(m_Volume);
            }

            if (HFPS_GameManager.LocalizationEnabled)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Obtained Name: " + m_containerName.stringValue, EditorStyles.miniBoldLabel);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
