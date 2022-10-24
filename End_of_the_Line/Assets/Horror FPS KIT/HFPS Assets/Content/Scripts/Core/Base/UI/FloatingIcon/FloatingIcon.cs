using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Utility;
using ThunderWire.Helpers;
using HFPS.Systems;

namespace HFPS.UI
{
    public class FloatingIcon : MonoBehaviour
    {
        [HideInInspector]
        public FloatingIconManager iconManager;

        [HideInInspector]
        public bool isVisible = true;

        public GameObject FollowObject;

        private Fader fader;
        private Image icon;
        private bool outOfDistance = false;

        void Awake()
        {
            icon = GetComponent<Image>();
            fader = Fader.Instance(gameObject);

            fader.OnFade += value =>
            {
                if (icon != null)
                {
                    Color color = icon.color;
                    color.a = value;
                    icon.color = color;
                }
            };
        }

        void Update()
        {
            if (!FollowObject || !icon) return;

            if (isVisible)
            {
                if (!outOfDistance)
                {
                    if (iconManager.IsVisibleFrustum(FollowObject))
                    {
                        fader.Fade(new Fader.FadeSettings()
                        {
                            startValue = icon.color.a,
                            endValue = 1f,
                            fadeInSpeed = 2.5f,
                            destroyFader = false
                        });
                    }
                    else
                    {
                        fader.Fade(new Fader.FadeSettings()
                        {
                            startValue = icon.color.a,
                            endValue = 0f,
                            fadeInSpeed = 4f,
                            destroyFader = false
                        });
                    }
                }

                Vector3 screenPos = Utilities.MainPlayerCamera().WorldToScreenPoint(FollowObject.transform.position);
                icon.transform.position = Vector3.MoveTowards(icon.transform.position, screenPos, Time.deltaTime * Mathf.Pow(iconManager.followSmooth, 5));
            }
            else
            {
                Color color = icon.color;
                color.a = 0;
                icon.color = color;
            }
        }

        public void SetIconVisible(bool state)
        {
            isVisible = state;
        }

        public void OutOfDistance(bool isOut)
        {
            outOfDistance = isOut;

            if (isOut)
            {
                fader.Fade(new Fader.FadeSettings()
                {
                    startValue = icon.color.a,
                    endValue = 0f,
                    fadeInSpeed = 4f,
                    destroyFader = false
                });
            }
        }
    }
}