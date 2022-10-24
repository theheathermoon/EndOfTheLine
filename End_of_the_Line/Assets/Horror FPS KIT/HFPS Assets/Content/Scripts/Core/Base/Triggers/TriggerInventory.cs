using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HFPS.Systems
{
    public class TriggerInventory : MonoBehaviour
    {
        [Serializable]
        public struct InventoryEvent
        {
            [Serializable]
            public struct ItemSettings
            {
                public uint Amount;
                public bool RemoveItem;
                public bool RemoveAllStacks;
                public bool UseSlotID;
                public bool BindShortcut;
                public bool AllowMessages;
                public ItemDataPair[] CustomData;
            }

            public uint ItemID;
            public uint SlotID;
            public ItemSettings settings;
        }

        private Inventory inventory;
        private HFPS_GameManager gameManager;

        public enum MessageType { None, PickupHint, PickupMessage, ItemName }
        public List<InventoryEvent> InventoryEvents = new List<InventoryEvent>();

        [Header("Messages")]
        public MessageType TypeMessage = MessageType.None;
        public uint MessageTime = 3;
        public string PickupMessage = "Picked up";
        public string NoInventorySpace = "No inventory space!";

        [Space(20)]
        public UnityEvent OnInventoryTrigger;

        [HideInInspector, SaveableField]
        public bool IsTriggered;

        private void Awake()
        {
            inventory = Inventory.Instance;
            gameManager = HFPS_GameManager.Instance;
            TextsSource.Subscribe(OnInitTexts);
        }

        private void OnInitTexts()
        {
            PickupMessage = TextsSource.GetText("Interact.PickupMessage", "Picked up");
            NoInventorySpace = TextsSource.GetText("Interact.NoInventorySpace", "No inventory space!");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !IsTriggered && inventory)
            {
                foreach (var ent in InventoryEvents)
                {
                    int amount = (int)ent.settings.Amount;

                    if (!ent.settings.RemoveItem)
                    {
                        UI.ItemData itemData = MakeItemCustomData(ent.settings.CustomData);
                        amount = amount < 1 ? 1 : amount;

                        if (!ent.settings.UseSlotID)
                            inventory.AddItem((int)ent.ItemID, amount, itemData, ent.settings.BindShortcut);
                        else
                            inventory.AddItemToSlot((int)ent.SlotID, (int)ent.ItemID, amount, itemData, ent.settings.BindShortcut);

                        Item item = inventory.GetItem((int)ent.ItemID);
                        ShowMessage(item.Title, ent.settings.AllowMessages);
                    }
                    else if (inventory.CheckItemInventory((int)ent.ItemID))
                    {
                        if (!ent.settings.UseSlotID)
                        {
                            if (amount < 1)
                                inventory.RemoveItem((int)ent.ItemID, ent.settings.RemoveAllStacks);
                            else
                                inventory.RemoveItemAmount((int)ent.ItemID, (int)ent.settings.Amount);
                        }
                        else
                        {
                            inventory.RemoveSlotItem((int)ent.SlotID, ent.settings.RemoveAllStacks);
                        }
                    }
                }

                OnInventoryTrigger?.Invoke();
                IsTriggered = true;
            }
        }

        UI.ItemData MakeItemCustomData(ItemDataPair[] dataPairs)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();

            if (dataPairs.Length > 0)
            {
                foreach (var dat in dataPairs)
                {
                    data.Add(dat.Key, dat.Value);
                }
            }

            return new UI.ItemData(data);
        }

        void ShowMessage(string itemName, bool allowMessages)
        {
            if (gameManager)
            {
                if (inventory.CheckInventorySpace())
                {
                    if (allowMessages)
                    {
                        if (TypeMessage == MessageType.PickupHint)
                        {
                            gameManager.ShowHintPopup($"{PickupMessage} {itemName}", MessageTime);
                        }
                        else if (TypeMessage == MessageType.PickupMessage)
                        {
                            gameManager.ShowQuickMessage($"{PickupMessage} {itemName}", "");
                        }
                        else if (TypeMessage == MessageType.ItemName)
                        {
                            gameManager.ShowQuickMessage(itemName, "");
                        }
                    }
                }
                else
                {
                    gameManager.ShowQuickMessage(NoInventorySpace, "NoSpace");
                }
            }
        }
    }
}