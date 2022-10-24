using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using HFPS.Systems;

namespace HFPS.UI
{
    public class InventoryContex : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Inventory inventory;

        public Image ContexImage;
        [Space(10)]
        public UnityEvent OnSelect;

        [HideInInspector]
        public bool isSelected = false;

        private bool isPointerDown = false;

        void OnDisable()
        {
            ContexImage.color = inventory.coloring.ContextNormal;
            isSelected = false;
        }

        void Awake()
        {
            inventory = Inventory.Instance;
        }

        public void Select()
        {
            ContexImage.color = Inventory.Instance.coloring.ContextSelected;
            isSelected = true;
        }

        public void Deselect()
        {
            ContexImage.color = Inventory.Instance.coloring.ContextNormal;
            isSelected = false;
        }

        public void Click()
        {
            if (isSelected)
            {
                OnSelect?.Invoke();
                isPointerDown = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isSelected) ContexImage.color = inventory.coloring.ContextSelected;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isSelected) ContexImage.color = inventory.coloring.ContextNormal;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isPointerDown)
            {
                OnSelect?.Invoke();
                isPointerDown = false;
            }
        }
    }
}