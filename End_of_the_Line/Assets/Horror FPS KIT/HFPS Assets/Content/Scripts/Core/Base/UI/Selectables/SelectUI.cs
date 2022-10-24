using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ThunderWire.Input;

namespace HFPS.UI
{
    public class SelectUI : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public string[] Items;
        public Text itemsText;

        [HideInInspector]
        public int index;

        private bool isSelected;

        private void Awake()
        {
            InputHandler.GetInputAction("Navigate", "UI").performed += OnUI;
        }

        void OnDestroy()
        {
            InputHandler.GetInputAction("Navigate", "UI").performed -= OnUI;
        }

        private void OnUI(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            if (isSelected)
            {
                Vector2 nav = ctx.ReadValue<Vector2>();

                if (nav.x > 0.1)
                {
                    index = index < Items.Length - 1 ? index + 1 : 0;
                }
                else if (nav.x < -0.1)
                {
                    index = index > 0 ? index - 1 : Items.Length - 1;
                }

                itemsText.text = Items[index];
            }
        }

        void Start()
        {
            itemsText.text = Items[index];
        }

        public void SetValue(int value)
        {
            if (value < Items.Length - 1)
            {
                itemsText.text = Items[value];
                index = value;
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            isSelected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isSelected = false;
        }

        void OnDisable()
        {
            isSelected = false;
        }
    }
}