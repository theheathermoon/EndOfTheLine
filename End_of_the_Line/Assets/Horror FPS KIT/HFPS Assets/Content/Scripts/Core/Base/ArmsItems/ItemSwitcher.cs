/*
 * ItemSwitcher.cs - by ThunderWire Studio
 * ver. 1.0
*/

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ThunderWire.Utility;
using ThunderWire.Input;
using HFPS.Systems;

namespace HFPS.Player
{
    /// <summary>
    /// Main script for Switching Items.
    /// </summary>
    public class ItemSwitcher : MonoBehaviour
    {
        private Inventory inventory;
        private HFPS_GameManager gameManager;
        private ScriptManager scriptManager;
        private Camera mainCamera;

        public List<GameObject> ItemList = new List<GameObject>();
        public int currentItem = -1;

        [Header("Wall Detecting")]
        public LayerMask HitMask;
        public bool detectWall;
        public bool showGizmos;
        public float wallHitRange;

        [Header("Wall Hit Position")]
        public Transform WallHitTransform;
        public float WallHitSpeed = 5f;
        public Vector3 WallHitHideWeapon;
        public Vector3 WallHitShowWeapon;

        [Header("Item On Start")]
        public bool startWithCurrentItem;
        public bool startWithoutAnimation;

        [Tooltip("Item ID in the inventory database. Leave -1 unless it is an inventory item.")]
        public int startingItemID = -1;

        [HideInInspector]
        public int weaponItem = -1;

        private int newItem = 0;

        private bool hideWeapon;
        private bool handsFreed;
        private bool antiSpam;
        private bool spam;

        void Awake()
        {
            scriptManager = ScriptManager.Instance;
            inventory = Inventory.Instance;
            gameManager = HFPS_GameManager.Instance;
            mainCamera = ScriptManager.Instance.MainCamera;
        }

        IEnumerator Start()
        {
            if (startWithCurrentItem)
            {
                yield return new WaitUntil(() => InputHandler.InputIsInitialized);

                if (startingItemID > -1)
                {
                    inventory.AddItem(startingItemID, 1, null, true);
                }

                if (!startWithoutAnimation)
                {
                    SelectSwitcherItem(currentItem);
                }
                else
                {
                    ActivateItem(currentItem);
                }
            }
        }

        void FixedUpdate()
        {
            if (!inventory.CheckSwitcherItemInventory(weaponItem))
            {
                weaponItem = -1;
            }
        }

        public void SelectSwitcherItem(int switchID)
        {
            if (IsBusy()) return;

            if (switchID != currentItem)
            {
                newItem = switchID;

                if (IsItemsDeactivated())
                {
                    if (ItemList[newItem].GetComponent<SwitcherBehaviour>() != null)
                    {
                        ItemList[newItem].GetComponent<SwitcherBehaviour>().OnSwitcherSelect();
                        currentItem = newItem;
                    }
                    else
                    {
                        Debug.LogError("[Item Switcher] Object does not contains SwitcherBehaviour subcalss!");
                    }
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(SwitchItem());
                }
            }
            else
            {
                DeselectItems();
            }
        }

        public void DeselectItems()
        {
            if (currentItem == -1) return;

            ItemList[currentItem].GetComponent<SwitcherBehaviour>().OnSwitcherDeselect();
            StopAllCoroutines();
            StartCoroutine(DeselectWait());
        }

        IEnumerator DeselectWait()
        {
            yield return new WaitUntil(() => IsItemsDeactivated());
            currentItem = -1;
        }

        public void DisableItems()
        {
            if (currentItem == -1) return;

            ItemList[currentItem].GetComponent<SwitcherBehaviour>().OnSwitcherDeactivate();
            currentItem = -1;
        }

        public int GetIDByObject(GameObject switcherObject)
        {
            return ItemList.IndexOf(switcherObject);
        }

        public GameObject GetCurrentItem()
        {
            if (currentItem != -1)
            {
                return ItemList[currentItem];
            }

            return null;
        }

        public bool IsBusy()
        {
            return transform.root.gameObject.GetComponentInChildren<ExamineManager>().isExamining || transform.root.gameObject.GetComponentInChildren<DragRigidbody>().CheckHold();
        }

        bool IsItemsDeactivated()
        {
            return ItemList.All(x => !x.transform.GetChild(0).gameObject.activeSelf);
        }

        IEnumerator SwitchItem()
        {
            if (currentItem > -1 && newItem > -1)
            {
                ItemList[currentItem].GetComponent<SwitcherBehaviour>().OnSwitcherDeselect();

                yield return new WaitUntil(() => !ItemList[currentItem].transform.GetChild(0).gameObject.activeSelf);

                if (ItemList[newItem].GetComponent<SwitcherBehaviour>() != null)
                {
                    ItemList[newItem].GetComponent<SwitcherBehaviour>().OnSwitcherSelect();
                    currentItem = newItem;
                }
                else
                {
                    Debug.LogError("[Item Switcher] Object does not contains SwitcherBehaviour subcalss!");
                }

                yield return new WaitForSeconds(1f);
            }
        }

        void Update()
        {
            if (WallHitTransform && detectWall && !handsFreed && currentItem != -1)
            {
                if (OnWallHit())
                {
                    if (ItemList[currentItem].GetComponent<SwitcherBehaviour>() != null)
                    {
                        ItemList[currentItem].GetComponent<SwitcherBehaviour>().OnSwitcherWallHit(true);
                    }

                    hideWeapon = true;
                }
                else
                {
                    if (ItemList[currentItem].GetComponent<SwitcherBehaviour>() != null)
                    {
                        ItemList[currentItem].GetComponent<SwitcherBehaviour>().OnSwitcherWallHit(false);
                    }

                    hideWeapon = false;
                }
            }

            if (WallHitTransform)
            {
                if (!hideWeapon)
                {
                    WallHitTransform.localPosition = Vector3.MoveTowards(WallHitTransform.localPosition, WallHitShowWeapon, Time.deltaTime * WallHitSpeed);
                }
                else
                {
                    WallHitTransform.localPosition = Vector3.MoveTowards(WallHitTransform.localPosition, WallHitHideWeapon, Time.deltaTime * WallHitSpeed);
                }
            }

            if (!scriptManager.ScriptGlobalState) return;

            if (!gameManager.isGrabbed)
            {
                if (!antiSpam)
                {
                    Vector2 scroll = InputHandler.ReadInput<Vector2>("Scroll", "PlayerExtra");

                    //Mouse ScrollWheel Backward - Deselect Current Item
                    if (scroll.y < 0f)
                    {
                        if (currentItem != -1)
                        {
                            DeselectItems();
                        }
                    }

                    //Mouse ScrollWheel Forward - Select Last Weapon Item
                    if (scroll.y > 0f)
                    {
                        if (weaponItem != -1)
                        {
                            MouseWHSelectWeapon();
                        }
                    }
                }
                else
                {
                    if (!spam)
                    {
                        StartCoroutine(AntiSwitchSpam());
                    }
                }
            }
            else
            {
                antiSpam = true;
            }

            if (currentItem != -1)
            {
                ItemList[currentItem].GetComponent<SwitcherBehaviour>().OnSwitcherDisable(gameManager.isGrabbed);
            }
        }

        void MouseWHSelectWeapon()
        {
            if (currentItem != weaponItem)
            {
                if (ItemList[weaponItem].GetComponent<WeaponController>() && inventory.CheckSwitcherItemInventory(weaponItem))
                {
                    SelectSwitcherItem(weaponItem);
                }
            }
        }

        IEnumerator AntiSwitchSpam()
        {
            spam = true;
            antiSpam = true;
            yield return new WaitForSeconds(1f);
            antiSpam = false;
            spam = false;
        }

        /// <summary>
        /// Activate Item without playing animation.
        /// </summary>
        public void ActivateItem(int switchID)
        {
            ItemList[switchID].GetComponent<SwitcherBehaviour>().OnSwitcherActivate();
            currentItem = switchID;
            newItem = switchID;
        }

        public void FreeHands(bool free)
        {
            if (currentItem != -1)
            {
                if (free && !handsFreed)
                {
                    if (ItemList[currentItem].GetComponent<SwitcherBehaviour>() != null)
                    {
                        ItemList[currentItem].GetComponent<SwitcherBehaviour>().OnSwitcherWallHit(true);
                    }

                    hideWeapon = true;
                    handsFreed = true;
                }
                else if (!free && handsFreed)
                {
                    if (ItemList[currentItem].GetComponent<SwitcherBehaviour>() != null)
                    {
                        ItemList[currentItem].GetComponent<SwitcherBehaviour>().OnSwitcherWallHit(false);
                    }

                    hideWeapon = false;
                    handsFreed = false;
                }
            }
        }

        private bool OnWallHit()
        {
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.TransformDirection(Vector3.forward), out RaycastHit hit, wallHitRange, HitMask))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (detectWall && showGizmos)
            {
                Camera cam;

                if ((cam = Utilities.MainPlayerCamera()) != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward * wallHitRange));
                }
            }
        }
    }
}