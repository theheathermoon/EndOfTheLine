using UnityEngine;
using UnityEngine.UI;

namespace HFPS.UI
{
    public class DropdownLink : MonoBehaviour
    {
        public Dropdown dropdown;
        public Dropdown[] linkedDropdowns;
        public int differentValue;

        private bool mainChange;
        private bool linkedChange;
        private int timesChanged;

        void Awake()
        {
            dropdown.onValueChanged.AddListener(delegate { OnMainChanged(dropdown); });

            foreach (var item in linkedDropdowns)
            {
                item.onValueChanged.AddListener(delegate { OnOtherChanged(item); });
            }
        }

        public void OnMainChanged(Dropdown drop)
        {
            if (!linkedChange)
            {
                mainChange = true;
                timesChanged = 0;

                foreach (var item in linkedDropdowns)
                {
                    if (item.options.Count >= drop.value)
                    {
                        item.value = drop.value;
                    }
                    else
                    {
                        item.value = item.options.Count;
                    }

                    item.RefreshShownValue();
                }
            }
            else
            {
                linkedChange = false;
            }
        }

        public void OnOtherChanged(Dropdown drop)
        {
            if (!mainChange)
            {
                if (dropdown.value != differentValue)
                {
                    linkedChange = true;
                    dropdown.value = differentValue;
                    dropdown.RefreshShownValue();
                }
            }
            else
            {
                timesChanged++;

                if (timesChanged == linkedDropdowns.Length)
                {
                    mainChange = false;
                    timesChanged = 0;
                }
            }
        }

        public void Refresh()
        {
            int same = 0;

            if (linkedDropdowns.Length > 1)
            {
                for (int i = 1; i < linkedDropdowns.Length; i++)
                {
                    if (linkedDropdowns[0].value == linkedDropdowns[i].value)
                    {
                        same++;
                    }
                }
            }

            if (same == linkedDropdowns.Length - 1)
            {
                dropdown.value = linkedDropdowns[0].value;
            }
            else
            {
                dropdown.value = differentValue;
            }

            dropdown.RefreshShownValue();
        }
    }
}