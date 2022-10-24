using UnityEngine;
using UnityEngine.UI;

namespace HFPS.UI
{
    public class KeybindUI : MonoBehaviour
    {
        public Button KeybindButton;
        public Text ActionNameText;

        [HideInInspector]
        public string KeyBind;
        [HideInInspector]
        public string ActionName;

        public void Initialize(string binding, string action, string name = "")
        {
            ActionNameText.text = string.IsNullOrEmpty(name) ? action : name;
            ActionName = action;
            KeybindButton.GetComponentInChildren<Text>().text = binding;
            KeyBind = binding;
        }

        public void RebindKey(string newBinding)
        {
            KeyBind = newBinding;
            KeybindButton.GetComponentInChildren<Text>().text = newBinding.ToString();
        }

        public void ResetKey()
        {
            KeybindButton.GetComponentInChildren<Text>().text = KeyBind.ToString();
        }

        public void KeyText(string text)
        {
            KeybindButton.GetComponentInChildren<Text>().text = text;
        }
    }
}