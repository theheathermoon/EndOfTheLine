using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomEditor(typeof(ObjectiveEvent)), CanEditMultipleObjects]
    public class ObjectiveEventEditor : Editor
    {
        ObjectiveManager objectiveManager;

        SerializedProperty p_EventID;
        SerializedProperty p_CompleteEvent;

        private void OnEnable()
        {
            p_EventID = serializedObject.FindProperty("EventID");
            p_CompleteEvent = serializedObject.FindProperty("CompleteEvent");

            if (ObjectiveManager.HasReference)
                objectiveManager = ObjectiveManager.Instance;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            Rect evtRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
            evtRect.xMax -= EditorGUIUtility.singleLineHeight;

            p_EventID.stringValue = EditorGUI.TextField(evtRect, "Event ID", p_EventID.stringValue);

            Rect selectBtn = evtRect;
            selectBtn.width = EditorGUIUtility.singleLineHeight;
            selectBtn.x = evtRect.xMax;

            GUIContent linkIcon = objectiveManager != null ? EditorUtils.Styles.Linked : EditorUtils.Styles.UnLinked;
            linkIcon.tooltip = "Show Objective Event Browser";

            using (new EditorGUI.DisabledGroupScope(objectiveManager == null))
            {
                if (GUI.Button(selectBtn, linkIcon, EditorUtils.Styles.IconButton))
                {
                    EditorWindow browser = EditorWindow.GetWindow<ObjEventWindow>(true, "Objective Event Browser", true);
                    browser.minSize = new Vector2(320, 500);
                    browser.maxSize = new Vector2(320, 500);

                    ObjEventWindow eventWindow = (ObjEventWindow)browser;

                    eventWindow.OnSelectItem += value =>
                    {
                        p_EventID.stringValue = value;
                        p_EventID.serializedObject.ApplyModifiedProperties();
                    };

                    eventWindow.Show(objectiveManager);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(p_CompleteEvent);

            serializedObject.ApplyModifiedProperties();
        }

        internal class ObjEventWindow : EditorWindow
        {
            protected ObjectiveManager objManager;
            protected SearchField searchField;

            protected string searchString;
            protected Vector2 scrollPos;

            public event System.Action<string> OnSelectItem;

            public void Show(ObjectiveManager manager)
            {
                searchField = new SearchField();
                objManager = manager;
            }

            void OnGUI()
            {
                var searchRect = GUILayoutUtility.GetRect(1f, 20f);
                searchRect.y += 6f;
                searchRect.x += 3f;
                searchRect.xMax -= 4f;
                searchString = searchField.OnGUI(searchRect, searchString);
                EditorGUILayout.Space();

                var searchResult = GetSearchResult(searchString);

                EditorGUILayout.LabelField($"Result [{searchResult.Length}]", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                if (searchResult.Length > 0)
                {
                    foreach (var key in searchResult)
                    {
                        var rect = GUILayoutUtility.GetRect(1f, 20f);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.LabelField(rect, key);

                        var assignBtn = rect;
                        var assignTxt = new GUIContent("Assign");
                        var scaleX = EditorStyles.miniButton.CalcSize(assignTxt).x;
                        assignBtn.xMin = rect.xMax - scaleX - 5f;

                        if (GUI.Button(assignBtn, assignTxt, EditorStyles.miniButton))
                        {
                            OnSelectItem?.Invoke(key);
                            Close();
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("The specified key could not be found!");
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }

            string[] GetSearchResult(string search)
            {
                if (objManager != null)
                {
                    var objs = objManager.SceneObjectives;

                    if (objs != null)
                    {
                        if (!string.IsNullOrEmpty(search))
                        {
                            string m_search = search.ToLower().Replace(" ", "");

                            return (from item in objs.Objectives
                                    let title = item.eventID
                                    where !string.IsNullOrEmpty(title)
                                    where title.Contains(m_search)
                                    select title).ToArray();
                        }
                        else
                        {
                            return objs.Objectives.Where(x => !string.IsNullOrEmpty(x.eventID)).Select(x => x.eventID).ToArray();
                        }
                    }
                }

                return new string[0];
            }
        }
    }
}