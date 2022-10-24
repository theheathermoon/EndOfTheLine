using UnityEngine;
using UnityEngine.Events;

namespace HFPS.Systems
{
    public class OnDestroyEvent : MonoBehaviour
    {
        public UnityEvent m_OnDestroy;

        private void OnDestroy()
        {
            m_OnDestroy?.Invoke();
        }
    }
}