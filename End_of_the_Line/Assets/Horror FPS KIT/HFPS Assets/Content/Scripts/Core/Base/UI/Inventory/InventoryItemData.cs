/*
 * InventoryItemData.cs - script by ThunderWire Games
 * ver. 1.3
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using ThunderWire.Helpers;
using HFPS.Systems;

namespace HFPS.UI
{
    public class InventoryItemData : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Inventory inventory;

        public Item item;
        public ItemData data = new ItemData();

        [ReadOnly(true)]
        public int itemID;
        [ReadOnly(true)]
        public int slotID;

        public string itemTitle;
        public int itemAmount;
        public string shortcut;

        [HideInInspector]
        public Image ShortcutImg;

        [HideInInspector]
        public Text textAmount;

        [HideInInspector]
        public bool isDraggable;

        [HideInInspector]
        public bool isMoving;

        private Vector2 offset;
        private bool itemDrag;

        public void InitializeData()
        {
            textAmount = transform.parent.GetChild(0).GetChild(0).GetComponent<Text>();
            itemTitle = item.Title;
            itemID = item.ID;
        }

        void Awake()
        {
            inventory = Inventory.Instance;
            ShortcutImg = transform.GetChild(0).GetComponent<Image>();
        }

        void Start()
        {
            inventory = Inventory.Instance;
            transform.position = transform.parent.position;
            isDraggable = true;
        }

        void Update()
        {
            if (ShortcutImg && !string.IsNullOrEmpty(shortcut) && !itemDrag)
            {
                ShortcutImg.enabled = true;
            }
            else
            {
                ShortcutImg.enabled = false;
            }

            if (itemDrag) return;

            if ((textAmount = transform.parent.GetChild(0).GetChild(0).GetComponent<Text>()) != null)
            {
                if (item.ItemType == ItemType.Bullets || item.ItemType == ItemType.Weapon)
                {
                    textAmount.text = itemAmount.ToString();
                }
                else
                {
                    if (itemAmount > 1)
                    {
                        textAmount.text = itemAmount.ToString();
                    }
                    else if (itemAmount == 1)
                    {
                        textAmount.text = string.Empty;
                    }
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (item != null && isDraggable)
            {
                itemDrag = true;
                offset = eventData.position - new Vector2(transform.position.x, transform.position.y);
                transform.SetParent(transform.parent.parent.parent);
                transform.position = eventData.position - offset;
                GetComponent<CanvasGroup>().blocksRaycasts = false;
                inventory.ResetInventory();
                inventory.isDragging = true;

                if (textAmount) textAmount.text = string.Empty;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (item != null && isDraggable)
            {
                transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isDraggable)
            {
                transform.SetParent(inventory.Slots[slotID].transform);
                transform.position = inventory.Slots[slotID].transform.position;
                GetComponent<CanvasGroup>().blocksRaycasts = true;
                inventory.ResetInventory();
                inventory.isDragging = false;
                itemDrag = false;
            }
        }
    }

    public class ItemData
    {
        public Dictionary<string, object> data;

        public ItemData()
        {
            data = new Dictionary<string, object>
            {
                { Inventory.ITEM_USE, true },
                { Inventory.ITEM_COMBINE, true }
            };
        }

        public ItemData(Dictionary<string, object> pairs)
        {
            data = pairs;
            data.Add(Inventory.ITEM_USE, true);
            data.Add(Inventory.ITEM_COMBINE, true);
        }

        public ItemData(params (string key, object value)[] args)
        {
            data = new Dictionary<string, object>
            {
                { Inventory.ITEM_USE, true },
                { Inventory.ITEM_COMBINE, true }
            };

            if (args.Length > 0)
            {
                foreach (var (key, value) in args)
                {
                    data.Add(key, value);
                }
            }
        }

        public bool Exist(string key)
        {
            return data.ContainsKey(key);
        }

        public object this[string key]
        {
            get
            {
                if (Exist(key))
                {
                    return data[key];
                }

                return default;
            }

            set
            {
                if (Exist(key))
                {
                    data[key] = value;
                }
                else
                {
                    data.Add(key, value);
                }
            }
        }

        public T Get<T>(string key)
        {
            if (Exist(key))
            {
                return Parser.Convert<T>(data[key].ToString());
            }

            return default;
        }

        public bool TryToGet<T>(string key, out T result)
        {
            if (Exist(key))
            {
                result = Parser.Convert<T>(data[key].ToString());
                return true;
            }

            result = default;
            return false;
        }
    }
}