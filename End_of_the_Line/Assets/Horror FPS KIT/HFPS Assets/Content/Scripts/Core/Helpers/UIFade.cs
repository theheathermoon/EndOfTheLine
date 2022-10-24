using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderWire.Helpers
{
    public class UIFade : MonoBehaviour
    {
        public class FadeValue
        {
            public GameObject Obj;
            public float Value;

            public FadeValue(GameObject obj, float value)
            {
                Obj = obj;
                Value = value;
            }
        }

        public enum FadeOutAfter { Bool, Time }
        public enum DisableTypeAfter { Disable, Destroy, BothDestroy }
        public bool fadeOut;

        public event System.Action OnFadeInEvent;
        public event System.Action OnFadeOutEvent;
        public event System.Action OnFadeCompleted;

        private GameObject FadeObj;
        private FadeValue[] fadeValues = null;
        private FadeOutAfter outAfter = FadeOutAfter.Bool;
        private DisableTypeAfter disableType = DisableTypeAfter.Disable;
        private Color m_fadeColor = new Color(1, 1, 1, 0);
        private float m_Alpha = 0;

        private float m_fadeTo;
        private float m_fadeInSpeed;
        private float m_fadeOutSpeed;
        private float m_fadeOutTime;
        private float m_imgAlpha = 1f;
        private float m_txtAlpha = 1f;
        private float time;

        private bool start;
        private bool single;
        private bool fadeCompleted;
        private bool startFadeIn;
        private bool startFadeOut;
        private bool fadedIn;
        private bool fadedOut;
        private bool colorFade;

        /// <summary>
        /// Create or Find Fading Instance
        /// </summary>
        public static UIFade Instance(GameObject FadeObject, string name = "[UIFader]", bool hide = true)
        {
            if (FindObjectsOfType<UIFade>().Count(x => x.FadeObj == FadeObject) < 1)
            {
                GameObject obj = new GameObject(name);
                if (hide) obj.hideFlags = HideFlags.HideInHierarchy;

                UIFade uIFade = obj.AddComponent<UIFade>();
                uIFade.FadeObj = FadeObject;
                return uIFade;
            }
            else
            {
                return FindObjectsOfType<UIFade>().FirstOrDefault(x => x.FadeObj == FadeObject);
            }
        }

        /// <summary>
        /// Find Fader with a specified FadeObject
        /// </summary>
        public static UIFade FindUIFader(GameObject FadeObject)
        {
            if (FindObjectsOfType<UIFade>().Count(x => x.FadeObj == FadeObject) > 0)
            {
                return FindObjectsOfType<UIFade>().FirstOrDefault(x => x.FadeObj == FadeObject);
            }

            return null;
        }

        /// <summary>
        /// Set Graphics Max Alpha Fade
        /// </summary>
        public void ImageTextAlpha(float ImageAlpha = 1f, float TextAlpha = 1f)
        {
            m_imgAlpha = ImageAlpha;
            m_txtAlpha = TextAlpha;
        }

        /// <summary>
        /// Set Objects FadeValue
        /// </summary>
        public void SetFadeValues(FadeValue[] values)
        {
            fadeValues = values;
        }

        /// <summary>
        /// Start FadeInOut Sequence (Color)
        /// </summary>
        public void FadeInOut(Color startColor, float fadeTo = 1f, float fadeInSpeed = 1.2f, float fadeOutSpeed = 2.5f, float fadeOutTime = 3f, bool SingleFade = false, DisableTypeAfter disableTypeAfter = DisableTypeAfter.Disable, FadeOutAfter fadeOutAfter = FadeOutAfter.Bool)
        {
            FadeObj.SetActive(true);
            outAfter = fadeOutAfter;
            m_fadeColor = startColor;
            m_fadeColor.a = 0;
            m_fadeTo = fadeTo;
            m_fadeInSpeed = fadeInSpeed;
            m_fadeOutSpeed = fadeOutSpeed;
            m_fadeOutTime = fadeOutTime;
            startFadeIn = true;
            startFadeOut = false;
            fadedIn = false;
            fadedOut = false;
            colorFade = true;
            disableType = disableTypeAfter;
            single = SingleFade;
            fadeCompleted = false;
            start = true;
            time = 0;
        }

        /// <summary>
        /// Start FadeInOut Sequence (Alpha)
        /// </summary>
        public void FadeInOut(float fadeTo = 1f, float fadeInSpeed = 1.2f, float fadeOutSpeed = 2.5f, float fadeOutTime = 3f, bool SingleFade = false, DisableTypeAfter disableTypeAfter = DisableTypeAfter.Disable, FadeOutAfter fadeOutAfter = FadeOutAfter.Bool)
        {
            FadeObj.SetActive(true);
            outAfter = fadeOutAfter;
            m_Alpha = 0;
            m_fadeTo = fadeTo;
            m_fadeInSpeed = fadeInSpeed;
            m_fadeOutSpeed = fadeOutSpeed;
            m_fadeOutTime = fadeOutTime;
            startFadeIn = true;
            startFadeOut = false;
            fadedIn = false;
            fadedOut = false;
            colorFade = false;
            disableType = disableTypeAfter;
            single = SingleFade;
            fadeCompleted = false;
            start = true;
            time = 0;
        }

        void Update()
        {
            if (colorFade)
            {
                m_fadeColor.a = m_Alpha;
            }

            if (startFadeIn)
            {
                if (!fadedIn)
                {
                    if (m_Alpha <= m_fadeTo)
                    {
                        m_Alpha += Time.deltaTime * m_fadeInSpeed;
                    }
                    else
                    {
                        fadedIn = true;
                        m_Alpha = m_fadeTo;
                        OnFadeInEvent?.Invoke();
                    }
                }
                else
                {
                    if (outAfter == FadeOutAfter.Bool)
                    {
                        if (fadeOut)
                        {
                            startFadeOut = true;
                            startFadeIn = false;
                        }
                    }
                    else if (outAfter == FadeOutAfter.Time)
                    {
                        time += Time.deltaTime;

                        if (time > m_fadeOutTime)
                        {
                            startFadeOut = true;
                            startFadeIn = false;
                        }
                    }
                }
            }
            else if (startFadeOut)
            {
                if (!fadedOut)
                {
                    if (m_Alpha > 0.01f)
                    {
                        m_Alpha -= Time.deltaTime * m_fadeOutSpeed;
                    }
                    else
                    {
                        fadeCompleted = true;
                        fadedIn = false;
                        fadedOut = true;
                        m_fadeColor.a = 0f;
                        m_Alpha = 0f;
                        OnFadeOutEvent?.Invoke();
                    }
                }
            }

            OnFadeCompleted?.Invoke();

            if (FadeObj)
            {
                if (!fadeCompleted && start)
                {
                    if (!single)
                    {
                        if (fadeValues != null && fadeValues.Length > 0)
                        {
                            foreach (var g in FadeObj.GetComponentsInChildren<Graphic>().Where(x => x.gameObject.activeSelf).ToArray())
                            {
                                float fadeVal = g.transform.GetComponent<Image>() ? GetAlpha(m_imgAlpha) : GetAlpha(m_txtAlpha);

                                if (fadeValues.Count(x => x.Obj == g.gameObject) > 0)
                                {
                                    FadeValue fade = fadeValues.SingleOrDefault(x => x.Obj == g.gameObject);
                                    fadeVal = fade.Value;
                                }

                                Color gColor = g.color;
                                gColor.a = fadeVal;
                                g.color = gColor;
                            }
                        }
                        else
                        {
                            foreach (var g in FadeObj.GetComponentsInChildren<Graphic>().Where(x => x.gameObject.activeSelf).ToArray())
                            {
                                Color gColor = g.color;
                                gColor.a = g.transform.GetComponent<Image>() ? GetAlpha(m_imgAlpha) : GetAlpha(m_txtAlpha);
                                g.color = gColor;
                            }
                        }
                    }
                    else
                    {
                        Graphic graphic = FadeObj.GetComponent<Graphic>();
                        Color gColor = graphic.color;
                        gColor.a = graphic.transform.GetComponent<Image>() ? GetAlpha(m_imgAlpha) : GetAlpha(m_txtAlpha);
                        graphic.color = gColor;
                    }
                }
                else
                {
                    if (disableType == DisableTypeAfter.Disable)
                    {
                        FadeObj.SetActive(false);
                        FadeObj = null;
                        start = false;
                    }
                    else if (disableType == DisableTypeAfter.Destroy)
                    {
                        Destroy(FadeObj);
                        FadeObj = null;
                        start = false;
                    }
                    else if (disableType == DisableTypeAfter.BothDestroy)
                    {
                        Destroy(FadeObj);
                        FadeObj = null;
                        start = false;
                        Destroy(gameObject);
                    }
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Reset Fade Obj Graphics Alpha
        /// </summary>
        public void ResetGraphicsColor(float alpha = 0f)
        {
            if (FadeObj)
            {
                foreach (var g in FadeObj.GetComponentsInChildren<Graphic>())
                {
                    Color gColor = g.color;
                    gColor.a = alpha;
                    g.color = gColor;
                }
            }
        }

        public float GetAlpha()
        {
            return m_Alpha;
        }

        public float GetAlpha(float max)
        {
            if (m_Alpha <= max)
            {
                return m_Alpha;
            }

            return max;
        }
    }
}