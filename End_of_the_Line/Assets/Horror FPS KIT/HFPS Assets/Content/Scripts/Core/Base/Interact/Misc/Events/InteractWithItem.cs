/*
 * InteractWithItem.cs - by ThunderWire Studio
 * ver. 1.0
*/

using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HFPS.Systems
{
    public class InteractWithItem : MonoBehaviour, ISaveable
    {
        private Inventory inventory;
        private HFPS_GameManager gameManager;

        [Header("Inventory")]
        public int itemID = -1;
        public int otherItemID = -1;
        public int otherItemAmount = 1;
        [Space(5)]
        public bool removeItem;
        public bool addOtherItemID;

        [Header("Messages")]
        public string hintMessage;
        public float messageTime = 3f;
        public bool showNoItemHint;

        [Header("Animation")]
        public Animation animObj;
        public string animName;
        public bool eventAfterAnim;
        public bool disableOnLoad;

        [Header("Sounds")]
        public AudioClip interactSound;
        public float interactVolume = 1f;

        [Header("Events")]
        public UnityEvent InteractEvent;

        private bool isInteracted;

        void Awake()
        {
            inventory = Inventory.Instance;
            gameManager = HFPS_GameManager.Instance;
        }

        public void UseObject()
        {
            if (!isInteracted && itemID >= 0)
            {
                if (inventory.CheckItemInventory(itemID))
                {
                    if (interactSound) AudioSource.PlayClipAtPoint(interactSound, transform.position, interactVolume);

                    if (animObj && !string.IsNullOrEmpty(animName))
                    {
                        animObj.Play(animName);

                        if (eventAfterAnim)
                        {
                            StartCoroutine(WaitAfterAnimation());
                        }
                        else
                        {
                            InteractEvent?.Invoke();
                            OnInventory();
                        }
                    }
                    else
                    {
                        InteractEvent?.Invoke();
                        OnInventory();
                    }

                    isInteracted = true;
                }
                else if (showNoItemHint)
                {
                    gameManager.ShowHintPopup(hintMessage, messageTime);
                }
            }
        }

        void OnInventory()
        {
            if (itemID >= 0 && removeItem)
            {
                inventory.RemoveItem(itemID);
            }

            if (otherItemID >= 0 && addOtherItemID)
            {
                inventory.AddItem(otherItemID, otherItemAmount);
            }
        }

        IEnumerator WaitAfterAnimation()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => !animObj.isPlaying);
            InteractEvent?.Invoke();
            OnInventory();
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>()
            {
                {"isInteracted", isInteracted}
            };
        }

        public void OnLoad(JToken token)
        {
            isInteracted = (bool)token["isInteracted"];

            if (isInteracted)
            {
                InteractEvent?.Invoke();

                if (disableOnLoad && animObj)
                {
                    animObj.gameObject.SetActive(false);
                }
            }
        }
    }
}