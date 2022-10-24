using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EmeraldAI.SoundDetection.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(ReactionObject))]
    public class ReactionObjectEditor : Editor
    {
        ReorderableList ReactionList;
        string DebugLogMessageInfo = "Debug Logs a message to the Unity Console (useful for testing mechanics and values).";
        string PlaySoundInfo = "Plays a sound at the position of this AI (useful for audible queues).";
        string PlayEmoteAnimationInfo = "Plays an emote animation using the Emote Animation ID set witin the Emerald AI Editor (useful for visual queues).";
        string LookAtLoudestTargetPositionInfo = "Looks in the direction of the loudest noise.";
        string ReturnToStartingPositionInfo = "Returns the AI back to its starting position.";
        string ExpandDetectionDistanceInfo = "Expand the AI's Detection Distance, in addition to its current detection distance (useful for detecting a target that may have recently attacked a nearby target).";
        string SetMovementStateInfo = "Changes the AI's movement type to either run or walk.";
        string ResetDetectionDistanceInfo = "Resets the AI's Detection Distance to its default/starting value.";
        string ResetLookAtPositionInfo = "Resets the AI's Look At Position to its default/starting position.";
        string AttractModifierInfo = "(Only Usable with an Attract Modifier) Called when the condition on Attract Modifier is invoked, which is set on the a gameobject with AttractModifier component.";
        string DelayInfo = "Delays the reaction below this reaction by the set amount of seconds.";
        string ResetAllToDefaultInfo = "Resets all modified values back to their default values (Look At Position, Detection Distance, Movement State, and Combat State).";
        string EnterCombatStateInfo = "Puts the AI into its combat state and allows it to use its combat state animations. If an AI uses equip animations, its Equip Weapon animation will be played before transitioning to its combat animations.";
        string ExitCombatStateInfo = "Returns the AI to its default non-combat state using non-combat animations, given there are no visible targets. If an AI uses equip animations, its Unequip Weapon animation will be played before transitioning to its non-combat animations.";
        string FleeFromLoudestTargetInfo = "Sets the AI's flee target as the loudest detected target. This reaction is only for AI with a Cautious Beahvior Type and with a Coward Confidence Type. (If no loudest target is present, this reaction will be ignored)";
        string MoveToLoudestTargetInfo = "Moves the AI directly to the loudest detected target. (If no loudest target is present, this reaction will be ignored)";
        string MoveAroundCurrentPositionInfo = "Allows the AI to generate new waypoints from the AI's current position based on the user set waypoint amount and radius.";
        string MoveAroundLoudestTargetInfo = "Allows the AI to generate new waypoints from the AI's loudest detected target based on the user set waypoint amount and radius. (If no loudest target is present, this reaction will be ignored)";
        string NoneInfo = "A None reaction is the default reaction. Nothing will happen when this reaction is triggered.";

        private void OnEnable()
        {
            UpdateReactionList();
        }

        void UpdateReactionList()
        {
            //Reaction List
            ReactionList = new ReorderableList(serializedObject, serializedObject.FindProperty("ReactionList"), true, true, true, true);
            ReactionList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    CustomCallback(ReactionList, rect, index, isActive, isFocused);
                };

            ReactionList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Reaction List", EditorStyles.boldLabel);
            };


            //Modify the heights of each element to create a cleaner reorderable list. This allows each element to exapnd its height based on how many options the element setting has.
            ReactionList.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = ReactionList.serializedProperty.GetArrayElementAtIndex(index);
                float height = 1;

                if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.One)
                    height -= 1.35f;
                else if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.Two)
                    height = 1;
                else if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.Three)
                    height += 1.35f;
                else if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.Four)
                    height += 2.7f;
                else if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.Five)
                    height += 4.05f;
                else if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.Six)
                    height += 5.4f;

                return EditorGUIUtility.singleLineHeight * (2.35f + height);
            };

            //Set a newly created element to defalt values.
            ReactionList.onAddCallback = ReactionList =>
            {
                var m_List = serializedObject.FindProperty("ReactionList");
                m_List.arraySize++;

                SerializedProperty element = ReactionList.serializedProperty.GetArrayElementAtIndex(m_List.arraySize-1);
                element.FindPropertyRelative("ReactionType").intValue = (int)ReactionTypes.None;
                element.FindPropertyRelative("IntValue1").intValue = 5;
                element.FindPropertyRelative("IntValue2").intValue = 2;
                element.FindPropertyRelative("StringValue").stringValue = "New Message";
                element.FindPropertyRelative("FloatValue").floatValue = 1f;
                element.FindPropertyRelative("BoolValue").boolValue = true;
                element.FindPropertyRelative("SoundRef").objectReferenceValue = null;
            };
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            ReactionObject self = (ReactionObject)target;

            serializedObject.Update();

            EditorGUILayout.BeginVertical("Box"); //Begin Title Box
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.25f);
            DisplayTitle("Reaction Object");

            CustomHelpLabelField("A list of reactions that will be execuded, in order from top to bottom, when this reaction is invoked. If an AI sees a target, this reaction will be canceled and it will rely on its Behavior Type.", false);
            EditorGUILayout.HelpBox("You can hover over each Reaction Type and its value to get a detailed tooltip of its usage/functionality.", MessageType.Info);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            ReactionList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                //Update the ReactionList on change as it changes size dynamically depending on the option.
                UpdateReactionList();
            }

            GUILayout.Space(15);
            EditorGUILayout.EndVertical(); //End Title Box
            GUILayout.Space(15);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RecordObject(self, "Undo");

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
#endif

            serializedObject.ApplyModifiedProperties();
        }

        void CustomHelpLabelField(string TextInfo, bool UseSpace)
        {
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
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
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }

        void CustomCallback(ReorderableList list, Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);            

            EditorGUI.PropertyField(new Rect(rect.x, rect.y + 11f, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("ReactionType"), new GUIContent("Reaction Type", ""));

            //One line elements
            if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ResetDetectionDistance)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ResetDetectionDistanceInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ResetLookAtPosition)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ResetLookAtPositionInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ReturnToStartingPosition)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ReturnToStartingPositionInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.None)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", NoneInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ResetAllToDefault)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ResetAllToDefaultInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.EnterCombatState)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", EnterCombatStateInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ExitCombatState)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ExitCombatStateInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.FleeFromLoudestTarget)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", FleeFromLoudestTargetInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }

            //Two line elements
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.DebugLogMessage)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", DebugLogMessageInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("StringValue"), new GUIContent("Debug Message", "The message that will be displayed in the Unity Console."));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.PlayEmoteAnimation)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", PlayEmoteAnimationInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), new GUIContent("Emote Animation ID", "The Emote Animation ID is the same " +
                    "Emote Animation ID set witin the Emerald AI Editor of Animation Settings tab."));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.LookAtLoudestTarget)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", LookAtLoudestTargetPositionInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), new GUIContent("Seconds", "The amount of time (in seconds) this AI will look at the loudest target."));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.Delay)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", DelayInfo));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0.25f, 10f, new GUIContent("Delay Seconds", "The delay (in seconds) before the reaction below this one is called."));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ExpandDetectionDistance)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ExpandDetectionDistanceInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), new GUIContent("Distance", "The distance in which an AI's Detection Radius will be expanded " +
                    "(in addition to its current detection distance.)."));            
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.SetMovementState)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", SetMovementStateInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("MovementState"), new GUIContent("Movement State", "The movement state that this AI will use (Walk or Run). This can be " +
                    "reset back to its default value by using the Reset All To Default reaction or by setting it manually with this same reaction."));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.AttractModifier && (AttractModifierReactionTypes)element.FindPropertyRelative("AttractModifierReaction").intValue == AttractModifierReactionTypes.LookAtAttractSource)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", AttractModifierInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AttractModifierReaction"), new GUIContent("Attract Modifier Reaction", "Allows the AI to look at the detected AttractSource, " +
                    "given that the Look At feature is enabled."));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }

            //Three line elements
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.PlaySound)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", PlaySoundInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("SoundRef"), new GUIContent("Audio Clip", "The audio clip that will play when this reaction is triggered."));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 1f, new GUIContent("Volume", "Controls the volume of the Audio Clip."));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Three;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.MoveToLoudestTarget)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", MoveToLoudestTargetInfo));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 10f, new GUIContent("Wait Seconds", "The amount of seconds the AI will wait at the loudest target position."));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("BoolValue"), new GUIContent("Delay Next Reaction", "Delays the next reaction until the AI has reached its destination " +
                    "or finished moving through all of its waypoints from this reaction. (Highly Recommended)"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Three;
            }

            //Four line elements
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.AttractModifier && (AttractModifierReactionTypes)element.FindPropertyRelative("AttractModifierReaction").intValue == AttractModifierReactionTypes.MoveToAttractSource)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", AttractModifierInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AttractModifierReaction"), new GUIContent("Attract Modifier Reaction", "Moves the AI to the AttractSource position."));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 10f, new GUIContent("Wait Seconds", "The amount of seconds the AI will wait at the AttractSource position."));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 80, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("BoolValue"), new GUIContent("Delay Next Reaction", "Delays the next reaction until the AI has reached its destination " +
                    "or finished moving through all of its waypoints from this reaction. (Highly Recommended)"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Four;
            }

            //Five line elements
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.MoveAroundCurrentPosition)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", MoveAroundCurrentPositionInfo));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), 1, 25, new GUIContent("Radius", "The radius in which a waypoint will be generated from the AI's current position."));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue2"), 1, 10, new GUIContent("Total Waypoints", "The amount of waypoints that will be generated from this Wander Reaction. " +
                    "The AI won't stop until all waypoints have been arrived at, unless the AI sees a target."));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 80, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 10f, new GUIContent("Wait Seconds", "The amount of seconds it will take to generate the next waypoint."));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 103, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("BoolValue"), new GUIContent("Delay Next Reaction", "Delays the next reaction until the AI has reached its destination " +
                    "or finished moving through all of its waypoints from this reaction. (Highly Recommended)"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Five;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.MoveAroundLoudestTarget)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", MoveAroundLoudestTargetInfo));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), 1, 25, new GUIContent("Radius", "The radius in which a waypoint will be generated from the AI's loudest target position."));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue2"), 1, 10, new GUIContent("Total Waypoints", "The amount of waypoints that will be generated from this Wander Reaction. " +
                    "The AI won't stop until all waypoints have been arrived at, unless the AI sees a target."));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 80, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 10f, new GUIContent("Wait Seconds", "The amount of seconds it will take to generate the next waypoint."));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 103, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("BoolValue"), new GUIContent("Delay Next Reaction", "Delays the next reaction until the AI has reached its destination " +
                    "or finished moving through all of its waypoints from this reaction. (Highly Recommended)"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Five;
            }

            //Six line elements
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.AttractModifier && (AttractModifierReactionTypes)element.FindPropertyRelative("AttractModifierReaction").intValue == AttractModifierReactionTypes.MoveAroundAttractSource)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", AttractModifierInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AttractModifierReaction"), new GUIContent("Attract Modifier Reaction", "Allows the AI to generate new waypoints from the detected " +
                    " AttractSource based on the user set waypoint amount and radius."));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), 1, 25, new GUIContent("Radius", "The radius in which a position will be generated from the Attract Modifier."));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 80, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue2"), 1, 10, new GUIContent("Total Waypoints", "The amount of waypoints that will be generated from this Wander Reaction. " +
                    "The AI won't stop until all waypoints have been arrived at, unless the AI sees a target."));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 103, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 10f, new GUIContent("Wait Seconds", "The amount of seconds it will take to generate the next waypoint."));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 126, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("BoolValue"), new GUIContent("Delay Next Reaction", "Delays the next reaction until the AI has reached its destination " +
                    "or finished moving through all of its waypoints from this reaction. (Highly Recommended)"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Six;
            }
        }
    }
}