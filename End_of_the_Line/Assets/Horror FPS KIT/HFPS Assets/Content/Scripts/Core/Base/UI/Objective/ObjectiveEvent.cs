using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HFPS.Systems
{
    public class ObjectiveEvent : MonoBehaviour
    {
        public string EventID;
        [Space]
        public UnityEvent CompleteEvent;

        public void ExecuteEvent()
        {
            CompleteEvent?.Invoke();
        }
    }
}