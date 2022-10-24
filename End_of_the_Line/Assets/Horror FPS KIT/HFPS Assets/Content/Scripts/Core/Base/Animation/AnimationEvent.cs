using System;
using UnityEngine;
using UnityEngine.Events;

namespace HFPS.Systems
{
    public class AnimationEvent : MonoBehaviour
    {
        [Serializable]
        public sealed class AnimEvents
        {
            public string EventCallName;
            public UnityEvent CallEvent;
        }

        public AnimEvents[] AnimationEvents;

        public void SendEvent(string CallName)
        {
            foreach (var ent in AnimationEvents)
            {
                if (ent.EventCallName == CallName)
                {
                    ent.CallEvent?.Invoke();
                }
            }
        }
    }
}