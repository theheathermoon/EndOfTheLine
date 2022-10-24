using UnityEngine;
using UnityEditor;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomEditor(typeof(Inventory)), CanEditMultipleObjects]
    public class InventoryEditor : Editor
    {
        private SerializedProperty pDatabase;
        private SerializedProperty pPanels;
        private SerializedProperty pPrefabs;
        private SerializedProperty pContent;
        private SerializedProperty pContext;
        private SerializedProperty pColoring;
        private SerializedProperty pSlotSprites;
        private SerializedProperty pSettings;
        private SerializedProperty pCPSettings;
        private SerializedProperty pStartingItems;

        private void OnEnable()
        {
            pDatabase = serializedObject.FindProperty("inventoryDatabase");
            pPanels = serializedObject.FindProperty("panels");
            pPrefabs = serializedObject.FindProperty("prefabs");
            pContent = serializedObject.FindProperty("content");
            pContext = serializedObject.FindProperty("context");
            pColoring = serializedObject.FindProperty("coloring");
            pSlotSprites = serializedObject.FindProperty("slotSprites");
            pSettings = serializedObject.FindProperty("settings");
            pCPSettings = serializedObject.FindProperty("cpSettings");
            pStartingItems = serializedObject.FindProperty("startingItems");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.PropertyField(pDatabase);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15f);
            EditorGUILayout.LabelField("Inventory Properties", EditorStyles.boldLabel);

            // icons: https://github.com/halak/unity-editor-icons
            EditorUtils.DrawHeaderProperty("Panels", "Transform Icon", pPanels);
            EditorUtils.DrawHeaderProperty("Prefabs", "Prefab Icon", pPrefabs);
            EditorUtils.DrawHeaderProperty("UI Contents", "GUIText Icon", pContent);
            EditorUtils.DrawHeaderProperty("UI Context", "VerticalLayoutGroup Icon", pContext);
            EditorUtils.DrawHeaderProperty("UI Coloring", "ColorPicker.ColorCycle", pColoring);
            EditorUtils.DrawHeaderProperty("UI Slot Sprites", "Sprite Icon", pSlotSprites);
            EditorUtils.DrawHeaderProperty("Inventory Settings", "Settings", pSettings);
            EditorUtils.DrawHeaderProperty("Cross-Platform Settings", "UnityEditor.GameView", pCPSettings);
            EditorUtils.DrawHeaderProperty("Starting Items", "Profiler.NetworkOperations", pStartingItems);

            Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}