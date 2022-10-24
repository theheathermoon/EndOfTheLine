using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.SoundDetection
{
    [System.Serializable]
    public class Reaction
    {
        public ReactionTypes ReactionType = ReactionTypes.None;
        public int IntValue1 = 5;
        public int IntValue2 = 2;
        public string StringValue = "New Message";
        public float FloatValue = 1f;
        public bool BoolValue = true;
        public AudioClip SoundRef;
        public AttractModifierReactionTypes AttractModifierReaction = AttractModifierReactionTypes.MoveToAttractSource;
        public EmeraldAISystem.MovementState MovementState = EmeraldAISystem.MovementState.Walk;
        public ElementLineHeights ElementLineHeight = ElementLineHeights.One;
        public enum ElementLineHeights
        { One,Two,Three,Four,Five,Six }
    }
}