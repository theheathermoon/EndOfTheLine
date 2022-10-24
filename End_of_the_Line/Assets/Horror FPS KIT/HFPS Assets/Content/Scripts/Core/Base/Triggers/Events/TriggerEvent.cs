using UnityEngine;
using UnityEngine.Events;
using ThunderWire.Utility;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace HFPS.Systems
{
    public class TriggerEvent : MonoBehaviour, ISaveable
    {
        public enum Modes { Once, NTimes, Infinite }

        public Modes Mode = Modes.Once;
        public uint MaxTriggerCount = 1;
        [Space(10)]
        public UnityEvent OnTriggerEvent;

        private bool triggerEnter;
        private bool isTriggered;
        private uint triggerCount;

        public void Trigger()
        {
            if (!isTriggered)
            {
                OnTriggerEvent.Invoke();

                switch (Mode)
                {
                    case Modes.Once:
                        isTriggered = true;
                        break;
                    case Modes.NTimes:
                        {
                            triggerCount++;
                            if (triggerCount >= MaxTriggerCount)
                                isTriggered = true;
                        }
                        break;
                    case Modes.Infinite:
                        isTriggered = false;
                        break;
                }
            }
        }

        void Update()
        {
            if(gameObject.HasComponent(out Collider col))
            {
                if (!col.enabled)
                    isTriggered = false;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !triggerEnter)
            {
                Trigger();
                triggerEnter = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && triggerEnter)
            {
                triggerEnter = false;
            }
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>()
            {
                { "isTriggered", isTriggered },
                { "triggerCount", triggerCount }
            };
        }

        public void OnLoad(JToken token)
        {
            isTriggered = (bool)token["isTriggered"];
            triggerCount = (uint)token["triggerCount"];
        }
    }
}