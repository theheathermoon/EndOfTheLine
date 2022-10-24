using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using HFPS.Systems;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
using ThunderWire.Localization.Editor;
#endif

namespace HFPS.Editors
{
    [CustomEditor(typeof(ObjectivesScriptable)), CanEditMultipleObjects]
    public class ObjectivesScriptableEditor : Editor
    {
        public sealed class Foldout
        {
            public bool baseState = false;
            public float height = 0f;
            public bool[] categories = new bool[3];
        }

        SerializedProperty m_Objectives;
        SerializedProperty m_Localization;
        ObjectivesScriptable reference;

        private bool localizationExist = false;

        private bool showListFoldout = false;
        protected List<Foldout> foldout = new List<Foldout>();

        private void OnEnable()
        {
            m_Objectives = serializedObject.FindProperty("Objectives");
            m_Localization = serializedObject.FindProperty("enableLocalization");
            reference = target as ObjectivesScriptable;

            for (int i = 0; i < reference.Objectives.Count; i++)
            {
                foldout.Add(new Foldout());
            }

#if TW_LOCALIZATION_PRESENT
            localizationExist = LocalizationSystem.HasReference;
#endif
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Rect localeRect = GUILayoutUtility.GetRect(1, 20);
            localeRect.y += EditorGUIUtility.standardVerticalSpacing * 2;

            Rect infoRect = localeRect;
            infoRect.xMax = EditorGUIUtility.singleLineHeight;
            infoRect.width = EditorGUIUtility.singleLineHeight;
            GUIContent icon = EditorGUIUtility.TrIconContent("console.warnicon.sml", "HFPS Localization Integration is Required!");
            EditorGUI.LabelField(infoRect, icon, GUIStyle.none);

            localeRect.xMin += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(localeRect, m_Localization);

            EditorGUILayout.Space();

            Repaint();

            EditorGUI.BeginChangeCheck();
            DrawObjectivesGUI(m_Objectives);

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in reference.Objectives)
                {
                    obj.objectiveID = reference.Objectives.IndexOf(obj);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawObjectivesGUI(SerializedProperty list)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            // Header
            Rect rect = GUILayoutUtility.GetRect(1f, 20);
            showListFoldout = DrawFoldoutHeader(rect, 20, serializedObject.targetObject.name, showListFoldout, true);

            // Add Button
            var addItemButton = rect;
            addItemButton.width = EditorGUIUtility.singleLineHeight;
            addItemButton.x += rect.width - (EditorGUIUtility.singleLineHeight * 2) - EditorGUIUtility.standardVerticalSpacing;
            addItemButton.y += EditorGUIUtility.standardVerticalSpacing;

            // Refresh Button
            var refreshButton = rect;
            refreshButton.width = EditorGUIUtility.singleLineHeight;
            refreshButton.x += rect.width - EditorGUIUtility.singleLineHeight;
            refreshButton.y += EditorGUIUtility.standardVerticalSpacing;

            if (GUI.Button(addItemButton, EditorUtils.Styles.PlusIcon, EditorUtils.Styles.IconButton))
            {
                reference.Objectives.Add(new ObjectivesScriptable.Objective());
                foldout.Add(new Foldout());
            }

            var refreshIcon = EditorUtils.Styles.RefreshIcon;
            refreshIcon.tooltip = "Refresh Objective IDs";
            if (GUI.Button(refreshButton, refreshIcon, EditorUtils.Styles.IconButton))
            {
                foreach (var obj in reference.Objectives)
                {
                    obj.objectiveID = reference.Objectives.IndexOf(obj);
                }
            }

            if (showListFoldout && list.arraySize > 0)
            {
                // Elements
                for (int i = 0; i < list.arraySize; i++)
                {
                    SerializedProperty element = list.GetArrayElementAtIndex(i);
                    SerializedProperty m_ObjID = element.FindPropertyRelative("objectiveID");
                    SerializedProperty m_Name = element.FindPropertyRelative("shortName");
                    SerializedProperty m_EventID = element.FindPropertyRelative("eventID");
                    SerializedProperty m_ObjText = element.FindPropertyRelative("objectiveText");
                    SerializedProperty m_ObjCompCount = element.FindPropertyRelative("completeCount");
                    SerializedProperty m_LocalizationKey = element.FindPropertyRelative("localeKey");

                    string title = $"[{m_ObjID.intValue}] " + (!string.IsNullOrEmpty(m_Name.stringValue) ? m_Name.stringValue : $"Objective {i + 1}");

                    GUILayout.BeginVertical(GUI.skin.box);

                    // Element Header
                    Rect elemRect = GUILayoutUtility.GetRect(1f, 20);

                    // Remove Button
                    var removeItemButton = elemRect;
                    removeItemButton.width = EditorGUIUtility.singleLineHeight;
                    removeItemButton.x += elemRect.width - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
                    removeItemButton.y += EditorGUIUtility.standardVerticalSpacing;

                    if (GUI.Button(removeItemButton, EditorUtils.Styles.MinusIcon, EditorUtils.Styles.IconButton))
                    {
                        reference.Objectives.RemoveAt(i);
                        foldout.RemoveAt(i);
                        continue;
                    }

                    //Element Content
                    if (foldout[i].baseState = DrawFoldoutHeader(elemRect, 2, title, foldout[i].baseState, true))
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(m_Name);

                        EditorGUILayout.Space(5f);
                        EditorGUILayout.LabelField("Objective Properties", EditorStyles.boldLabel);

                        // Events Foldout
                        Rect eventsRect = GUILayoutUtility.GetRect(1f, 20);
                        eventsRect.width -= EditorGUIUtility.singleLineHeight + (EditorGUIUtility.standardVerticalSpacing * 2);
                        eventsRect.x += EditorGUIUtility.singleLineHeight;

                        if (foldout[i].categories[0] = EditorGUI.BeginFoldoutHeaderGroup(eventsRect, foldout[i].categories[0], "Events"))
                        {
                            EditorGUILayout.PropertyField(m_EventID);
                        }
                        EditorGUI.EndFoldoutHeaderGroup();

                        // Objective Foldout
                        Rect objRect = GUILayoutUtility.GetRect(1f, 20);
                        objRect.width -= EditorGUIUtility.singleLineHeight + (EditorGUIUtility.standardVerticalSpacing * 2);
                        objRect.x += EditorGUIUtility.singleLineHeight;

                        if (foldout[i].categories[1] = EditorGUI.BeginFoldoutHeaderGroup(objRect, foldout[i].categories[1], "Objective"))
                        {
                            if(localizationExist && m_Localization.boolValue)
                            {
                                EditorUtils.TrHelpIconText("<b>Objective Text</b> will change depending on the current localization.", MessageType.Warning, true, false);
                            }

                            using (new EditorGUI.DisabledGroupScope(m_Localization.boolValue && localizationExist))
                            {
                                EditorGUILayout.PropertyField(m_ObjText);
                            }
                            EditorGUILayout.PropertyField(m_ObjCompCount);
                        }
                        EditorGUI.EndFoldoutHeaderGroup();

                        // Localization Foldout
                        Rect localeRect = GUILayoutUtility.GetRect(1f, 20);
                        localeRect.width -= EditorGUIUtility.singleLineHeight + (EditorGUIUtility.standardVerticalSpacing * 2);
                        localeRect.x += EditorGUIUtility.singleLineHeight;
                        DrawLocalizationHeader(ref foldout[i].categories[2], localeRect, m_LocalizationKey, "Localization");

                        EditorGUI.indentLevel--;
                    }

                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawLocalizationHeader(ref bool state, Rect rect, SerializedProperty property, string label)
        {
            if (state = EditorGUI.BeginFoldoutHeaderGroup(rect, state, label))
            {
#if !TW_LOCALIZATION_PRESENT
                EditorUtils.TrHelpIconText("<b>HFPS Localization System Integration</b> is required in order to translate Objective Text!", MessageType.Warning, true, false);
#endif
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                using (new EditorGUI.DisabledGroupScope(!m_Localization.boolValue || !localizationExist))
                {
                    Rect localeKeyRect = GUILayoutUtility.GetRect(1, 20);
                    DrawLocalizationSelector(ref localeKeyRect, property);
                    EditorGUI.PropertyField(localeKeyRect, property);
                }

                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
            EditorGUI.EndFoldoutHeaderGroup();
        }

        private void DrawLocalizationSelector(ref Rect pos, SerializedProperty property)
        {
            pos.xMax -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            Rect selectRect = pos;
            selectRect.width = EditorGUIUtility.singleLineHeight;
            selectRect.x = pos.xMax + EditorGUIUtility.standardVerticalSpacing;
            selectRect.y += 0.6f;

            GUIContent Linked = EditorGUIUtility.TrIconContent("d_Linked", "Open Localization System Key Browser");
            GUIContent UnLinked = EditorGUIUtility.TrIconContent("d_Unlinked", "Localization System not found!");

            GUIContent icon = localizationExist ? Linked : UnLinked;

            using (new EditorGUI.DisabledGroupScope(!m_Localization.boolValue && !localizationExist))
            {
                if (GUI.Button(selectRect, icon, EditorUtils.Styles.IconButton))
                {
#if TW_LOCALIZATION_PRESENT
                    EditorWindow browser = EditorWindow.GetWindow<LocalizationUtility.LocaleKeyBrowserWindow>(true, "Localization Key Browser", true);
                    browser.minSize = new Vector2(320, 500);
                    browser.maxSize = new Vector2(320, 500);

                    LocalizationUtility.LocaleKeyBrowserWindow keyBrowser = browser as LocalizationUtility.LocaleKeyBrowserWindow;
                    keyBrowser.OnSelectKey += key =>
                    {
                        property.stringValue = key;
                        property.serializedObject.ApplyModifiedProperties();
                    };

                    keyBrowser.Show(LocalizationSystem.Instance);
#endif
                }
            }
        }

        bool DrawFoldoutHeader(Rect rect, float hoverDiff, string title, bool state, bool miniLabel)
        {
            Color headerColor = new Color(0.1f, 0.1f, 0.1f, 0f);

            var foldoutRect = rect;
            foldoutRect.y += 4f;
            foldoutRect.x += 2f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            var labelRect = rect;
            labelRect.y += miniLabel ? EditorGUIUtility.standardVerticalSpacing : 0f;
            labelRect.xMin += 16f;
            labelRect.xMax -= 20f;

            var btnRect = rect;
            btnRect.xMin = labelRect.xMax - 3f;
            btnRect.yMax = labelRect.yMax;

            var hoverRect = rect;
            hoverRect.xMax -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + hoverDiff;

            // events
            var e = Event.current;
            if (hoverRect.Contains(e.mousePosition))
            {
                headerColor = new Color(0.6f, 0.6f, 0.6f, 0.2f);

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    state = !state;
                    e.Use();
                }
            }

            // background
            EditorGUI.DrawRect(rect, headerColor);

            // foldout toggle
            state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);

            // title
            EditorGUI.LabelField(labelRect, new GUIContent(title), miniLabel ? EditorStyles.miniBoldLabel : EditorStyles.boldLabel);

            return state;
        }
    }
}