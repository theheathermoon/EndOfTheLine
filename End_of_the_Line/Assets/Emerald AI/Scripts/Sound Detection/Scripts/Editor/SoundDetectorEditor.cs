using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using EmeraldAI.Utility;

namespace EmeraldAI.SoundDetection.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(SoundDetector))]
    public class SoundDetectorEditor : Editor
    {
        SerializedProperty CheckIncrementProp;
        SerializedProperty MinVelocityThresholdProp;
        SerializedProperty AttentionRateProp;
        SerializedProperty AttentionFalloffProp;
        SerializedProperty AttractModifierCooldownProp;
        SerializedProperty DelayUnawareSecondsProp;
        SerializedProperty DetectableLayersProp;

        SerializedProperty UnawareReactionProp;
        SerializedProperty SuspiciousReactionProp;
        SerializedProperty AwareReactionProp;

        SerializedProperty UnawareThreatLevelProp;
        SerializedProperty SuspiciousThreatLevelProp;
        SerializedProperty AwareThreatLevelProp;

        SerializedProperty UnawareEventProp;
        SerializedProperty SuspiciousEventProp;
        SerializedProperty AwareEventProp;

        private void OnEnable()
        {
            CheckIncrementProp = serializedObject.FindProperty("CheckIncrement");
            MinVelocityThresholdProp = serializedObject.FindProperty("MinVelocityThreshold");
            AttentionRateProp = serializedObject.FindProperty("AttentionRate");
            AttentionFalloffProp = serializedObject.FindProperty("AttentionFalloff");
            DelayUnawareSecondsProp = serializedObject.FindProperty("DelayUnawareSeconds");
            AttractModifierCooldownProp = serializedObject.FindProperty("AttractModifierCooldown");
            DetectableLayersProp = serializedObject.FindProperty("DetectableLayers");

            UnawareThreatLevelProp = serializedObject.FindProperty("UnawareThreatLevel");
            SuspiciousThreatLevelProp = serializedObject.FindProperty("SuspiciousThreatLevel");
            AwareThreatLevelProp = serializedObject.FindProperty("AwareThreatLevel");

            UnawareEventProp = serializedObject.FindProperty("UnawareEvent");
            SuspiciousEventProp = serializedObject.FindProperty("SuspiciousEvent");
            AwareEventProp = serializedObject.FindProperty("AwareEvent");

            UnawareReactionProp = serializedObject.FindProperty("UnawareReaction");
            SuspiciousReactionProp = serializedObject.FindProperty("SuspiciousReaction");
            AwareReactionProp = serializedObject.FindProperty("AwareReaction");
        }

        public override void OnInspectorGUI()
        {
            SoundDetector self = (SoundDetector)target;

            GUILayout.Space(5);

            //Help
            EditorGUILayout.LabelField("The Sound Detector component gives AI the ability to hear player targets and other sounds made by external sources. When these events happen, it will trigger Reaction Objects that will determine what the AI does. These Reaction Objects can " +
                "be customized by the user.", EditorStyles.helpBox);
            EditorGUILayout.HelpBox("AI will only listen for player targets. The tags and layers used for this are based on this AI's Emerald AI settings from its Detection Settings.", MessageType.Info);
            GUILayout.Space(5);

            var HelpButtonStyle = new GUIStyle(GUI.skin.button);
            HelpButtonStyle.normal.textColor = Color.white;
            HelpButtonStyle.fontStyle = FontStyle.Bold;
            GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
            EditorGUILayout.LabelField("For a tutorial on using the Sound Detector, please see the tutorial below.", EditorStyles.helpBox);
            GUI.backgroundColor = new Color(0, 0.65f, 0, 0.8f);
            if (GUILayout.Button("See Tutorial", HelpButtonStyle, GUILayout.Height(20)))
            {
                Application.OpenURL("https://github.com/Black-Horizon-Studios/Emerald-AI/wiki/Using-the-Sound-Detector-System#using-the-sound-detector-system");
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(15);
            //Help

            EditorGUILayout.BeginVertical("Box"); //Begin Title Box

            DisplayTitle("Info"); //Title

            CustomHelpLabelField("Current Threat Level: " + self.CurrentThreatLevel.ToString(), false);

            Rect r = EditorGUILayout.BeginVertical();
            r.height = 25;
            EditorGUI.ProgressBar(r, self.CurrentThreatAmount, "Current Threat Amount: " + (Mathf.Round(self.CurrentThreatAmount * 100f) / 100f).ToString()); 
            EditorGUILayout.EndVertical();
            GUILayout.Space(35);
            EditorGUILayout.EndVertical(); //End Title Box
            GUILayout.Space(15);

            serializedObject.Update();

            EditorGUILayout.BeginVertical("Box"); //Begin Title Box
            DisplayTitle("Settings");

            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), CheckIncrementProp, "Check Increment", 0.0f, 1f);
            CustomHelpLabelField("Controls how often sound detecting calculations are made.", true);

            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), MinVelocityThresholdProp, "Min Velocity Threshold", 0.05f, 10f);
            CustomHelpLabelField("Controls the minimum detected velocity 'sound'. Any amount lower than this will be handled by the Attention Falloff and will not be detected.", true);

            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttentionRateProp, "Attention Rate", 0.0025f, 1.0f);
            CustomHelpLabelField("Controls how quickly an AI's Current Threat Amount will increase, given that any detected targets' velocity is at or above the Min Velocity Threshold.", true);

            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttentionFalloffProp, "Attention Fall off", 0.0025f, 1.0f);
            CustomHelpLabelField("Controls how quickly an AI's Current Threat Amount will decrease, given that all detected targets' velocity is below the Min Velocity Threshold.", true);

            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttractModifierCooldownProp, "Attract Modifier Cooldown", 1f, 25f);
            CustomHelpLabelField("Controls how many seconds need to pass before the AI can detect Attract Modifier again, after already detecting one.", true);
            GUILayout.Space(5);
            EditorGUILayout.EndVertical(); //End Title Box
            GUILayout.Space(15);

            //Unaware
            EditorGUILayout.BeginVertical("Box"); //Begin Title Box
            DisplayTitle("Unaware");
            CustomHelpLabelField("An Unaware Reaction will only be triggered after an AI has become Suspicious or Aware. This can happen after a target has been lost or is too quite to be detected. This should be used for resetting an AI back to its original settings, " +
                "given they've been modified with a Reaction Object.", false);
            GUILayout.Space(15);

            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), UnawareThreatLevelProp, "Unaware Threat Level", 0.0f, 1f);
            CustomHelpLabelField("Controls the Threat Amount that's needed for an AI to reach the Unaware Threat Level.", false);
            GUILayout.Space(15);
            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DelayUnawareSecondsProp, "Delay Unaware Seconds", 0f, 25f);
            CustomHelpLabelField("Controls how many seconds need to pass before the Unaware level is invoked, given the Unware Threat Level has been met.", false);
            GUILayout.Space(15);
            EditorGUILayout.PropertyField(UnawareReactionProp, new GUIContent("Unaware Reaction"));
            CustomHelpLabelField("The Reaction Object that will be used for this Reaction. Reaction Objects can be shared between multiple AI so it does not have to be recreated.", false);
            GUILayout.Space(15);
            CustomHelpLabelField("Unaware Events - Controls the custom events that happen when the AI becomes unaware.", false);
            EditorGUILayout.PropertyField(UnawareEventProp, new GUIContent("Unaware Event"));

            GUILayout.Space(15);
            EditorGUILayout.EndVertical(); //End Title Box
            GUILayout.Space(15);
            //Unaware

            //Suspicious
            EditorGUILayout.BeginVertical("Box"); //Begin Title Box
            DisplayTitle("Suspicious");
            CustomHelpLabelField("A Suspicious Reaction will only be triggered once and after an AI has reached a Suspicious Threat Level. It will not trigger again until after the AI has engaged with a target or if it has reached the Unaware Threat Level.", false);
            GUILayout.Space(15);

            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), SuspiciousThreatLevelProp, "Suspicious Threat Level", 0.0f, 1f);
            CustomHelpLabelField("Controls the Threat Amount that's needed for an AI to reach the Suspicious Threat Level.", false);
            GUILayout.Space(15);
            EditorGUILayout.PropertyField(SuspiciousReactionProp, new GUIContent("Suspicious Reaction"));
            CustomHelpLabelField("The Reaction Object that will be used for this Reaction. Reaction Objects can be shared between multiple AI so it does not have to be recreated.", false);
            GUILayout.Space(15);
            CustomHelpLabelField("Suspicious Events - Controls the custom events that happen when an AI reaches a Suspicious Threat Level.", false);
            EditorGUILayout.PropertyField(SuspiciousEventProp, new GUIContent("Suspicious Event"));

            GUILayout.Space(15);
            EditorGUILayout.EndVertical(); //End Title Box
            GUILayout.Space(15);
            //Suspicious


            //Aware
            EditorGUILayout.BeginVertical("Box"); //Begin Title Box
            DisplayTitle("Aware");
            CustomHelpLabelField("An Aware Reaction will only be triggered once and after an AI has reached an Aware Threat Level. It will not trigger again until after the AI has engaged with a target or if it has reached the Unaware Threat Level.", false);
            GUILayout.Space(15);

            CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AwareThreatLevelProp, "Aware Threat Level", 0.0f, 1f);
            CustomHelpLabelField("Controls the Threat Amount that's needed for an AI to reach the Aware Threat Level.", false);
            GUILayout.Space(15);
            EditorGUILayout.PropertyField(AwareReactionProp, new GUIContent("Aware Reaction"));
            CustomHelpLabelField("The Reaction Object that will be used for this Reaction. Reaction Objects can be shared between multiple AI so it does not have to be recreated.", false);
            GUILayout.Space(15);
            CustomHelpLabelField("Aware Events - Controls the custom events that happen when an AI reaches a Aware Threat Level.", false);
            EditorGUILayout.PropertyField(AwareEventProp, new GUIContent("Aware Event"));


            GUILayout.Space(15);
            EditorGUILayout.EndVertical(); //End Title Box
            GUILayout.Space(15);
            //Aware

            serializedObject.ApplyModifiedProperties();
        }

        void CustomHelpLabelField(string TextInfo, bool UseSpace)
        {
            GUI.backgroundColor = new Color(1f, 1f, 1f, 1f);
            EditorGUILayout.LabelField(TextInfo, EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
        }

        void CustomPopup(Rect position, GUIContent label, SerializedProperty property, string nameOfLabel, string[] names)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            string[] enumNamesList = names;
            var newValue = EditorGUI.Popup(position, property.intValue, enumNamesList);

            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        void DisplayTitle(string Title)
        {
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.25f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }
    }
}