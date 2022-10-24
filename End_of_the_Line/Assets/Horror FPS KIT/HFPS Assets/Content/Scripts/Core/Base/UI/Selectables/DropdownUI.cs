using UnityEngine.UI;
using HFPS.Systems;

namespace HFPS.UI
{
    public class DropdownUI : OptionBehaviour
    {
        public Dropdown dropdown;
        public bool useOptionName;

        void Awake()
        {
            if (dropdown == null)
                throw new System.NullReferenceException($"Dropdown Component in \"{gameObject.name}\" is not assigned!");

            dropdown.onValueChanged.AddListener(delegate { OnChanged(dropdown); });
        }

        public void SetDropdownValues(Dropdown.OptionData[] options)
        {
            dropdown.ClearOptions();
            dropdown.options.AddRange(options);
            dropdown.RefreshShownValue();
        }

        public override void SetValue(string value)
        {
            if (useOptionName)
            {
                for (int i = 0; i < dropdown.options.Count; i++)
                {
                    if (dropdown.options[i].text == value)
                    {
                        dropdown.value = i;
                        break;
                    }
                }
            }
            else
            {
                int index = int.Parse(value);

                if (dropdown.options.Count > index)
                {
                    dropdown.value = index;
                }
            }

            dropdown.RefreshShownValue();
        }

        public override object GetValue()
        {
            if (useOptionName)
            {
                return dropdown.options[dropdown.value].text;
            }

            return dropdown.value;
        }

        public void OnChanged(Dropdown drop)
        {
            OptionsController.Instance.OnOptionChanged(this);
        }
    }
}