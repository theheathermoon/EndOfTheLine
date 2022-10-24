using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using ThunderWire.Utility;
using ThunderWire.Helpers;
using HFPS.UI;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    public class InteractiveItem : MonoBehaviour, ISaveable
    {
        #region Structures
        [Serializable]
        public sealed class MessageTip
        {
            public string InputAction;
            public string Message;
            public string MessageKey;
            public bool TextUppercased;
        }

        public enum ItemType { OnlyExamine, GenericItem, InventoryItem, SwitcherItem, BackpackExpand }
        public enum ExamineType { None, Object, AdvancedObject, Paper }
        public enum ExamineRotate { None, Horizontal, Vertical, Both }
        public enum MessageType { None, PickupHint, Message, ItemName }
        public enum DisableType { None, DisableRenderer, DisableObject, Destroy }
        #endregion

        public ItemData itemData;
        public string itemTitle;

        public ItemType itemType = ItemType.GenericItem;
        public ExamineType examineType = ExamineType.None;
        public ExamineRotate examineRotate = ExamineRotate.None;
        public MessageType messageType = MessageType.None;
        public DisableType disableType = DisableType.None;

        [Tooltip("Message Tips that appear when you pick up an object. Note that only the first message with the action identifier \"?\" will be changed depending on the item shortcut.")]
        public MessageTip[] messageTips;

        [Tooltip("Title or Message that appears when you pickup the object.")]
        public string titleOrMsg;
        [Tooltip("Title that will be displayed during the object examination.")]
        public string examineTitle;
        [Tooltip("The text that appears when you press the read key during object examination.")]
        [Multiline] public string paperText;

        [Tooltip("Title or Message localization key.")]
        public string titleMsgKey;
        [Tooltip("Examination title localization key.")]
        public string examineKey;

        [Tooltip("Object Inventory Item ID.")]
        public int inventoryID;
        [Tooltip("Item Switcher Element ID")]
        public int switcherID;

        [Tooltip("Inventory slots expand amount.")]
        public int invExpandAmount = 5;
        [Tooltip("Inventory item pickup amount.")]
        public int itemAmount = 1;
        [Tooltip("Paper read text font size.")]
        public int paperFontSize = 15;
        [Tooltip("The minimum and maximum distance at which the object can be zoomed.")]
        public MinMaxValue examineDistance = new MinMaxValue(0.3f, 0.5f, 0.3f);
        [Tooltip("The time that represents how long the pop-up message will be displayed.")]
        public float messageTime = 3f;

        [Tooltip("Would you rather like to use an inventory title?")]
        public bool useInvTitle;
        [Tooltip("Would you rather like to use an item title?")]
        public bool useItemTitle;
        [Tooltip("Show hint with a predefined message before the item title?")]
        public bool useDefaultHint = true;
        [Tooltip("Automatically bind shortcut key to an inventory item. With \"?\" you can automatically set the button action in message tips when a shortcut is bound.")]
        public bool autoShortcut;
        [Tooltip("Automatically select an ItemSwitcher Item when you pick up an object.")]
        public bool autoSwitch;
        [Tooltip("Display the title of the item while hovering the crosshair on the object.")]
        public bool interactShowTitle;
        [Tooltip("Allow the object to be taken from the examination.")]
        public bool allowExamineTake = true;

        [Tooltip("Enable floating icon over object.")]
        public bool floatingIcon = true;
        [Tooltip("Enable object to face the camera.")]
        public bool faceCamera;

        [Tooltip("Take the second object during the first object examination.")]
        public bool mouseClickPickup;
        [Tooltip("Enable cursor to be shown.")]
        public bool enableCursor;

        public AudioClip pickupSound;
        public float pickupVolume = 1f;
        public AudioClip examineSound;
        public float examineVolume = 1f;

        [Tooltip("Object face to camera rotation.")]
        public Vector3 faceRotation;
        [Tooltip("Inventory item custom data.")]
        public ItemDataPair[] itemCustomData;
        [Tooltip("Colliders that will be disabled during the object examination.")]
        public Collider[] collidersDisable;
        [Tooltip("Colliders that will be enabled during the object examination.")]
        public Collider[] collidersEnable;
        [Tooltip("Interactive Items that can be picked during the examination.")]
        public InteractiveItem[] allowedInteracts;

        public bool isExamining;
        public bool isChild;

        public bool isExamined;
        public bool floatingIconState = true;
        public Vector3 lastFloorPosition;

        private AudioSource audioSource;
        private string objectParentPath;

        void Awake()
        {
            if (Inventory.HasReference && (itemType == ItemType.InventoryItem || itemType == ItemType.SwitcherItem))
            {
                Inventory.Subscribe(OnInventoryItemUpdate);
            }
            else if (examineType != ExamineType.None)
            {
                if (useItemTitle) examineTitle = titleOrMsg;
            }

#if TW_LOCALIZATION_PRESENT
            if (HFPS_GameManager.LocalizationEnabled)
            {
                List<string> keys = new List<string>();

                if (!useInvTitle)
                    keys.Add(titleMsgKey);

                if(!useItemTitle)
                    keys.Add(examineKey);

                if (messageTips.Length > 0)
                {
                    foreach (var tip in messageTips)
                    {
                        keys.Add(tip.MessageKey);
                    }
                }

                LocalizationSystem.SubscribeAndGet(OnLocalizationUpdate, keys.ToArray());
            }
#endif

            foreach (var item in allowedInteracts)
            {
                item.isChild = true;
            }

            if (ScriptManager.HasReference)
                audioSource = ScriptManager.Instance.SoundEffects;

            if (itemData == null)
                SetItemCustomData(itemCustomData);
        }

        public void OnLocalizationUpdate(string[] trs)
        {
            int index = 0;

            if (!useInvTitle)
            {
                titleOrMsg = trs[index];
                index++;
            }

            if (examineType == ExamineType.Paper)
            {
                paperText = trs[index];
                index++;
            }
            else
            {
                if (!useItemTitle)
                {
                    examineTitle = trs[index];
                    index++;
                }
                else
                {
                    examineTitle = titleOrMsg;
                }
            }

            if (messageTips.Length > 0)
            {
                foreach (var tip in messageTips)
                {
                    tip.Message = tip.TextUppercased ? trs[index].ToUpper() : trs[index];
                    index++;
                }
            }
        }

        public void OnInventoryItemUpdate()
        {
            Item item = Inventory.Instance.GetItem(inventoryID);

            if (item != null)
            {
                titleOrMsg = item.Title;
                if (useItemTitle) examineTitle = item.Title;
            }
        }

        void FixedUpdate()
        {
            if (itemData != null && itemType == ItemType.InventoryItem || itemType == ItemType.SwitcherItem)
            {
                string newPath = gameObject.GameObjectPath();

                if (objectParentPath != newPath)
                {
                    objectParentPath = newPath;
                    if (itemData.Exist("object_path"))
                        itemData.data["object_path"] = newPath;
                }
            }
        }

        public void SetItemCustomData(ItemDataPair[] newItemData)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();

            if (newItemData.Length > 0)
            {
                foreach (var dat in newItemData)
                {
                    data.Add(dat.Key, dat.Value);
                }
            }

            if (itemType == ItemType.InventoryItem || itemType == ItemType.SwitcherItem)
            {
                objectParentPath = gameObject.GameObjectPath();
                data.Add("object_path", objectParentPath);
                data.Add("object_scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }

            itemData = new ItemData(data);
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider.isTrigger && !collision.collider.CompareTag("Player"))
            {
                lastFloorPosition = transform.position;
            }
        }

        public void UseObject()
        {
            if (itemType == ItemType.OnlyExamine) return;

            if (pickupSound)
            {
                audioSource.clip = pickupSound;
                audioSource.volume = pickupVolume;
                audioSource.Play();
            }

            if (GetComponent<ItemEvent>())
            {
                GetComponent<ItemEvent>().OnItemEvent();
            }

            if (GetComponent<TriggerObjective>())
            {
                GetComponent<TriggerObjective>().OnTrigger();
            }

            SaveGameHandler.Instance.RemoveSaveableObject(gameObject, false, false);

            if (disableType == DisableType.DisableRenderer)
            {
                DisableObject();
            }
            else if (disableType == DisableType.DisableObject)
            {
                gameObject.SetActive(false);
            }
            else if (disableType == DisableType.Destroy)
            {
                FloatingIconManager.Instance.DestroySafely(gameObject);
            }
        }

        public void DisableObject()
        {
            if (GetComponent<Rigidbody>())
            {
                GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
                GetComponent<Rigidbody>().useGravity = false;
                GetComponent<Rigidbody>().isKinematic = true;
            }

            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;

            if (transform.childCount > 0)
            {
                foreach (Transform child in transform.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        public void EnableObject()
        {
            if (GetComponent<Rigidbody>())
            {
                GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
                GetComponent<Rigidbody>().useGravity = true;
                GetComponent<Rigidbody>().isKinematic = false;
            }

            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Collider>().enabled = true;

            if (itemType == ItemType.InventoryItem)
            {
                if (transform.childCount > 0)
                {
                    foreach (Transform child in transform.transform)
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
        }

        public Dictionary<string, object> OnSave()
        {
            if (GetComponent<MeshRenderer>())
            {
                bool disableState = true;

                if (disableType == DisableType.DisableRenderer)
                {
                    disableState = GetComponent<MeshRenderer>().enabled;
                }
                else if (disableType == DisableType.DisableObject)
                {
                    disableState = gameObject.activeSelf;
                }

                Vector3 position = transform.position;
                Vector3 rotation = transform.eulerAngles;

                Dictionary<string, object> saveData = new Dictionary<string, object>()
                {
                    { "inv_id", inventoryID },
                    { "inv_amount", itemAmount },
                    { "weapon_id", switcherID },
                    { "examined", isExamined },
                    { "customData", itemData },
                    { "stateDisable", disableState },
                    { "floatingIcon", floatingIconState }
                };

                if (!isExamining && !isChild)
                {
                    List<KeyValuePair<string, object>> dictList = saveData.ToList();
                    dictList.Insert(0, new KeyValuePair<string, object>("position", transform.position));
                    dictList.Insert(1, new KeyValuePair<string, object>("rotation", transform.eulerAngles));
                    saveData = dictList.ToDictionary(x => x.Key, x => x.Value);
                }

                return saveData;
            }

            return null;
        }

        public void OnLoad(JToken token)
        {
            if (token["position"] != null && token["rotation"] != null)
            {
                transform.position = token["position"].ToObject<Vector3>();
                transform.eulerAngles = token["rotation"].ToObject<Vector3>();
            }

            inventoryID = (int)token["inv_id"];
            itemAmount = (int)token["inv_amount"];
            switcherID = (int)token["weapon_id"];
            isExamined = (bool)token["examined"];
            floatingIconState = (bool)token["floatingIcon"];
            itemData = token["customData"].ToObject<ItemData>();

            if (disableType == DisableType.DisableRenderer)
            {
                bool state = token["stateDisable"].ToObject<bool>();

                if (!state)
                {
                    DisableObject();
                }
                else
                {
                    EnableObject();
                }
            }
            else if (disableType == DisableType.DisableObject)
            {
                gameObject.SetActive(token["stateDisable"].ToObject<bool>());
            }

            OnInventoryItemUpdate();
        }
    }

    [Serializable]
    public sealed class ItemDataPair
    {
        public string Key;
        public string Value;

        public ItemDataPair(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}