/*
 * LanternItem.cs - written by ThunderWire Studio
 * version 1.0
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HFPS.Systems;

namespace HFPS.Player
{
    public class LanternItem : SwitcherBehaviour, ISaveableArmsItem
    {
        public struct HingeData
        {
            public Vector3 Position;
            public Vector3 Rotation;
        }

        private HFPS_GameManager gameManager;
        private Animation anim;
        private AudioSource audioS;

        [Header("Inventory")]
        [InventorySelector]
        public int lanternID;

        [Header("Main")]
        public Sprite LanternIcon;
        public Light LanternLight;
        public string ColorString = "_Color";

        [Space]
        public float oilLifeInSec = 300f;
        public float oilPercentage = 100;
        public float lightReductionRate = 5f;
        public float canReloadPercent;
        public float hideIntensitySpeed;
        public float oilReloadSpeed;
        public float timeWaitToReload;

        [Header("Settings")]
        public bool useHingeJoint = false;
        public HingeJoint hingeLantern;
        [Tooltip("Difference after second lantern draw")]
        public float secondDrawDiff;

        private HingeData defaultHinge;
        private bool hingeDataSet = false;
        private bool limitsFixed = false;

        [Header("Animation")]
        public GameObject LanternGO;
        public string DrawAnim;
        [Range(0, 5)] public float DrawSpeed = 1f;
        public string HideAnim;
        [Range(0, 5)] public float HideSpeed = 1f;
        public string ReloadAnim;
        [Range(0, 5)] public float ReloadSpeed = 1f;
        public string IdleAnim;

        [Header("Sounds")]
        public AudioClip ShowSound;
        [Range(0, 1)] public float ShowVolume;
        public AudioClip HideSound;
        [Range(0, 1)] public float HideVolume;
        public AudioClip ReloadOilSound;
        [Range(0, 1)] public float ReloadVolume;

        private bool isSelected;
        private bool isSelecting;
        private bool isReloading;

        private float reductionFactor;
        private float reduceIntensity;
        private float oldIntensity;

        private float fullIntnesity;
        private float defaultOilPercentagle;

        private Color FlameTint;

        [HideInInspector]
        public bool CanReload;

        void Awake()
        {
            anim = LanternGO.GetComponent<Animation>();
            gameManager = HFPS_GameManager.Instance;

            if (LanternGO.GetComponent<AudioSource>())
            {
                audioS = LanternGO.GetComponent<AudioSource>();
            }

            defaultOilPercentagle = oilPercentage;
            fullIntnesity = LanternLight.intensity;
            oldIntensity = LanternLight.intensity;
            reduceIntensity = LanternLight.intensity;
            FlameTint = LanternLight.transform.GetChild(0).GetComponent<MeshRenderer>().material.GetColor(ColorString);
            FlameTint.a = 0f;
            LanternLight.intensity = 0f;
            reductionFactor = oilPercentage - lightReductionRate;
        }

        void Start()
        {
            anim[DrawAnim].speed = DrawSpeed;
            anim[HideAnim].speed = HideSpeed;
            anim[ReloadAnim].speed = ReloadSpeed;
        }

        public void Reload()
        {
            if (LanternGO.activeSelf)
            {
                if (oilPercentage < canReloadPercent && !isReloading)
                {
                    StartCoroutine(ReloadCorountine());
                    isReloading = true;
                }
            }
        }

        IEnumerator ReloadCorountine()
        {
            anim.Play(ReloadAnim);

            yield return new WaitForSeconds(timeWaitToReload);

            if (audioS && ReloadOilSound)
            {
                audioS.clip = ReloadOilSound;
                audioS.volume = ReloadVolume;
                audioS.Play();
            }

            while (LanternLight.intensity <= fullIntnesity)
            {
                LanternLight.intensity += Time.deltaTime * oilReloadSpeed;
                yield return null;
            }

            oilPercentage = defaultOilPercentagle;
            reductionFactor = oilPercentage - lightReductionRate;
            LanternLight.intensity = fullIntnesity;
            reduceIntensity = fullIntnesity;
            FlameTint.a = fullIntnesity;

            isReloading = false;
        }

        public override void OnSwitcherSelect()
        {
            if (isSelected || isSelecting || anim.isPlaying) return;

            isSelecting = true;

            if (hingeDataSet && useHingeJoint)
            {
                hingeLantern.transform.localPosition = defaultHinge.Position;
                hingeLantern.transform.localEulerAngles = defaultHinge.Rotation;

                if (!limitsFixed)
                {
                    JointLimits limits = hingeLantern.limits;
                    limits.min -= secondDrawDiff;
                    limits.max -= secondDrawDiff;
                    hingeLantern.limits = limits;
                    limitsFixed = true;
                }
            }

            LanternGO.SetActive(true);
            LanternLight.gameObject.SetActive(true);

            anim.Play(DrawAnim);

            if (audioS && ShowSound)
            {
                audioS.clip = ShowSound;
                audioS.volume = ShowVolume;
                audioS.Play();
            }

            gameManager.ShowLightUI(oilPercentage, true, LanternIcon);
            StartCoroutine(SelectCoroutine());
        }

        public override void OnSwitcherDeselect()
        {
            if (!isSelected && !isSelecting || anim.isPlaying) return;

            isSelecting = false;
            oldIntensity = LanternLight.intensity;

            if (audioS && HideSound)
            {
                audioS.clip = HideSound;
                audioS.volume = HideVolume;
                audioS.Play();
            }

            if (LanternGO.activeSelf)
            {
                gameManager.ShowLightUI(oilPercentage, false, LanternIcon);
                StartCoroutine(DeselectCoroutine());
            }
        }

        public override void OnSwitcherActivate()
        {
            LanternGO.SetActive(true);
            LanternLight.gameObject.SetActive(true);
            gameManager.ShowLightUI(oilPercentage, true, LanternIcon);
            anim.Play(IdleAnim);

            if (useHingeJoint && !hingeDataSet)
            {
                defaultHinge = new HingeData()
                {
                    Position = hingeLantern.transform.localPosition,
                    Rotation = hingeLantern.transform.localEulerAngles
                };

                hingeDataSet = true;
            }

            FlameTint.a = oldIntensity;
            LanternLight.intensity = oldIntensity;
            isSelected = true;
        }

        public override void OnSwitcherDeactivate()
        {
            LanternLight.intensity = 0f;
            gameManager.ShowLightUI(oilPercentage, false, LanternIcon);

            isSelecting = false;
            isSelected = false;

            LanternLight.gameObject.SetActive(false);
            LanternGO.SetActive(false);
        }

        IEnumerator SelectCoroutine()
        {
            if (useHingeJoint && !hingeDataSet)
            {
                defaultHinge = new HingeData()
                {
                    Position = hingeLantern.transform.localPosition,
                    Rotation = hingeLantern.transform.localEulerAngles
                };

                hingeDataSet = true;
            }

            while (LanternLight.intensity <= oldIntensity)
            {
                LanternLight.intensity += Time.deltaTime * hideIntensitySpeed;
                FlameTint.a += Time.deltaTime * hideIntensitySpeed;
                yield return null;
            }

            FlameTint.a = oldIntensity;
            LanternLight.intensity = oldIntensity;
            isSelected = true;
        }

        IEnumerator DeselectCoroutine()
        {
            anim.Play(HideAnim);

            while (LanternLight.intensity >= 0.01f)
            {
                LanternLight.intensity -= Time.deltaTime * hideIntensitySpeed;
                FlameTint.a -= Time.deltaTime * hideIntensitySpeed;
                yield return null;
            }

            LanternLight.intensity = 0f;

            yield return new WaitUntil(() => !anim.isPlaying);

            isSelected = false;
            LanternLight.gameObject.SetActive(false);
            LanternGO.SetActive(false);
        }

        void Update()
        {
            CanReload = (oilPercentage < canReloadPercent) && isSelected;

            if (isSelected)
            {
                if (oilPercentage > 0.1f)
                {
                    oilPercentage -= Time.deltaTime * (100 / oilLifeInSec);

                    if (oilPercentage <= reductionFactor)
                    {
                        reduceIntensity -= lightReductionRate / 100;
                        reductionFactor -= lightReductionRate;
                        StartCoroutine(Reduce());
                    }
                }
                else
                {
                    oilPercentage = 0;
                }

                gameManager.UpdateSliderValue(0, oilPercentage);
            }

            oilPercentage = Mathf.Clamp(oilPercentage, 0, 100);
            reductionFactor = Mathf.Clamp(reductionFactor, 0, 100);
            LanternLight.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor(ColorString, FlameTint);
        }

        IEnumerator Reduce()
        {
            while (LanternLight.intensity >= reduceIntensity)
            {
                LanternLight.intensity -= Time.deltaTime * 0.15f;
                FlameTint.a -= Time.deltaTime * 0.15f;
                yield return null;
            }

            LanternLight.intensity = (float)System.Math.Round(reduceIntensity, 2);
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>
            {
                {"lightPercentage", oilPercentage},
                {"lightIntensity", oldIntensity},
                {"flameAlpha", FlameTint.a}
            };
        }

        public void OnLoad(Newtonsoft.Json.Linq.JToken token)
        {
            float lp = (float)token["lightPercentage"];
            float li = (float)token["lightIntensity"];
            float fa = (float)token["flameAlpha"];

            oilPercentage = lp;
            oldIntensity = li;
            FlameTint.a = fa;

            reduceIntensity = li;
            reductionFactor = oilPercentage - lightReductionRate;
        }
    }
}