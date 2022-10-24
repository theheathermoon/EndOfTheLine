using UnityEngine;
using System.Collections;

namespace HFPS.Systems
{
    public class AnimationPlayEvent : MonoBehaviour
    {
        public enum WrapModes { Default, Once, Loop, PingPong, ClampForever }

        public Animation m_animation;
        public string animationName;
        public WrapModes wrapMode = WrapModes.Default;

        [Header("Destroy")]
        public bool destroyAfterPlay;
        public bool sendDestroyEvent;
        public GameObject destroyEventObj;

        [HideInInspector, SaveableField]
        public bool isPlayed = false;

        public void PlayAnimation()
        {
            if (!isPlayed)
            {
                m_animation[animationName].wrapMode = (WrapMode)System.Enum.Parse(typeof(WrapMode), wrapMode.ToString());
                m_animation.Play(animationName);

                if (destroyAfterPlay)
                {
                    StartCoroutine(DestroyAfter());
                }

                isPlayed = true;
            }
        }

        IEnumerator DestroyAfter()
        {
            yield return new WaitUntil(() => !m_animation.isPlaying);

            if (!sendDestroyEvent)
            {
                Destroy(gameObject);
            }
            else
            {
                destroyEventObj.SendMessage("DestroyEvent", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}