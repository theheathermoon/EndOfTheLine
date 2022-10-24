using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using HFPS.Systems;
using Diagnostics = System.Diagnostics;
using TWTools = ThunderWire.Utility.Utilities;

namespace HFPS.Editors
{
    [CustomEditor(typeof(SaveGameHandler)), CanEditMultipleObjects]
    public class SaveGameHandlerEditor : Editor
    {
        private SerializedProperty p_objectReferences;
        private SerializedProperty p_crossScene;
        private SerializedProperty p_forceSaveLoad;
        private SerializedProperty p_fadeControl;
        private SerializedProperty p_saveableDataPairs;
        private SerializedProperty p_run_saveableDataPairs;
        private SaveGameHandler handler;

        void OnEnable()
        {
            handler = target as SaveGameHandler;
            p_objectReferences = serializedObject.FindProperty("objectReferences");
            p_crossScene = serializedObject.FindProperty("crossSceneSaving");
            p_forceSaveLoad = serializedObject.FindProperty("forceSaveLoad");
            p_fadeControl = serializedObject.FindProperty("fadeControl");
            p_saveableDataPairs = serializedObject.FindProperty("constantSaveables");
            p_run_saveableDataPairs = serializedObject.FindProperty("runtimeSaveables");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.PropertyField(p_objectReferences);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(p_crossScene);
            EditorGUILayout.PropertyField(p_forceSaveLoad);
            EditorGUILayout.PropertyField(p_fadeControl);

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Saveables Search", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            GetSaveablesInfo();

            EditorGUILayout.Space();

            if (GUILayout.Button("Find Saveables", GUILayout.MinHeight(30)))
            {
                FindSaveables();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        void GetSaveablesInfo()
        {
            bool forceSL = p_forceSaveLoad.boolValue;

            if (handler.constantSaveables.Count > 0 && handler.runtimeSaveables.Count > 0)
            {
                if (handler.constantSaveables.All(pair => pair.Instance != null) || handler.runtimeSaveables.All(pair => pair.Data.All(x => x.Instance != null)))
                {
                    EditorUtils.TrIconText($"Constant Saveables: {p_saveableDataPairs.arraySize}\n Runtime Saveables: {p_run_saveableDataPairs.arraySize}", MessageType.Info, EditorStyles.miniLabel);
                }
                else if (!forceSL)
                {
                    EditorUtils.TrIconText("Some of the saveable instances are missing!\n Please find scene saveables again!", MessageType.Error, EditorStyles.miniLabel);
                }
                else
                {
                    EditorUtils.TrIconText("Some of the saveable instances are missing!", MessageType.Warning, EditorStyles.miniLabel);

                }
            }
            else if (handler.constantSaveables.Count > 0)
            {
                if (handler.constantSaveables.All(pair => pair.Instance != null))
                {
                    EditorUtils.TrIconText("Constant Saveables: " + p_saveableDataPairs.arraySize, MessageType.Info, EditorStyles.miniLabel);
                }
                else if (!forceSL)
                {
                    EditorUtils.TrIconText("Some of the saveable instances are missing!\n Please find scene saveables again!", MessageType.Error, EditorStyles.miniLabel);
                }
                else
                {
                    EditorUtils.TrIconText("Some of the saveable instances are missing!", MessageType.Warning, EditorStyles.miniLabel);
                }
            }
            else if (handler.runtimeSaveables.Count > 0)
            {
                if (handler.runtimeSaveables.All(pair => pair.Data.All(x => x.Instance != null)))
                {
                    EditorUtils.TrIconText("Runtime Saveables: " + p_run_saveableDataPairs.arraySize, MessageType.Info, EditorStyles.miniLabel);
                }
                else if (!forceSL)
                {
                    EditorUtils.TrIconText("Some of the Runtime instances are missing!", MessageType.Error, EditorStyles.miniLabel);
                }
                else
                {
                    EditorUtils.TrIconText("Some of the Runtime instances are missing!", MessageType.Warning, EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorUtils.TrIconText("Click the <b>Find Saveables</b> button, to find\n all scene saveables.", MessageType.Warning, EditorStyles.miniLabel, true);
            }
        }

        private void FindSaveables()
        {
            if (handler != null)
            {
                Diagnostics.Stopwatch stopwatch = new Diagnostics.Stopwatch();
                stopwatch.Reset();
                stopwatch.Start();

                var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>(true);

                var saveablesQuery = from Instance in allMonoBehaviours
                                     where Instance != null
                                     where typeof(ISaveable).IsAssignableFrom(Instance.GetType()) && !Instance.GetType().IsInterface && !Instance.GetType().IsAbstract
                                     let key = string.Format("{0}_{1}", Instance.GetType().Name, System.Guid.NewGuid().ToString("N"))
                                     select new SaveableDataPair(SaveableDataPair.DataBlockType.ISaveable, key, Instance, new string[0]);

                var attributesQuery = from Instance in allMonoBehaviours
                                      where Instance != null
                                      let attr = Instance.GetType().GetFields().Where(field => field.GetCustomAttributes(typeof(SaveableField), false).Count() > 0 && !field.IsLiteral && field.IsPublic).Select(fls => fls.Name).ToArray()
                                      let key = string.Format("{0}_{1}", Instance.GetType().Name, System.Guid.NewGuid().ToString("N"))
                                      where attr.Count() > 0
                                      select new SaveableDataPair(SaveableDataPair.DataBlockType.Attribute, key, Instance, attr);

                var pairs = saveablesQuery.Union(attributesQuery);
                stopwatch.Stop();

                handler.constantSaveables = pairs.ToList();
                EditorUtility.SetDirty(handler);

                Debug.Log("<color=green>[Setup SaveGame Successful]</color> Found Saveable Objects: " + pairs.Count() + ", Time Elapsed: " + stopwatch.ElapsedMilliseconds + "ms");
            }
            else
            {
                Debug.LogError("[Setup SaveGame Error] To Setup SaveGame you need to Setup your scene first.");
            }
        }
    }
}