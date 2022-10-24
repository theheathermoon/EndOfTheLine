using UnityEngine;
using UnityEngine.UI;

namespace HFPS.UI
{
    public class MenuEvents : MonoBehaviour
    {
        private Button btn;

        private Color TxtNormalColor;
        private Color BtnNormalColor;

        [Header("Text Colors")]
        public Color HoverColor = Color.white;
        public Color PressedColor = Color.white;
        public Color HoldColor = Color.white;
        public Color DisabledColor = Color.black;

        [Header("Button Colors")]
        public Color ButtonHover = Color.white;
        public Color ButtonPressed = Color.white;
        public Color ButtonHold = Color.white;

        [Header("Button Hold")]
        public bool isPressed;

        private bool hover;
        private bool pressed;

        void Start()
        {
            if (transform.parent.GetComponent<Button>())
            {
                btn = transform.parent.GetComponent<Button>();
            }

            if (transform.childCount > 0 && transform.GetChild(0).GetComponent<Text>())
            {
                TxtNormalColor = transform.GetChild(0).GetComponent<Text>().color;
            }

            if (GetComponent<Image>())
            {
                BtnNormalColor = GetComponent<Image>().color;
            }

            if (isPressed)
            {
                ButtonHoldEvent(true);
            }
        }

        public void ButtonHoverEvent()
        {
            GetComponent<Image>().color = ButtonHover;
            transform.GetChild(0).GetComponent<Text>().color = HoverColor;
            hover = true;
        }

        public void ButtonPressedEvent()
        {
            GetComponent<Image>().color = ButtonPressed;
            transform.GetChild(0).GetComponent<Text>().color = PressedColor;
            pressed = true;
        }

        public void ButtonNormalEvent()
        {
            if (hover && !pressed)
            {
                GetComponent<Image>().color = BtnNormalColor;
                transform.GetChild(0).GetComponent<Text>().color = TxtNormalColor;
                hover = false;
            }
        }

        public void ButtonHoldEvent(bool Hold)
        {
            if (Hold)
            {
                pressed = true;
                hover = false;
                GetComponent<Image>().color = ButtonHold;
                transform.GetChild(0).GetComponent<Text>().color = HoldColor;
            }
            else
            {
                pressed = false;
                hover = false;
                GetComponent<Image>().color = BtnNormalColor;
                transform.GetChild(0).GetComponent<Text>().color = TxtNormalColor;
            }
        }

        public void ChangeTextColor(string color)
        {
            if (btn && !btn.interactable)
            {
                GetComponent<Text>().color = Color.black;
                return;
            }

            Color col = Color.clear;
            ColorUtility.TryParseHtmlString(color, out col);
            GetComponent<Text>().color = col;
        }

        public void ChangeImageColor(string color)
        {
            if (btn && !btn.interactable)
            {
                GetComponent<Text>().color = Color.black;
                return;
            }

            ColorUtility.TryParseHtmlString(color, out Color col);
            GetComponent<Image>().color = col;
        }
    }
}