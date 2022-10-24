using HFPS.UI;

namespace HFPS.Systems
{
    interface IItemValueProvider
    {
        string OnGetValue();
        void OnSetValue(string value);
    }

    interface IItemSelect
    {
        void OnItemSelect(int ID, ItemData data);
    }
}