using UnityEngine;
using UnityEngine.Events;

namespace HFPS.Systems
{
    public class DisableEnableEvent : MonoBehaviour
    {
        public enum EventOn { Disable, Enable }

        public EventOn eventOn = EventOn.Disable;
        public UnityEvent m_Event;

        private bool isEnabled = false;

        private void OnDisable()
        {
            if (eventOn == EventOn.Disable && isEnabled)
                m_Event?.Invoke();

            isEnabled = false;
        }

        private void OnEnable()
        {
            if (eventOn == EventOn.Enable && !isEnabled)
                m_Event?.Invoke();

            isEnabled = true;
        }
    }
}