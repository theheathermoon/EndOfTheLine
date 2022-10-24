/*
 * JumpscareEffects.cs - by ThunderWire Studio
 * Version 2.0
*/

using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using HFPS.Systems;

namespace HFPS.Player
{
    /// <summary>
    /// Jumpscare Controller
    /// </summary>
    public class JumpscareEffects : MonoBehaviour
    {
        private PostProcessVolume postProcessing;
        private ChromaticAberration chromatic;
        private Vignette vignette;
        private ItemSwitcher itemSwitcher;
        private AudioSource PlayerBreath;

        [Header("Speed Settings")]
        public float scareEffectSpeed;
        public float chromaticOutSpeed;
        public float vignetteOutSpeed;

        private float lerpSpeed = 1f;
        private float defaultVolume;

        private bool isFeelingBetter;
        private bool enableEffects;

        private float chromaticMax;
        private float vigneteMax;

        void Start()
        {
            if (GetComponent<ScriptManager>().ArmsCamera.GetComponent<PostProcessVolume>())
            {
                postProcessing = GetComponent<ScriptManager>().ArmsCamera.GetComponent<PostProcessVolume>();

                if (postProcessing.profile.HasSettings<ChromaticAberration>())
                {
                    chromatic = postProcessing.profile.GetSetting<ChromaticAberration>();
                }
                else
                {
                    Debug.LogError($"[PostProcessing] Please add Chromatic Aberration Effect to a {postProcessing.profile.name} profile in order to use Jumpscare Effects!");
                }

                if (postProcessing.profile.HasSettings<Vignette>())
                {
                    vignette = postProcessing.profile.GetSetting<Vignette>();
                }
                else
                {
                    Debug.LogError($"[PostProcessing] Please add Vignette Effect to a {postProcessing.profile.name} profile in order to use Jumpscare Effects!");
                }
            }
            else
            {
                Debug.LogError($"[PostProcessing] There is no PostProcessVolume script added to a {GetComponent<ScriptManager>().ArmsCamera.gameObject.name}!");
            }

            itemSwitcher = GetComponentInChildren<ItemSwitcher>();

            PlayerBreath = PlayerController.Instance.transform.GetChild(1).transform.GetChild(0).GetComponent<AudioSource>();
            defaultVolume = PlayerBreath.volume;
        }

        void Update()
        {
            if (isFeelingBetter)
            {
                if (PlayerBreath.volume > 0.05f)
                {
                    PlayerBreath.volume = Mathf.MoveTowards(PlayerBreath.volume, 0f, lerpSpeed * Time.deltaTime);
                }
                else
                {
                    PlayerBreath.Stop();
                    isFeelingBetter = false;
                }
            }

            if (enableEffects)
            {
                if (chromatic.intensity.value <= chromaticMax)
                {
                    chromatic.intensity.value = Mathf.MoveTowards(chromatic.intensity.value, chromaticMax, scareEffectSpeed * Time.deltaTime);
                }

                if (vignette.intensity.value <= vigneteMax)
                {
                    vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, vigneteMax, scareEffectSpeed * Time.deltaTime);
                }
            }
            else
            {
                chromatic.intensity.value = Mathf.MoveTowards(chromatic.intensity.value, 0f, chromaticOutSpeed * Time.deltaTime);
                vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, 0f, vignetteOutSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Apply Scare and Settings
        /// </summary>
        public void Scare(CameraShakeInstance shakeInstance, float chromaticAmount, float vigneteAmount, float scaredBreath, float effectsTime = 5f, AudioClip scaredBreathSound = null)
        {
            CameraShaker.Instance.Shake(shakeInstance);

            chromaticMax = chromaticAmount;
            vigneteMax = vigneteAmount;

            if (itemSwitcher.currentItem != -1 && itemSwitcher.GetCurrentItem().GetComponent<FlashlightItem>())
            {
                itemSwitcher.GetCurrentItem().GetComponent<FlashlightItem>().Event_Scare();
            }

            if (scaredBreathSound != null)
            {
                PlayerBreath.clip = scaredBreathSound;
            }

            enableEffects = true;
            StartCoroutine(ScareBreath(scaredBreath));
            StartCoroutine(WaitEffects(effectsTime));
        }

        IEnumerator WaitEffects(float time)
        {
            yield return new WaitForSeconds(time);
            enableEffects = false;
        }

        IEnumerator ScareBreath(float time)
        {
            PlayerBreath.volume = defaultVolume;
            PlayerBreath.Play();
            yield return new WaitForSeconds(time);
            isFeelingBetter = true;
        }
    }
}