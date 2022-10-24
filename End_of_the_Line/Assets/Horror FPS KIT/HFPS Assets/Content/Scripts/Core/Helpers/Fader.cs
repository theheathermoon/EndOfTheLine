using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using ThunderWire.Utility;

namespace ThunderWire.Helpers
{
    public class Fader : MonoBehaviour
    {
        public enum DestroyType { None, Disable, Destroy }

        public sealed class FadeSettings
        {
            public Color startColor;
            public Color endColor;

            public float startValue = 0f;
            public float endValue = 0f;

            public float fadeInSpeed = 1.2f;
            public float fadeOutSpeed = 2.5f;
            public float fadeOutWait = 0f;

            public bool fadeOutAfterSignal = false;
            public bool colorFade = false;
            public bool oneWayFade = false;

            public bool destroyFader = true;
            public GameObject actionObj;
            public DestroyType fadedOutAction = DestroyType.None;
        }

        public event Action<float> OnFade;
        public event Action<Color> OnColorFade;
        public event Action OnFadeCompleted;

        public bool IsFadedIn { get; private set; }
        public bool IsFadedOut { get; private set; }
        public Color FadingColor { get; private set; }
        public float FadingValue { get; private set; }

        private GameObject FadingObj;
        private bool fadeOut;

        /// <summary>
        /// Create or Find Fading Instance
        /// </summary>
        public static Fader Instance(GameObject FadingObj, string name = "[Fader]", HideFlags hideFlags = HideFlags.HideInHierarchy)
        {
            if (FindObjectsOfType<Fader>().Count(x => x.FadingObj == FadingObj) < 1)
            {
                GameObject obj = new GameObject(name)
                {
                    hideFlags = hideFlags
                };

                Fader fader = obj.AddComponent<Fader>();
                fader.FadingObj = FadingObj;
                return fader;
            }
            else
            {
                return FindObjectsOfType<Fader>().FirstOrDefault(x => x.FadingObj == FadingObj);
            }
        }

        /// <summary>
        /// Start Fading Sequence
        /// </summary>
        /// <param name="fadeSettings">Settings which specifies fading.</param>
        /// <param name="colorFade">Should we fade color?</param>
        public void Fade(FadeSettings fadeSettings)
        {
            if (fadeSettings.colorFade)
                FadingColor = fadeSettings.startColor;
            else
                FadingValue = fadeSettings.startValue;

            fadeOut = false;
            IsFadedOut = false;
            IsFadedIn = false;

            StopAllCoroutines();
            StartCoroutine(StartFade(fadeSettings));
        }

        /// <summary>
        /// Send fade out signal.
        /// </summary>
        public void FadeOutSignal()
        {
            fadeOut = true;
        }

        public IEnumerator StartFade(FadeSettings fadeSettings)
        {
            if (!fadeSettings.destroyFader)
            {
                gameObject.SetActive(true);
            }

            if (!FadingObj.activeSelf)
                FadingObj.SetActive(true);

            if (fadeSettings.colorFade)
            {
                while (!FadingColor.Equals(fadeSettings.endColor))
                {
                    FadingColor = FadingColor.MoveTowards(fadeSettings.endColor, Time.unscaledDeltaTime * fadeSettings.fadeInSpeed);
                    OnColorFade?.Invoke(FadingColor);
                    yield return null;
                }

                IsFadedIn = true;

                if (!fadeSettings.oneWayFade)
                {
                    if (fadeSettings.fadeOutAfterSignal)
                    {
                        yield return new WaitUntil(() => fadeOut);
                    }
                    else if (fadeSettings.fadeOutWait > 0)
                    {
                        yield return new WaitForSecondsRealtime(fadeSettings.fadeOutWait);
                    }

                    while (!FadingColor.Equals(fadeSettings.startColor))
                    {
                        FadingColor = FadingColor.MoveTowards(fadeSettings.startColor, Time.unscaledDeltaTime * fadeSettings.fadeInSpeed);
                        OnColorFade?.Invoke(FadingColor);
                        yield return null;
                    }

                    IsFadedOut = true;
                }
            }
            else
            {
                while (!Mathf.Approximately(FadingValue, fadeSettings.endValue))
                {
                    FadingValue = Mathf.MoveTowards(FadingValue, fadeSettings.endValue, Time.unscaledDeltaTime * fadeSettings.fadeInSpeed);
                    OnFade?.Invoke(FadingValue);
                    yield return null;
                }

                IsFadedIn = true;

                if (!fadeSettings.oneWayFade)
                {
                    if (fadeSettings.fadeOutAfterSignal)
                    {
                        yield return new WaitUntil(() => fadeOut);
                    }
                    else if (fadeSettings.fadeOutWait > 0)
                    {
                        yield return new WaitForSecondsRealtime(fadeSettings.fadeOutWait);
                    }

                    while (!Mathf.Approximately(FadingValue, fadeSettings.startValue))
                    {
                        FadingValue = Mathf.MoveTowards(FadingValue, fadeSettings.startValue, Time.unscaledDeltaTime * fadeSettings.fadeOutSpeed);
                        OnFade?.Invoke(FadingValue);
                        yield return null;
                    }

                    IsFadedOut = true;
                }
            }

            OnFadeCompleted?.Invoke();

            if (fadeSettings.fadedOutAction == DestroyType.Destroy)
            {
                if(fadeSettings.actionObj)
                    Destroy(fadeSettings.actionObj);
            }
            else if (fadeSettings.fadedOutAction == DestroyType.Disable)
            {
                if (fadeSettings.actionObj)
                    fadeSettings.actionObj.SetActive(false);
            }

            if (fadeSettings.destroyFader)
                Destroy(gameObject);
        }
    }
}