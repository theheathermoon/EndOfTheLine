/*
 * InteractManager.cs - by ThunderWire Studio
 * ver. 2.0
*/

using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Utility;
using ThunderWire.Input;
using HFPS.Systems;
using HFPS.UI;

namespace HFPS.Player
{
    /// <summary>
    /// Main Interact Manager
    /// </summary>
    public class InteractManager : MonoBehaviour
    {
        private HFPS_GameManager gameManager;
        private ItemSwitcher itemSelector;
        private Inventory inventory;

        private Camera mainCamera;
        private DynamicObject dynamicObj;
        private InteractiveItem interactItem;
        private UIObjectInfo objectInfo;
        private DraggableObject dragRigidbody;

        [Header("Raycast")]
        public float RaycastRange = 3;
        public LayerMask cullLayers;
        public LayerMask interactLayers;

        [Header("Crosshair Textures")]
        public Sprite defaultCrosshair;
        public Sprite interactCrosshair;
        private Sprite default_interactCrosshair;

        [Header("Crosshair")]
        private Image CrosshairUI;
        public int crosshairSize = 5;
        public int interactSize = 10;

        #region Texts
        private string TakeText;
        private string UseText;
        private string GrabText;
        private string UnlockText;
        private string DragText;
        private string ExamineText;
        private string RemoveText;

        private string PickupHint;
        private string PickupMessage;
        private string BackpacksMax;
        private string CantTake;
        private string NoInventorySpace;
        #endregion

        #region Private Variables
        [HideInInspector] public bool isHeld = false;
        [HideInInspector] public bool inUse;
        [HideInInspector] public Ray playerAim;
        [HideInInspector] public GameObject RaycastObject;
        private GameObject LastRaycastObject;

        private int default_interactSize;
        private int default_crosshairSize;

        private string bp_Use;
        private string bp_Pickup;
        private bool UsePressed;

        private bool isPressed;
        private bool isCorrectLayer;
        #endregion

        void Awake()
        {
            inventory = Inventory.Instance;
            gameManager = HFPS_GameManager.Instance;
            mainCamera = ScriptManager.Instance.MainCamera;
            itemSelector = ScriptManager.Instance.C<ItemSwitcher>();

            CrosshairUI = gameManager.userInterface.Crosshair;
            default_interactCrosshair = interactCrosshair;
            default_crosshairSize = crosshairSize;
            default_interactSize = interactSize;
            RaycastObject = null;
            dynamicObj = null;

            if (TextsSource.HasReference) 
                TextsSource.Subscribe(OnInitTexts);
            else OnInitTexts();
        }

        private void OnInitTexts()
        {
            TakeText = TextsSource.GetText("Interact.Take", "Take");
            UseText = TextsSource.GetText("Interact.Use", "Use");
            GrabText = TextsSource.GetText("Interact.Grab", "Grab");
            UnlockText = TextsSource.GetText("Interact.Unlock", "Unlock");
            DragText = TextsSource.GetText("Interact.Drag", "Drag");
            ExamineText = TextsSource.GetText("Interact.Examine", "Examine");
            RemoveText = TextsSource.GetText("Interact.Remove", "Remove");
            PickupHint = TextsSource.GetText("Interact.PickupHint", "You took the");
            PickupMessage = TextsSource.GetText("Interact.PickupMessage", "Picked up");
            BackpacksMax = TextsSource.GetText("Interact.BackpacksMax", "You can't carry more backpacks!");
            CantTake = TextsSource.GetText("Interact.CantTake", "You can't take more");
            NoInventorySpace = TextsSource.GetText("Interact.NoInventorySpace", "No inventory space!");
        }

        void Update()
        {
            if (InputHandler.InputIsInitialized)
            {
                bp_Use = InputHandler.CompositeOf("Use").GetBindingPath();
                bp_Pickup = InputHandler.CompositeOf("Examine").GetBindingPath();
                UsePressed = InputHandler.ReadButtonOnce(this, "Use");
            }

            if (UsePressed && RaycastObject && !isPressed && !isHeld && !inUse && !gameManager.isWeaponZooming)
            {
                Interact(RaycastObject);
                isPressed = true;
            }

            if (!UsePressed && isPressed)
            {
                isPressed = false;
            }

            Ray playerAim = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(playerAim, out RaycastHit hit, RaycastRange, cullLayers))
            {
                if (interactLayers.CompareLayer(hit.collider.gameObject.layer) && !gameManager.isWeaponZooming)
                {
                    if (hit.collider.gameObject != RaycastObject)
                    {
                        gameManager.HideSprites(0);
                    }

                    RaycastObject = hit.collider.gameObject;
                    dragRigidbody = RaycastObject.GetComponent<DraggableObject>();
                    isCorrectLayer = true;

                    RaycastObject.HasComponent(out interactItem);
                    RaycastObject.HasComponent(out dynamicObj);
                    RaycastObject.HasComponent(out objectInfo);

                    if (RaycastObject.HasComponent(out CrosshairReticle reticle))
                    {
                        if (dynamicObj)
                        {
                            if (dynamicObj.useType != Type_Use.Locked)
                            {
                                interactCrosshair = reticle.interactSprite;
                                interactSize = reticle.size;
                            }
                        }
                        else
                        {
                            interactCrosshair = reticle.interactSprite;
                            interactSize = reticle.size;
                        }
                    }

                    if (LastRaycastObject)
                    {
                        if (!(LastRaycastObject == RaycastObject))
                        {
                            ResetCrosshair();
                        }
                    }
                    LastRaycastObject = RaycastObject;

                    if (objectInfo && !string.IsNullOrEmpty(objectInfo.ObjectTitle))
                    {
                        gameManager.ShowInteractInfo(objectInfo.ObjectTitle);
                    }

                    if (!inUse)
                    {
                        if (dynamicObj)
                        {
                            if (dynamicObj.useType == Type_Use.Locked && dynamicObj.CheckHasKey())
                            {
                                gameManager.ShowInteractSprite(1, UnlockText, bp_Use);
                            }
                            else
                            {
                                if (!objectInfo || (!objectInfo && objectInfo.overrideDoorText))
                                {
                                    if (dynamicObj.interactType == Type_Interact.Mouse)
                                    {
                                        gameManager.ShowInteractSprite(1, DragText, bp_Use);
                                    }
                                    else
                                    {
                                        gameManager.ShowInteractSprite(1, UseText, bp_Use);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(objectInfo.UseText))
                                        gameManager.ShowInteractSprite(1, objectInfo.UseText, bp_Use);
                                    else
                                        gameManager.ShowInteractSprite(1, UseText, bp_Use);
                                }
                            }
                        }
                        else
                        {
                            if (interactItem)
                            {
                                if (interactItem.interactShowTitle && !string.IsNullOrEmpty(interactItem.examineTitle))
                                {
                                    gameManager.ShowInteractInfo(interactItem.examineTitle);
                                }

                                if (!dragRigidbody || dragRigidbody && !dragRigidbody.dragAndUse)
                                {
                                    if (interactItem.itemType == InteractiveItem.ItemType.OnlyExamine)
                                    {
                                        gameManager.ShowInteractSprite(1, ExamineText, bp_Pickup);
                                    }
                                    else if (interactItem.itemType == InteractiveItem.ItemType.GenericItem)
                                    {
                                        if (interactItem.examineType != InteractiveItem.ExamineType.None)
                                        {
                                            gameManager.ShowInteractSprite(1, UseText, bp_Use);
                                            gameManager.ShowInteractSprite(2, ExamineText, bp_Pickup);
                                        }
                                        else
                                        {
                                            gameManager.ShowInteractSprite(1, UseText, bp_Use);
                                        }
                                    }
                                    else if (interactItem.examineType != InteractiveItem.ExamineType.None && interactItem.itemType != InteractiveItem.ItemType.GenericItem)
                                    {
                                        gameManager.ShowInteractSprite(1, TakeText, bp_Use);
                                        gameManager.ShowInteractSprite(2, ExamineText, bp_Pickup);
                                    }
                                    else if (interactItem.examineType == InteractiveItem.ExamineType.Paper)
                                    {
                                        gameManager.ShowInteractSprite(1, ExamineText, bp_Pickup);
                                    }
                                    else
                                    {
                                        gameManager.ShowInteractSprite(1, TakeText, bp_Use);
                                    }
                                }
                                else if (dragRigidbody && dragRigidbody.dragAndUse)
                                {
                                    if (interactItem.itemType != InteractiveItem.ItemType.OnlyExamine)
                                    {
                                        gameManager.ShowInteractSprite(1, TakeText, bp_Use);
                                        gameManager.ShowInteractSprite(2, GrabText, bp_Pickup);
                                    }
                                }
                            }
                            else if (RaycastObject.GetComponent<DynamicObjectPlank>())
                            {
                                gameManager.ShowInteractSprite(1, RemoveText, bp_Use);
                            }
                            else if (dragRigidbody)
                            {
                                if (!dragRigidbody.dragAndUse)
                                {
                                    gameManager.ShowInteractSprite(1, GrabText, bp_Pickup);
                                }
                                else if(objectInfo && !string.IsNullOrEmpty(objectInfo.UseText))
                                {
                                    gameManager.ShowInteractSprite(1, objectInfo.UseText, bp_Use);
                                    gameManager.ShowInteractSprite(2, GrabText, bp_Pickup);
                                }
                                else
                                {
                                    gameManager.ShowInteractSprite(1, UseText, bp_Use);
                                    gameManager.ShowInteractSprite(2, GrabText, bp_Pickup);
                                }
                            }
                            else if (objectInfo && !string.IsNullOrEmpty(objectInfo.UseText))
                            {
                                gameManager.ShowInteractSprite(1, objectInfo.UseText, bp_Use);
                            }
                            else
                            {
                                gameManager.ShowInteractSprite(1, UseText, bp_Use);
                            }
                        }
                    }

                    CrosshairChange(true);
                }
                else if (RaycastObject)
                {
                    isCorrectLayer = false;
                }
            }
            else if (RaycastObject)
            {
                isCorrectLayer = false;
            }

            if (!isCorrectLayer)
            {
                ResetCrosshair();
                CrosshairChange(false);
                gameManager.HideSprites(0);
                interactItem = null;
                RaycastObject = null;
                dynamicObj = null;
            }

            if (!RaycastObject)
            {
                gameManager.HideSprites(0);
                CrosshairChange(false);
                dynamicObj = null;
            }
        }

        void CrosshairChange(bool useTexture)
        {
            if (useTexture && CrosshairUI.sprite != interactCrosshair)
            {
                CrosshairUI.sprite = interactCrosshair;
                CrosshairUI.GetComponent<RectTransform>().sizeDelta = new Vector2(interactSize, interactSize);
            }
            else if (!useTexture && CrosshairUI.sprite != defaultCrosshair)
            {
                CrosshairUI.sprite = defaultCrosshair;
                CrosshairUI.GetComponent<RectTransform>().sizeDelta = new Vector2(crosshairSize, crosshairSize);
            }

            CrosshairUI.DisableSpriteOptimizations();
        }

        private void ResetCrosshair()
        {
            crosshairSize = default_crosshairSize;
            interactSize = default_interactSize;
            interactCrosshair = default_interactCrosshair;
        }

        public void CrosshairVisible(bool state)
        {
            switch (state)
            {
                case true:
                    CrosshairUI.enabled = true;
                    break;
                case false:
                    CrosshairUI.enabled = false;
                    break;
            }
        }

        public bool GetInteractBool()
        {
            if (RaycastObject)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Interact(GameObject InteractObject)
        {
            InteractiveItem interactiveItem = interactItem;

            if (!interactiveItem)
                InteractObject.HasComponent(out interactiveItem);

            if (interactiveItem && interactiveItem.itemType == InteractiveItem.ItemType.OnlyExamine) 
                return;

            if (InteractObject.HasComponent(out Message msg))
            {
                if (msg.messageType == Message.MessageType.Hint)
                {
                    gameManager.ShowHintPopup(msg.message, msg.messageTime);
                }
                else if (msg.messageType == Message.MessageType.PickupHint)
                {
                    gameManager.ShowHintPopup($"{PickupHint} {msg.message}", msg.messageTime);
                }
                else if (msg.messageType == Message.MessageType.Message)
                {
                    gameManager.ShowQuickMessage(msg.message, "");
                }
                else if (msg.messageType == Message.MessageType.ItemName)
                {
                    gameManager.ShowQuickMessage($"{PickupMessage} {msg.message}", "");
                }
            }

            if (interactiveItem)
            {
                Item item = new Item();
                bool showMessage = true;
                string autoShortcut = string.Empty;

                if (interactiveItem.itemType == InteractiveItem.ItemType.InventoryItem || interactiveItem.itemType == InteractiveItem.ItemType.SwitcherItem)
                    item = inventory.GetItem(interactiveItem.inventoryID);

                if (interactiveItem.itemType == InteractiveItem.ItemType.GenericItem)
                {
                    InteractEvent(InteractObject);
                }
                else if (interactiveItem.itemType == InteractiveItem.ItemType.BackpackExpand)
                {
                    if ((inventory.settings.SlotAmount + interactiveItem.invExpandAmount) > inventory.settings.MaxSlots)
                    {
                        gameManager.ShowQuickMessage(BackpacksMax, "", true);
                        return;
                    }

                    inventory.ExpandSlots(interactiveItem.invExpandAmount);
                    InteractEvent(InteractObject);
                }
                else if (interactiveItem.itemType == InteractiveItem.ItemType.InventoryItem)
                {
                    if (inventory.CheckInventorySpace() || inventory.CheckItemInventoryStack(interactiveItem.inventoryID))
                    {
                        if (inventory.GetItemAmount(item.ID) < item.Settings.maxStackAmount || item.Settings.maxStackAmount == 0)
                        {
                            autoShortcut = inventory.AddItem(interactiveItem.inventoryID, interactiveItem.itemAmount, interactiveItem.itemData, interactiveItem.autoShortcut);
                            InteractEvent(InteractObject);
                        }
                        else if (inventory.GetItemAmount(item.ID) >= item.Settings.maxStackAmount)
                        {
                            gameManager.ShowQuickMessage($"{CantTake} {item.Title}", "MaxItemCount");
                            showMessage = false;
                        }
                    }
                    else
                    {
                        gameManager.ShowQuickMessage(NoInventorySpace, "NoSpace");
                        showMessage = false;
                    }
                }
                else if (interactiveItem.itemType == InteractiveItem.ItemType.SwitcherItem)
                {
                    if (inventory.CheckInventorySpace() || inventory.CheckItemInventoryStack(interactiveItem.inventoryID))
                    {
                        if (inventory.GetItemAmount(item.ID) < item.Settings.maxStackAmount || item.Settings.maxStackAmount == 0)
                        {
                            autoShortcut = inventory.AddItem(interactiveItem.inventoryID, interactiveItem.itemAmount, null, interactiveItem.autoShortcut);

                            if (interactiveItem.autoSwitch)
                            {
                                itemSelector.SelectSwitcherItem(interactiveItem.switcherID);
                            }

                            if (item.ItemType == ItemType.Weapon)
                            {
                                itemSelector.weaponItem = interactiveItem.switcherID;
                            }

                            InteractEvent(InteractObject);
                        }
                        else if (inventory.GetItemAmount(item.ID) >= item.Settings.maxStackAmount)
                        {
                            gameManager.ShowQuickMessage($"{CantTake} {item.Title}", "MaxItemCount");
                            showMessage = false;
                        }
                    }
                    else
                    {
                        gameManager.ShowQuickMessage(NoInventorySpace, "NoSpace");
                        showMessage = false;
                    }
                }
                else if (interactiveItem.itemType == InteractiveItem.ItemType.GenericItem)
                {
                    InteractEvent(InteractObject);
                }

                if (showMessage)
                {
                    if (interactiveItem.messageType == InteractiveItem.MessageType.PickupHint)
                    {
                        foreach (var tip in interactiveItem.messageTips)
                        {
                            if (tip.InputAction.Equals("?"))
                            {
                                tip.InputAction = autoShortcut;
                                break;
                            }
                        }

                        if (interactiveItem.useDefaultHint) 
                            gameManager.ShowHintPopup($"{PickupHint} {interactiveItem.titleOrMsg.ToLower()}", interactiveItem.messageTime, interactiveItem.messageTips);
                        else
                            gameManager.ShowHintPopup(interactiveItem.titleOrMsg, interactiveItem.messageTime, interactiveItem.messageTips);
                    }
                    else if (interactiveItem.messageType == InteractiveItem.MessageType.Message)
                    {
                        gameManager.ShowQuickMessage(interactiveItem.titleOrMsg, "");
                    }
                    else if (interactiveItem.messageType == InteractiveItem.MessageType.ItemName)
                    {
                        gameManager.ShowQuickMessage($"{PickupMessage} {interactiveItem.titleOrMsg}", "");
                    }
                }
            }
            else
            {
                InteractEvent(InteractObject);
            }
        }

        void InteractEvent(GameObject InteractObject)
        {
            gameManager.HideSprites(0);
            InteractObject.SendMessage("UseObject", SendMessageOptions.DontRequireReceiver);
        }
    }
}