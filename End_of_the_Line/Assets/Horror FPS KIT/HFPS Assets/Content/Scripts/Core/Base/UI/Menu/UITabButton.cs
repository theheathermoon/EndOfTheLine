using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using HFPS.Systems;

namespace HFPS.UI
{
    public class UITabButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public bool holdColor;

        [Header("Graphic")]
        public Image ButtonImage;
        public Text ButtonText;

        [Header("Button Colors")]
        public Color NormalColor = Color.white;
        public Color HoverColor = Color.white;
        public Color PressedColor = Color.white;
        public Color HoldColor = Color.white;

        [Header("Text Colors")]
        public bool useTextColor;
        public Color TextNormalColor = Color.white;
        public Color TextHoverColor = Color.white;
        public Color TextPressedColor = Color.white;
        public Color TextHoldColor = Color.white;

        [Space]
        public bool interactable = true;

        [Space]
        public UnityEvent OnClick;

        void OnEnable()
        {
            interactable = true;

            if (transform.childCount > 0 && transform.GetChild(0).GetComponent<Text>() && useTextColor)
            {
                ButtonText = transform.GetChild(0).GetComponent<Text>();
                ButtonText.color = TextNormalColor;
            }

            if (holdColor)
            {
                ButtonImage.color = HoldColor;
                if (useTextColor) ButtonText.color = TextHoldColor;
            }

            MenuController.OnBeforeRebind += () =>
            {
                interactable = false;
            };

            MenuController.OnAfterRebind += () =>
            {
                interactable = true;
            };
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;

            if (holdColor)
            {
                return;
            }

            ButtonImage.color = PressedColor;
            if (useTextColor) ButtonText.color = TextPressedColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;

            holdColor = true;
            ButtonImage.color = HoldColor;

            OnClick?.Invoke();

            if (useTextColor) ButtonText.color = TextHoldColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable) return;

            if (holdColor)
            {
                return;
            }

            ButtonImage.color = HoverColor;
            if (useTextColor) ButtonText.color = TextHoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!interactable) return;

            if (holdColor)
            {
                return;
            }

            ButtonImage.color = NormalColor;
            if (useTextColor) ButtonText.color = TextNormalColor;
        }

        public void Select()
        {
            if (!interactable) return;

            holdColor = true;
            ButtonImage.color = HoldColor;
            if (useTextColor) ButtonText.color = TextHoldColor;
        }

        public void Unhold()
        {
            if (!interactable) return;

            holdColor = false;
            ButtonImage.color = NormalColor;
            if (useTextColor) ButtonText.color = TextNormalColor;
        }
    }
}