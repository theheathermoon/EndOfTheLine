using UnityEngine;
using UnityEngine.Events;

public class DropdownEvents : MonoBehaviour
{
    public UnityEvent OnShow;
    public UnityEvent OnHide;

    private void OnEnable()
    {
        OnShow?.Invoke();
    }

    private void OnDestroy()
    {
        OnHide?.Invoke();
    }
}
