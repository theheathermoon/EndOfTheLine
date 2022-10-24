using UnityEngine;
using UnityEditor;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomEditor(typeof(Message)), CanEditMultipleObjects]
    public class MessageEditor : Editor
    {
        private SerializedProperty m_MessageType;
        private SerializedProperty m_Message;
        private SerializedProperty m_MessageKey;
        private SerializedProperty m_MessageTime;

        private void OnEnable()
        {
            m_MessageType = serializedObject.FindProperty("messageType");
            m_Message = serializedObject.FindProperty("message");
            m_MessageKey = serializedObject.FindProperty("messageKey");
            m_MessageTime = serializedObject.FindProperty("messageTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (HFPS_GameManager.LocalizationEnabled)
            {
                Rect rect = GUILayoutUtility.GetRect(1, 20);
                EditorUtils.DrawLocaleSelector(rect, m_MessageKey, new GUIContent("Message Key"));
            }
            else
            {
                EditorGUILayout.PropertyField(m_Message, new GUIContent("Message Text"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_MessageType);
            EditorGUILayout.PropertyField(m_MessageTime);

            serializedObject.ApplyModifiedProperties();
        }
    }
}