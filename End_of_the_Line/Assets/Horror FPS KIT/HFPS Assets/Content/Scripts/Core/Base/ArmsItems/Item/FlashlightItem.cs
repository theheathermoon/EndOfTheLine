/*
 * FlashlightItem.cs - by ThunderWire Studio
 * version 1.4
*/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using HFPS.Systems;

namespace HFPS.Player
{
    /// <summary>
    /// Script for Flashlight Controls
    /// </summary>
    public class FlashlightItem : SwitcherBehaviour, ISaveableArmsItem
    {
        private HFPS_GameManager gameManager;
        private Animation AnimationComp;

        [Header("Inventory")]
        [InventorySelector]
        public int FlashlightID;

        [Header("Setup")]
        public Sprite FlashlightIcon;
        public Light LightObject;
        public AudioSource audioSource;

        [Header("Flashlight Settings")]
        [ReadOnly, SerializeField] 
        private float batteryPercentage = 100;
        [ReadOnly, SerializeField]
        private float m_flashlightIntensity;
        [Space]

        public AudioClip ClickSound;
        public bool InfiniteBattery = false;
        public float flashlightIntensity = 1f;
        public float batteryLifeInSec = 300f;
        public float canReloadPercent;

        [Header("Animation")]
        public GameObject FlashlightGO;
        public string DrawAnim;
        [Range(0, 5)] public float DrawSpeed = 1f;
        public string HideAnim;
        [Range(0, 5)] public float HideSpeed = 1f;
        public string ReloadAnim;
        [Range(0, 5)] public float ReloadSpeed = 1f;
        public string IdleAnim;

        [Header("Extra Animations")]
        public bool enableExtra = true;
        public string ScareAnim;
        [Range(0, 5)] public float ScareAnimSpeed = 1f;
        public string NoPowerAnim;
        [Range(0, 5)] public float NoPowerAnimSpeed = 1f;

        [HideInInspector]
        public bool CanReload;

        private bool isOn;
        private bool isSelected;
        private bool isReloading;
        private bool noPower;
        private bool scare;

        void Awake()
        {
            gameManager = HFPS_GameManager.Instance;
            AnimationComp = FlashlightGO.GetComponent<Animation>();
            FlashlightGO.SetActive(false);
            LightObject.intensity = flashlightIntensity;
        }

        void Start()
        {
            AnimationComp[DrawAnim].speed = DrawSpeed;
            AnimationComp[HideAnim].speed = HideSpeed;
            AnimationComp[ReloadAnim].speed = ReloadSpeed;

            if (enableExtra)
            {
                AnimationComp[ScareAnim].speed = ScareAnimSpeed;
                AnimationComp[NoPowerAnim].speed = NoPowerAnimSpeed;
            }
        }

        public override void OnSwitcherSelect()
        {
            if (AnimationComp.isPlaying || isSelected) return;
            if (!InfiniteBattery) gameManager.ShowLightUI(batteryPercentage, true, FlashlightIcon);
            FlashlightGO.SetActive(true);
            AnimationComp.Play(DrawAnim);
            StartCoroutine(SelectCoroutine());
        }

        IEnumerator SelectCoroutine()
        {
            yield return new WaitUntil(() => !AnimationComp.isPlaying);
            isSelected = true;
        }

        public override void OnSwitcherDeselect()
        {
            if (AnimationComp.isPlaying || !isSelected) return;

            if (FlashlightGO.activeSelf && !isReloading)
            {
                if (!InfiniteBattery) gameManager.ShowLightUI(batteryPercentage, false, FlashlightIcon);
                StartCoroutine(DeselectCorountine());
            }
        }

        IEnumerator DeselectCorountine()
        {
            AnimationComp.Play(HideAnim);

            isOn = false;
            if (ClickSound)
            {
                audioSource.clip = ClickSound;
                audioSource.Play();
            }

            yield return new WaitUntil(() => !AnimationComp.isPlaying);

            FlashlightGO.SetActive(false);
            isSelected = false;
        }

        public override void OnSwitcherActivate()
        {
            if (!InfiniteBattery) gameManager.ShowLightUI(batteryPercentage, true, FlashlightIcon);
            FlashlightGO.SetActive(true);
            AnimationComp.Play(IdleAnim);
            isSelected = true;
            isOn = true;
        }

        public override void OnSwitcherDeactivate()
        {
            if (!InfiniteBattery) gameManager.ShowLightUI(batteryPercentage, false, FlashlightIcon);
            FlashlightGO.SetActive(false);
            isSelected = false;
            isOn = false;
        }

        void Update()
        {
            CanReload = (batteryPercentage < canReloadPercent) && isSelected;

            if (scare && !AnimationComp.isPlaying)
            {
                scare = false;
            }

            if (isSelected && !noPower && !scare)
            {
                gameManager.UpdateSliderValue(0, batteryPercentage);
            }

            if (isOn)
            {
                LightObject.enabled = true;

                if (!InfiniteBattery)
                {
                    batteryPercentage -= Time.deltaTime * (100 / batteryLifeInSec);
                    batteryPercentage = Mathf.Clamp(batteryPercentage, 0, 100);

                    m_flashlightIntensity = flashlightIntensity * batteryPercentage / 100;
                    LightObject.intensity = m_flashlightIntensity;

                    if (batteryPercentage <= 1.0f)
                    {
                        if (!AnimationComp.isPlaying && !noPower)
                        {
                            LightObject.intensity = 0f;
                            if (enableExtra) AnimationComp.Play(NoPowerAnim);
                            noPower = true;
                        }
                    }
                }
            }
            else
            {
                LightObject.enabled = false;
            }
        }

        public void Reload()
        {
            if (FlashlightGO.activeSelf)
            {
                if (batteryPercentage < canReloadPercent)
                {
                    StartCoroutine(ReloadCorountine());
                    isReloading = true;
                }
            }
        }

        IEnumerator ReloadCorountine()
        {
            AnimationComp.Play(ReloadAnim);

            isOn = false;
            if (ClickSound)
            {
                audioSource.clip = ClickSound;
                audioSource.Play();
            }

            yield return new WaitUntil(() => !AnimationComp.isPlaying);

            batteryPercentage = 100;
            noPower = false;
            isReloading = false;
        }

        public void Event_FlashlightOn()
        {
            isOn = !isOn;
            if (ClickSound)
            {
                audioSource.clip = ClickSound;
                audioSource.Play();
            }
        }

        public void Event_Scare()
        {
            if (enableExtra)
            {
                AnimationComp.Play(ScareAnim);
                scare = true;
            }
        }

        public Dictionary<string, object> OnSave()
        {
            if (!InfiniteBattery)
            {
                return new Dictionary<string, object>
                {
                    {"batteryPercentage", batteryPercentage}
                };
            }

            return null;
        }

        public void OnLoad(Newtonsoft.Json.Linq.JToken token)
        {
            if (!InfiniteBattery)
            {
                batteryPercentage = (float)token["batteryPercentage"];
            }
        }
    }
}