using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomEditor(typeof(InventoryScriptable)), CanEditMultipleObjects]
    public class InventoryScriptableEditor : Editor
    {
        private static InventoryScriptable Target;
        private SerializedProperty p_EnableLocalization;

        void OnEnable()
        {
            Target = target as InventoryScriptable;
            p_EnableLocalization = serializedObject.FindProperty("enableLocalization");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);

            if(obj != null && obj is InventoryScriptable inventory)
            {
                OpenDatabaseEditor(inventory);
                return true;
            }

            return false;
        }

        public static void OpenDatabaseEditor(InventoryScriptable database)
        {
            if (database != null)
            {
                InventoryEditorWindow itemsEditor = EditorWindow.GetWindow<InventoryEditorWindow>(false, database.name, true);
                itemsEditor.minSize = new Vector2(800, 450);
                itemsEditor.Show(database);
            }
            else
            {
                Debug.LogError("[OpenDatabaseEditor] Database object is not initialized!");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorUtils.HelpBox("Contains a database of inventory items.", MessageType.Info, true);
            EditorGUILayout.Space(2);
            EditorUtils.HelpBox("Assign this asset to Inventory script to enable item picker with this asset.", MessageType.Warning, true);
            EditorGUILayout.Space(10);

            var rect = GUILayoutUtility.GetRect(1f, 30f);
            if (GUI.Button(rect, "Open Inventory Database Editor"))
            {
                OpenDatabaseEditor(Target);
            }

            Rect localeRect = GUILayoutUtility.GetRect(1, 20);
            localeRect.y += EditorGUIUtility.standardVerticalSpacing * 2;

            Rect infoRect = localeRect;
            infoRect.xMax = EditorGUIUtility.singleLineHeight;
            infoRect.width = EditorGUIUtility.singleLineHeight;
            GUIContent icon = EditorGUIUtility.TrIconContent("console.warnicon.sml", "HFPS Localization Integration is Required!");
            EditorGUI.LabelField(infoRect, icon, GUIStyle.none);

            localeRect.xMin += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(localeRect, p_EnableLocalization);

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Items Count: " + Target.ItemDatabase.Count, EditorStyles.miniBoldLabel);
            EditorGUILayout.EndVertical();

            string[] items = Target.ItemDatabase.Select(x => x.Title).ToArray();

            if (items.Length > 0)
            {
                if (items.Length < 50)
                {
                    EditorGUILayout.HelpBox(string.Join(", ", items), MessageType.None);
                }
                else
                {
                    string[] items_short = items.Take(50).ToArray();
                    string itemsText = string.Join(", ", items_short) + " etc.";

                    EditorGUILayout.HelpBox(itemsText, MessageType.None);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}