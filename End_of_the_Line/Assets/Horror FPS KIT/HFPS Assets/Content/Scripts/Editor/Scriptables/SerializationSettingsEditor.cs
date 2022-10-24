using UnityEditor;
using ThunderWire.Helpers;

namespace ThunderWire.Editors
{
    [CustomEditor(typeof(SerializationSettings)), CanEditMultipleObjects]
    public class SerializationSettingsEditor : Editor
    {
        private SerializedProperty p_EnableEncription;
        private SerializedProperty p_SerializePath;
        private SerializedProperty p_EncryptionKey;

        private void OnEnable()
        {
            p_EnableEncription = serializedObject.FindProperty("EncryptData");
            p_SerializePath = serializedObject.FindProperty("SerializePath");
            p_EncryptionKey = serializedObject.FindProperty("EncryptionKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(p_EncryptionKey);
            EditorGUILayout.PropertyField(p_EnableEncription);
            EditorGUILayout.PropertyField(p_SerializePath);

            serializedObject.ApplyModifiedProperties();
        }
    }
}