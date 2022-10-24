using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ThunderWire.Input.Editor
{
    [CustomPropertyDrawer(typeof(InputActionRef))]
    public class InputActionRefDrawer : PropertyDrawer
    {
        private float SPACING => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        private int mapsPopupIndex;
        private int actionsPopupIndex;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + SPACING * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty m_ActionMap = property.FindPropertyRelative("ActionMap");
            SerializedProperty m_ActionName = property.FindPropertyRelative("ActionName");

            EditorGUI.BeginProperty(position, label, property);

            Rect labelRect = position;
            labelRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);

            Rect mapRect = position;
            mapRect.height = EditorGUIUtility.singleLineHeight;
            mapRect.y += SPACING;

            Rect actionNameRect = position;
            actionNameRect.height = EditorGUIUtility.singleLineHeight;
            actionNameRect.y += SPACING * 2;

            EditorGUI.indentLevel++;

            if (InputHandler.HasReference)
            {
                IList<string> maps = GetInputSchemeMaps();

                if (maps.Count > 0 && !string.IsNullOrEmpty(m_ActionMap.stringValue) && maps.Contains(m_ActionMap.stringValue))
                    mapsPopupIndex = maps.IndexOf(m_ActionMap.stringValue);

                int mapsIndex = EditorGUI.Popup(mapRect, "Action Map", mapsPopupIndex, maps.ToArray());
                if (mapsIndex != mapsPopupIndex)
                {
                    actionsPopupIndex = 0;
                    m_ActionName.stringValue = string.Empty;
                }

                mapsPopupIndex = mapsIndex;
                m_ActionMap.stringValue = maps[mapsPopupIndex];

                if (maps.Count > 0 && !string.IsNullOrEmpty(m_ActionMap.stringValue))
                {
                    IList<string> actions = GetMapsActions(m_ActionMap.stringValue);
                    if (actions.Count > 0 && !string.IsNullOrEmpty(m_ActionName.stringValue) && actions.Contains(m_ActionName.stringValue))
                        actionsPopupIndex = actions.IndexOf(m_ActionName.stringValue);

                    actionsPopupIndex = EditorGUI.Popup(actionNameRect, "Action Name", actionsPopupIndex, actions.ToArray());
                    m_ActionName.stringValue = actions[actionsPopupIndex];
                }
            }
            else
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUI.TextField(mapRect, "Action Map", "Input Handler missing!");
                    EditorGUI.TextField(actionNameRect, "Action Name", "Input Handler missing!");
                }
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        IList<string> GetInputSchemeMaps()
        {
            InputHandler instance = InputHandler.Instance;
            if (instance.inputActionAsset != null)
            {
                return instance.inputActionAsset.actionMaps.Select(x => x.name).ToList();
            }

            return new List<string> { "None" };
        }

        IList<string> GetMapsActions(string map)
        {
            InputHandler instance = InputHandler.Instance;
            if (instance.inputActionAsset != null)
            {
                return instance.inputActionAsset.FindActionMap(map).actions.Select(x => x.name).ToList();
            }

            return new List<string> { "None" };
        }
    }
}