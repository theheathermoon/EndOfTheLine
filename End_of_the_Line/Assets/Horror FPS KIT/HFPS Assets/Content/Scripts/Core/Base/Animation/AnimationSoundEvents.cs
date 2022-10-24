using System;
using System.Collections.Generic;
using UnityEngine;

namespace HFPS.Systems
{
    public class AnimationSoundEvents : MonoBehaviour
    {
        public enum SoundMode { Default, Once }

        [Serializable]
        public sealed class SoundEvents
        {
            public string eventName;
            public AudioClip eventSound;
            public SoundMode playMode = SoundMode.Default;
            [HideInInspector]
            public bool isPlayed = false;
        }

        public float soundVolume = 0.75f;
        public List<SoundEvents> soundEvents = new List<SoundEvents>();

        private AudioSource Audio;

        private void Awake()
        {
            if (GetComponent<AudioSource>())
            {
                Audio = GetComponent<AudioSource>();
            }
        }

        public void EventPlaySound(string SoundEvent)
        {
            foreach (var soundEvent in soundEvents)
            {
                if (soundEvent.eventName == SoundEvent)
                {
                    if (soundEvent.eventSound)
                    {
                        if (soundEvent.playMode == SoundMode.Once && !soundEvent.isPlayed)
                        {
                            if (!Audio)
                            {
                                AudioSource.PlayClipAtPoint(soundEvent.eventSound, transform.position, soundVolume);
                            }
                            else
                            {
                                Audio.clip = soundEvent.eventSound;
                                Audio.volume = soundVolume;
                                Audio.Play();
                            }
                            soundEvent.isPlayed = true;
                        }

                        if (soundEvent.playMode == SoundMode.Default)
                        {
                            if (!Audio)
                            {
                                AudioSource.PlayClipAtPoint(soundEvent.eventSound, transform.position, soundVolume);
                            }
                            else
                            {
                                Audio.clip = soundEvent.eventSound;
                                Audio.volume = soundVolume;
                                Audio.Play();
                            }
                        }
                    }
                }
            }
        }
    }
}