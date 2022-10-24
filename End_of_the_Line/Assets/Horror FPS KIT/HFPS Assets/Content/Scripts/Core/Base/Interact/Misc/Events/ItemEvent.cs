using UnityEngine;
using UnityEngine.Events;

namespace HFPS.Systems
{
    public class ItemEvent : MonoBehaviour, IItemEvent
    {
        public UnityEvent InteractEvent;

        [SaveableField, HideInInspector]
        public bool eventExecuted;

        public void OnItemEvent()
        {
            if (!eventExecuted)
            {
                InteractEvent?.Invoke();
            }
        }
    }
}