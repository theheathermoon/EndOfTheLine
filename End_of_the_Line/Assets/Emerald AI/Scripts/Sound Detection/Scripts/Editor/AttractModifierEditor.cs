using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.SoundDetection.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(AttractModifier))]
    public class AttractModifierEditor : Editor
    {
        SerializedProperty RadiusProp, MinVelocityProp, ReactionCooldownSecondsProp, SoundCooldownSecondsProp, EmeraldAILayerProp, TriggerTypeProp, AttractReactionProp, TriggerLayersProp, EnemyRelationsOnlyProp;
        ReorderableList TriggerSoundsList;

        private void OnEnable()
        {
            RadiusProp = serializedObject.FindProperty("Radius");
            MinVelocityProp = serializedObject.FindProperty("MinVelocity");
            ReactionCooldownSecondsProp = serializedObject.FindProperty("ReactionCooldownSeconds");
            SoundCooldownSecondsProp = serializedObject.FindProperty("SoundCooldownSeconds");
            EmeraldAILayerProp = serializedObject.FindProperty("EmeraldAILayer");
            TriggerTypeProp = serializedObject.FindProperty("TriggerType");
            AttractReactionProp = serializedObject.FindProperty("AttractReaction");
            TriggerLayersProp = serializedObject.FindProperty("TriggerLayers");
            EnemyRelationsOnlyProp = serializedObject.FindProperty("EnemyRelationsOnly");

            //Trigger Sounds
            TriggerSoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("TriggerSounds"), true, true, true, true);
            TriggerSoundsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Trigger Sounds List", EditorStyles.boldLabel);
            };
            TriggerSoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = TriggerSoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
        }

        public override void OnInspectorGUI()
        {
            var HelpButtonStyle = new GUIStyle(GUI.skin.button);
            HelpButtonStyle.normal.textColor = Color.white;
            HelpButtonStyle.fontStyle = FontStyle.Bold;

            serializedObject.Update();

            AttractModifier self = (AttractModifier)target;
            EditorGUILayout.LabelField("This system will attract all AI that are within range and invoke the 'Attract Reaction'. The object the Attract Modifier is attached to will be the source of attraction. This system is intended to extend the functionality of " +
                "the Sound Detection component by allowing certain objects, collisions, and custom calls to attract nearby AI.", EditorStyles.helpBox);

            GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
            EditorGUILayout.LabelField("For a tutorial on using the Attract Modifier, please see the tutorial below.", EditorStyles.helpBox);
            GUI.backgroundColor = new Color(0, 0.65f, 0, 0.8f);
            if (GUILayout.Button("See Tutorial", HelpButtonStyle, GUILayout.Height(20)))
            {
                Application.OpenURL("https://github.com/Black-Horizon-Studios/Emerald-AI/wiki/Using-the-Sound-Detector-System#using-an-attract-modifier");
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(EmeraldAILayerProp, new GUIContent("Emerald AI Layer"));
            EditorGUILayout.LabelField("The Emerald AI layers used by your AI (Only objects with this layer, and that are Emerald AI agents with a Sound Detection component, will be detected).", EditorStyles.helpBox);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(AttractReactionProp, new GUIContent("Attract Reaction"));
            EditorGUILayout.LabelField("The Reaction Object that will be called when this modifier is invoked/triggered (Reaction Objects can be created by right clicking in the project tab and going to Create>Emerald AI>Create>Reaction Object).", EditorStyles.helpBox);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(EnemyRelationsOnlyProp, new GUIContent("Enemy Relations Only"));
            EditorGUILayout.LabelField("Controls whether or not this Attract Modifier will only be received by AI with a Player Relation of Enemy. If set to false, all AI within range will receive this Attract Modifier if it's triggered.", EditorStyles.helpBox);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(TriggerTypeProp, new GUIContent("Trigger Type"));
            EditorGUILayout.LabelField("Controls the how the Attract Modifier will be invoked.", EditorStyles.helpBox);

            if (TriggerTypeProp.intValue == (int)TriggerTypes.OnStart)
            {
                EditorGUILayout.LabelField("OnStart - Invokes the Reaction Object on Start and uses this gameobject as the attraction source.", EditorStyles.helpBox);
                EditorGUILayout.Space();
            }
            else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnTrigger)
            {
                EditorGUILayout.LabelField("OnTrigger - Invokes the Reaction Object when a trigger collision happens with this object. This gameobject as the attraction source.", EditorStyles.helpBox);
                TriggerLayerMaskDrawer();
            }
            else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnCollision)
            {
                EditorGUILayout.LabelField("OnCollision - Invokes the Reaction Object when a non-trigger collision happens with this object. This gameobject as the attraction source.", EditorStyles.helpBox);
                TriggerLayerMaskDrawer();
            }
            else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnCustomCall)
            {
                EditorGUILayout.LabelField("OnCustomCall - Invokes the Reaction Object when the ActivateAttraction function, located within the AttractModifier script, is called. This gameobject as the attraction source.", EditorStyles.helpBox);
                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(RadiusProp, new GUIContent("Radius"));
            EditorGUILayout.LabelField("Controls the range of affect for this Attract Modifier. AI within this range will receive the Reaction Object when this Attract Modifier is triggered.", EditorStyles.helpBox);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ReactionCooldownSecondsProp, new GUIContent("Reaction Cooldown Seconds"));
            EditorGUILayout.LabelField("The amount of time (in seconds) until the Attract Reaction can be invoked again.", EditorStyles.helpBox);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(SoundCooldownSecondsProp, new GUIContent("Sound Cooldown Seconds"));
            EditorGUILayout.LabelField("The amount of time (in seconds) until the trigger sound can be played again.", EditorStyles.helpBox);
            EditorGUILayout.Space();

            if ((TriggerTypes)TriggerTypeProp.intValue == TriggerTypes.OnCollision)
            {
                EditorGUILayout.PropertyField(MinVelocityProp, new GUIContent("Min Velocity"));
                EditorGUILayout.LabelField("The minimum velocity required to invoke the attached Attract Reaction (usable only with Collision Trigger Type).", EditorStyles.helpBox);
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("A random sound from the Trigger Sounds list will be played when the Trigger Type condition is met.", EditorStyles.helpBox);
            TriggerSoundsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI()
        {
            AttractModifier self = (AttractModifier)target;
            Handles.color = new Color(0.5f, 0.4f, 0, 0.15f);
            Handles.DrawSolidDisc(self.transform.position, self.transform.up, (float)self.Radius);
        }

        void TriggerLayerMaskDrawer ()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();

            AttractModifier self = (AttractModifier)target;
            EditorGUI.BeginChangeCheck();
            var layersSelectioBackup = EditorGUILayout.MaskField("Trigger Layers", EmeraldAI.Utility.LayerMaskDrawer.LayerMaskToField(TriggerLayersProp.intValue), InternalEditorUtility.layers);
            EditorGUILayout.LabelField("Controls which collision layers are allowed to trigger this Attract Modifier.", EditorStyles.helpBox);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(self, "Layers changed");
                TriggerLayersProp.intValue = EmeraldAI.Utility.LayerMaskDrawer.FieldToLayerMask(layersSelectioBackup);
            }

            if (TriggerLayersProp.intValue == 0)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("The Trigger Layers LayerMask cannot be set to Nothing", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
    }
}