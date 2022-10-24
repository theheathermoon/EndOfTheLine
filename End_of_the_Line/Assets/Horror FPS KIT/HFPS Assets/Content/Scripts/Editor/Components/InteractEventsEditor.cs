using UnityEditor;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomEditor(typeof(InteractEvents))]
    public class InteractEventsEditor : Editor
    {
        private SerializedProperty m_InteractType;
        private SerializedProperty m_RepeatMode;

        private SerializedProperty m_InteractObject;
        private SerializedProperty m_InteractCall;

        private SerializedProperty m_AnimationName;
        private SerializedProperty m_AnimationSpeed;

        private SerializedProperty m_InteractEvent;
        private SerializedProperty m_InteractBackEvent;

        private SerializedProperty m_CancelExamine;
        private SerializedProperty m_WaitForNextSound;

        private SerializedProperty m_InteractSound;
        private SerializedProperty m_InteractVolume;

        private void OnEnable()
        {
            m_InteractType = serializedObject.FindProperty("InteractType");
            m_RepeatMode = serializedObject.FindProperty("RepeatMode");

            m_InteractObject = serializedObject.FindProperty("InteractObject");
            m_InteractCall = serializedObject.FindProperty("InteractCall");

            m_AnimationName = serializedObject.FindProperty("AnimationName");
            m_AnimationSpeed = serializedObject.FindProperty("AnimationSpeed");

            m_InteractEvent = serializedObject.FindProperty("InteractEvent");
            m_InteractBackEvent = serializedObject.FindProperty("InteractBackEvent");

            m_CancelExamine = serializedObject.FindProperty("CancelExamine");
            m_WaitForNextSound = serializedObject.FindProperty("WaitForNextSound");

            m_InteractSound = serializedObject.FindProperty("InteractSound");
            m_InteractVolume = serializedObject.FindProperty("InteractVolume");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_InteractType);
            EditorGUILayout.PropertyField(m_RepeatMode);
            InteractEvents.Type interactType = (InteractEvents.Type)m_InteractType.enumValueIndex;

            EditorGUILayout.Space();

            if (interactType == InteractEvents.Type.InteractCall)
            {
                EditorGUILayout.LabelField("Call Settings", EditorStyles.miniBoldLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_InteractObject);
                    EditorGUILayout.PropertyField(m_InteractCall);
                }
            }
            else if (interactType == InteractEvents.Type.Animation)
            {
                EditorGUILayout.LabelField("Animation Settings", EditorStyles.miniBoldLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_AnimationName);
                    EditorGUILayout.PropertyField(m_AnimationSpeed);
                }
            }
            else if (interactType == InteractEvents.Type.Event)
            {
                EditorGUILayout.LabelField("Event Settings", EditorStyles.miniBoldLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_InteractEvent);
                    EditorGUILayout.PropertyField(m_InteractBackEvent);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Interact Settings", EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_CancelExamine);
                EditorGUILayout.PropertyField(m_WaitForNextSound);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sounds", EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_InteractSound);
                EditorGUILayout.PropertyField(m_InteractVolume);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}