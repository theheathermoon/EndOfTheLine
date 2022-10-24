using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ThunderWire.Input;
using HFPS.Systems;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.UI
{
    public class ToggleUI : OptionBehaviour, ISelectHandler, IDeselectHandler
    {
        public bool isOn = false;

        [Header("Objects")]
        public Text ToggleText;

        [Header("Text")]
        public string EnabledText = "Enabled";
        public string DisabledText = "Disabled";

        private bool isSelected = false;

        private void Awake()
        {
            InputHandler.GetInputAction("Navigate", "UI").performed += OnUI;
        }

        private void OnDestroy()
        {
            InputHandler.GetInputAction("Navigate", "UI").performed -= OnUI;
        }

        private void OnUI(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            if (isSelected)
            {
                Vector2 nav = ctx.ReadValue<Vector2>();

                if (nav.x != 0)
                {
                    ChangeToggle();
                }
            }
        }

#if TW_LOCALIZATION_PRESENT
        [LocalizedMethod("OptText.Enabled", "OptText.Disabled")]
        public void OnLocalizationUpdate(string[] translations)
        {
            EnabledText = translations[0];
            DisabledText = translations[1];
        }
#endif

        private void Update()
        {
            if (isOn)
            {
                ToggleText.text = EnabledText;
            }
            else
            {
                ToggleText.text = DisabledText;
            }
        }

        public void ChangeToggle()
        {
            if (!isOn)
            {
                ToggleText.text = EnabledText;
                isOn = true;
            }
            else
            {
                ToggleText.text = DisabledText;
                isOn = false;
            }

            OnChanged();
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

        public override object GetValue()
        {
            return isOn;
        }

        public override void SetValue(string value)
        {
            isOn = bool.Parse(value);
        }

        void OnChanged()
        {
            OptionsController.Instance.OnOptionChanged(this);
        }
    }
}