using UnityEngine;
using UnityEngine.EventSystems;

namespace HFPS.UI
{
    public class UIPointerEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool pointerEnter;

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerEnter = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerEnter = false;
        }
    }
}