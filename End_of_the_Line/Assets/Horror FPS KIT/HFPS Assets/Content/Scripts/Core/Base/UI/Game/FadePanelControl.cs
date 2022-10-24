/*
 * FadePanelControl.cs - by ThunderWire Studio
*/

using UnityEngine;
using UnityEngine.UI;

namespace HFPS.UI
{
    public class FadePanelControl : MonoBehaviour
    {
        public Image fadePanel;
        public string FadeIn;
        public string FadeOut;
        public float speed;

        private Animation anim;

        private void Awake()
        {
            if (!fadePanel.GetComponent<Animation>())
            {
                Debug.LogError("Animation component does not exist!");
                return;
            }

            anim = fadePanel.GetComponent<Animation>();
        }

        public void FadeInPanel()
        {
            anim[FadeIn].speed = speed;
            anim.Play(FadeIn);
        }

        public void FadeOutPanel()
        {
            anim[FadeOut].speed = speed;
            anim.Play(FadeOut);
        }

        public bool isFading()
        {
            return anim.isPlaying;
        }
    }
}