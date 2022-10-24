using UnityEngine;
using UnityEditor;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomEditor(typeof(TriggerObjective)), CanEditMultipleObjects]
    public class TriggerObjectiveEditor : Editor
    {
        private SerializedProperty p_triggerType;
        private SerializedProperty p_objective;
        private SerializedProperty p_objectivesID;
        private SerializedProperty p_showTime;
        private SerializedProperty p_preComplete;
        private SerializedProperty p_newWhenContains;
        private bool listVisibility = false;

        void OnEnable()
        {
            p_triggerType = serializedObject.FindProperty("triggerType");
            p_objective = serializedObject.FindProperty("objective");
            p_objectivesID = serializedObject.FindProperty("objectivesID");
            p_showTime = serializedObject.FindProperty("showTime");
            p_preComplete = serializedObject.FindProperty("preComplete");
            p_newWhenContains = serializedObject.FindProperty("newWhenContains");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            TriggerObjective.TriggerType triggerType = (TriggerObjective.TriggerType)p_triggerType.enumValueIndex;

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(p_triggerType);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Objective Settings", EditorStyles.boldLabel);
            if (triggerType == TriggerObjective.TriggerType.Complete)
            {
                EditorGUILayout.PropertyField(p_objective, new GUIContent("Complete Objective"));
            }
            else if (triggerType == TriggerObjective.TriggerType.NewObjective)
            {
                ArrayGUI(p_objectivesID, "New Objectives", "ID", ref listVisibility);
            }
            else if (triggerType == TriggerObjective.TriggerType.CompleteAndNew)
            {
                EditorGUILayout.PropertyField(p_objective, new GUIContent("Complete Objective"));
                ArrayGUI(p_objectivesID, "New Objectives", "ID", ref listVisibility);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Other Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(p_showTime, new GUIContent("Show Time"));

            if (triggerType == TriggerObjective.TriggerType.CompleteAndNew || triggerType == TriggerObjective.TriggerType.Complete)
            {
                EditorGUILayout.PropertyField(p_preComplete, new GUIContent("Pre Complete"));
            }

            if (triggerType == TriggerObjective.TriggerType.CompleteAndNew)
            {
                EditorGUILayout.PropertyField(p_newWhenContains, new GUIContent("New When Contains"));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ArrayGUI(SerializedProperty property, string propertyName, string itemType, ref bool visible)
        {
            visible = EditorGUILayout.Foldout(visible, propertyName, true);
            if (visible)
            {
                EditorGUI.indentLevel++;
                SerializedProperty arraySizeProp = property.FindPropertyRelative("Array.size");
                EditorGUILayout.PropertyField(arraySizeProp);

                for (int i = 0; i < arraySizeProp.intValue; i++)
                {
                    EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i), new GUIContent(itemType + " " + i.ToString()), true);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}