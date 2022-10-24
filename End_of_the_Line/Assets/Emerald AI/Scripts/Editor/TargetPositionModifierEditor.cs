using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(TargetPositionModifier))]
    [CanEditMultipleObjects]
    [System.Serializable]
    public class TargetPositionModifierEditor : Editor
    {
        SerializedProperty PositionModifierProp, PositionSourceProp, GizmoRadiusProp, GizmoColorProp;

        private void OnEnable()
        {            
            PositionModifierProp = serializedObject.FindProperty("PositionModifier");
            PositionSourceProp = serializedObject.FindProperty("PositionSource");
            GizmoRadiusProp = serializedObject.FindProperty("GizmoRadius");
            GizmoColorProp = serializedObject.FindProperty("GizmoColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            TargetPositionModifier self = (TargetPositionModifier)target;
            EditorGUILayout.LabelField("This system modifies the height of non-AI targets and player targets allowing AI agents to target the sphere gizmo shown. " +
                "This is helpful if you find an AI targeting their target incorrectly. This is especially useful for targets who have a center position near the base of the model.", EditorStyles.helpBox);
            GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
            EditorGUILayout.HelpBox("Enusre that the gizmo does not go into the ground or this could make a target undetectable to AI. This is just a precaution message.", MessageType.Info);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Controls the position source for the Target Position Modifier. For character controllers who's height can change, you will want to use Collider as this will automatically match the height of your character's collider with things like crouching and crawling.", EditorStyles.helpBox);
            EditorGUILayout.PropertyField(PositionSourceProp, new GUIContent("Position Source"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Controls the height of the position modifier.", EditorStyles.helpBox);
            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), PositionModifierProp, "Height Modifier", -5, 5);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Controls the radius of the sphere gizmo.", EditorStyles.helpBox);
            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), GizmoRadiusProp, "Gizmo Radius", 0.05f, 2.5f);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Controls the color of the sphere gizmo.", EditorStyles.helpBox);
            CustomEditorProperties.CustomColorField(new Rect(), new GUIContent(), GizmoColorProp, "Gizmo Color");

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            SeeTutorialButton();          

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }

        void SeeTutorialButton()
        {
            var HelpButtonStyle = new GUIStyle(GUI.skin.button);
            HelpButtonStyle.normal.textColor = Color.white;
            HelpButtonStyle.fontStyle = FontStyle.Bold;

            GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
            EditorGUILayout.LabelField("For a detailed tutorial on using the Target Position Modifier, please see the tutorial below.", EditorStyles.helpBox);
            GUI.backgroundColor = new Color(0, 0.65f, 0, 0.8f);
            if (GUILayout.Button("See Tutorial", HelpButtonStyle, GUILayout.Height(20)))
            {
                Application.OpenURL("https://github.com/Black-Horizon-Studios/Emerald-AI/wiki/Using-the-Target-Position-Modifier#using-the-target-position-modifier");
            }
            GUI.backgroundColor = Color.white;
        }
    }
}