using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using HFPS.UI;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    public class InventoryContainer : ItemsContainer, ISaveable
    {
        [System.Serializable]
        public sealed class StartingItem
        {
            public int item;
            public int amount = 1;
        }

        private Inventory inventory => Inventory.Instance;
        private readonly ContainerItem selectedItem;
        public List<ContainerItemData> containerItems = new List<ContainerItemData>();

        public StartingItem[] startingItems;
        public bool randomItems;
        public int randomCount;

        public string containerName;
        public string containerNameKey;

        public int containerSpace;
        public bool canStore = true;

        public AudioClip OpenSound;
        [Range(0, 1)] public float Volume = 1f;

        private bool canAddStartingItem = true;

#if TW_LOCALIZATION_PRESENT
        void Awake()
        {
            if (HFPS_GameManager.LocalizationEnabled)
            {
                LocalizationSystem.SubscribeAndGet((result) =>
                {
                    if (!string.IsNullOrEmpty(result[0]))
                    {
                        containerName = result[0];
                    }
                }, containerNameKey);
            }
        }
#endif

        IEnumerator Start()
        {
            yield return new WaitForSeconds(1);

            if (startingItems.Length > 0 && canAddStartingItem)
            {
                if (!randomItems)
                {
                    foreach (var item in startingItems)
                    {
                        containerItems.Add(new ContainerItemData(inventory.GetItem(item.item), item.amount));
                    }
                }
                else if (randomCount <= startingItems.Length)
                {
                    StartingItem[] items = startingItems.AsEnumerable().OrderBy(x => System.Guid.NewGuid()).Take(randomCount).ToArray();

                    foreach (var item in items)
                    {
                        containerItems.Add(new ContainerItemData(inventory.GetItem(item.item), item.amount));
                    }
                }
            }
        }

        public void UseObject()
        {
            if (OpenSound) AudioSource.PlayClipAtPoint(OpenSound, transform.position, Volume);

            inventory.ContainterItemsCache.Clear();
            inventory.ShowInventoryContainer(this, containerItems.ToArray(), containerName);
            isOpened = true;
        }

        public override void Store(Item item, int amount, ItemData customData = null)
        {
            GameObject coItem = Instantiate(inventory.prefabs.ContainerItem, inventory.panels.ContainterContent);
            ContainerItemData itemData = new ContainerItemData(item, amount, customData);
            ContainerItem containerItem = coItem.GetComponent<ContainerItem>();
            containerItem.item = item;
            containerItem.amount = amount;
            containerItem.customData = customData;
            coItem.name = "CoItem_" + item.Title.Replace(" ", "");
            containerItems.Add(itemData);
            inventory.ContainterItemsCache.Add(coItem.GetComponent<ContainerItem>());
        }

        public override void Take(ContainerItem item, bool allAmount)
        {
            if (inventory.CheckInventorySpace())
            {
                GameObject destroyObj = item.gameObject;

                if (allAmount)
                {
                    inventory.AddItem(item.item.ID, item.amount, item.customData);
                    RemoveItem(item, true);
                    Destroy(destroyObj);
                }
                else
                {
                    if (item.item.ItemType != ItemType.Weapon && item.item.ItemType != ItemType.Bullets)
                    {
                        if (item.amount == 1)
                        {
                            Destroy(destroyObj);
                        }

                        inventory.AddItem(item.item.ID, 1, item.customData);
                        RemoveItem(item, false);
                    }
                    else
                    {
                        inventory.AddItem(item.item.ID, item.amount, item.customData);
                        RemoveItem(item, true);
                        Destroy(destroyObj);
                    }
                }
            }
            else
            {
                inventory.ResetInventory();
                inventory.ShowNotification("No Space in Inventory!");
            }
        }

        public override void IncreaseItemAmount(Item item, int amount)
        {
            ContainerItemData itemData = containerItems.SingleOrDefault(citem => citem.StoredItem.ID == item.ID);
            itemData.Amount += amount;
            inventory.GetContainerItem(item.ID).amount = itemData.Amount;
        }

        public override void RemoveItem(ContainerItem item, bool allAmount)
        {
            int itemIndex = inventory.ContainterItemsCache.IndexOf(item);

            if (allAmount)
            {
                containerItems.RemoveAt(itemIndex);
                inventory.ContainterItemsCache.RemoveAt(itemIndex);
            }
            else
            {
                if (containerItems[itemIndex].Amount > 1)
                {
                    containerItems[itemIndex].Amount--;
                    inventory.ContainterItemsCache[itemIndex].amount--;
                }
                else
                {
                    containerItems.RemoveAt(itemIndex);
                    inventory.ContainterItemsCache.RemoveAt(itemIndex);
                }
            }
        }

        public override bool ContainsID(int id) => containerItems.Any(x => x.StoredItem.ID == id);

        public override int ContainerCount() => containerItems.Count;

        public override ContainerItem GetSelectedItem() => selectedItem;

        public override ContainerInfo GetContainerInfo()
        {
            return new ContainerInfo(containerName, containerSpace, canStore);
        }

        public Dictionary<string, object> OnSave()
        {
            if (containerItems.Count > 0)
            {
                Dictionary<string, object> containerData = new Dictionary<string, object>();

                foreach (var item in containerItems)
                {
                    containerData.Add(item.StoredItem.ID.ToString(), new Dictionary<string, object> { { "item_amount", item.Amount }, { "item_custom", item.ItemCustomData } });
                }

                return containerData;
            }
            else
            {
                return null;
            }
        }

        public void OnLoad(JToken token)
        {
            if (token != null && token.HasValues)
            {
                canAddStartingItem = false;

                foreach (var item in token.ToObject<Dictionary<int, JToken>>())
                {
                    containerItems.Add(new ContainerItemData(inventory.GetItem(item.Key), (int)item.Value["item_amount"], item.Value["item_custom"].ToObject<ItemData>()));
                }
            }
        }
    }
}