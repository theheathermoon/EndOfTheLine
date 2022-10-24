using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Helpers;

namespace HFPS.UI
{
    public class Notification : MonoBehaviour
    {

        [HideInInspector]
        public string id = string.Empty;

        public GameObject Graphic;
        public Image IconSprite;
        public Vector3 FadeTo;
        public float FadeSmooth = 10f;
        public float FadeInSpeed = 2f;
        public float FadeOutSpeed = 5f;

        private Vector3 currPos;

        private bool start = false;
        private Vector3 velocity;

        void Update()
        {
            if (start && gameObject.transform.parent != null)
            {
                currPos = Vector3.SmoothDamp(currPos, FadeTo, ref velocity, Time.deltaTime * FadeSmooth);
                Graphic.transform.localPosition = currPos;
            }
        }

        public void SetMessage(string message, float time = 3f, Sprite icon = null, bool upper = true)
        {
            ResetGraphicColor();
            Graphic.GetComponentInChildren<Text>().text = upper ? message.ToUpper() : message;

            if (IconSprite && icon != null)
            {
                IconSprite.sprite = icon;
            }

            UIFade uIFade = UIFade.Instance(gameObject, "[UIFader]" + gameObject.name);
            uIFade.ImageTextAlpha(0.75f, 1f);
            uIFade.SetFadeValues(new UIFade.FadeValue[]
            {
            new UIFade.FadeValue(IconSprite.gameObject, 1f)
            });
            uIFade.FadeInOut(1f, FadeInSpeed, FadeOutSpeed, time, disableTypeAfter: UIFade.DisableTypeAfter.BothDestroy, fadeOutAfter: UIFade.FadeOutAfter.Time);

            currPos = Graphic.transform.localPosition;
            start = true;
        }

        void ResetGraphicColor()
        {
            foreach (var g in GetComponentsInChildren<Graphic>())
            {
                Color gColor = g.color;
                gColor.a = 0f;
                g.color = gColor;
            }
        }
    }
}