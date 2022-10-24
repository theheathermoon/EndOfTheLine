using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomPropertyDrawer(typeof(ObjectReference))]
    public class ObjectReferenceDrawer : PropertyDrawer
    {
        private bool ReferencesAssetExist;
        private bool ReferenceExist;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIContent guiText = new GUIContent("None (ObjectReference)");
            SerializedProperty propGUID = property.FindPropertyRelative("GUID");
            SerializedProperty propObject = property.FindPropertyRelative("Object");
            GameObject go = propObject.objectReferenceValue as GameObject;

            ReferenceExist = false;
            if (SaveGameHandler.HasReference)
            {
                ObjectReferences references = SaveGameHandler.Instance.objectReferences;
                ReferencesAssetExist = references != null;
                ReferenceExist = ReferencesAssetExist && references.HasReference(go);
            }

            using (new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.BeginProperty(position, label, property);
                {
                    position = EditorGUI.PrefixLabel(position, label);

                    if (go != null)
                    {
                        if (ReferenceExist)
                        {
                            string title = $"{go.name} ({propGUID.stringValue})";
                            guiText = EditorGUIUtility.TrTextContentWithIcon(title, "PrefabVariant Icon");
                        }
                        else
                        {
                            string title = $"Not Linked! ({go.name})";
                            guiText = EditorGUIUtility.TrTextContentWithIcon(title, "Error");
                        }

                        Event e = Event.current;
                        Rect pingRect = position;
                        pingRect.xMax -= 19;
                        if (pingRect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
                        {
                            EditorGUIUtility.PingObject(go);
                        }
                    }

                    EditorGUIUtility.SetIconSize(new Vector2(12, 12));
                    GUI.Box(position, guiText, EditorStyles.objectField);

                    GUIStyle buttonStyle = new GUIStyle("ObjectFieldButton");
                    Rect buttonRect = buttonStyle.margin.Remove(new Rect(position.xMax - 19, position.y, 19, position.height));

                    GUIContent btnContent = new GUIContent();
                    if (!ReferencesAssetExist)
                        btnContent.tooltip = "SaveGameHandler instance missing or no Object References assigned!";

                    using (new EditorGUI.DisabledGroupScope(!ReferencesAssetExist))
                    {
                        if (GUI.Button(buttonRect, btnContent, buttonStyle))
                        {
                            ObjectReferencePicker objPicker = EditorWindow.GetWindow<ObjectReferencePicker>(true, "Select Object Reference", true);
                            objPicker.minSize = new Vector2(400, 600);
                            objPicker.maxSize = new Vector2(400, 600);

                            objPicker.OnSelectReference += obj =>
                            {
                                if (obj.Object == null) property.SetValue(null);
                                else property.SetValue(obj);
                            };

                            objPicker.Show();
                        }
                    }
                }
                EditorGUI.EndProperty();
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }
        }

        internal class ObjectReferencePicker : EditorWindow
        {
            public event Action<ObjectReference> OnSelectReference;

            List<ObjectReference> objectReferences;
            private SearchField searchField;
            private string searchString;
            private Vector2 scrollPos;

            private void OnEnable()
            {
                SaveGameHandler saveGameHandler = SaveGameHandler.Instance;
                searchField = new SearchField();

                if (saveGameHandler && saveGameHandler.objectReferences)
                {
                    objectReferences = new List<ObjectReference>(saveGameHandler.objectReferences.References);
                    objectReferences.Insert(0, new ObjectReference() { Object = null });
                }
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
                    foreach (var result in searchResult)
                    {
                        var rect = GUILayoutUtility.GetRect(1f, 20f);

                        EditorGUILayout.BeginHorizontal();
                        {
                            string title = result.Object != null ? result.Object.name : "None";
                            EditorGUI.LabelField(rect, title);

                            var assignBtn = rect;
                            var assignTxt = new GUIContent("Assign");
                            var scaleX = EditorStyles.miniButton.CalcSize(assignTxt).x;
                            assignBtn.xMin = rect.xMax - scaleX - 5f;

                            if (GUI.Button(assignBtn, assignTxt, EditorStyles.miniButton))
                            {
                                OnSelectReference?.Invoke(result);
                                Close();
                            }
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

            ObjectReference[] GetSearchResult(string search)
            {
                if (!string.IsNullOrEmpty(search))
                {
                    string m_search = search.ToLower().Replace(" ", "");

                    return (from reference in objectReferences
                            where reference.Object != null
                            let title = reference.Object.name.ToLower().Replace(" ", "")
                            where title.Contains(m_search)
                            select reference).ToArray();
                }

                return objectReferences.ToArray();
            }
        }
    }
}