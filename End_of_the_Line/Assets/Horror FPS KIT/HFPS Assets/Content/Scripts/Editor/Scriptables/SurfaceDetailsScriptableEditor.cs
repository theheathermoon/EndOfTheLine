using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomEditor(typeof(SurfaceDetailsScriptable)), CanEditMultipleObjects]
    public class SurfaceDetailsScriptableEditor : Editor
    {
        private SerializedProperty m_SurfaceDetails;
        private SurfaceDetailsScriptable reference;

        private bool showListFoldout = false;
        protected List<bool> foldout = new List<bool>();

        void OnEnable()
        {
            m_SurfaceDetails = serializedObject.FindProperty("surfaceDetails");
            reference = target as SurfaceDetailsScriptable;

            for (int i = 0; i < reference.surfaceDetails.Count; i++)
            {
                foldout.Add(false);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Repaint();
            DrawSurfaceDetailsGUI(m_SurfaceDetails);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawSurfaceDetailsGUI(SerializedProperty list)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            // Header
            Rect rect = GUILayoutUtility.GetRect(1f, 20);
            showListFoldout = DrawFoldoutHeader(rect, 2, "Surface Details", showListFoldout, true);

            // Add Button
            var addItemButton = rect;
            addItemButton.width = EditorGUIUtility.singleLineHeight;
            addItemButton.x += rect.width - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
            addItemButton.y += EditorGUIUtility.standardVerticalSpacing;

            if (GUI.Button(addItemButton, EditorUtils.Styles.PlusIcon, EditorUtils.Styles.IconButton))
            {
                reference.surfaceDetails.Add(new SurfaceDetails());
                foldout.Add(false);
            }

            if (showListFoldout && list.arraySize > 0)
            {
                // Elements
                for (int i = 0; i < list.arraySize; i++)
                {
                    SerializedProperty element = list.GetArrayElementAtIndex(i);
                    SerializedProperty m_Name = element.FindPropertyRelative("SurfaceName");
                    SerializedProperty m_Surface = element.FindPropertyRelative("SurfaceProperties");
                    SerializedProperty m_Footsteps = element.FindPropertyRelative("FootstepProperties");
                    SerializedProperty m_Impacts = element.FindPropertyRelative("ImpactProperties");
                    string title = $"[{i}] " + (!string.IsNullOrEmpty(m_Name.stringValue) ? m_Name.stringValue : $"Surface {i}");

                    GUILayout.BeginVertical(GUI.skin.box);

                    //Element Header
                    Rect elemRect = GUILayoutUtility.GetRect(1f, 20);

                    // Remove Button
                    var removeItemButton = elemRect;
                    removeItemButton.width = EditorGUIUtility.singleLineHeight;
                    removeItemButton.x += elemRect.width - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
                    removeItemButton.y += EditorGUIUtility.standardVerticalSpacing;

                    if (GUI.Button(removeItemButton, EditorUtils.Styles.MinusIcon, EditorUtils.Styles.IconButton))
                    {
                        reference.surfaceDetails.RemoveAt(i);
                        foldout.RemoveAt(i);
                    }

                    //Element Content
                    if (foldout[i] = DrawFoldoutHeader(elemRect, 2, title, foldout[i], true))
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(m_Name);

                        EditorGUILayout.Space(5f);
                        EditorGUILayout.LabelField("Surface Properties", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(m_Surface, true);
                        EditorGUILayout.PropertyField(m_Footsteps, true);
                        EditorGUILayout.PropertyField(m_Impacts, true);
                        EditorGUI.indentLevel--;
                    }

                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndVertical();
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