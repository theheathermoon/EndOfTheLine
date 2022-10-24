using UnityEngine;
using UnityEngine.UI;
using HFPS.Systems;

namespace HFPS.UI
{
    public class InputActionProperties : MonoBehaviour
    {
        public enum DisplayType { None, Text, Image }

        [Header("Display Type")]
        public DisplayType consoleDisplay = DisplayType.Text;
        public DisplayType keybaordDisplay = DisplayType.Text;
        private DisplayType currentDisplay = DisplayType.None;

        [Header("Display")]
        public Text actionNameText;
        public Text displayText;
        public Image displaySprite;
        public Button controlBtn;

        [Header("Binding")]
        public int bindingIndex;
        public string realActionName;
        public bool notRebindable;

        private string m_ActionName;
        public string ActionName
        {
            set
            {
                if (actionNameText)
                {
                    actionNameText.text = value;
                    m_ActionName = value;
                }
            }
            get => m_ActionName;
        }

        private string oldDisplayString;
        private Sprite oldDisplaySprite;

        private string ovrdDisplayString;
        private Sprite ovrdDisplaySprite;
        private bool isOverride;

        void OnEnable()
        {
            if (notRebindable && controlBtn)
            {
                controlBtn.interactable = false;
            }
        }

        public void StartRebind()
        {
            if (!notRebindable)
            {
                if (MenuController.HasReference)
                {
                    displaySprite.gameObject.SetActive(false);
                    displayText.gameObject.SetActive(true);

                    displayText.text = "Press button...";
                    MenuController.Instance.StartInteractiveRebind(this, realActionName, bindingIndex);
                }
                else
                {
                    Debug.LogError("[InputControl] MenuController reference does not exist on the scene!");
                }
            }
        }

        public void SetDisplay(Sprite sprite, bool setOvrd = false)
        {
            currentDisplay = DisplayType.Image;

            oldDisplaySprite = displaySprite.sprite == null ?
                sprite : displaySprite.sprite;

            ovrdDisplaySprite = sprite;
            displaySprite.sprite = sprite;
            displaySprite.gameObject.SetActive(true);
            displayText.gameObject.SetActive(false);

            isOverride = setOvrd;
        }

        public void SetDisplay(string text, bool setOvrd = false)
        {
            currentDisplay = DisplayType.Text;

            oldDisplayString = string.IsNullOrEmpty(displayText.text) ?
                text : displayText.text;

            ovrdDisplayString = text;
            displayText.text = text;
            displayText.gameObject.SetActive(true);
            displaySprite.gameObject.SetActive(false);

            isOverride = setOvrd;
        }

        public void ResetDisplay()
        {
            if (!isOverride)
            {
                displayText.text = oldDisplayString;
                displaySprite.sprite = oldDisplaySprite;
            }
            else
            {
                displayText.text = ovrdDisplayString;
                displaySprite.sprite = ovrdDisplaySprite;
            }

            if(currentDisplay == DisplayType.Text)
            {
                displayText.gameObject.SetActive(true);
                displaySprite.gameObject.SetActive(false);
            }
            else if(currentDisplay == DisplayType.Image)
            {
                displayText.gameObject.SetActive(false);
                displaySprite.gameObject.SetActive(true);
            }
        }
    }
}