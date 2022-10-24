using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditSettingsWindow : EditorWindow
{
    private SerializedObject serializedObject;
    private SerializedProperty currentProperty;

    Vector2 scrollPos;

    private enum Categories
    {
        GENERATION,
        WALLS,
        CORNERS,
        FLOOR,
        ENEMIES,
        CONTENT
    }

    private Categories m_categories = Categories.GENERATION;

    public static void OpenWindow()
    {
        EditSettingsWindow window = (EditSettingsWindow)GetWindow(typeof(EditSettingsWindow));
        window.minSize = new Vector2(900, 700);
        window.Show();
    }

    public static void OpenWindow(GenerationSettings genSetts)
    {
        EditSettingsWindow window = (EditSettingsWindow)GetWindow(typeof(EditSettingsWindow));
        window.serializedObject = new SerializedObject(genSetts);
        window.minSize = new Vector2(450, 300);
        window.Show();
    }

    private void DrawProperties(ref SerializedProperty prop, bool drawChildren)
    {
        string lastPropPath = string.Empty;

        bool isShowing = false;

        int i = 0;
        int difference = 0;

        List<SerializedProperty> mainList = new List<SerializedProperty>();

        foreach (SerializedProperty p in prop)
        {
            if(i == 0)
                difference = 0;

            switch (m_categories)
            {
                case Categories.GENERATION:

                    if (i >= 0 && i <= 13)
                    {
                        if (isShowing == false)
                        {
                            EditorGUILayout.LabelField("Generation Options", EditorStyles.boldLabel);
                        }

                        isShowing = true;
                    }
                    else
                    {
                        isShowing = false;
                    }

                    break;

                case Categories.WALLS:

                    if (i == 14)
                    {
                        isShowing = true;
                    }
                    else
                    {
                        isShowing = false;
                    }

                    break;

                case Categories.CORNERS:

                    if (i >= 16 + difference && i <= 27 + difference)
                    {
                        isShowing = true;
                    }
                    else
                    {
                        isShowing = false;
                    }

                    break;

                case Categories.FLOOR:

                    if (i == 29 + difference)
                    {
                        isShowing = true;
                    }
                    else
                    {
                        isShowing = false;
                    }

                    break;

                case Categories.ENEMIES:

                    if (i >= 30 + difference && i <= 36 + difference)
                    {
                        isShowing = true;
                    }
                    else
                    {
                        isShowing = false;
                    }

                    break;

                case Categories.CONTENT:

                    if (i >= 36 + difference && i <= 69 + difference)
                    {
                        isShowing = true;
                    }
                    else
                    {
                        isShowing = false;
                    }

                    break;

                default:
                    break;
            }

            if (p.isArray && p.propertyType == SerializedPropertyType.Generic)
            {
                if (isShowing)
                {
                    EditorGUILayout.PropertyField(p, new GUIContent(p.name), true);
                }
            }
            else
            {
                if (isShowing == true && p.name != "size" && p.name != "data" && p.name != "gameObject" && p.name != "name" && p.name != "x" && p.name != "y" && p.name != "z")
                {
                    if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath)) { continue; }
                    lastPropPath = p.propertyPath;

                    EditorGUILayout.PropertyField(p, false);
                }
            }

            difference += CheckArraySize(p);
            i++;
        }
    }

    protected void DrawSidebar()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

        if (GUILayout.Button("Generation", GUILayout.Height(90)))
        {
            m_categories = Categories.GENERATION;
        }

        if (GUILayout.Button("Walls", GUILayout.Height(90)))
        {
            m_categories = Categories.WALLS;
        }

        if (GUILayout.Button("Corners", GUILayout.Height(90)))
        {
            m_categories = Categories.CORNERS;
        }

        if (GUILayout.Button("Floor", GUILayout.Height(90)))
        {
            m_categories = Categories.FLOOR;
        }

        if (GUILayout.Button("Entities", GUILayout.Height(90)))
        {
            m_categories = Categories.ENEMIES;
        }

        if (GUILayout.Button("Content", GUILayout.Height(90)))
        {
            m_categories = Categories.CONTENT;
        }

        EditorGUILayout.EndVertical();
    }

    private void OnGUI()
    {
        currentProperty = serializedObject.FindProperty("generation");

        EditorGUILayout.BeginHorizontal();

        DrawSidebar();

        EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);

        if (m_categories != null)
        {
            DrawProperties(ref currentProperty, true);
        }
        else
        {
            EditorGUILayout.LabelField("Select a tab from the list");
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Apply Modified Changes", GUILayout.Width(Screen.width / 2), GUILayout.Height(Screen.height / 6)))
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Generate", GUILayout.Width(Screen.width / 2), GUILayout.Height(Screen.height / 6)))
            {
                if(GameObject.FindObjectOfType<Generator>())
                {
                    if(GameObject.FindObjectOfType<Generator>().GenerationSettings)
                    {
                        GameObject.FindObjectOfType<Generator>().RunProgram();
                    }
                    else
                    {
                        Debug.LogWarning("Need Generation Settings connected to generator.");
                    }
                }
                else
                {
                    Debug.LogWarning("Need Object of type Generator in Scene");
                }
            }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private int CheckArraySize(SerializedProperty p)
    {
        if(p.isArray && p.propertyType == SerializedPropertyType.Generic)
        {
            if (p.arraySize == 0)
            {
                return 0;
            }
            else
            {
                if(p.GetArrayElementAtIndex(0).propertyType == SerializedPropertyType.Vector3)
                {
                    return p.arraySize * 4;
                }
                return p.arraySize * 3;
            }
        }

        return 0;
    }
}
