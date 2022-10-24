using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Helpers;
using ThunderWire.Utility;
using ThunderWire.Input;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    public class TipsManager : MonoBehaviour
    {
        [System.Serializable]
        public struct TipInfo
        {
            public string TipMessage;
            public string TipKey;
        }

        public TipInfo[] Tips;

        [Header("UI")]
        public Text TipsText;
        public string TipPrefix;
        public float TipTime;

        [Header("Fader")]
        public bool TipChangeFade;
        public float TipFadeInSpeed;
        public float TipFadeOutSpeed;

        private readonly RandomHelper random = new RandomHelper();
        private Fader tipFader;

        private void Awake()
        {
            if (TipChangeFade)
            {
                tipFader = Fader.Instance(gameObject, "[Fader] Tips");
                tipFader.OnFade += value =>
                {
                    Color color = TipsText.color;
                    color.a = value;
                    TipsText.color = color;
                };
            }

#if TW_LOCALIZATION_PRESENT
            string[] keys = Tips.Select(x => x.TipKey).ToArray();
            LocalizationSystem.SubscribeAndGet(OnLocalizationUpdate, keys);
#endif
        }

        void OnLocalizationUpdate(string[] values)
        {
            for (int i = 0; i < Tips.Length; i++)
            {
                Tips[i].TipMessage = values[i];
            }
        }

        IEnumerator Start()
        {
            if (TipsText)
            {
                Color color = TipsText.color;
                color.a = TipChangeFade ? 0 : 1;
                TipsText.color = color;

                yield return new WaitUntil(() => InputHandler.HasReference && InputHandler.InputIsInitialized);
                yield return new WaitUntil(() => TipsText.gameObject.activeSelf);
                while (TipsText.gameObject.activeSelf)
                {
                    ChangeTip();

                    if (TipChangeFade)
                    {
                        tipFader.Fade(new Fader.FadeSettings()
                        {
                            startValue = TipsText.color.a,
                            endValue = 1f,
                            fadeInSpeed = TipFadeInSpeed,
                            fadeOutSpeed = TipFadeOutSpeed,
                            fadeOutAfterSignal = true,
                            destroyFader = false
                        });

                        yield return new WaitUntil(() => tipFader.IsFadedIn);
                    }

                    yield return new WaitForSeconds(TipTime);

                    if (TipChangeFade)
                    {
                        tipFader.FadeOutSignal();
                        yield return new WaitUntil(() => tipFader.IsFadedOut);
                    }
                }
            }
        }

        private void ChangeTip()
        {
            int nextTip = random.Range(0, Tips.Length);

            if (string.IsNullOrEmpty(TipPrefix))
                TipsText.text = Tips[nextTip].TipMessage.GetStringWithInput('{', '}', '[', ']');
            else
                TipsText.text = TipPrefix + ": " + Tips[nextTip].TipMessage.GetStringWithInput('{', '}', '[', ']');
        }
    }
}