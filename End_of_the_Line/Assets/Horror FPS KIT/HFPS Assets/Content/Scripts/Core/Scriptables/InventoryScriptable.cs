using System;
using System.Collections.Generic;
using UnityEngine;

namespace HFPS.Systems
{
    public enum ItemType { Normal, Heal, ItemPart, Weapon, Bullets }
    public enum ItemAction { None, Increase, Decrease, ItemValue }

    public class InventoryScriptable : ScriptableObject
    {
        public List<ItemMapper> ItemDatabase = new List<ItemMapper>();
        public bool enableLocalization;

        [Serializable]
        public class ItemMapper
        {
            public string Title;
            public int ID;
            [Multiline] 
            public string Description;

            public ItemType itemType;
            public ItemAction useActionType = ItemAction.None;
            public Sprite itemSprite;

            [Tooltip("A model of the dropped item.")]
            public ObjectReference DropObject;
            [Tooltip("A model of the item, that contains multiple items.")]
            public ObjectReference PackDropObject;

            [Serializable]
            public class Toggles
            {
                [Tooltip("Item can be stored in one item depending on the MaxStackAmount.")]
                public bool isStackable;
                [Tooltip("Item can be used from inventory.")]
                public bool isUsable;
                [Tooltip("Item can be combined with the corresponding item to create new item, depending on the CombineSettings.")]
                public bool isCombinable;
                [Tooltip("Item can be dropped to the ground.")]
                public bool isDroppable;
                [Tooltip("Item can be removed from the inventory.")]
                public bool isRemovable;
                [Tooltip("The item Use action will restore player's Stamina.")]
                public bool restoreStamina;

                [Tooltip("Item can be examined from the inventory.")]
                public bool canExamine;
                [Tooltip("You can bind a shortcut key to an item, and use it by pressing a bound shortcut key.")]
                public bool canBindShortcut;

                [Tooltip("Combining two items creates a new item that is added to inventory depending on the CombineSettings.")]
                public bool combineAddItem;
                [Tooltip("Combining two items does not delete used items.")]
                public bool combineKeepItem;
                [Tooltip("Combining two items will selects an item from the ItemSwitcher, depending on the CombineSwitcherID.")]
                public bool combineShowItem;

                [Tooltip("The Item Use action will selects an item from the ItemSwitcher, depending on the UseSwitcherID.")]
                public bool showItemOnUse;
                [Tooltip("Show more information about the container item.")]
                public bool bagDescription;

                [Tooltip("The item Use action will triggers a Custom Action, depending on the UseActionSettings.")]
                public bool doActionUse;
                [Tooltip("The item Combine action will triggers a Custom Action, depending on the UseActionSettings.")]
                public bool doActionCombine;
            }
            public Toggles itemToggles = new Toggles();

            [Serializable]
            public sealed class Sounds
            {
                [Tooltip("The Sound of the Use Action. (If Any)")]
                public AudioClip useSound;
                [Range(0, 1f)] public float useVolume = 1f;

                [Tooltip("The Sound of the Combine Action. (If Any)")]
                public AudioClip combineSound;
                [Range(0, 1f)] public float combineVolume = 1f;
            }
            public Sounds itemSounds = new Sounds();

            [Serializable]
            public sealed class Settings
            {
                [Tooltip("How many items can be stored in one item.")]
                public int maxStackAmount;
                [Tooltip("The ID of the ItemSwitcher item. (-1 = default)")]
                public int useSwitcherID = -1;
                [Tooltip("The amount of restoring player's health.")]
                [Range(0, 100)]
                public int healAmount;
                [Tooltip("The amount of restoring player's stamina.")]
                [Range(0, 100)]
                public int staminaAmount;
                [Tooltip("Default Rotation of the examined item.")]
                public Vector3 examineRotation;
            }
            public Settings itemSettings = new Settings();

            [Serializable]
            public sealed class CustomActionSettings
            {
                [Tooltip("The value that triggers the specified action.")]
                public int triggerValue;
                [Tooltip("The ID of the specific item that will be added to the inventory when the specified value is reached.")]
                public int triggerItemID;
                [Tooltip("A string value that will be set to the added item.")]
                public string triggerCustomValue;

                [Tooltip("When a specific value is reached, the item will be removed from inventory.")]
                public bool actionRemove;
                [Tooltip("When a specific value is reached, the item is added to the inventory according to the TriggerItemID value.")]
                public bool actionAddItem;
                [Tooltip("When a specific value is reached, the Use action on the item will be restricted.")]
                public bool actionRestrictUse;
                [Tooltip("When a specific value is reached, the Combine action on the item will be restricted.")]
                public bool actionRestrictCombine;
            }
            public CustomActionSettings useActionSettings = new CustomActionSettings();

            [Serializable]
            public sealed class CombineSettings
            {
                [Tooltip("The second Item ID that can be used for combining.")]
                public int combineWithID;
                [Tooltip("The ID of the item that results from combining the two items.")]
                public int resultCombineID;
                [Tooltip("The ID of the ItemSwitcher item that results from combining the two items.")]
                public int combineSwitcherID;
            }
            public CombineSettings[] combineSettings;

            [Serializable]
            public sealed class LocalizationSettings
            {
                [Tooltip("This key will be used to identify the translation of the Item Title.")]
                public string titleKey;
                [Tooltip("This key will be used to identify the translation of the Item Description.")]
                public string descriptionKey;
            }
            public LocalizationSettings localizationSettings = new LocalizationSettings();

            public ItemMapper Clone()
            {
                return new ItemMapper()
                {
                    Title = Title,
                    ID = ID,
                    Description = Description,
                    itemType = itemType,
                    useActionType = useActionType,
                    itemSprite = itemSprite,
                    DropObject = DropObject,
                    PackDropObject = PackDropObject,

                    itemToggles = itemToggles,
                    itemSounds = itemSounds,
                    itemSettings = itemSettings,
                    useActionSettings = useActionSettings,
                    combineSettings = combineSettings,
                    localizationSettings = localizationSettings
                };
            }
        }
    }
}