using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    public class ObjectReferencesWindow : EditorWindow
    {
        public struct ObjectReferenceElement
        {
            public GameObject Obj;
            public string Path;
            public string GUID;
        }

        const float HEADER_HEIGHT = 25;
        const float ROW_HEIGHT = 20;
        private ObjectReferences Target;

        [SerializeField]
        private MultiColumnHeaderState ObjRefColumnState;
        private ReferencesHeader ObjRefColumn;
        private Vector2 ColumnScrollPosition;

        private SearchField SearchField;
        private string SearchString;
        private Color DragDropColor;

        private List<ObjectReferenceElement> ObjRefArray;

        public void Init(ObjectReferences target)
        {
            Target = target;
            Refresh();

            SearchField = new SearchField();
            ObjRefColumnState = CreateMultiColumnHeaderState();
            ObjRefColumn = new ReferencesHeader(ObjRefColumnState, position.width - 10f)
            {
                height = HEADER_HEIGHT
            };

            Show();
        }

        private void Refresh()
        {
            ObjRefArray = new List<ObjectReferenceElement>();
            foreach (var kv in Target.References)
            {
                string path = AssetDatabase.GetAssetPath(kv.Object);
                ObjRefArray.Add(new ObjectReferenceElement()
                {
                    Obj = kv.Object,
                    Path = path,
                    GUID = kv.GUID
                });
            }
        }

        private void OnGUI()
        {
            Rect infoRect = new Rect(0, 0f, position.width, 30f);

            GUIContent infoText = new GUIContent("To add elements to the list, Drag and Drop a Folder or Prefabs to the window.");
            infoText = EditorGUIUtility.TrTextContentWithIcon(infoText.text, MessageType.Info);

            GUIStyle infoTextStyle = EditorStyles.helpBox;
            infoTextStyle.alignment = TextAnchor.MiddleCenter;
            infoTextStyle.fontStyle = FontStyle.Bold;

            float infoWidth = infoTextStyle.CalcSize(infoText).x;
            infoRect.width = infoWidth;
            infoRect.x = (position.width - infoWidth) / 2;
            infoRect.y += 10f;

            EditorGUI.LabelField(infoRect, infoText, infoTextStyle);

            Rect rect = new Rect(0, 0, position.width, position.height);
            rect.yMin += 50f;
            rect.width -= 10f;
            rect.x += 5f;

            ObjRefColumn.maxHeaderWidth = rect.width;

            Rect outlineRect = rect;
            outlineRect.yMin += EditorGUIUtility.singleLineHeight;
            outlineRect.width += 2f;
            outlineRect.x -= 1f;
            EditorUtils.DrawOutline(outlineRect, new RectOffset(1, 1, 1, 0));

            GUILayout.BeginArea(rect);
            {
                Rect searchFieldRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
                searchFieldRect.width = 400f;
                searchFieldRect.x = (rect.width / 2) - (searchFieldRect.width / 2);

                Rect clearRect = searchFieldRect;
                GUIContent clearText = new GUIContent("Clear all");
                clearRect.width = EditorStyles.miniButton.CalcSize(clearText).x;
                clearRect.x = rect.width - clearRect.width;
                clearRect.y -= 1f;

                using (new EditorGUI.DisabledGroupScope(Target.References.Count <= 0))
                {
                    SearchString = SearchField.OnGUI(searchFieldRect, SearchString);

                    if (GUI.Button(clearRect, "Clear All", EditorStyles.toolbarButton))
                    {
                        if (EditorUtility.DisplayDialog("Clear all object references",
                            $"Are you sure you want to clear all object references?", "OK", "Cancel"))
                        {
                            Target.References.Clear();
                            Refresh();
                            EditorUtility.SetDirty(Target);
                        }
                    }
                }

                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                Rect columnHeader = GUILayoutUtility.GetRect(1, ObjRefColumn.height);
                ObjRefColumn.OnGUI(columnHeader, 0);

                ObjectReferenceElement[] SearchResult = GetSearchResult(SearchString);
                ColumnScrollPosition = EditorGUILayout.BeginScrollView(ColumnScrollPosition);
                {
                    Event e = Event.current;

                    foreach (var elm in SearchResult)
                    {
                        int i = ObjRefArray.IndexOf(elm);

                        Rect rowRect = GUILayoutUtility.GetRect(1, ROW_HEIGHT);
                        Color color = Color.clear;
                        if (rowRect.Contains(e.mousePosition))
                            color = new Color(0.1f, 0.1f, 0.1f, 0.4f);

                        EditorGUI.DrawRect(rowRect, color);

                        for (int j = 0; j < ObjRefColumnState.columns.Length; j++)
                        {
                            int index = ObjRefColumn.GetVisibleColumnIndex(j);
                            Rect cellRect = ObjRefColumn.GetCellRect(index, rowRect);
                            OnColumnRowGUI(cellRect, index, i);
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
                DragDropAreaGUI();
            }
            GUILayout.EndArea();
            Repaint();
        }

        private ObjectReferenceElement[] GetSearchResult(string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                string m_search = search.ToLower().Replace(" ", "");

                return (from reference in ObjRefArray
                        where reference.Obj != null
                        let title = reference.Obj.name.ToLower().Replace(" ", "")
                        where title.Contains(m_search)
                        select reference).ToArray();
            }

            return ObjRefArray.ToArray();
        }

        private void OnColumnRowGUI(Rect rect, int column, int index)
        {
            ObjectReferenceElement element = ObjRefArray[index];

            rect.x += 3f;
            rect.xMax -= 10f;

            switch (column)
            {
                case 0:
                    rect.width = 15;
                    rect.height = 15;
                    rect.y += 3f;
                    EditorGUI.LabelField(rect, EditorGUIUtility.TrIconContent("PrefabVariant Icon"));
                    break;
                case 1:
                    EditorGUI.LabelField(rect, element.Obj.name);
                    break;
                case 2:
                    EditorGUI.LabelField(rect, element.Path);
                    break;
                case 3:
                    EditorGUI.LabelField(rect, element.GUID);
                    break;
                case 4:
                    rect.y += 2f;
                    if (GUI.Button(rect, EditorGUIUtility.TrIconContent("winbtn_win_close"), EditorUtils.Styles.IconButton))
                    {
                        Target.References.RemoveAt(index);
                        EditorUtility.SetDirty(Target);
                        Refresh();
                    }
                    break;
            }
        }

        private void DragDropAreaGUI()
        {
            Event evt = Event.current;

            Rect dropAreaRect = new Rect(0, 0, position.width, position.height);
            float height = HEADER_HEIGHT + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            dropAreaRect.height -= height * 2;
            dropAreaRect.y += height;

            if (!dropAreaRect.Contains(evt.mousePosition))
                DragDropColor = Color.clear;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropAreaRect.Contains(evt.mousePosition))
                        break;

                    Object[] objects = DragAndDrop.objectReferences;
                    string[] paths = DragAndDrop.paths;

                    if (CanAcceptDrag(objects, paths))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        DragDropColor = new Color(1, 1, 1, 0.05f);

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (string path in paths)
                            {
                                AddObjectToTarget(path);
                            }

                            EditorUtility.SetDirty(Target);
                            Refresh();
                        }
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                    break;
            }

            EditorGUI.DrawRect(dropAreaRect, DragDropColor);
        }

        private bool CanAcceptDrag(Object[] drag, string[] paths)
        {
            bool flag1 = drag.Any(x => x is GameObject);
            bool flag2 = paths.Any(x => AssetDatabase.IsValidFolder(x));

            return flag1 || flag2;
        }

        private void AddObjectToTarget(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                string[] assets = AssetDatabase.FindAssets("t:GameObject", new[] { path });

                foreach (string pathGuid in assets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(pathGuid);
                    GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));

                    if (!Target.References.Any(x => x.GUID == pathGuid))
                    {
                        Target.References.Add(new ObjectReference()
                        {
                            Object = obj,
                            GUID = pathGuid
                        });
                    }
                }
            }
            else
            {
                Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                if (asset.GetType() == typeof(GameObject))
                {
                    GameObject obj = (GameObject)asset;
                    string guid = AssetDatabase.AssetPathToGUID(path);

                    if (!Target.References.Any(x => x.GUID == guid))
                    {
                        Target.References.Add(new ObjectReference()
                        {
                            Object = obj,
                            GUID = guid
                        });
                    }
                }
            }
        }

        private MultiColumnHeaderState CreateMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(),
                    headerTextAlignment = TextAlignment.Left,
                    width = 20,
                    minWidth = 20,
                    maxWidth = 20,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Object Reference Name"),
                    headerTextAlignment = TextAlignment.Left,
                    width = 200,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Path"),
                    headerTextAlignment = TextAlignment.Left,
                    width = 150,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("GUID"),
                    headerTextAlignment = TextAlignment.Left,
                    width = 200,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerTextAlignment = TextAlignment.Center,
                    width = 25,
                    minWidth = 25,
                    maxWidth = 25,
                    autoResize = false,
                    allowToggleVisibility = false,
                    canSort = false
                }
            });
        }

        internal class ReferencesHeader : MultiColumnHeader
        {
            private readonly float[] columnWidths;
            public float maxHeaderWidth;

            public ReferencesHeader(MultiColumnHeaderState state, float maxWidth) : base(state)
            {
                maxHeaderWidth = maxWidth;
                columnWidths = new float[state.columns.Length];

                for (int i = 0; i < state.columns.Length; i++)
                {
                    columnWidths[i] = state.columns[i].width;
                }
            }

            protected float GetColumnsWidth()
            {
                float columnsWidth = 0;
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    columnsWidth += columnWidths[i];
                }
                return columnsWidth;
            }

            protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
            {
                columnWidths[columnIndex] = column.width;
                float columnsWidths = GetColumnsWidth();

                if (columnIndex > 0 && columnIndex < 4)
                {
                    float maxWidth = maxHeaderWidth - columnsWidths + column.width - 20f;
                    column.width = Mathf.Clamp(column.width, column.minWidth, maxWidth);
                    column.maxWidth = maxWidth;
                }

                base.ColumnHeaderGUI(column, headerRect, columnIndex);
            }
        }
    }
}