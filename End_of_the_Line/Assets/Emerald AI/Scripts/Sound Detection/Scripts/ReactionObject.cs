using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.SoundDetection
{
    [CreateAssetMenu(fileName = "Reaction Object", menuName = "Emerald AI/Create/Reaction Object")]
    [System.Serializable]
    public class ReactionObject : ScriptableObject
    {
        [SerializeField]
        public List<Reaction> ReactionList = new List<Reaction>();
    }
}