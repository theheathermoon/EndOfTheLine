using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using ThunderWire.Editors;

namespace ThunderWire.Input.Editor
{
    [CustomEditor(typeof(InputHandler))]
    public class InputHandlerEditor : UnityEditor.Editor
    {
        private InputHandler inputHandler;

        private SerializedProperty m_InputActionAsset;
        private SerializedProperty m_InputSprites;
        private SerializedProperty m_DefaultMap;
        private SerializedProperty m_DefaultMapID;
        private SerializedProperty m_PrefferedDevice;
        private SerializedProperty m_DebugMode;
        private SerializedProperty m_ApplyManually;
        private SerializedProperty m_MaskManually;

        void OnEnable()
        {
            m_InputActionAsset = serializedObject.FindProperty("inputActionAsset");
            m_InputSprites = serializedObject.FindProperty("inputSprites");
            m_DefaultMap = serializedObject.FindProperty("defaultMapString");
            m_DefaultMapID = serializedObject.FindProperty("defaultMapID");
            m_PrefferedDevice = serializedObject.FindProperty("prefferedDevice");
            m_DebugMode = serializedObject.FindProperty("debugMode");
            m_ApplyManually = serializedObject.FindProperty("applyManually");
            m_MaskManually = serializedObject.FindProperty("maskInputManually");

            inputHandler = target as InputHandler;
            inputHandler.GetConnectedDevices();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_InputActionAsset, new GUIContent("Input Asset"));
            EditorGUILayout.PropertyField(m_InputSprites, new GUIContent("Input Sprites"));
            EditorGUILayout.PropertyField(m_DebugMode, new GUIContent("Debug Mode"));
            EditorGUILayout.PropertyField(m_ApplyManually, new GUIContent("Apply Manually"));
            EditorGUILayout.PropertyField(m_MaskManually, new GUIContent("Mask Input Manually"));
            EditorGUILayout.Space();

            if (m_InputActionAsset.objectReferenceValue != null)
            {
                string[] actionMaps = (m_InputActionAsset.objectReferenceValue as InputActionAsset).actionMaps.Select(x => x.name).ToArray();
                string[] schemes = (m_InputActionAsset.objectReferenceValue as InputActionAsset).controlSchemes.Select(x => x.name).ToArray();

                if (actionMaps.Length > 0 && schemes.Length > 0)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);

                    EditorGUILayout.LabelField("Input Settings", EditorStyles.miniBoldLabel);
                    using (new EditorGUI.IndentLevelScope())
                    {
                        m_DefaultMapID.intValue = EditorGUILayout.Popup(new GUIContent("Default Map"), m_DefaultMapID.intValue, actionMaps);
                        m_DefaultMap.stringValue = actionMaps[m_DefaultMapID.intValue];

                        EditorGUILayout.PropertyField(m_PrefferedDevice);
                    }

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    Rect devicesRectLabel = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(devicesRectLabel, "Connected Devices", EditorStyles.miniBoldLabel);

                    Rect refreshRect = devicesRectLabel;
                    refreshRect.xMin = refreshRect.xMax - EditorGUIUtility.singleLineHeight;
                    if(GUI.Button(refreshRect, EditorGUIUtility.TrIconContent("Refresh", "Refresh Devices"), EditorUtils.Styles.IconButton))
                    {
                        inputHandler.GetConnectedDevices();
                    }

                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.LabelField(GetConnectedDevicesString(), EditorStyles.helpBox);
                    }
                    EditorGUILayout.EndVertical();

                    string[] actions = (m_InputActionAsset.objectReferenceValue as InputActionAsset).actionMaps[m_DefaultMapID.intValue].actions.Select(x => x.name).ToArray();
                    string actionString = string.Join(", ", actions);

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.LabelField("All Actions", EditorStyles.miniBoldLabel);
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.LabelField(actionString, EditorStyles.helpBox);
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.HelpBox("It seems that Inputs Asset does not have any Schemes or Actions. Add two schemes \"Keyboard\", \"Gamepad\".", MessageType.Error);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please assign the Input Actions Asset.", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();
        }

        string GetConnectedDevicesString()
        {
            var devices = inputHandler.connectedDevices.ToArray();
            string resultString = string.Empty;

            for(int i = 0; i < devices.Length; i++)
            {
                string deviceName = devices[i].DeviceToPrettyName();

                if (i < devices.Length - 1)
                {
                    resultString += deviceName + ", ";
                }
                else
                {
                    resultString += deviceName;
                }
            }

            return resultString;
        }
    }
}