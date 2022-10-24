/*
 * Inventory.cs - by ThunderWire Studio
 * ver. 1.6.3
 * 
 * The most complex script in whole asset :)
 * 
 * Bugs please report here: thunderwiregames@gmail.com
*/

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using ThunderWire.Helpers;
using ThunderWire.Utility;
using ThunderWire.Input;
using HFPS.Player;
using HFPS.UI;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    /// <summary>
    /// Main Inventory Script
    /// </summary>
    public class Inventory : Singleton<Inventory>
    {
        public const string ITEM_VALUE = "Value";
        public const string ITEM_PATH = "Path";
        public const string ITEM_TAG = "Tag";
        public const string ITEM_COMBINE = "CanCombine";
        public const string ITEM_USE = "CanUse";
        public const string ITEM_STARTING = "StartingItem";

        private (string, string) ts_bagCapacity = ("GameUI.BagCapacity", "Capacity {0}/{1}");
        private (string, string) ts_bagItemCount = ("GameUI.BagItemCount", "Items Count: {0}");

        private HFPS_GameManager gameManager;
        private ScriptManager scriptManager;
        private HealthManager healthManager;
        private ItemsContainer currentContainer;
        private ObjectiveManager objectives;
        private MenuController menuController;
        private Fader fader;

        [Serializable]
        public struct InventoryPanels
        {
            public GameObject ContainterPanel;
            public GameObject ItemInfoPanel;
            public GameObject ContexPanel;
            public Transform SlotsContent;
            public Transform ContainterContent;
        }

        [Serializable]
        public struct InventoryPrefabs
        {
            public GameObject InventorySlot;
            public GameObject InventoryItem;
            public GameObject ContainerItem;
        }

        [Serializable]
        public struct ContentObjects
        {
            public Text ItemLabel;
            public Text ItemDescription;
            public Text ContainerNameText;
            public Text ContainerCapacityText;
            public Image InventoryNotification;
        }

        [Serializable]
        public struct ContextButtons
        {
            public InventoryContex contexUse;
            public InventoryContex contexCombine;
            public InventoryContex contexExamine;
            public InventoryContex contexDrop;
            public InventoryContex contexStore;
            public InventoryContex contexShortcut;
            public InventoryContex contexRemovable;
        }

        [Serializable]
        public struct InventoryColoring
        {
            [Header("Contex Coloring")]
            public Color ContextNormal;
            public Color ContextSelected;

            [Header("Inventory Coloring")]
            public Color SlotDisabled;
            public Color ItemDisabled;
        }

        [Serializable]
        public struct SlotSprites
        {
            public Sprite SlotWithItem;
            public Sprite SlotSelected;
            public Sprite SlotFrameEmpty;
            public Sprite SlotFrameItem;
        }

        [Serializable]
        public struct InventorySettings
        {
            public int SlotAmount;
            public int SlotsInRow;
            public int MaxSlots;
            public int DropStrength;
            public bool TakeBackOneByOne;
        }

        [Serializable]
        public struct CrossPlatformSettings
        {
            public GameObject ButtonsInfoPC;
            public GameObject ButtonsInfoConsole;
            public string[] ShortcutActions;
        }

        [Serializable]
        public struct StartingItem
        {
            [InventorySelector]
            public int itemID;
            public int amount;
            public bool autoShortcut;
            public ItemDataPair[] data;
        }

        [Serializable]
        public struct StartingItems
        {
            public StartingItem[] startingItems;
        }

        [Tooltip("Database of all inventory items")]
        public InventoryScriptable inventoryDatabase;

        public InventoryPanels panels = new InventoryPanels();
        public InventoryPrefabs prefabs = new InventoryPrefabs();
        public ContentObjects content = new ContentObjects();
        public ContextButtons context = new ContextButtons();
        public InventoryColoring coloring = new InventoryColoring();
        public SlotSprites slotSprites = new SlotSprites();
        public InventorySettings settings = new InventorySettings();
        public CrossPlatformSettings cpSettings = new CrossPlatformSettings();
        public StartingItems startingItems = new StartingItems();

        public static event Action OnInventoryInitialized;
        public static bool IsInitialized { get; protected set; }

        #region Hidden Variables
        public bool preventUse;
        public bool isDragging;
        public bool isStoring;
        public bool isSelecting;
        public bool isContexVisible;
        public int selectedSlotID;
        public int selectedSwitcherID = -1;

        public ItemSwitcher itemSwitcher;
        public InventoryItemData itemToMove;
        public List<GameObject> Slots = new List<GameObject>();
        public List<ShortcutModel> Shortcuts = new List<ShortcutModel>();
        public List<ContainerItemData> FixedContainerData = new List<ContainerItemData>();
        public List<ContainerItem> ContainterItemsCache = new List<ContainerItem>();
        #endregion

        #region Private Variables
        private int selectedContex;
        private int selectedBind;
        private string bindControl;

        private bool fadeNotification;
        private bool isContainerFixed;
        private bool isNavDisabled;
        private bool isNavContainer;
        private bool isShortcutBind;
        private bool isBindPressed;
        private bool inventoryOpened;

        private InventorySlot firstCandidate;
        private ContainerItem selectedCoItem;
        private MonoBehaviour IItemSelectComponent;

        private readonly List<SlotGrid> SlotsGrid = new List<SlotGrid>();
        private readonly List<Item> ListOfItems = new List<Item>();
        private readonly List<InventoryItem> ItemsCache = new List<InventoryItem>();
        private readonly List<InventoryContex> InventoryContexts = new List<InventoryContex>();
        #endregion

        #region Texts
        private string ContainerNoSpace;
        private string InventoryNoSpace;
        private string KBShortcutBind;
        private string GPShortcutBind;
        private string CannotReload;
        #endregion

        void OnEnable()
        {
            if (!InputHandler.HasReference) return;
            InputHandler.GetInputAction("NavigateAlt", "UI").performed += OnNavigateAlt;
            InputHandler.GetInputAction("MoveInvItem", "UI").performed += OnInventoryMove;
            InputHandler.GetInputAction("Submit", "UI").performed += OnSubmit;
            InputHandler.GetInputAction("Cancel", "UI").performed += OnCancel;
        }

        private void OnDestroy()
        {
            IsInitialized = false;
            InputHandler.GetInputAction("NavigateAlt", "UI").performed -= OnNavigateAlt;
            InputHandler.GetInputAction("MoveInvItem", "UI").performed -= OnInventoryMove;
            InputHandler.GetInputAction("Submit", "UI").performed -= OnSubmit;
            InputHandler.GetInputAction("Cancel", "UI").performed -= OnCancel;

            if (OnInventoryInitialized != null)
            {
                foreach (Delegate d in OnInventoryInitialized.GetInvocationList())
                {
                    OnInventoryInitialized -= (Action)d;
                }
            }
        }

        private void OnInitTexts()
        {
            ContainerNoSpace = TextsSource.GetText("Inventory.ContainerSpace", "No Container Space!");
            InventoryNoSpace = TextsSource.GetText("Inventory.Space", "No Inventory Space!");
            KBShortcutBind = TextsSource.GetText("Inventory.KBShortcut", "Select 1, 2, 3, 4 to bind item shortcut.");
            GPShortcutBind = TextsSource.GetText("Inventory.GPShortcut", "Select Dpad Button to bind item shortcut.");
            CannotReload = TextsSource.GetText("Inventory.CannotReload", "Cannot reload item yet!");
        }

        public static void Subscribe(Action action)
        {
            if (IsInitialized)
            {
                OnInventoryInitialized += action;
                action?.Invoke();
            }
            else
            {
                OnInventoryInitialized += action;
            }
        }

        void Awake()
        {
            TextsSource.Subscribe(OnInitTexts);

            if (!inventoryDatabase)
                throw new NullReferenceException("Inventory Database was not set!");

            if (!gameObject.HasComponent(out gameManager))
                throw new NullReferenceException("HFPS_GameManager script not found on object " + gameManager.name);

            if (!gameObject.HasComponent(out objectives))
                throw new NullReferenceException("ObjectiveManager script not found on object " + gameManager.name);

            if(!gameObject.HasComponent(out menuController))
                throw new NullReferenceException("MenuController script not found on object " + gameManager.name);

            if (ScriptManager.HasReference)
                scriptManager = ScriptManager.Instance;
            else
                throw new NullReferenceException("ScriptManager script not found on scene!");

            if (!PlayerController.HasReference || !PlayerController.Instance.gameObject.HasComponent(out healthManager))
                throw new NullReferenceException("PlayerController or HealthManager script not found on Player object!");

            itemSwitcher = scriptManager.C<ItemSwitcher>();
            fader = Fader.Instance(gameObject);

            int row = 0;
            int column = 0;

            for (int i = 0; i < settings.SlotAmount; i++)
            {
                GameObject slot = Instantiate(prefabs.InventorySlot);
                InventorySlot sc = slot.GetComponent<InventorySlot>();
                slot.transform.SetParent(panels.SlotsContent);
                slot.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                SlotsGrid.Add(new SlotGrid(row, column, i));
                Slots.Add(slot);
                sc.inventory = this;
                sc.slotID = i;

                if (column >= settings.SlotsInRow - 1)
                {
                    column = 0;
                    row++;
                }
                else
                {
                    column++;
                }
            }

            ListOfItems.Clear();
            string[] locKeys = new string[inventoryDatabase.ItemDatabase.Count * 2];
            int locIndex = 0;

            for (int i = 0; i < inventoryDatabase.ItemDatabase.Count; i++)
            {
                var map = inventoryDatabase.ItemDatabase[i].Clone();
                ListOfItems.Add(new Item(i, map));

                locKeys[locIndex] = map.localizationSettings.titleKey;
                locKeys[locIndex + 1] = map.localizationSettings.descriptionKey;
                locIndex += 2;
            }

#if TW_LOCALIZATION_PRESENT
            if (HFPS_GameManager.LocalizationEnabled && inventoryDatabase.enableLocalization)
            {
                LocalizationSystem.Subscribe(OnLocalizationUpdate, locKeys);
            }
            else if (!HFPS_GameManager.LocalizationEnabled)
            {
                Debug.LogError($"You are trying to use localization but it is disabled on \"HFPS_GameManager\"!");
            }
            else if (!inventoryDatabase.enableLocalization)
            {
                Debug.LogError($"You are trying to use localization but it is disabled on \"{inventoryDatabase.name}\"!");
            }
#endif

            if (!HFPS_GameManager.LocalizationEnabled || !inventoryDatabase.enableLocalization)
            {
                OnInventoryInitialized?.Invoke();
                IsInitialized = true;
            }
        }

        void OnLocalizationUpdate(string[] trs)
        {
            int trIndex = 0;

            foreach (var item in ListOfItems)
            {
                string newTitle = trs[trIndex];
                string newDescription = trs[trIndex + 1];
                trIndex += 2;

                if (!string.IsNullOrEmpty(newTitle))
                {
                    item.Title = newTitle;
                }

                if (!string.IsNullOrEmpty(newDescription))
                {
                    newDescription = newDescription.Replace("\\n", Environment.NewLine);
                    item.Description = newDescription;
                }
            }

            OnInventoryInitialized?.Invoke();
            IsInitialized = true;
        }

        void AddItemOnStart()
        {
            foreach (var item in startingItems.startingItems)
            {
                if (!CheckItemInventory(item.itemID))
                {
                    int amount = item.amount;
                    if (amount <= 0) amount = 1;

                    ItemData itemData = new ItemData();
                    itemData.data.Add(ITEM_STARTING, "");
                    foreach (var data in item.data)
                    {
                        itemData.data.Add(data.Key, data.Value);
                    }

                    AddItem(item.itemID, amount, itemData, item.autoShortcut);
                }
            }
        }

        void Start()
        {
            content.ItemLabel.text = string.Empty;
            content.ItemDescription.text = string.Empty;
            panels.ItemInfoPanel.SetActive(false);
            ShowContexMenu(false);
            selectedContex = 0;
            selectedSlotID = -1;

            if(!SaveGameHandler.GameBeingLoaded)
                AddItemOnStart();
        }

        void Update()
        {
            if (itemSwitcher) selectedSwitcherID = itemSwitcher.currentItem;
            if (gameManager)
            {
                preventUse = gameManager.isInventoryShown || gameManager.isPaused;
                isNavDisabled = !gameManager.isInventoryShown || gameManager.isPaused;
            }

            if (!preventUse && !inventoryOpened)
            {
                EventSystem.current.SetSelectedGameObject(null);
                panels.ItemInfoPanel.SetActive(false);
                ShowContexMenu(false);
                ResetSlotProperties();
                StopAllCoroutines();

                foreach (var item in panels.ContainterContent.GetComponentsInChildren<ContainerItem>())
                {
                    Destroy(item.gameObject);
                }

                if (currentContainer)
                {
                    currentContainer.isOpened = false;
                    currentContainer = null;
                }

                panels.ContainterPanel.SetActive(false);
                objectives.ShowObjectives(true);

                foreach (var slot in Slots)
                {
                    slot.GetComponent<InventorySlot>().isSelectable = true;
                    slot.GetComponent<InventorySlot>().contexVisible = false;
                }

                if (ContainterItemsCache.Count > 0)
                {
                    foreach (var item in ContainterItemsCache)
                    {
                        item.Deselect();
                    }
                }

                ContainterItemsCache.Clear();
                content.InventoryNotification.gameObject.SetActive(false);

                if (itemToMove)
                {
                    itemToMove.isMoving = false;
                    itemToMove = null;
                }

                selectedContex = 0;
                selectedSlotID = -1;
                isSelecting = false;
                isStoring = false;
                isNavContainer = false;
                isShortcutBind = false;
                isContainerFixed = false;
                isContexVisible = false;
                fadeNotification = false;

                fader.FadeOutSignal();
                IItemSelectComponent = null;
                inventoryOpened = true;
            }
            else if (preventUse)
            {
                inventoryOpened = false;
            }

            if (currentContainer != null || isContainerFixed)
            {
                if (isContainerFixed)
                {
                    selectedCoItem = ContainterItemsCache.SingleOrDefault(item => item.IsSelected());
                }
                else if (currentContainer.GetSelectedItem() is var item && item != null)
                {
                    selectedCoItem = item;
                }
                else
                {
                    selectedCoItem = null;
                }

                if (!isContainerFixed)
                {
                    string capacityFormat = TextsSource.GetText(ts_bagCapacity.Item1, ts_bagCapacity.Item2);
                    content.ContainerCapacityText.text = string.Format(capacityFormat, currentContainer.ContainerCount(), currentContainer.GetContainerInfo().ContainerSpace);
                }
                else
                {
                    string countFormat = TextsSource.GetText(ts_bagItemCount.Item1, ts_bagItemCount.Item2);
                    content.ContainerCapacityText.text = string.Format(countFormat, FixedContainerData.Count);
                }
            }

            if (isShortcutBind && selectedBind == selectedSlotID && selectedBind > -1 && InputHandler.InputIsInitialized)
            {
                string pressed = IsSpecificActionPressed(cpSettings.ShortcutActions);

                if (!string.IsNullOrEmpty(pressed) && !isBindPressed)
                {
                    bindControl = pressed;
                    isBindPressed = true;
                }
                else if (isBindPressed)
                {
                    InventoryItemData itemData = ItemDataOfSlot(selectedSlotID);
                    ShortcutBind(itemData.itemID, itemData.slotID, bindControl);
                    bindControl = string.Empty;
                    isBindPressed = false;
                }
            }
            else if(!preventUse)
            {
                if (Shortcuts.Count > 0 && !isDragging && !itemToMove)
                {
                    for (int i = 0; i < Shortcuts.Count; i++)
                    {
                        int slotID = Shortcuts[i].slot;

                        if (!IsAnyItemInSlot(slotID))
                        {
                            Shortcuts.RemoveAt(i);
                            break;
                        }

                        if (InputHandler.ReadButtonOnce(this, Shortcuts[i].shortcut))
                        {
                            UseItem(Shortcuts[i].slot);
                        }
                    }
                }

                //fader.fadeOut = true;
                isShortcutBind = false;
                selectedBind = -1;
            }

            if (!fader.IsFadedOut && fadeNotification)
            {
                Color colorN = content.InventoryNotification.color;
                Color colorT = content.InventoryNotification.transform.GetComponentInChildren<Text>().color;
                colorN.a = fader.FadingValue;
                colorT.a = fader.FadingValue;
                content.InventoryNotification.color = colorN;
                content.InventoryNotification.transform.GetComponentInChildren<Text>().color = colorT;
            }
            else
            {
                content.InventoryNotification.gameObject.SetActive(false);
                fadeNotification = false;
            }
        }

        string IsSpecificActionPressed(params string[] specificActions)
        {
            return specificActions.SingleOrDefault(x => InputHandler.ReadButtonOnce(this, x));
        }

#region PlayerInput Callbacks
        InventorySlot GetCloseSlot(int selected, bool nextLine)
        {
            InventorySlot slot = null;
            int lineSlot = 0;

            if (nextLine)
            {
                if ((selected + settings.SlotsInRow) < settings.SlotAmount)
                {
                    lineSlot = selected + settings.SlotsInRow;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if ((selected - settings.SlotsInRow) >= 0)
                {
                    lineSlot = selected - settings.SlotsInRow;
                }
                else
                {
                    return null;
                }
            }

            if (GetSlot(lineSlot).itemData != null)
            {
                return GetSlot(lineSlot);
            }

            SlotGrid lb = SlotsGrid.Where(x => x.slotID == lineSlot).FirstOrDefault();
            SlotGrid[] lineSlots = SlotsGrid.Where(x => x.row == lb.row).ToArray();
            int leftSteps = 0, rightSteps = 0;
            int leftCloseID = -1, rightCloseID = -1;

            for (int i = lb.column; i >= 0; i--)
            {
                InventorySlot cl = GetSlot(lineSlots[i].slotID);
                if (cl.isSelectable && cl.itemData)
                {
                    leftCloseID = lineSlots[i].slotID;
                    break;
                }

                leftSteps++;
            }

            for (int i = lb.column; i < lineSlots.Length; i++)
            {
                InventorySlot cl = GetSlot(lineSlots[i].slotID);
                if (cl.isSelectable && cl.itemData)
                {
                    rightCloseID = lineSlots[i].slotID;
                    break;
                }

                rightSteps++;
            }

            if (leftCloseID >= 0 && rightCloseID >= 0 && leftSteps < rightSteps)
            {
                slot = GetSlot(leftCloseID);
            }
            else if (leftCloseID >= 0 && rightCloseID >= 0 && leftSteps > rightSteps)
            {
                slot = GetSlot(rightCloseID);
            }
            else
            {
                if (rightSteps == leftSteps && leftCloseID >= 0 && rightCloseID >= 0)
                {
                    slot = GetSlot(leftCloseID >= 0 ? leftCloseID : rightCloseID);
                }
                else
                {
                    if (leftCloseID >= 0)
                    {
                        slot = GetSlot(leftCloseID);
                    }
                    else if (rightCloseID >= 0)
                    {
                        slot = GetSlot(rightCloseID);
                    }
                }
            }

            return slot;
        }

        void OnNavigateAlt(InputAction.CallbackContext value)
        {
            Vector2 move = value.ReadValue<Vector2>();

            if (!isNavDisabled)
            {
                if (!isNavContainer)
                {
                    if (selectedSlotID < 0 && move.magnitude > 0)
                    {
                        if ((currentContainer != null || isContainerFixed) && move.x < 0 && ContainterItemsCache.Count > 0)
                        {
                            isNavContainer = true;
                            ContainterItemsCache[0].Select();
                        }
                        else
                        {
                            if (AnyItemInventroy())
                            {
                                InventorySlot slot = FirstPopulatedSlot();
                                if (slot != null) slot.Select();
                            }
                            else if (ContainterItemsCache.Count > 0)
                            {
                                isNavContainer = true;
                                ContainterItemsCache[0].Select();
                            }
                        }
                    }
                    else
                    {
                        if (!isContexVisible)
                        {
                            if (itemToMove)
                            {
                                int curr = itemToMove.slotID;

                                if (move.x > 0 && move.y == 0)
                                {
                                    if (curr < settings.SlotAmount - 1)
                                    {
                                        Slots[curr + 1].GetComponent<InventorySlot>().PutItem(itemToMove.gameObject);
                                    }
                                }
                                else if (move.x < 0 && move.y == 0)
                                {
                                    if (curr > 0)
                                    {
                                        Slots[curr - 1].GetComponent<InventorySlot>().PutItem(itemToMove.gameObject);
                                    }
                                }
                                else if (move.y > 0 && move.x == 0)
                                {
                                    if ((curr - settings.SlotsInRow) >= 0)
                                    {
                                        Slots[curr - settings.SlotsInRow].GetComponent<InventorySlot>().PutItem(itemToMove.gameObject);
                                    }
                                }
                                else if (move.y < 0 && move.x == 0)
                                {
                                    if ((curr + settings.SlotsInRow) <= settings.SlotAmount - 1)
                                    {
                                        Slots[curr + settings.SlotsInRow].GetComponent<InventorySlot>().PutItem(itemToMove.gameObject);
                                    }
                                }
                            }
                            else if (selectedSlotID >= 0)
                            {
                                if (move.x > 0 && move.y == 0)
                                {
                                    if (selectedSlotID < settings.SlotAmount - 1)
                                    {
                                        for (int i = selectedSlotID + 1; i < settings.SlotAmount; i++)
                                        {
                                            InventorySlot close = GetSlot(i);
                                            if (close.isSelectable && close.itemData)
                                            {
                                                close.Select();
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (move.x < 0 && move.y == 0)
                                {
                                    //Select slot item or container item if container view is visible
                                    if (currentContainer == null && !isContainerFixed)
                                    {
                                        //Select slot item
                                        if (selectedSlotID > 0)
                                        {
                                            for (int i = selectedSlotID - 1; i >= 0; i--)
                                            {
                                                InventorySlot close = GetSlot(i);
                                                if (close.isSelectable && close.itemData)
                                                {
                                                    close.Select();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Select slot item or contaier item if selected slot id equals zero or there are no next slot items
                                        if (selectedSlotID >= 1)
                                        {
                                            for (int i = selectedSlotID - 1; i >= 0; i--)
                                            {
                                                InventorySlot next = GetSlot(i);

                                                if (next.isSelectable && next.itemData)
                                                {
                                                    next.Select();
                                                    break;
                                                }
                                                else if (i <= 0 && ContainterItemsCache.Count > 0)
                                                {
                                                    isNavContainer = true;
                                                    ResetInventory();
                                                    ContainterItemsCache[0].Select();
                                                }
                                            }
                                        }
                                        else if (ContainterItemsCache.Count > 0)
                                        {
                                            isNavContainer = true;
                                            ResetInventory();
                                            ContainterItemsCache[0].Select();
                                        }
                                    }
                                }
                                else if (move.y > 0 && move.x == 0)
                                {
                                    InventorySlot slot = GetCloseSlot(selectedSlotID, false);
                                    if (slot != null) slot.Select();
                                }
                                else if (move.y < 0 && move.x == 0)
                                {
                                    InventorySlot slot = GetCloseSlot(selectedSlotID, true);
                                    if (slot != null) slot.Select();
                                }
                            }
                        }
                        else
                        {
                            if (move.y > 0 && move.x == 0)
                            {
                                InventoryContexts[selectedContex].Deselect();
                                selectedContex = selectedContex > 0 ? selectedContex - 1 : 0;
                                InventoryContexts[selectedContex].Select();
                            }
                            else if (move.y < 0 && move.x == 0)
                            {
                                InventoryContexts[selectedContex].Deselect();
                                selectedContex = selectedContex < InventoryContexts.Count - 1 ? selectedContex + 1 : InventoryContexts.Count - 1;
                                InventoryContexts[selectedContex].Select();
                            }
                        }
                    }
                }
                else
                {
                    if (move.x > 0 && move.y == 0 && AnyItemInventroy())
                    {
                        if (selectedCoItem != null) selectedCoItem.Deselect();
                        InventorySlot slot = FirstPopulatedSlot();
                        if (slot != null) slot.Select();
                        isNavContainer = false;
                    }
                    else if (move.y < 0 && selectedCoItem != null)
                    {
                        int id = ContainterItemsCache.IndexOf(selectedCoItem);
                        selectedCoItem.Deselect();
                        ContainterItemsCache[id < ContainterItemsCache.Count - 1 ? id + 1 : 0].Select();
                    }
                    else if (move.y > 0 && selectedCoItem != null)
                    {
                        int id = ContainterItemsCache.IndexOf(selectedCoItem);
                        selectedCoItem.Deselect();
                        ContainterItemsCache[id > 0 ? id - 1 : ContainterItemsCache.Count - 1].Select();
                    }
                }
            }
        }

        void OnInventoryMove(InputAction.CallbackContext value)
        {
            if (!isNavDisabled && selectedSlotID >= 0 && !itemToMove && IItemSelectComponent == null)
            {
                InventorySlot slot = GetSlot(selectedSlotID);

                if (!slot.itemIsMoving && slot.isSelected && slot.isSelectable && !slot.contexVisible)
                {
                    SetSlotsState(false, slot.gameObject);
                    itemToMove = slot.itemData;
                    slot.itemData.isMoving = true;
                    ShowNotificationFixed("Move selected item where do you want.");

                    foreach (var sobj in Slots)
                    {
                        sobj.GetComponent<InventorySlot>().itemIsMoving = true;
                    }
                }
            }
        }

        void OnSubmit(InputAction.CallbackContext value)
        {
            if (!isNavDisabled)
            {
                if (selectedCoItem != null)
                {
                    TakeBackToInventory(selectedCoItem);
                }
                else
                {
                    if (itemToMove)
                    {
                        foreach (var sobj in Slots)
                        {
                            sobj.GetComponent<InventorySlot>().isSelectable = true;
                            sobj.GetComponent<InventorySlot>().itemIsMoving = false;
                        }

                        fader.FadeOutSignal();
                        itemToMove.isMoving = false;
                        itemToMove = null;
                    }
                    else if (selectedSlotID >= 0)
                    {
                        InventorySlot slot = GetSlot(selectedSlotID);

                        if (!slot.contexVisible && !slot.itemIsMoving && slot.isSelectable && slot.isSelected)
                        {
                            if (!slot.isCombineCandidate && !slot.isItemSelect)
                            {
                                slot.ShowContext();
                            }
                            else
                            {
                                slot.CombineSelect();
                            }
                        }
                        else if (isContexVisible)
                        {
                            InventoryContexts[selectedContex].Click();
                        }
                    }
                }
            }
        }

        void OnCancel(InputAction.CallbackContext value)
        {
            if (!isNavDisabled && !gameManager.isPaused && !menuController.optionsShown)
            {
                if (itemToMove == null && !isContexVisible && !isShortcutBind)
                {
                    gameManager.ShowInventory(false);
                    isContainerFixed = false;
                }

                EventSystem.current.SetSelectedGameObject(null);
                SetSlotsState(true);
                ShowContexMenu(false);
                fader.FadeOutSignal();
                isContexVisible = false;
                isBindPressed = false;
                isShortcutBind = false;

                itemToMove = null;
                bindControl = string.Empty;
                selectedBind = -1;

                foreach (var sobj in Slots)
                {
                    sobj.GetComponent<InventorySlot>().contexVisible = false;
                    sobj.GetComponent<InventorySlot>().isSelectable = true;
                    sobj.GetComponent<InventorySlot>().itemIsMoving = false;
                }
            }
        }
#endregion

        /// <summary>
        /// Deselect current selected Item
        /// </summary>
        public void DeselectContainerItem()
        {
            if (currentContainer != null && currentContainer.GetSelectedItem() != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        /// <summary>
        /// Callback to Take Back Item from Inventory Container
        /// </summary>
        public void TakeBackToInventory(ContainerItem coitem)
        {
            if (coitem == null) return;

            if (!isContainerFixed)
            {
                currentContainer.Take(coitem, !settings.TakeBackOneByOne);
                isNavContainer = false;
            }
            else
            {
                if (CheckInventorySpace())
                {
                    GameObject destroyObj = coitem.gameObject;
                    Item containerItem = coitem.item;

                    if (!settings.TakeBackOneByOne)
                    {
                        AddItem(containerItem.ID, coitem.amount, coitem.customData);

                        if (containerItem.Toggles.isStackable)
                        {
                            FixedContainerData.RemoveAll(x => x.StoredItem.ID == coitem.item.ID);
                            ContainterItemsCache.RemoveAll(x => x.item.ID == coitem.item.ID);
                            Destroy(destroyObj);
                        }
                        else
                        {
                            int itemIndex = ContainterItemsCache.IndexOf(coitem);
                            FixedContainerData.RemoveAt(itemIndex);
                            ContainterItemsCache.RemoveAt(itemIndex);
                            Destroy(destroyObj);
                        }
                    }
                    else
                    {
                        int itemIndex = ContainterItemsCache.IndexOf(coitem);

                        if (containerItem.ItemType != ItemType.Weapon && containerItem.ItemType != ItemType.Bullets)
                        {
                            AddItem(containerItem.ID, 1, coitem.customData);

                            if (coitem.amount == 1)
                            {
                                FixedContainerData.RemoveAt(itemIndex);
                                ContainterItemsCache.RemoveAt(itemIndex);
                                Destroy(destroyObj);
                            }
                            else
                            {
                                FixedContainerData[itemIndex].Amount--;
                                coitem.amount--;
                            }
                        }
                        else
                        {
                            AddItem(containerItem.ID, coitem.amount, coitem.customData);
                            FixedContainerData.RemoveAt(itemIndex);
                            ContainterItemsCache.RemoveAt(itemIndex);
                            Destroy(destroyObj);
                        }
                    }

                    isNavContainer = false;
                }
                else
                {
                    ResetInventory();
                    ShowNotification("No Space in Inventory!");
                }
            }
        }

        /// <summary>
        /// Function to show normal Inventory Container
        /// </summary>
        public void ShowInventoryContainer(ItemsContainer container, ContainerItemData[] containerItems, string name = "CONTAINER")
        {
            if (!string.IsNullOrEmpty(name))
                content.ContainerNameText.text = name.ToUpper();
            else
                content.ContainerNameText.text = "CONTAINER";

            if (containerItems.Length > 0)
            {
                foreach (var citem in containerItems)
                {
                    GameObject coItem = Instantiate(prefabs.ContainerItem, panels.ContainterContent.transform);
                    ContainerItem item = coItem.GetComponent<ContainerItem>();
                    item.item = citem.StoredItem;
                    item.amount = citem.Amount;
                    item.customData = citem.ItemCustomData;
                    coItem.name = "CoItem_" + citem.StoredItem.Title.Replace(" ", "");
                    ContainterItemsCache.Add(coItem.GetComponent<ContainerItem>());
                }
            }

            isContainerFixed = false;
            currentContainer = container;
            objectives.ShowObjectives(false);
            panels.ContainterPanel.SetActive(true);
            gameManager.ShowInventory(true);
            isStoring = true;
        }

        /// <summary>
        /// Function to show Fixed Container
        /// </summary>
        public void ShowFixedInventoryContainer(string name = "CONTAINER")
        {
            content.ContainerNameText.text = !string.IsNullOrEmpty(name) ? name.ToUpper() : "CONTAINER";

            if (FixedContainerData.Count > 0)
            {
                foreach (var citem in FixedContainerData)
                {
                    GameObject coItem = Instantiate(prefabs.ContainerItem, panels.ContainterContent.transform);
                    ContainerItem item = coItem.GetComponent<ContainerItem>();
                    item.item = citem.StoredItem;
                    item.amount = citem.Amount;
                    item.customData = citem.ItemCustomData;
                    coItem.name = "CoItem_" + citem.StoredItem.Title.Replace(" ", "");
                    ContainterItemsCache.Add(coItem.GetComponent<ContainerItem>());
                }
            }

            isContainerFixed = true;
            objectives.ShowObjectives(false);
            panels.ContainterPanel.SetActive(true);
            gameManager.ShowInventory(true);
            isStoring = true;
        }

        public Dictionary<int, Dictionary<string, object>> GetFixedContainerData()
        {
            return FixedContainerData.ToDictionary(x => x.StoredItem.ID, y => new Dictionary<string, object> { { "item_amount", y.Amount }, { "item_custom", y.ItemCustomData } });
        }

        /// <summary>
        /// Callback for UI Store Button
        /// </summary>
        public void StoreSelectedItem()
        {
            if (selectedSlotID < 0) return;

            InventoryItemData itemData = ItemDataOfSlot(selectedSlotID);

            if (!isContainerFixed)
            {
                if (selectedSlotID != -1 && currentContainer != null)
                {
                    if (currentContainer.ContainsID(itemData.item.ID) && itemData.item.Toggles.isStackable)
                    {
                        currentContainer.IncreaseItemAmount(itemData.item, itemData.itemAmount);
                        RemoveSelectedItem(true);
                    }
                    else
                    {
                        if (currentContainer.ContainerCount() < currentContainer.GetContainerInfo().ContainerSpace)
                        {
                            currentContainer.Store(itemData.item, itemData.itemAmount, itemData.data);

                            if (itemSwitcher.currentItem == itemData.item.Settings.useSwitcherID)
                            {
                                itemSwitcher.DeselectItems();
                            }

                            RemoveSelectedItem(true);
                        }
                        else
                        {
                            ShowNotification(ContainerNoSpace);
                            ResetInventory();
                        }
                    }
                }
            }
            else
            {
                if (selectedSlotID != -1)
                {
                    if (FixedContainerData.Any(item => item.StoredItem.ID == itemData.item.ID) && itemData.item.Toggles.isStackable)
                    {
                        foreach (var item in FixedContainerData)
                        {
                            if (item.StoredItem.ID == itemData.item.ID)
                            {
                                item.Amount += itemData.itemAmount;
                                GetContainerItem(itemData.item.ID).amount = item.Amount;
                            }
                        }
                    }
                    else
                    {
                        StoreFixedContainerItem(itemData.item, itemData.itemAmount, itemData.data);

                        if (itemSwitcher.currentItem == itemData.item.Settings.useSwitcherID)
                        {
                            itemSwitcher.DeselectItems();
                        }
                    }

                    RemoveSelectedItem(true);
                }
            }
        }

        void StoreFixedContainerItem(Item item, int amount, ItemData custom)
        {
            GameObject coItem = Instantiate(prefabs.ContainerItem, panels.ContainterContent.transform);
            ContainerItem citem = coItem.GetComponent<ContainerItem>();
            citem.item = item;
            citem.amount = amount;
            citem.customData = custom;
            coItem.name = "CoItem_" + item.Title.Replace(" ", "");
            FixedContainerData.Add(new ContainerItemData(item, amount, custom));
            ContainterItemsCache.Add(coItem.GetComponent<ContainerItem>());
        }

        /// <summary>
        /// Get UI ContainerItem Object
        /// </summary>
        public ContainerItem GetContainerItem(int id)
        {
            foreach (var item in ContainterItemsCache)
            {
                if (item.item.ID == id)
                {
                    return item;
                }
            }

            Debug.LogError($"Item with ID ({id}) does not found!");
            return null;
        }

        /// <summary>
        /// Start Shortcut Bind process
        /// </summary>
        public void BindShortcutItem()
        {
            if (cpSettings.ShortcutActions.Length > 0)
            {
                for (int i = 0; i < Slots.Count; i++)
                {
                    InventorySlot slot = Slots[i].GetComponent<InventorySlot>();

                    slot.isSelected = false;
                    slot.contexVisible = false;
                    slot.isSelectable = false;
                    slot.isDraggable = false;
                }

                ShowContexMenu(false);
                int controller = InputHandler.CurrentDevice.IsGamepadDevice();
                if (controller == 0)
                {
                    ShowNotificationFixed(KBShortcutBind);
                }
                else if (controller == 1)
                {
                    ShowNotificationFixed(GPShortcutBind);
                }

                selectedBind = selectedSlotID;
                isShortcutBind = true;
            }
            else
            {
                Debug.LogError("[Shortcut Bind] ShortcutActions are Empty!");
            }
        }

        /// <summary>
        /// Bind new or Exchange Inventory Shortcut
        /// </summary>
        public void ShortcutBind(int itemID, int slotID, string control)
        {
            Item item = GetItem(itemID);

            if (Shortcuts.Count > 0)
            {
                if (Shortcuts.All(s => s.slot != slotID && !s.shortcut.Equals(control)))
                {
                    // shortcut does not exist
                    Shortcuts.Add(new ShortcutModel(item, slotID, control));
                    ItemDataOfSlot(slotID).shortcut = control;
                }
                else
                {
                    // shortcut already exist
                    for (int i = 0; i < Shortcuts.Count; i++)
                    {
                        if (Shortcuts.Any(s => s.slot == slotID))
                        {
                            if (Shortcuts[i].slot == slotID)
                            {
                                // change shortcut key
                                if (Shortcuts.Any(s => s.shortcut.Equals(control)))
                                {
                                    // find equal shortcut with key and exchange it
                                    foreach (var equal in Shortcuts)
                                    {
                                        if (equal.shortcut.Equals(control))
                                        {
                                            equal.shortcut = Shortcuts[i].shortcut;
                                            ItemDataOfSlot(equal.slot).shortcut = Shortcuts[i].shortcut;
                                        }
                                    }
                                }

                                // change actual shortcut key
                                Shortcuts[i].shortcut = control;
                                ItemDataOfSlot(Shortcuts[i].slot).shortcut = Shortcuts[i].shortcut;
                                break;
                            }
                        }
                        else if (Shortcuts[i].shortcut.Equals(control))
                        {
                            // change shortcut item
                            ItemDataOfSlot(Shortcuts[i].slot).shortcut = string.Empty;
                            ItemDataOfSlot(slotID).shortcut = control;
                            Shortcuts[i].slot = slotID;
                            Shortcuts[i].item = item;
                            break;
                        }
                    }
                }
            }
            else
            {
                Shortcuts.Add(new ShortcutModel(item, slotID, control));
                ItemDataOfSlot(slotID).shortcut = control;
            }

            isShortcutBind = false;
            fader.FadeOutSignal();
            ResetInventory();
        }

        /// <summary>
        /// Update Shortcut slot with binded Control
        /// </summary>
        public void UpdateShortcut(string control, int newSlotID)
        {
            foreach (var shortcut in Shortcuts)
            {
                if (shortcut.shortcut.Equals(control))
                {
                    shortcut.slot = newSlotID;
                }
            }
        }

        /// <summary>
        /// Automatically Bind Shortcut
        /// </summary>
        /// <returns>Shortcut Action</returns>
        public string AutoBindShortcut(int slotID, int itemID)
        {
            Item item = GetItem(itemID);

            if (IsItemInSlot(slotID, itemID) && item.Toggles.canBindShortcut && item.Toggles.isUsable)
            {
                if (cpSettings.ShortcutActions.Length > 0)
                {
                    Dictionary<string, string> avaiableShortcuts = new Dictionary<string, string>();

                    foreach (var shortcut in cpSettings.ShortcutActions)
                    {
                        if (InputHandler.IsActionExist(shortcut))
                        {
                            if (!Shortcuts.Select(x => x.shortcut).Any(x => x.Equals(shortcut)))
                            {
                                avaiableShortcuts.Add(shortcut, shortcut);
                            }
                        }
                        else
                        {
                            Debug.LogError($"[AutoBind] Shortcut ({shortcut}) does not exist in control scheme!");
                            break;
                        }
                    }

                    if (avaiableShortcuts.Count > 0)
                    {
                        var newShortcut = avaiableShortcuts.FirstOrDefault();
                        ShortcutBind(itemID, slotID, newShortcut.Value);

                        return newShortcut.Key;
                    }
                }
                else
                {
                    Debug.LogError("[AutoBind] ShortcutActions are Empty!");
                }
            }
            else
            {
                Debug.LogError("[AutoBind] Cannot bind shortcut!");
            }

            return string.Empty;
        }

        /// <summary>
        /// Function to Open Inventory with Highlighted Items with Select Option.
        /// </summary>
        public void OnInventorySelect(int[] selectableItems, string[] tags, MonoBehaviour script, string selectText = "", string nullText = "")
        {
            if (selectableItems.Length > 0 && ItemsCache.Any(x => selectableItems.Any(y => x.item.ID.Equals(y))))
            {
                IItemSelectComponent = script;
                gameManager.ShowInventory(true);

                if (!string.IsNullOrEmpty(selectText))
                    ShowNotificationFixed(selectText);

                for (int i = 0; i < Slots.Count; i++)
                {
                    InventorySlot slot = Slots[i].GetComponent<InventorySlot>();
                    int slotItemID = slot.slotItem != null ? slot.slotItem.ID : -1;

                    slot.isSelectable = false;
                    slot.isItemSelect = true;
                    slot.isDraggable = false;

                    if(slotItemID > 0)
                    {
                        if(selectableItems.Length > 0)
                        {
                            if (selectableItems.Any(x => slotItemID == x))
                            {
                                if (tags.Length > 0)
                                {
                                    if (tags.Any(x => slot.itemData.data.data.ContainsValue(x)))
                                    {
                                        slot.isSelectable = true;
                                    }
                                }
                                else
                                {
                                    slot.isSelectable = true;
                                }
                            }
                            else
                            {
                                slot.GetComponent<Image>().color = coloring.SlotDisabled;
                            }
                        }
                        else
                        {
                            slot.isSelectable = true;
                        }
                    }
                }

                ShowContexMenu(false);
            }
            else if (!string.IsNullOrEmpty(nullText))
            {
                gameManager.ShowQuickMessage(nullText, "NoItems");
            }
        }
        public void OnInventorySelect(MonoBehaviour script, string selectText = "", string nullText = "")
        {
            if (AnyItemInventroy())
            {
                IItemSelectComponent = script;
                gameManager.ShowInventory(true);

                if (!string.IsNullOrEmpty(selectText))
                    ShowNotificationFixed(selectText);

                for (int i = 0; i < Slots.Count; i++)
                {
                    InventorySlot slot = Slots[i].GetComponent<InventorySlot>();
                    int slotItemID = slot.slotItem != null ? slot.slotItem.ID : -1;

                    slot.isSelectable = slotItemID >= 0;
                    slot.isItemSelect = true;
                    slot.isDraggable = false;
                }

                ShowContexMenu(false);
            }
            else if (!string.IsNullOrEmpty(nullText))
            {
                gameManager.ShowQuickMessage(nullText, "NoItems");
            }
        }

        /// <summary>
        /// Function to add a new item to an specific inventory slot.
        /// </summary>
        /// <returns>Auto Shortcut Input</returns>
        public string AddItemToSlot(int slotID, int itemID, int amount = 1, ItemData customData = null, bool autoShortcut = false)
        {
            // get item from inventory database and set item amount
            Item item = GetItem(itemID);
            amount = amount <= 0 ? 1 : amount;

            // check if there is a space in the inventory
            if (CheckInventorySpace())
            {
                // get item data if the item is already in the slot
                InventoryItemData itemData = ItemDataOfItem(item.ID, slotID);

                if (item.Toggles.isStackable && IsItemInSlot(slotID, itemID))
                {
                    // increase item amount
                    if (itemData != null) itemData.itemAmount += amount;
                }
                else
                {
                    // iterate each slot
                    for (int i = 0; i < Slots.Count; i++)
                    {
                        // check if the slot index is equal to slotID
                        if (i == slotID)
                        {
                            // create a new item object and set the item data
                            GameObject uiItem = Instantiate(prefabs.InventoryItem, Slots[i].transform);
                            itemData = uiItem.GetComponent<InventoryItemData>();
                            itemData.item = item;
                            itemData.itemAmount = amount;
                            itemData.slotID = i;
                            itemData.InitializeData();

                            // set item custom data
                            if (customData != null)
                                itemData.data = customData;

                            // add the item and item data to the slot component
                            InventorySlot slotData = Slots[i].GetComponent<InventorySlot>();
                            slotData.slotItem = item;
                            slotData.itemData = itemData;

                            // change slot background
                            Image slotImage = Slots[i].GetComponent<Image>();
                            slotImage.sprite = slotSprites.SlotWithItem;
                            slotImage.enabled = true;

                            // change the item object sprite to item sprite
                            uiItem.GetComponent<Image>().sprite = item.ItemSprite;
                            uiItem.GetComponent<RectTransform>().position = Vector2.zero;
                            uiItem.name = item.Title;

                            // add an inventory item to the item cache
                            ItemsCache.Add(new InventoryItem(item, customData));
                            break;
                        }
                    }

                    // if auto-shortcut is enabled, link the shortcut to the item and return the shortcut action name
                    if (autoShortcut)
                    {
                        return AutoBindShortcut(slotID, itemID);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Function to add new Item to Inventory.
        /// </summary>
        /// <returns>Auto Shortcut Input</returns>
        public string AddItem(int itemID, int amount = 1, ItemData customData = null, bool autoShortcut = false)
        {
            // get item from inventory database and set item amount
            Item item = GetItem(itemID);
            amount = amount <= 0 ? 1 : amount;

            // check if there is a space in the inventory or if the item is already in the inventory
            bool itemInInventory = CheckItemInventory(itemID);
            if(CheckInventorySpace() || itemInInventory)
            {
                // check is item is stackabe
                bool isStackable = item.Toggles.isStackable;

                // get item data if the item is already in inventory
                InventoryItemData itemData = ItemDataOfItem(itemID);

                // if the item is already in inventory and the item is stackable
                if (itemInInventory && isStackable)
                {
                    // increase item amount
                    if (itemData != null) itemData.itemAmount += amount;
                }
                else
                {
                    int slot = -1;
                    int iterations = 1;

                    // check if the item is a weapon to avoid multiple additions
                    bool isWeapon = item.ItemType == ItemType.Weapon || item.ItemType == ItemType.Bullets;

                    // if the item cannot be stacked, add it amount times
                    if (!isStackable && !isWeapon)
                    {
                        iterations = amount;
                        amount = 1;
                    }

                    // iterate each slot
                    for (int i = 0; i < Slots.Count; i++)
                    {
                        // check is slot is empty
                        if (Slots[i].transform.childCount == 1)
                        {
                            // create a new item object and set the item data
                            GameObject uiItem = Instantiate(prefabs.InventoryItem, Slots[i].transform);
                            itemData = uiItem.GetComponent<InventoryItemData>();
                            itemData.item = item;
                            itemData.itemAmount = amount;
                            itemData.slotID = i;
                            itemData.InitializeData();

                            // set item custom data
                            if (customData != null)
                                itemData.data = customData;

                            // add the item and item data to the slot component
                            InventorySlot slotData = Slots[i].GetComponent<InventorySlot>();
                            slotData.slotItem = item;
                            slotData.itemData = itemData;

                            // change slot background
                            Image slotImage = Slots[i].GetComponent<Image>();
                            slotImage.sprite = slotSprites.SlotWithItem;
                            slotImage.enabled = true;

                            // change the item object sprite to item sprite
                            uiItem.GetComponent<Image>().sprite = item.ItemSprite;
                            uiItem.GetComponent<RectTransform>().position = Vector2.zero;
                            uiItem.name = item.Title;

                            // add an inventory item to the item cache
                            ItemsCache.Add(new InventoryItem(item, customData));
                            slot = i;

                            // break the for loop if the number of iterations is less than 1
                            iterations--;
                            if (iterations <= 0) break;
                        }
                    }

                    // if auto-shortcut is enabled, link the shortcut to the item and return the shortcut action name
                    if (autoShortcut && slot > -1)
                    {
                        return AutoBindShortcut(slot, itemID);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Remove Item from Slot
        /// </summary>
        public void RemoveSlotItem(int slotID, bool all = false)
        {
            if (slotID >= 0)
            {
                InventoryItemData data = ItemDataOfSlot(slotID);

                if (data != null)
                {
                    Item itemToRemove = data.item;

                    if (itemToRemove.Toggles.isStackable && IsItemInSlot(slotID, itemToRemove.ID) && !all)
                    {
                        data.itemAmount--;
                        data.textAmount.text = data.itemAmount.ToString();

                        if (data.itemAmount <= 0)
                        {
                            Destroy(Slots[slotID].transform.GetChild(1).gameObject);
                            RemoveFromCache(itemToRemove, data.data);
                            ResetInventory();
                        }

                        if (data.itemAmount == 1)
                        {
                            data.textAmount.text = "";
                        }
                    }
                    else
                    {
                        Destroy(Slots[slotID].transform.GetChild(1).gameObject);
                        RemoveFromCache(itemToRemove, data.data);
                        ResetInventory();
                    }
                }
            }
        }

        /// <summary>
        /// Remove one or all Item stacks by Item ID
        /// </summary>
        public void RemoveItem(int ID, bool all = false, bool lastItem = false)
        {
            Item itemToRemove = GetItem(ID);
            int slotID;

            if (lastItem)
            {
                slotID = GetItemSlotID(itemToRemove.ID, true);
            }
            else
            {
                slotID = GetItemSlotID(itemToRemove.ID);
            }

            if (slotID >= 0)
            {
                if (itemToRemove.Toggles.isStackable && CheckItemInventory(itemToRemove.ID) && !all)
                {
                    InventoryItemData data = Slots[slotID].GetComponentInChildren<InventoryItemData>();
                    data.itemAmount--;
                    data.textAmount.text = data.itemAmount.ToString();

                    if (data.itemAmount <= 0)
                    {
                        Destroy(Slots[slotID].transform.GetChild(1).gameObject);
                        RemoveFromCache(itemToRemove);
                        ResetInventory();
                    }

                    if (data.itemAmount == 1)
                    {
                        data.textAmount.text = "";
                    }
                }
                else
                {
                    Destroy(Slots[slotID].transform.GetChild(1).gameObject);
                    RemoveFromCache(itemToRemove);
                    ResetInventory();
                }
            }
        }

        /// <summary>
        /// Remove one or all Item stacks from selected Slot
        /// </summary>
        public void RemoveSelectedItem(bool all = false)
        {
            if (selectedSlotID >= 0)
            {
                int slot = selectedSlotID;
                Item item = GetSlotItem(slot);

                if (item.Toggles.isStackable && CheckItemInventory(item.ID) && !all)
                {
                    InventoryItemData data = Slots[slot].GetComponentInChildren<InventoryItemData>();
                    data.itemAmount--;
                    data.textAmount.text = data.itemAmount.ToString();

                    if (data.itemAmount <= 0)
                    {
                        Destroy(Slots[slot].transform.GetChild(1).gameObject);
                        RemoveFromCache(item);
                        ResetInventory();
                    }

                    if (data.itemAmount == 1)
                    {
                        data.textAmount.text = "";
                    }
                }
                else
                {
                    Destroy(Slots[slot].transform.GetChild(1).gameObject);
                    RemoveFromCache(item);
                    ResetInventory();
                }
            }
        }

        /// <summary>
        /// Remove specific item amount by ItemID
        /// </summary>
        public void RemoveItemAmount(int ID, int Amount)
        {
            if (CheckItemInventory(ID))
            {
                int slotID = GetItemSlotID(ID, true);
                InventoryItemData data = Slots[slotID].GetComponentInChildren<InventoryItemData>();

                if (data.itemAmount > Amount)
                {
                    data.itemAmount -= Amount;
                    data.transform.parent.GetChild(0).GetChild(0).GetComponent<Text>().text = data.itemAmount.ToString();
                }
                else
                {
                    RemoveSlotItem(slotID, true);
                }
            }
        }

        /// <summary>
        /// Remove selected slot Item Amount
        /// </summary>
        public void RemoveSelectedItemAmount(int Amount)
        {
            if (selectedSlotID >= 0)
            {
                int slot = selectedSlotID;
                Item item = GetSlotItem(slot);

                if (CheckItemInventory(item.ID))
                {
                    InventoryItemData data = Slots[slot].GetComponentInChildren<InventoryItemData>();

                    if (data.itemAmount > Amount)
                    {
                        data.itemAmount -= Amount;
                        data.transform.parent.GetChild(0).GetChild(0).GetComponent<Text>().text = data.itemAmount.ToString();
                    }
                    else
                    {
                        Destroy(Slots[slot].transform.GetChild(1).gameObject);
                        RemoveFromCache(item);
                        ResetInventory();
                    }
                }
            }
        }

        public void RemoveAllItems()
        {
            for (int i = 0; i < settings.SlotAmount; i++)
            {
                if (GetSlotItem(i) is var item && item != null)
                {
                    if (Slots[i].transform.childCount > 0)
                    {
                        Destroy(Slots[i].transform.GetChild(1).gameObject);
                    }

                    RemoveFromCache(item);
                    ResetInventory();
                }
            }
        }

        /// <summary>
        /// Remove Item from current Items Cache
        /// </summary>
        private void RemoveFromCache(Item item, ItemData customData = null, bool all = false)
        {
            if (all)
            {
                if (customData != null && customData.data.ContainsKey(ITEM_TAG))
                {
                    ItemsCache.RemoveAll(i => i.item.ID.Equals(item.ID) && i.customData.data.Any(x => customData.data.Any(y => x.Value == y.Value)));
                }
                else
                {
                    ItemsCache.RemoveAll(x => x.item.ID.Equals(item.ID));
                }
            }
            else
            {
                int index = -1;

                if (customData != null && customData.data.ContainsKey(ITEM_TAG))
                {
                    index = ItemsCache.FindIndex(i => i.item.ID.Equals(item.ID) && i.customData.data.Any(x => customData.data.Any(y => x.Value == y.Value)));
                }
                else
                {
                    index = ItemsCache.FindIndex(x => x.item.ID.Equals(item.ID));
                }

                if (index != -1)
                {
                    ItemsCache.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Use Selected Item
        /// </summary>
        public void UseSelectedItem()
        {
            UseItem();
        }

        /// <summary>
        /// Use Selected Item
        /// </summary>
        /// <param name="slotItem">Slot with item</param>
        public void UseItem(int slotItem = -1)
        {
            Item usableItem = null;

            if (slotItem >= 0)
            {
                if (IsAnyItemInSlot(slotItem))
                {
                    usableItem = GetSlotItem(slotItem);
                }
            }
            else if (usableItem == null && selectedSlotID >= 0)
            {
                usableItem = GetSlotItem(selectedSlotID);
                slotItem = selectedSlotID;
            }

            if (usableItem == null)
            {
                Debug.LogError("[Inventory Use] Cannot use a null Item!");
                return;
            }

            if (GetItemAmount(usableItem.ID) < 2 || usableItem.Toggles.showItemOnUse)
            {
                if (selectedSlotID >= 0)
                {
                    GetSlot(selectedSlotID).Select();
                }

                if (usableItem.Toggles.showItemOnUse)
                {
                    ResetInventory();
                }
            }

            if (usableItem.Toggles.doActionUse)
            {
                TriggerItemAction(slotItem, usableItem.Settings.useSwitcherID);
            }

            if (healthManager && usableItem.ItemType == ItemType.Heal)
            {
                healthManager.ApplyHeal(usableItem.Settings.healAmount);

                if (!healthManager.isMaximum)
                {
                    if (usableItem.Sounds.useSound)
                    {
                        Utilities.PlayOneShot2D(Utilities.MainPlayerCamera().transform.position, usableItem.Sounds.useSound, usableItem.Sounds.useVolume);
                    }

                    if (slotItem >= 0)
                    {
                        RemoveSlotItem(slotItem);
                    }
                    else
                    {
                        Debug.LogError("[Inventory] slotItem parameter cannot be (-1)!");
                    }
                }

                if(usableItem.Toggles.restoreStamina && healthManager.gameObject.HasComponent(out PlayerController playerController))
                {
                    playerController.RestoreStamina(usableItem.Settings.staminaAmount);
                }
            }

            if (usableItem.ItemType == ItemType.Weapon || usableItem.Toggles.showItemOnUse)
            {
                itemSwitcher.SelectSwitcherItem(usableItem.Settings.useSwitcherID);
                itemSwitcher.weaponItem = usableItem.Settings.useSwitcherID;
            }

            ShowContexMenu(false);
            ResetSlotProperties(true);
        }

        /// <summary>
        /// Drop selected slot Item to ground
        /// </summary>
        public void DropItemGround()
        {
            InventoryItemData itemData = ItemDataOfSlot(selectedSlotID);

            if (!PlayerController.HasReference)
                return;

            Item item = itemData.item;
            int itemAmount = itemData.itemAmount;

            Transform dropPos = PlayerController.Instance.GetComponentInChildren<PlayerFunctions>().inventoryDropPos;
            InteractiveItem interactiveItem = null;
            GameObject worldItem = null;

            SaveGameHandler.SaveableType saveableType = SaveGameHandler.SaveableType.None;

            if (item.ItemType == ItemType.Weapon || item.Toggles.showItemOnUse)
            {
                if (itemSwitcher.currentItem == item.Settings.useSwitcherID)
                {
                    itemSwitcher.DisableItems();
                }
            }

            if (itemData.data.Exist("object_scene"))
            {
                if (itemData.data["object_scene"].Equals(gameManager.currentScene.name) && itemData.data.Exist("object_path"))
                {
                    string objPath = (string)itemData.data["object_path"];

                    if (worldItem = GameObject.Find(objPath))
                    {
                        saveableType = SaveGameHandler.Instance.GetSaveableType(worldItem);
                    }
                }
            }

            if (itemAmount >= 2 && item.ItemType != ItemType.Weapon)
            {
                if (worldItem)
                {
                    if (saveableType != SaveGameHandler.SaveableType.Constant)
                    {
                        SaveGameHandler.Instance.RemoveSaveableObject(worldItem, true);
                    }
                }

                GameObject drop = SaveGameHandler.Instance.InstantiateSaveableReference(item.ItemPackDrop, dropPos.position, dropPos.eulerAngles);
                if (drop != null)
                {
                    worldItem = drop;
                    worldItem.name = drop.name;
                }
                else
                {
                    Debug.LogError($"[Inventory Drop] Cannot instantiate item!");
                    return;
                }

                if (worldItem && worldItem.HasComponent(out interactiveItem))
                {
                    interactiveItem.EnableObject();
                    interactiveItem.inventoryID = item.ID;
                    interactiveItem.OnInventoryItemUpdate();

                    string title = string.Format("{0}x {1}", itemAmount, item.Title);
                    interactiveItem.examineTitle = title;
                    interactiveItem.useItemTitle = false;
                    interactiveItem.itemType = InteractiveItem.ItemType.InventoryItem;
                    interactiveItem.disableType = InteractiveItem.DisableType.Destroy;
                }
                else
                {
                    Debug.LogError($"[Inventory Drop] {worldItem.name} does not have InteractiveItem script");
                    return;
                }
            }
            else if (itemAmount == 1 || item.ItemType == ItemType.Weapon)
            {
                itemAmount = 1;

                if (saveableType != SaveGameHandler.SaveableType.Constant)
                {
                    if (!worldItem)
                    {
                        GameObject drop = SaveGameHandler.Instance.InstantiateSaveableReference(item.ItemDrop, dropPos.position, dropPos.eulerAngles);
                        if (drop != null)
                        {
                            worldItem = drop;
                            worldItem.name = drop.name;
                        }
                    }
                    else
                    {
                        worldItem.transform.SetParent(null);
                        worldItem.transform.SetPositionAndRotation(dropPos.position, dropPos.rotation);
                    }
                }
                else if (saveableType == SaveGameHandler.SaveableType.Constant)
                {
                    worldItem.transform.SetParent(null);
                    worldItem.transform.SetPositionAndRotation(dropPos.position, dropPos.rotation);
                }

                if (worldItem && worldItem.HasComponent(out interactiveItem))
                {
                    interactiveItem.EnableObject();

                    if (saveableType != SaveGameHandler.SaveableType.Constant)
                    {
                        interactiveItem.disableType = InteractiveItem.DisableType.Destroy;
                    }
                }
                else
                {
                    Debug.LogError($"[Inventory Drop] {worldItem.name} does not have InteractiveItem script!");
                    return;
                }

                if (itemData.data.Exist(ITEM_PATH))
                {
                    Texture tex = Resources.Load<Texture2D>((string)itemData.data[ITEM_PATH]);
                    worldItem.GetComponentInChildren<MeshRenderer>().material.SetTexture("_MainTex", tex);
                }
            }

            if (worldItem != null)
            {
                if (interactiveItem)
                {
                    interactiveItem.itemAmount = itemAmount;

                    if (interactiveItem.itemData != null && saveableType != SaveGameHandler.SaveableType.Constant)
                        interactiveItem.itemData = itemData.data;
                }

                if (worldItem.HasComponent(out Collider col))
                {
                    Physics.IgnoreCollision(col, Utilities.MainPlayerCamera().transform.root.GetComponent<Collider>());
                    col.isTrigger = false;
                }

                if (worldItem.HasComponent(out Rigidbody rigid))
                {
                    rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    rigid.AddForce(Utilities.MainPlayerCamera().transform.forward * (settings.DropStrength * 10));
                }
                else
                {
                    Debug.Log($"[Inventory Drop] Rigidbody component added to {worldItem.name}!");

                    Rigidbody rb = worldItem.AddComponent<Rigidbody>();
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    rb.AddForce(Utilities.MainPlayerCamera().transform.forward * (settings.DropStrength * 10));
                }

                if (itemAmount < 2 || item.Toggles.showItemOnUse || item.ItemType == ItemType.Bullets)
                {
                    ShowContexMenu(false);
                }

                RemoveSelectedItem(itemAmount > 1);
            }
            else
            {
                Debug.LogError($"[Inventory Drop] Cannot Instantiate World Item!");
            }
        }

        /// <summary>
        /// Callback for CombineItem UI Button
        /// </summary>
        public void CombineItem()
        {
            firstCandidate = GetSlot(selectedSlotID);

            for (int i = 0; i < Slots.Count; i++)
            {
                Slots[i].GetComponent<InventorySlot>().isItemSelect = true;
                Slots[i].GetComponent<InventorySlot>().isDraggable = false;

                if (IsCombineSlot(i))
                {
                    Slots[i].GetComponent<InventorySlot>().isSelectable = true;
                    Slots[i].GetComponent<InventorySlot>().isCombineCandidate = true;
                }
                else
                {
                    Slots[i].GetComponent<InventorySlot>().isSelectable = false;
                    Slots[i].GetComponent<InventorySlot>().isCombineCandidate = false;
                }
            }

            ShowContexMenu(false);
        }

        /// <summary>
        /// Check if slot has item which is combinable.
        /// </summary>
        bool IsCombineSlot(int slotID)
        {
            InventoryItemData itemData;

            if ((itemData = ItemDataOfSlot(selectedSlotID)) != null)
            {
                InventoryScriptable.ItemMapper.CombineSettings[] combineSettings = itemData.item.CombineSettings;

                foreach (var id in combineSettings)
                {
                    InventoryItemData slotData;

                    if ((slotData = ItemDataOfSlot(slotID)) != null)
                    {
                        if (slotData.item.ID == id.combineWithID && (bool)slotData.data[ITEM_COMBINE])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if item has partner item to combine.
        /// </summary>
        public bool HasCombinePartner(Item Item)
        {
            InventoryScriptable.ItemMapper.CombineSettings[] combineSettings = Item.CombineSettings;
            return ItemsCache.Any(item => combineSettings.Any(item2 => item.item.ID == item2.combineWithID) && CanAnyCombine(item.item));
        }

        bool CanAnyCombine(Item item)
        {
            bool canCombine = false;

            foreach (var slot in Slots)
            {
                InventoryItemData itemData = slot.GetComponentInChildren<InventoryItemData>();

                if (itemData != null && itemData.itemID == item.ID && itemData.data.TryToGet(ITEM_COMBINE, out bool combine))
                {
                    if (combine)
                    {
                        canCombine = true;
                        break;
                    }
                }
            }

            return canCombine;
        }

        /// <summary>
        /// Function to combine selected Item with second Item.
        /// </summary>
        public void CombineWith(Item SecondItem, int slotID)
        {
            if (IItemSelectComponent != null)
            {
                if (IItemSelectComponent is IItemSelect)
                {
                    InventoryItemData data = ItemDataOfSlot(slotID);
                    RemoveSlotItem(slotID);
                    (IItemSelectComponent as IItemSelect).OnItemSelect(SecondItem.ID, data.data);
                    IItemSelectComponent = null;
                    gameManager.ShowInventory(false);
                }
            }
            else if (firstCandidate != null)
            {
                int firstItemSlot = firstCandidate.slotID;
                int secondItemSlot = slotID;

                if (firstItemSlot != secondItemSlot)
                {
                    Item SelectedItem = firstCandidate.itemData.item;
                    InventoryScriptable.ItemMapper.CombineSettings[] selectedCombineSettings = SelectedItem.CombineSettings;

                    int CombinedItemID = -1;
                    int CombineSwitcherID = -1;

                    foreach (var item in selectedCombineSettings)
                    {
                        if (item.combineWithID == SecondItem.ID)
                        {
                            CombinedItemID = item.resultCombineID;
                            CombineSwitcherID = item.combineSwitcherID;
                        }
                    }

                    for (int i = 0; i < Slots.Count; i++)
                    {
                        Slots[i].GetComponent<InventorySlot>().isSelectable = true;
                    }

                    if (SelectedItem.Sounds.combineSound)
                    {
                        Utilities.PlayOneShot2D(Utilities.MainPlayerCamera().transform.position, SelectedItem.Sounds.combineSound, SelectedItem.Sounds.combineVolume);
                    }
                    else if (SecondItem.Sounds.combineSound)
                    {
                        Utilities.PlayOneShot2D(Utilities.MainPlayerCamera().transform.position, SecondItem.Sounds.combineSound, SecondItem.Sounds.combineVolume);
                    }

                    if (SelectedItem.Toggles.doActionCombine)
                    {
                        TriggerItemAction(firstItemSlot, CombineSwitcherID);
                    }
                    if (SecondItem.Toggles.doActionCombine)
                    {
                        TriggerItemAction(secondItemSlot, CombineSwitcherID);
                    }

                    if (SelectedItem.ItemType == ItemType.ItemPart && SelectedItem.Toggles.isCombinable)
                    {
                        int switcherID = GetItem(SelectedItem.CombineSettings[0].combineWithID).Settings.useSwitcherID;
                        GameObject MainObject = itemSwitcher.ItemList[switcherID];

                        MonoBehaviour script = MainObject.GetComponents<MonoBehaviour>().SingleOrDefault(sc => sc.GetType().GetField("CanReload") != null);
                        FieldInfo info = script.GetType().GetField("CanReload");

                        if (info != null)
                        {
                            bool canReload = Parser.Convert<bool>(script.GetType().InvokeMember("CanReload", BindingFlags.GetField, null, script, null).ToString());

                            if (canReload)
                            {
                                MainObject.SendMessage("Reload", SendMessageOptions.DontRequireReceiver);
                                RemoveSlotItem(firstItemSlot);
                            }
                            else
                            {
                                gameManager.ShowQuickMessage(CannotReload, "");
                                ResetInventory();
                            }
                        }
                        else
                        {
                            Debug.Log(MainObject.name + " object does not have script with CanReload property!");
                        }

                        gameManager.ShowInventory(false);
                    }
                    else if (SelectedItem.Toggles.isCombinable)
                    {
                        if (SelectedItem.Toggles.combineShowItem && CombineSwitcherID != -1)
                        {
                            if (CombineSwitcherID != -1)
                            {
                                itemSwitcher.SelectSwitcherItem(CombineSwitcherID);
                            }
                        }

                        if (SelectedItem.Toggles.combineAddItem && CombinedItemID != -1)
                        {
                            int a_count = ItemDataOfSlot(firstItemSlot).itemAmount;
                            int b_count = ItemDataOfSlot(secondItemSlot).itemAmount;

                            if (!CheckInventorySpace())
                            {
                                if (a_count > 1 && b_count > 1)
                                {
                                    gameManager.ShowQuickMessage(InventoryNoSpace, "InventorySpace", true);
                                    return;
                                }
                            }

                            if (a_count < 2 && b_count >= 2)
                            {
                                if (!SelectedItem.Toggles.combineKeepItem)
                                {
                                    StartCoroutine(WaitForRemoveAddItem(secondItemSlot, CombinedItemID));
                                }
                                else
                                {
                                    AddItem(CombinedItemID, 1);
                                }
                            }
                            if (a_count >= 2 && b_count < 2)
                            {
                                if (!SecondItem.Toggles.combineKeepItem)
                                {
                                    StartCoroutine(WaitForRemoveAddItem(secondItemSlot, CombinedItemID));
                                }
                                else
                                {
                                    AddItem(CombinedItemID, 1);
                                }
                            }
                            if (a_count < 2 && b_count < 2)
                            {
                                if (!SelectedItem.Toggles.combineKeepItem)
                                {
                                    StartCoroutine(WaitForRemoveAddItem(secondItemSlot, CombinedItemID));
                                }
                                else
                                {
                                    AddItem(CombinedItemID, 1);
                                }
                            }
                            if (a_count >= 2 && b_count >= 2)
                            {
                                AddItem(CombinedItemID, 1);
                            }
                        }

                        if (!SelectedItem.Toggles.combineKeepItem && !SelectedItem.CustomActions.actionRemove)
                        {
                            RemoveSlotItem(firstItemSlot);
                        }
                        if (!SecondItem.Toggles.combineKeepItem && !SecondItem.CustomActions.actionRemove)
                        {
                            RemoveSlotItem(secondItemSlot);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Selected Slot ID cannot be null!");
            }

            ResetSlotProperties();
            selectedSlotID = -1;
            firstCandidate = null;
        }

        /// <summary>
        /// Function to Trigger Item Actions
        /// </summary>
        void TriggerItemAction(int itemSlot, int switcherID = -1)
        {
            Item SelectedItem = GetSlotItem(itemSlot);

            bool trigger = false;

            if (SelectedItem.UseActionType != ItemAction.None)
            {
                if (SelectedItem.UseActionType == ItemAction.Increase)
                {
                    InventoryItemData itemData = ItemDataOfSlot(itemSlot);

                    if (itemData.data.TryToGet(ITEM_VALUE, out int value))
                    {
                        value++;
                        itemData.data[ITEM_VALUE] = value;

                        if (value >= SelectedItem.CustomActions.triggerValue)
                        {
                            trigger = true;
                        }
                    }
                }
                else if (SelectedItem.UseActionType == ItemAction.Decrease)
                {
                    InventoryItemData itemData = ItemDataOfSlot(itemSlot);

                    if (itemData.data.TryToGet(ITEM_VALUE, out int value))
                    {
                        value--;
                        itemData.data[ITEM_VALUE] = value;

                        if (value <= SelectedItem.CustomActions.triggerValue)
                        {
                            trigger = true;
                        }
                    }
                }
                else if (SelectedItem.UseActionType == ItemAction.ItemValue)
                {
                    IItemValueProvider itemValue = itemSwitcher.ItemList[switcherID].GetComponent<IItemValueProvider>();
                    if (itemValue != null)
                    {
                        InventoryItemData itemData = ItemDataOfSlot(itemSlot);

                        if (GetSlotItem(itemSlot).Description.RegexMatch('{', '}', "value"))
                        {
                            if(itemData.data.TryToGet(ITEM_VALUE, out string value))
                            {
                                itemValue.OnSetValue(value);
                            }
                        }
                    }
                }
            }

            if (trigger)
            {
                if (SelectedItem.CustomActions.actionRemove)
                {
                    RemoveSlotItem(itemSlot);
                }
                if (SelectedItem.CustomActions.actionAddItem)
                {
                    AddItem(SelectedItem.CustomActions.triggerItemID, 1, new ItemData
                    (
                        (ITEM_VALUE, SelectedItem.CustomActions.triggerCustomValue)
                    ));
                }
                if (SelectedItem.CustomActions.actionRestrictCombine)
                {
                    ItemDataOfSlot(itemSlot).data[ITEM_COMBINE] = false;
                }
                if (SelectedItem.CustomActions.actionRestrictUse)
                {
                    ItemDataOfSlot(itemSlot).data[ITEM_USE] = false;
                }
            }
        }

        /// <summary>
        /// Wait until old Item will be removed, then add new Item
        /// </summary>
        IEnumerator WaitForRemoveAddItem(int oldItemSlot, int newItem)
        {
            int oldItemCount = ItemDataOfSlot(oldItemSlot).itemAmount;

            if (oldItemCount < 2)
            {
                yield return new WaitUntil(() => !IsAnyItemInSlot(oldItemSlot));
                AddItemToSlot(oldItemSlot, newItem);
            }
            else
            {
                AddItem(newItem, 1);
            }
        }

        /// <summary>
        /// Callback for Examine Item Button
        /// </summary>
        public void ExamineItem()
        {
            InventoryItemData itemData = ItemDataOfSlot(selectedSlotID);
            Item item = itemData.item;
            gameManager.ShowInventory(false, examine: true);
            gameManager.ShowCursor(false);
            gameManager.ShowConsoleCursor(false);

            if (item.ItemDrop.Object.GetComponent<InteractiveItem>())
            {
                GameObject examine = Instantiate(item.ItemDrop.Object);

                if (itemData.data.Exist(ITEM_PATH))
                {
                    Texture tex = Resources.Load<Texture2D>((string)itemData.data[ITEM_PATH]);
                    examine.GetComponentInChildren<MeshRenderer>().material.SetTexture("_MainTex", tex);
                }

                ExamineManager examineManager = scriptManager.gameObject.GetComponent<ExamineManager>();
                examineManager.ExamineObject(examine, item.Settings.examineRotation);
                ShowContexMenu(false);
            }
            else
            {
                Debug.LogError($"Examinable object {item.ItemDrop.Object.name} does not have an InteractiveItem.");
            }
        }

        /// <summary>
        /// Function to set specific item amount
        /// </summary>
        public void SetItemAmount(int ID, int Amount)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].transform.childCount > 1)
                {
                    if (Slots[i].GetComponentInChildren<InventoryItemData>().item.ID == ID)
                    {
                        Slots[i].GetComponentInChildren<InventoryItemData>().itemAmount = Amount;
                    }
                }
            }
        }

        /// <summary>
        /// Function to expand item slots
        /// </summary>
        public void ExpandSlots(int SlotsAmount)
        {
            int extendedSlots = settings.SlotAmount + SlotsAmount;

            for (int i = settings.SlotAmount; i < extendedSlots; i++)
            {
                GameObject slot = Instantiate(prefabs.InventorySlot);
                Slots.Add(slot);
                slot.GetComponent<InventorySlot>().inventory = this;
                slot.GetComponent<InventorySlot>().slotID = i;
                slot.transform.SetParent(panels.SlotsContent);
                slot.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }

            settings.SlotAmount = extendedSlots;
        }

        /// <summary>
        /// Function to set slots button enabled state
        /// </summary>
        public void SetSlotsState(bool state, params GameObject[] except)
        {
            foreach (var slot in Slots)
            {
                if (!state && except.Length > 0 && !except.Any(x => x == slot))
                {
                    slot.GetComponent<InventorySlot>().isSelectable = state;
                }
                else if (state)
                {
                    slot.GetComponent<InventorySlot>().isSelectable = state;
                }
            }
        }

        /// <summary>
        /// Get item reference from Inventory Database
        /// </summary>
        public Item GetItem(int ID)
        {
            foreach (var item in ListOfItems)
            {
                if (item.ID == ID)
                    return item;
            }

            return default;
        }

        /// <summary>
        /// Get new item from Inventory Database
        /// </summary>
        public Item GetNewItem(int ID)
        {
            foreach (var item in inventoryDatabase.ItemDatabase)
            {
                if (item.ID == ID)
                    return new Item(item.ID, item);
            }

            return default;
        }

        /// <summary>
        /// Check if there is space in the inventory
        /// </summary>
        public bool CheckInventorySpace()
        {
            return Slots.Any(x => x.transform.childCount < 2);
        }

        /// <summary>
        /// Check if there is any item in the inventory
        /// </summary>
        public bool AnyItemInventroy()
        {
            return Slots.Any(x => x.transform.childCount > 1);
        }

        /// <summary>
        /// Check if the itemID is in inventory
        /// </summary>
        public bool CheckItemInventory(int ID)
        {
            return ItemsCache.Any(x => x.item.ID == ID);
        }

        /// <summary>
        /// Check if the item is in inventory and stackable by item ID
        /// </summary>
        public bool CheckItemInventoryStack(int ID)
        {
            return ItemsCache.Any(x => x.item.ID == ID && x.item.Toggles.isStackable);
        }

        /// <summary>
        /// Check if the switcher item is in inventory
        /// </summary>
        public bool CheckSwitcherItemInventory(int switcherID)
        {
            return ItemsCache.Any(x => x.item.Settings.useSwitcherID == switcherID);
        }

        /// <summary>
        /// Check if the slot contains a specific item
        /// </summary>
        bool IsItemInSlot(int slotID, int itemID)
        {
            InventoryItemData itemData = Slots[slotID].GetComponentInChildren<InventoryItemData>();
            if (itemData != null) return itemData.item.ID == itemID;
            return false;
        }

        /// <summary>
        /// Check if the slot contains an item
        /// </summary>
        bool IsAnyItemInSlot(int slotID)
        {
            InventoryItemData itemData = Slots[slotID].GetComponentInChildren<InventoryItemData>();
            return itemData != null;
        }

        /// <summary>
        /// Get InventoryItemData of item
        /// </summary>
        public InventoryItemData ItemDataOfItem(int itemID, int slotID = -1)
        {
            if (slotID > -1)
            {
                if (IsItemInSlot(slotID, itemID))
                    return Slots[slotID].GetComponentInChildren<InventoryItemData>();
            }
            else
            {
                foreach (var slot in Slots)
                {
                    InventoryItemData itemData = slot.GetComponentInChildren<InventoryItemData>();
                    if (itemData && itemData.item.ID == itemID)
                        return itemData;
                }
            }

            return null;
        }

        /// <summary>
        /// Get InventoryItemData from slot
        /// </summary>
        InventoryItemData ItemDataOfSlot(int slotID)
        {
            InventoryItemData itemData = Slots[slotID].GetComponentInChildren<InventoryItemData>();
            if (itemData != null) return itemData;
            return null;
        }

        /// <summary>
        /// Get the first populated slot
        /// </summary>
        InventorySlot FirstPopulatedSlot()
        {
            foreach (var slot in Slots)
            {
                InventorySlot invSlot = slot.GetComponent<InventorySlot>();
                if (invSlot.itemData != null) return invSlot;
            }

            return null;
        }

        /// <summary>
        /// Get InventorySlot by SlotID
        /// </summary>
        public InventorySlot GetSlot(int slotID)
        {
            return Slots[slotID].GetComponent<InventorySlot>();
        }

        /// <summary>
        /// Get SlotID by ItemID
        /// </summary>
        /// <param name="reverse">Iterate slots from last to first?</param>
        int GetItemSlotID(int itemID, bool reverse = false)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                int index = !reverse ? i : Slots.Count - i - 1;
                InventoryItemData itemData = Slots[index].GetComponentInChildren<InventoryItemData>();
                if (itemData != null && itemData.item.ID == itemID)
                    return index;
            }

            return -1;
        }

        /// <summary>
        /// Get item of SlotID
        /// </summary>
        Item GetSlotItem(int slotID)
        {
            InventoryItemData itemData = Slots[slotID].GetComponentInChildren<InventoryItemData>();
            if (itemData != null) return itemData.item;
            return null;
        }

        /// <summary>
        /// Get item amount of ItemID
        /// </summary>
        public int GetItemAmount(int itemID)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                InventoryItemData itemData = Slots[i].GetComponentInChildren<InventoryItemData>();
                if (itemData != null && itemData.item.ID == itemID) return itemData.itemAmount;
            }

            return -1;
        }

        /// <summary>
        /// Reset all slot properties
        /// </summary>
        public void ResetSlotProperties(bool exceptSelected = false)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                InventorySlot slot = GetSlot(i);

                slot.isDraggable = true;
                slot.isSelectable = true;
                slot.isSelected = exceptSelected && slot.isSelected;
                slot.contexVisible = false;
                slot.itemIsMoving = false;
                slot.isItemSelect = false;
                slot.isCombineCandidate = false;
            }
        }

        /// <summary>
        /// Deselect specific slot
        /// </summary>
        public void Deselect(int slotID)
        {
            Slots[slotID].GetComponent<Image>().color = Color.white;

            content.ItemLabel.text = string.Empty;
            content.ItemDescription.text = string.Empty;
            ShowContexMenu(false);
            ResetSlotProperties();

            selectedSlotID = -1;
        }

        /// <summary>
        /// Reset Inventory
        /// </summary>
        public void ResetInventory()
        {
            if (IItemSelectComponent != null || isShortcutBind) return;

            EventSystem.current.SetSelectedGameObject(null);
            ResetSlotProperties();
            SetSlotsState(true);

            if (selectedSlotID >= 0)
            {
                if (itemToMove)
                {
                    itemToMove.isMoving = false;
                    itemToMove = null;
                }

                GetSlot(selectedSlotID).isSelected = false;
                ShowContexMenu(false);
                content.ItemLabel.text = string.Empty;
                content.ItemDescription.text = string.Empty;
                selectedSlotID = -1;
                isContexVisible = false;
            }

            isSelecting = false;
            isShortcutBind = false;
        }

        /// <summary>
        /// Show selected Item Contex Menu
        /// </summary>
        public void ShowContexMenu(bool show, Item item = null, int slot = -1, bool ctx_use = true, bool ctx_combine = true, bool ctx_examine = true, bool ctx_drop = true, bool ctx_shortcut = false, bool ctx_store = false, bool ctx_remove = false)
        {
            InventoryItemData itemData = null;

            if (show && item != null && slot > -1)
            {
                Vector3[] corners = new Vector3[4];
                Slots[slot].GetComponent<RectTransform>().GetWorldCorners(corners);
                int[] cornerSlots = Enumerable.Range(0, settings.MaxSlots + 1).Where(x => x % settings.SlotsInRow == 0).ToArray();
                int n_slot = slot + 1;

                if (slot > -1)
                {
                    itemData = ItemDataOfSlot(slot);
                }

                if (!cornerSlots.Contains(n_slot))
                {
                    panels.ContexPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    panels.ContexPanel.transform.position = corners[2];
                }
                else
                {
                    panels.ContexPanel.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                    panels.ContexPanel.transform.position = corners[1];
                }

                itemData.data.TryToGet(ITEM_USE, out bool canUse);
                itemData.data.TryToGet(ITEM_COMBINE, out bool canCombine);

                bool use = item.Toggles.isUsable && ctx_use && canUse;
                bool combine = item.Toggles.isCombinable && ctx_combine && canCombine;
                bool examine = item.Toggles.canExamine && ctx_examine;
                bool drop = item.Toggles.isDroppable && ctx_drop;
                bool shortcut = item.Toggles.canBindShortcut && ctx_shortcut;
                bool remove = item.Toggles.isRemovable && ctx_remove;
                bool store = ctx_store &&
                    (currentContainer == null ||
                    (currentContainer != null && currentContainer.GetContainerInfo().IsStoreable));

                context.contexUse.gameObject.SetActive(use);
                context.contexCombine.gameObject.SetActive(combine);
                context.contexExamine.gameObject.SetActive(examine);
                context.contexDrop.gameObject.SetActive(drop);
                context.contexShortcut.gameObject.SetActive(shortcut);
                context.contexStore.gameObject.SetActive(store);
                context.contexRemovable.gameObject.SetActive(remove);

                for (int i = 0; i < panels.ContexPanel.transform.childCount; i++)
                {
                    if (panels.ContexPanel.transform.GetChild(i).gameObject.activeSelf)
                    {
                        InventoryContexts.Add(panels.ContexPanel.transform.GetChild(i).GetComponent<InventoryContex>());
                    }
                }

                if (use || combine || examine || drop || store || remove)
                {
                    panels.ContexPanel.SetActive(true);
                }
                else
                {
                    panels.ContexPanel.SetActive(false);
                    InventoryContexts.Clear();
                    selectedContex = 0;
                    isContexVisible = false;
                }
            }
            else
            {
                panels.ContexPanel.SetActive(false);
                context.contexUse.gameObject.SetActive(false);
                context.contexCombine.gameObject.SetActive(false);
                context.contexExamine.gameObject.SetActive(false);
                context.contexDrop.gameObject.SetActive(false);
                context.contexShortcut.gameObject.SetActive(false);
                context.contexStore.gameObject.SetActive(false);
                context.contexRemovable.gameObject.SetActive(false);
                InventoryContexts.Clear();
                selectedContex = 0;
                isContexVisible = false;
            }
        }

        /// <summary>
        /// Show timed UI Notification
        /// </summary>
        public void ShowNotification(string text)
        {
            content.InventoryNotification.transform.GetComponentInChildren<Text>().text = text;
            content.InventoryNotification.gameObject.SetActive(true);
            fadeNotification = true;

            fader.Fade(new Fader.FadeSettings()
            {
                startValue = content.InventoryNotification.color.a,
                endValue = 0.8f,
                fadeInSpeed = 1.2f,
                fadeOutSpeed = 3,
                fadeOutWait = 4,
                destroyFader = false
            });
        }

        /// <summary>
        /// Show fixed UI Notification (Bool Fade Out)
        /// </summary>
        public void ShowNotificationFixed(string text)
        {
            content.InventoryNotification.transform.GetComponentInChildren<Text>().text = text;
            content.InventoryNotification.gameObject.SetActive(true);
            fadeNotification = true;

            fader.Fade(new Fader.FadeSettings()
            {
                startValue = content.InventoryNotification.color.a,
                endValue = 0.8f,
                fadeInSpeed = 1.2f,
                fadeOutSpeed = 3,
                fadeOutAfterSignal = true,
                destroyFader = false
            });
        }

        [Serializable]
        public class ShortcutModel
        {
            public Item item;
            public int slot;
            public string shortcut;

            public ShortcutModel(Item item, int slot, string control)
            {
                this.item = item;
                this.slot = slot;
                shortcut = control;
            }
        }

        [Serializable]
        public struct SlotGrid
        {
            public int row;
            public int column;
            public int slotID;

            public SlotGrid(int row, int column, int id)
            {
                this.row = row;
                this.column = column;
                slotID = id;
            }
        }
    }

    public class InventoryItem
    {
        public Item item;
        public ItemData customData;

        public InventoryItem(Item item, ItemData data)
        {
            this.item = item;
            customData = data;
        }
    }

    [Serializable]
    public sealed class Item
    {
        public int ID;
        public string Title;
        public string Description;
        public ItemType ItemType;
        public ItemAction UseActionType;
        public Sprite ItemSprite;
        public ObjectReference ItemDrop;
        public ObjectReference ItemPackDrop;

        public InventoryScriptable.ItemMapper.Toggles Toggles;
        public InventoryScriptable.ItemMapper.Sounds Sounds;
        public InventoryScriptable.ItemMapper.Settings Settings;
        public InventoryScriptable.ItemMapper.CustomActionSettings CustomActions;
        public InventoryScriptable.ItemMapper.CombineSettings[] CombineSettings;

        public Item()
        {
            ID = 0;
        }

        public Item(int ItemID, InventoryScriptable.ItemMapper Mapper)
        {
            ID = ItemID;
            Title = Mapper.Title;
            Description = Mapper.Description;
            ItemType = Mapper.itemType;
            UseActionType = Mapper.useActionType;
            ItemSprite = Mapper.itemSprite;
            ItemDrop = Mapper.DropObject;
            ItemPackDrop = Mapper.PackDropObject;

            Toggles = Mapper.itemToggles;
            Sounds = Mapper.itemSounds;
            Settings = Mapper.itemSettings;
            CustomActions = Mapper.useActionSettings;
            CombineSettings = Mapper.combineSettings;
        }
    }
}