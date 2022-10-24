using UnityEngine;
using UnityEngine.Events;

namespace HFPS.UI
{
    public class TabPanelEvents : MonoBehaviour
    {
        public UnityEvent OnCancel;
        public UnityEvent OnApply;

        public bool canInvoke = true;

        public void SetInvokeEnable(bool state)
        {
            canInvoke = state;
        }

        public void Cancel()
        {
            if (canInvoke)
            {
                OnCancel?.Invoke();
            }
        }

        public void Apply()
        {
            if (canInvoke)
            {
                OnApply?.Invoke();
            }
        }
    }
}