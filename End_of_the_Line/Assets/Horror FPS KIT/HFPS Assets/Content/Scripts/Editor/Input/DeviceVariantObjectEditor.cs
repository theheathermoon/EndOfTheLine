using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HFPS.Systems;

namespace ThunderWire.Input.Editor
{
    [CustomEditor(typeof(DeviceVariantObject)), CanEditMultipleObjects]
    public class DeviceVariantObjectEditor : UnityEditor.Editor
    {
        SerializedProperty m_actionImageObj;
        SerializedProperty m_disableOnPC;
        SerializedProperty m_deviceChangeActions;
        SerializedProperty m_actionReference;

        private void OnEnable()
        {
            m_actionImageObj = serializedObject.FindProperty("actionImageObj");
            m_disableOnPC = serializedObject.FindProperty("disableOnPC");
            m_deviceChangeActions = serializedObject.FindProperty("deviceChangeActions");
            m_actionReference = serializedObject.FindProperty("actionReference");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_deviceChangeActions);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Input Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_actionImageObj);
            EditorGUILayout.PropertyField(m_disableOnPC);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_actionReference);

            serializedObject.ApplyModifiedProperties();
        }

        [CustomPropertyDrawer(typeof(DeviceVariantObject.DeviceChange))]
        internal class DeviceChangeDrawer : PropertyDrawer
        {
            private float SPACING => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            private int bindingPopupIndex;

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return (SPACING * 2) + EditorGUIUtility.standardVerticalSpacing;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                property.serializedObject.Update();

                DeviceVariantObject instance = property.serializedObject.targetObject as DeviceVariantObject;

                EditorGUI.BeginProperty(position, label, property);

                Rect deviceRect = position;
                deviceRect.height = EditorGUIUtility.singleLineHeight;
                deviceRect.y += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(deviceRect, property.FindPropertyRelative("device"));

                Rect bindingIndexRect = position;
                bindingIndexRect.height = EditorGUIUtility.singleLineHeight;
                bindingIndexRect.y += SPACING + EditorGUIUtility.standardVerticalSpacing;

                if (InputHandler.HasReference)
                {
                    SerializedProperty bindingIndex = property.FindPropertyRelative("bindingIndex");
                    string[] bindings = GetActionIndexes(instance.actionReference);

                    IList<int> indexes = new List<int>();
                    foreach (var value in bindings)
                    {
                        if (value == "None")
                            indexes.Add(-1);
                        else
                            indexes.Add(int.Parse(value.Split(' ')[0]));
                    }

                    if (indexes.Count > 0 && indexes.Contains(bindingIndex.intValue))
                        bindingPopupIndex = indexes.IndexOf(bindingIndex.intValue);

                    bindingPopupIndex = EditorGUI.Popup(bindingIndexRect, "Binding Index", bindingPopupIndex, bindings.ToArray());
                    bindingIndex.intValue = indexes[bindingPopupIndex];
                }
                else
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUI.TextField(bindingIndexRect, "Binding Index", "Input Handler missing!");
                    }
                }

                EditorGUI.EndProperty();

                property.serializedObject.ApplyModifiedProperties();
            }

            string[] GetActionIndexes(InputActionRef inputAction)
            {
                IList<string> indexes = new List<string>
                {
                    "None"
                };

                if (!string.IsNullOrEmpty(inputAction.ActionMap) && !string.IsNullOrEmpty(inputAction.ActionName))
                {
                    InputHandler input = InputHandler.Instance;

                    if (input.inputActionAsset != null)
                    {
                        if (input.inputActionAsset.actionMaps.Any(x => x.name == inputAction.ActionMap))
                        {
                            var inputActionMap = input.inputActionAsset.FindActionMap(inputAction.ActionMap);

                            if (inputActionMap.actions.Any(x => x.name == inputAction.ActionName))
                            {
                                var inputBindings = inputActionMap.FindAction(inputAction.ActionName).bindings;

                                for (int i = 0; i < inputBindings.Count; i++)
                                {
                                    var binding = inputBindings[i];

                                    if (!binding.isComposite)
                                    {
                                        string name = InputHandler.RealDisplayString(binding.path);
                                        indexes.Add(string.Format("{0} {1} [{2}]", i, name, binding.groups));
                                    }
                                }
                            }
                        }


                    }
                }

                return indexes.ToArray();
            }
        }
    }
}