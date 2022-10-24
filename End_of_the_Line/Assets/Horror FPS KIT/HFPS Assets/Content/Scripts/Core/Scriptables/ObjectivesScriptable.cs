using System;
using System.Collections.Generic;
using UnityEngine;

namespace HFPS.Systems
{
    public class ObjectivesScriptable : ScriptableObject
    {
        public List<Objective> Objectives = new List<Objective>();
        public bool enableLocalization;

        [Serializable]
        public sealed class Objective
        {
            public string shortName;
            public string eventID;
            [Multiline]
            public string objectiveText;
            public int completeCount;
            [ReadOnly]
            public int objectiveID;
            public string localeKey;

            public Objective Clone()
            {
                return new Objective()
                {
                    shortName = shortName,
                    eventID = eventID,
                    objectiveText = objectiveText,
                    completeCount = completeCount,
                    objectiveID = objectiveID,
                    localeKey = localeKey
                };
            }
        }
    }
}