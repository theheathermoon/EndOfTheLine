/*
 * InventorySlot.cs - by ThunderWire Studio
 * ver. 1.3
*/

using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Utility;
using ThunderWire.Input;
using HFPS.Systems;

namespace HFPS.UI
{
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerDownHandler, IPointerUpHandler
    {
        public int slotID;

        [Header("Paths")]
        public Image SlotBackground;
        public Image SlotFrame;
        public Text ItemAmount;

        [HideInInspector]
        public Inventory inventory;
        [HideInInspector]
        public Item slotItem;
        [HideInInspector]
        public InventoryItemData itemData;

        [HideInInspector]
        public bool isSelectable = true;
        [HideInInspector]
        public bool isDraggable = true;
        [HideInInspector]
        public bool isSelected = false;
        [HideInInspector]
        public bool contexVisible = false;
        [HideInInspector]
        public bool itemIsMoving = false;
        [HideInInspector]
        public bool isCombineCandidate = false;
        [HideInInspector]
        public bool isItemSelect = false;

        private bool isPointerDown = false;

        void Update()
        {
            if (!inventory) return;

            if (GetComponentInChildren<InventoryItemData>())
            {
                itemData = GetComponentInChildren<InventoryItemData>();
                slotItem = itemData.item;
            }
            else
            {
                itemData = null;
                slotItem = null;
            }

            if (transform.childCount > 1 && itemData)
            {
                SlotBackground.enabled = true;
                SlotBackground.sprite = isSelected && !contexVisible ? inventory.slotSprites.SlotSelected : inventory.slotSprites.SlotWithItem;
                SlotFrame.sprite = inventory.slotSprites.SlotFrameItem;
                itemData.isDraggable = isDraggable;

                if (isSelectable)
                {
                    SlotBackground.color = Color.white;
                    itemData.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    SlotBackground.color = inventory.coloring.SlotDisabled;
                    itemData.GetComponent<Image>().color = inventory.coloring.ItemDisabled;
                }

                if (itemData.ShortcutImg)
                {
                    if (!string.IsNullOrEmpty(itemData.shortcut))
                    {
                        var composite = InputHandler.CompositeOf(itemData.shortcut);
                        Sprite icon = InputHandler.GetSprites().GetSprite(composite.bindingPath);

                        if (icon != null)
                        {
                            itemData.ShortcutImg.sprite = icon;
                        }
                    }
                }
            }
            else
            {
                SlotBackground.enabled = false;
                SlotFrame.sprite = inventory.slotSprites.SlotFrameEmpty;
                SlotFrame.color = Color.white;
                SlotBackground.color = Color.white;
                ItemAmount.text = string.Empty;
                contexVisible = false;
                isSelected = false;
            }
        }

        public void ShowContext()
        {
            if (!slotItem.Toggles.showItemOnUse)
            {
                inventory.ShowContexMenu(true, slotItem, slotID, ctx_use: !inventory.isStoring, ctx_examine: !inventory.isStoring, ctx_combine: inventory.HasCombinePartner(slotItem), ctx_shortcut: !inventory.isStoring, ctx_store: inventory.isStoring, ctx_remove: true);
            }
            else
            {
                inventory.ShowContexMenu(true, slotItem, slotID, inventory.selectedSwitcherID != slotItem.Settings.useSwitcherID && !inventory.isStoring, inventory.HasCombinePartner(slotItem) && !inventory.isStoring, ctx_shortcut: !inventory.isStoring, ctx_store: inventory.isStoring, ctx_remove: true);
            }

            for (int i = 0; i < inventory.panels.ContexPanel.transform.childCount; i++)
            {
                if (inventory.panels.ContexPanel.transform.GetChild(i).gameObject.activeSelf)
                {
                    inventory.panels.ContexPanel.transform.GetChild(i).GetComponent<InventoryContex>().Select();
                    break;
                }
            }

            inventory.SetSlotsState(false, gameObject);
            inventory.isContexVisible = true;
            contexVisible = true;
        }

        public void Select()
        {
            if (!itemData) return;

            foreach (var sobj in inventory.Slots)
            {
                if (sobj.transform.childCount > 1)
                {
                    InventorySlot slot = sobj.GetComponent<InventorySlot>();
                    slot.GetComponent<InventorySlot>().isSelected = false;
                }
            }

            inventory.content.ItemLabel.text = itemData.item.Title;
            string description = itemData.item.Description;

            if (description.RegexMatch('{', '}', "value") && itemData.data.data.ContainsKey(Inventory.ITEM_VALUE))
            {
                if (float.TryParse(itemData.data.data[Inventory.ITEM_VALUE].ToString(), out float value))
                {
                    inventory.content.ItemDescription.text = description.RegexReplaceTag('{', '}', "value", Mathf.Round(value).ToString());
                }
                else
                {
                    inventory.content.ItemDescription.text = description.RegexReplaceTag('{', '}', "value", (string)itemData.data[Inventory.ITEM_VALUE]);
                }
            }
            else
            {
                inventory.content.ItemDescription.text = itemData.item.Description;
            }

            inventory.selectedSlotID = slotID;
            inventory.panels.ItemInfoPanel.SetActive(true);
            inventory.isSelecting = true;
            isSelected = true;
        }

        /// <summary>
        /// Select second combine cadidate.
        /// </summary>
        public void CombineSelect()
        {
            if (itemData)
            {
                inventory.CombineWith(itemData.item, slotID);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag.gameObject.GetComponent<InventoryItemData>().isDraggable && !isItemSelect)
            {
                PutItem(eventData.pointerDrag.gameObject);
            }
        }

        /// <summary>
        /// Function to Drag or Move item to slot from other slot.
        /// </summary>
        public void PutItem(GameObject obj)
        {
            InventoryItemData itemDrop = obj.GetComponent<InventoryItemData>();
            itemData = itemDrop;

            if (inventory.Slots[slotID].transform.childCount < 2)
            {
                if (itemDrop.isMoving)
                {
                    inventory.Slots[itemDrop.slotID].GetComponent<InventorySlot>().isSelectable = false;
                    inventory.Slots[itemDrop.slotID].GetComponent<InventorySlot>().isSelected = false;
                    isSelectable = true;
                    isSelected = true;
                }

                itemDrop.textAmount.text = string.Empty;
                itemDrop.slotID = slotID;
                itemDrop.transform.SetParent(transform);
                itemDrop.transform.position = transform.position;
                itemDrop.textAmount = transform.GetChild(0).GetChild(0).GetComponent<Text>();
                inventory.selectedSlotID = slotID;

                if (!string.IsNullOrEmpty(itemDrop.shortcut))
                {
                    inventory.UpdateShortcut(itemDrop.shortcut, slotID);
                }
            }
            else if (itemDrop.slotID != slotID)
            {
                InventoryItemData item = transform.GetComponentInChildren<InventoryItemData>();

                item.textAmount.text = string.Empty;
                item.slotID = itemDrop.slotID;
                item.transform.SetParent(inventory.Slots[itemDrop.slotID].transform);
                item.transform.position = inventory.Slots[itemDrop.slotID].transform.position;

                if (!string.IsNullOrEmpty(item.shortcut))
                {
                    inventory.UpdateShortcut(item.shortcut, item.slotID);
                }

                if (itemDrop.isMoving)
                {
                    inventory.Slots[itemDrop.slotID].GetComponent<InventorySlot>().isSelectable = false;
                    inventory.Slots[itemDrop.slotID].GetComponent<InventorySlot>().isSelected = false;
                    isSelectable = true;
                    isSelected = true;
                }

                itemDrop.textAmount.text = string.Empty;
                itemDrop.slotID = slotID;
                itemDrop.transform.SetParent(transform);
                itemDrop.transform.position = transform.position;
                inventory.selectedSlotID = slotID;

                if (!string.IsNullOrEmpty(itemDrop.shortcut))
                {
                    inventory.UpdateShortcut(itemDrop.shortcut, slotID);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!inventory.isContexVisible)
            {
                if (itemData && isSelectable) isPointerDown = true;
            }
            else
            {
                inventory.ResetInventory();
                Select();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isPointerDown)
            {
                if (!isSelected)
                {
                    Select();
                }
                else if (!isCombineCandidate && !isItemSelect)
                {
                    ShowContext();
                }
                else
                {
                    CombineSelect();
                }

                isPointerDown = false;
            }
        }
    }
}