/*
 * CandleItem.cs - by ThunderWire Studio
 * ver. 1.0
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Utility;
using HFPS.Systems;
using HFPS.UI;

namespace HFPS.Player
{
    public class CandleItem : SwitcherBehaviour, ISaveableArmsItem, IItemValueProvider
    {
        private Inventory inventory;
        private Animation anim;

        [Header("Inventory")]
        [InventorySelector]
        public int CandleID;

        [Header("Candle Objects")]
        public AudioClip BlowOut;
        public GameObject Candle;
        public GameObject CandleFlame;
        public GameObject CandleLight;
        public Transform FlamePosition;

        [Header("Candle Inventory")]
        public int InventoryID;
        public bool blowOutKeepCandle;
        public float scaleKeepCandle;

        [Header("Candle Animations")]
        public GameObject CandleGO;
        public string DrawAnimation;
        public string HideAnimation;
        public string BlowOutAnimation;
        public string IdleAnimation;

        public float DrawSpeed = 1f;
        public float HideSpeed = 1f;

        [Header("Candle Settings")]
        public bool candleReduction;
        public float reductionRate;
        public float maxScale;
        public float minScale;

        private bool isSelected;
        private bool IsPressed;
        private bool IsBlocked;

        void Awake()
        {
            inventory = Inventory.Instance;
            anim = CandleGO.GetComponent<Animation>();
        }

        void Start()
        {
            if (isSelected)
            {
                OnSwitcherSelect();
            }
        }

        public override void OnSwitcherSelect()
        {
            if (!anim.isPlaying)
            {
                CandleGO.SetActive(true);
                anim.Play(DrawAnimation);
                CandleFlame.SetActive(true);
                CandleLight.SetActive(true);

                if (candleReduction)
                {
                    StartCoroutine(Scale());
                }

                StartCoroutine(OnSelect());
            }
        }

        IEnumerator OnSelect()
        {
            yield return new WaitUntil(() => !anim.isPlaying);
            isSelected = true;
        }

        public override void OnSwitcherDeselect()
        {
            if (CandleGO.activeSelf && !anim.isPlaying && isSelected)
            {
                StopAllCoroutines();
                StartCoroutine(BlowOutHide());
                IsPressed = true;
            }
        }

        public override void OnSwitcherActivate()
        {
            isSelected = true;
            CandleGO.SetActive(true);
            CandleFlame.SetActive(true);
            CandleLight.SetActive(true);
            anim.Play(IdleAnimation);

            if (candleReduction)
            {
                StartCoroutine(Scale());
            }
        }

        public override void OnSwitcherDeactivate()
        {
            StopAllCoroutines();
            isSelected = false;
            CandleFlame.SetActive(false);
            CandleLight.SetActive(false);
            CandleGO.SetActive(false);
        }

        IEnumerator BlowOutHide()
        {
            anim.Play(BlowOutAnimation);

            yield return new WaitUntil(() => !anim.isPlaying);

            IsPressed = true;
            isSelected = false;
        }

        public void OnItemBlock(bool blocked)
        {
            IsBlocked = blocked;
        }

        public void BlowOut_Event()
        {
            AudioSource.PlayClipAtPoint(BlowOut, Utilities.MainPlayerCamera().transform.position, 0.35f);
            CandleFlame.SetActive(false);
            CandleLight.SetActive(false);

            if (blowOutKeepCandle && Candle.transform.localScale.y > scaleKeepCandle)
            {
                if (inventory && inventory.CheckInventorySpace())
                {
                    inventory.AddItem(InventoryID, 1, new ItemData((Inventory.ITEM_VALUE, OnGetValue())));
                }
            }
        }

        void Update()
        {
            CandleFlame.transform.position = FlamePosition.position;

            if (IsPressed && !(anim.isPlaying))
            {
                CandleGO.SetActive(false);
                IsPressed = false;
            }
        }

        IEnumerator Scale()
        {
            while (minScale <= Candle.transform.localScale.y)
            {
                Vector3 temp = Candle.transform.localScale;
                temp.y -= temp.y * Time.deltaTime * reductionRate;
                Candle.transform.localScale = temp;
                yield return null;
            }

            FlameBurnOut();

            yield return new WaitForSeconds(1f);

            anim.Play(HideAnimation);
            GetItemSwitcher().DeselectItems();

            yield return new WaitUntil(() => !anim.isPlaying);

            IsPressed = true;
        }

        void FlameBurnOut()
        {
            CandleFlame.SetActive(false);
            CandleLight.SetActive(false);
        }

        public string OnGetValue()
        {
            return ((Candle.transform.localScale.y - minScale) * 100 / (maxScale - minScale)).ToString();
        }

        public void OnSetValue(string value)
        {
            Vector3 scale = Candle.transform.localScale;
            float percent = System.Convert.ToSingle(value);
            float val = (percent * (maxScale - minScale) / 100) + minScale;
            scale.y = val;
            Candle.transform.localScale = scale;
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>
            {
                {"candleScale", Candle.transform.localScale.y}
            };
        }

        public void OnLoad(Newtonsoft.Json.Linq.JToken token)
        {
            Vector3 scale = Candle.transform.localScale;
            scale.y = (float)token["candleScale"];
            Candle.transform.localScale = scale;
        }
    }
}