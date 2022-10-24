using UnityEngine;
using UnityEditor;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomEditor(typeof(InteractiveLight)), CanEditMultipleObjects]
    public class InteractiveLightEditor : Editor
    {
        private SerializedProperty interactType;
        private SerializedProperty lightType;
        private SerializedProperty lightObj;
        private SerializedProperty emissionObj;
        private SerializedProperty emissionString;
        private SerializedProperty emissionKeyword;

        private SerializedProperty electricity;
        private SerializedProperty animationObj;
        private SerializedProperty animationName;

        private SerializedProperty switchOnAnim;
        private SerializedProperty switchOffAnim;

        private SerializedProperty SwitchOn;
        private SerializedProperty SwitchOff;
        private SerializedProperty volume;

        private SerializedProperty isPoweredOn;

        void OnEnable()
        {
            interactType = serializedObject.FindProperty("interactType");
            lightType = serializedObject.FindProperty("lightType");
            lightObj = serializedObject.FindProperty("lightObj");
            emissionObj = serializedObject.FindProperty("emissionObj");
            emissionString = serializedObject.FindProperty("emissionString");
            emissionKeyword = serializedObject.FindProperty("emissionKeyword");

            electricity = serializedObject.FindProperty("electricity");
            animationObj = serializedObject.FindProperty("animationObj");
            animationName = serializedObject.FindProperty("animationName");
            switchOnAnim = serializedObject.FindProperty("switchOnAnim");
            switchOffAnim = serializedObject.FindProperty("switchOffAnim");
            SwitchOn = serializedObject.FindProperty("SwitchOn");
            SwitchOff = serializedObject.FindProperty("SwitchOff");
            volume = serializedObject.FindProperty("volume");
            isPoweredOn = serializedObject.FindProperty("isPoweredOn");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            InteractiveLight.InteractType m_interactType = (InteractiveLight.InteractType)interactType.enumValueIndex;
            InteractiveLight.LightType m_lightType = (InteractiveLight.LightType)lightType.enumValueIndex;

            EditorGUILayout.PropertyField(interactType);
            EditorGUILayout.PropertyField(lightType);
            EditorGUILayout.Space();

            if (m_interactType == InteractiveLight.InteractType.Lamp)
            {
                EditorGUILayout.PropertyField(lightObj, new GUIContent("Light"));
                EditorGUILayout.PropertyField(emissionObj, new GUIContent("Emission Renderer"));
                EditorGUILayout.PropertyField(emissionKeyword, new GUIContent("Emission Keyword"));
                EditorGUILayout.PropertyField(emissionString, new GUIContent("Emission Property"));

                if (m_lightType == InteractiveLight.LightType.Animation)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(animationObj, new GUIContent("Animation"));
                    EditorGUILayout.PropertyField(animationName, new GUIContent("Animation Name"));
                }
            }
            else if (m_interactType == InteractiveLight.InteractType.Switch)
            {
                if (m_lightType == InteractiveLight.LightType.Animation)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(animationObj, new GUIContent("Animation"));
                    EditorGUILayout.PropertyField(switchOnAnim, new GUIContent("Switch On Animation"));
                    EditorGUILayout.PropertyField(switchOffAnim, new GUIContent("Switch Off Animation"));
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sounds", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(SwitchOn, new GUIContent("Switch ON"));
            EditorGUILayout.PropertyField(SwitchOff, new GUIContent("Switch OFF"));
            EditorGUILayout.PropertyField(volume, new GUIContent("Switch Volume"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Extra", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(electricity, new GUIContent("Electricity"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(isPoweredOn, new GUIContent("isPoweredOn"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}