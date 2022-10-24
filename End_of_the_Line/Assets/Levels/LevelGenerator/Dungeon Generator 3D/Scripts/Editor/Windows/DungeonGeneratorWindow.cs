using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.PackageManager.UI;
using NUnit.Framework.Internal;
using System;

public class DungeonGeneratorWindow : EditorWindow
{
    Rect headerSection;
    Rect editSection;
    Rect createSection;
    Rect generationSettingsSection;

    Color headerSectionColor = new Color(13.0f / 255.0f, 32.0f / 255.0f, 44.0f / 255.0f, 1.0f);

    Color createButtonColor;
    Color editButtonColor;

    Texture2D headerSectionTexture;
    Texture2D createButtonTexture;
    Texture2D editButtonTexture;

    GUISkin style;
    GUIStyle createButtonStyle;
    GUIStyle editButtonStyle;

    GenerationSettings generationSettings;

    [MenuItem("Window/Dungeon Generation")]
    static void OpenWindow()
    {
        DungeonGeneratorWindow window = (DungeonGeneratorWindow)GetWindow(typeof(DungeonGeneratorWindow));
        window.minSize = new Vector2(400, 200);
        window.Show();
    }

    private void OnEnable()
    {
        InitTextures();
    }

    private void OnGUI()
    {
        createButtonStyle = new GUIStyle(GUI.skin.button);
        createButtonStyle.normal.textColor = Color.white;
        createButtonStyle.normal.background = createButtonTexture;

        DrawLayouts();
        DrawHeader();
        DrawSettings();
        DrawContentSettings();
    }

    private void InitTextures()
    {
        //Header Texture
        headerSectionTexture = new Texture2D(1, 1);
        headerSectionTexture.SetPixel(0, 0, headerSectionColor);
        headerSectionTexture.Apply();

        style = Resources.Load<GUISkin>("GUIStyles/DungeonGeneration");

        createButtonTexture = new Texture2D(1, 1);
        createButtonTexture.SetPixel(0, 0, createButtonColor);
        createButtonTexture.Apply();
    }

    void DrawContentSettings()
    {
        GUILayout.Space(20);

        GUILayout.BeginArea(editSection);

        if(generationSettings == null)
        {
            EditorGUILayout.HelpBox("Need Generation Settings", MessageType.Warning);
        }
        else
        {
            if(GUILayout.Button("Edit", GUILayout.Height(150), GUILayout.Width(Screen.width / 2 - 5)))
            {
                EditSettingsWindow.OpenWindow(generationSettings);
            }
        }

        GUILayout.EndArea();

        GUILayout.BeginArea(createSection);

            if (GUILayout.Button("Create new Settings", GUILayout.Height(150), GUILayout.Width(Screen.width / 2 - 5)))
            {
                GenerationSettings asset = ScriptableObject.CreateInstance<GenerationSettings>();
                AssetDatabase.CreateAsset(asset, "Assets/NewScripableObject.asset");
                AssetDatabase.SaveAssets();
            }

        GUILayout.EndArea();
    }

    private void DrawLayouts()
    {
        //Header Layouts
        headerSection.x = 0;
        headerSection.y = 0;
        headerSection.width = Screen.width;
        headerSection.height = 50;

        generationSettingsSection.x = 0;
        generationSettingsSection.y = 75;
        generationSettingsSection.width = Screen.width;
        generationSettingsSection.height = 50;

        createSection.x = 0;
        createSection.y = 125;
        createSection.width = Screen.width / 2;
        createSection.height = Screen.width - 50;

        editSection.x = Screen.width / 2;
        editSection.y = 125;
        editSection.width = Screen.width / 2;
        editSection.height = Screen.width - 50;

        GUI.DrawTexture(headerSection, headerSectionTexture);
    }

    private void DrawHeader()
    {
        GUILayout.BeginArea(headerSection);
        GUILayout.Label("Dungeon Generation", style.GetStyle("Header1"));
        GUILayout.EndArea();
    }

    private void DrawSettings()
    {
        GUILayout.BeginArea(generationSettingsSection);

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Connect Generation Settings: ");
        generationSettings = EditorGUILayout.ObjectField(generationSettings, typeof(GenerationSettings), false) as GenerationSettings;

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}