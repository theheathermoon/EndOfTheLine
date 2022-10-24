using UnityEngine;

namespace HFPS.Systems
{
    public class DragSound : MonoBehaviour, IDragRigidbody
    {
        public enum DragSoundType { Default, Once }

        public DragSoundType soundType = DragSoundType.Default;
        public AudioClip dragSound;
        [Range(0, 1)] public float dragVolume;

        [SaveableField, HideInInspector]
        public bool isPlayed;

        private bool isPlayedOnce;

        public void OnRigidbodyDrag()
        {
            if (!isPlayedOnce)
            {
                if (soundType == DragSoundType.Default)
                {
                    AudioSource.PlayClipAtPoint(dragSound, transform.position, dragVolume);
                }
                else if (!isPlayed)
                {
                    AudioSource.PlayClipAtPoint(dragSound, transform.position, dragVolume);
                    isPlayed = true;
                }

                isPlayedOnce = true;
            }
        }

        public void OnRigidbodyRelease()
        {
            isPlayedOnce = false;
        }
    }
}