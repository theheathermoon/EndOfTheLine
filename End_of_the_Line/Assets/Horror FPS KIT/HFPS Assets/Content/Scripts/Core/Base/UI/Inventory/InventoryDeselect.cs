using UnityEngine;
using UnityEngine.EventSystems;
using HFPS.Systems;

namespace HFPS.UI
{
    public class InventoryDeselect : MonoBehaviour, IPointerClickHandler
    {
        private Inventory inventory;

        void Awake()
        {
            inventory = transform.root.GetComponentInChildren<Inventory>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            inventory.ResetInventory();
        }
    }
}