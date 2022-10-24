using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Helpers;

namespace HFPS.UI
{
    public class UIFadePanel : MonoBehaviour
    {
        private Fader fader;

        public Image FadeImage;
        public float FadeInSpeed;
        public float FadeOutSpeed;
        public bool startFadeOut;

        public bool IsFadedIn => fader.IsFadedIn;
        public bool IsFadedOut => fader.IsFadedOut;

        private void Awake()
        {
            FadeImage.raycastTarget = false;
        }

        void OnEnable()
        {
            fader = Fader.Instance(FadeImage.gameObject, "[Fader] Fade Panel");

            fader.OnFade += value =>
            {
                Color color = FadeImage.color;
                color.a = value;
                FadeImage.color = color; 
            };
        }

        void Start()
        {
            if (startFadeOut)
            {
                FadeOut();
            }
        }

        public void FadeIn(bool signalFadeOut)
        {
            FadeImage.raycastTarget = true;

            fader.Fade(new Fader.FadeSettings()
            {
                startValue = FadeImage.color.a,
                endValue = 1,
                fadeInSpeed = FadeInSpeed,
                fadeOutSpeed = FadeOutSpeed,
                oneWayFade = !signalFadeOut,
                fadeOutAfterSignal = signalFadeOut,
                destroyFader = false,
                actionObj = FadeImage.gameObject,
                fadedOutAction = Fader.DestroyType.None
            });
        }

        public void FadeOut()
        {
            FadeImage.raycastTarget = false;

            fader.Fade(new Fader.FadeSettings()
            {
                startValue = FadeImage.color.a,
                endValue = 0,
                fadeInSpeed = FadeInSpeed,
                fadeOutSpeed = FadeOutSpeed,
                oneWayFade = true,
                destroyFader = false,
                actionObj = FadeImage.gameObject,
                fadedOutAction = Fader.DestroyType.Disable
            });
        }

        public void FadeOutManually()
        {
            FadeImage.raycastTarget = false;
            fader.FadeOutSignal();
        }
    }
}