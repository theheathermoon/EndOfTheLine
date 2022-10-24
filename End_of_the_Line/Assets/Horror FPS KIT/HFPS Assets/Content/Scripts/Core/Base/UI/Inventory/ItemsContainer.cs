using UnityEngine;
using HFPS.UI;

namespace HFPS.Systems
{
    public abstract class ItemsContainer : MonoBehaviour
    {
        [HideInInspector]
        public bool isOpened;

        /// <summary>
        /// Store Inventory Item to Container.
        /// </summary>
        public abstract void Store(Item item, int amount, ItemData customData = null);

        /// <summary>
        /// Take Container Item to Inventory.
        /// </summary>
        public abstract void Take(ContainerItem item, bool allAmount);

        /// <summary>
        /// Increase Container Item Amount. 
        /// </summary>
        public abstract void IncreaseItemAmount(Item item, int amount);

        /// <summary>
        /// Remove Item from the Container.
        /// </summary>
        public abstract void RemoveItem(ContainerItem item, bool allAmount);

        /// <summary>
        /// Check if Container Item with specified ID is in the Container.
        /// </summary>
        public abstract bool ContainsID(int id);

        /// <summary>
        /// Get Item Container Count.
        /// </summary>
        public abstract int ContainerCount();

        /// <summary>
        /// Get Selected Container Item.
        /// </summary>
        public abstract ContainerItem GetSelectedItem();

        /// <summary>
        /// Get Container Info.
        /// </summary>
        public abstract ContainerInfo GetContainerInfo();
    }

    public sealed class ContainerInfo
    {
        public string ContainerName = "Container";
        public int ContainerSpace = 5;
        public bool IsStoreable = true;

        public ContainerInfo(string name, int space, bool canStore)
        {
            ContainerName = name;
            ContainerSpace = space;
            IsStoreable = canStore;
        }
    }

    [System.Serializable]
    public sealed class ContainerItemData
    {
        public Item StoredItem;
        public int Amount;
        public ItemData ItemCustomData;

        public ContainerItemData(Item item, int amount, ItemData customData = null)
        {
            StoredItem = item;
            Amount = amount;
            ItemCustomData = customData;
        }
    }
}