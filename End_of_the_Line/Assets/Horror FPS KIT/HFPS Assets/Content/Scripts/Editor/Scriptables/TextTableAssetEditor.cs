using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using HFPS.Systems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HFPS.Editors
{
    [CustomEditor(typeof(TextTableScriptable))]
    public class TextTableAssetEditor : Editor
    {
        private SerializedProperty m_textTables;
        private TextTableScriptable textTableAsset;

        private GUIContent Export => EditorGUIUtility.TrTextContentWithIcon(" Export", "SaveAs");
        private GUIContent Import => EditorGUIUtility.TrTextContentWithIcon(" Import", "Import");

        private void OnEnable()
        {
            m_textTables = serializedObject.FindProperty("textTables");
            textTableAsset = target as TextTableScriptable;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Text-Table Tools", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(Export, GUILayout.MinHeight(25)))
            {
                string path = EditorUtility.SaveFilePanel("Export Text-Table Data", "", target.name, "json");

                if (!string.IsNullOrEmpty(path))
                {
                    Dictionary<string, string> exportData = textTableAsset.textTables.ToDictionary(x => x.Key, y => y.Text);

                    string jsonString = JsonConvert.SerializeObject(exportData, Formatting.Indented, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                    File.WriteAllText(path, jsonString);
                }
            }

            if (GUILayout.Button(Import, GUILayout.MinHeight(25)))
            {
                string path = EditorUtility.OpenFilePanel("Import Text-Table Data", "", "json");

                if (!string.IsNullOrEmpty(path))
                {
                    string contents = File.ReadAllText(path);

                    Dictionary<string, JToken> import = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(contents);

                    foreach (var item in import)
                    {
                        JToken[] jArray = item.Value.ToArray();

                        if (jArray.Length > 1)
                        {
                            textTableAsset.textTables.Add(new TextTableScriptable.TextData()
                            {
                                Key = item.Key,
                                Text = item.Value["Text"].ToString()
                            });
                        }
                        else
                        {
                            textTableAsset.textTables.Add(new TextTableScriptable.TextData()
                            {
                                Key = item.Key,
                                Text = item.Value["Text"].ToString()
                            });
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_textTables);

            serializedObject.ApplyModifiedProperties();
        }
    }
}