using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Example
{
    public class SetCompanionFollower : MonoBehaviour
    {
        public Transform FollowerTarget;

        void Start()
        {
            EmeraldAIEventsManager EmeraldEventComponent = GetComponent<EmeraldAIEventsManager>();
            EmeraldEventComponent.ChangeBehavior(EmeraldAISystem.CurrentBehavior.Companion, true);
            EmeraldEventComponent.SetFollowerTarget(FollowerTarget);
        }
    }
}