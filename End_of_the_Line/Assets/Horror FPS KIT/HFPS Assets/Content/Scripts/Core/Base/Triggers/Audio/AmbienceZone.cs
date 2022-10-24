/*
 * AmbienceZone.cs - wirted by ThunderWire Games
 * ver. 1.0
*/

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HFPS.Systems
{
    public class AmbienceZone : MonoBehaviour, ISaveable
    {
        private AudioSource AmbienceSource;

        public AudioClip Ambience;
        public float TargetVolume = 1f;
        public float FadeSpeed;

        private bool triggerEnter;
        private bool fadedOut;
        private bool fadedIn;

        void Awake()
        {
            AmbienceSource = ScriptManager.Instance.AmbienceSource;
        }

        void Update()
        {
            if (triggerEnter)
            {
                if (!fadedOut)
                {
                    if (AmbienceSource.volume > 0.01f && !fadedOut)
                    {
                        AmbienceSource.volume -= Time.deltaTime * FadeSpeed;
                    }
                    else
                    {
                        AmbienceSource.volume = 0f;
                        AmbienceSource.clip = Ambience;
                        fadedOut = true;
                    }
                }

                if (!fadedIn && fadedOut)
                {
                    if (AmbienceSource.volume <= TargetVolume)
                    {
                        if (!AmbienceSource.isPlaying) { AmbienceSource.Play(); }
                        AmbienceSource.volume += Time.deltaTime * FadeSpeed;
                    }
                    else
                    {
                        AmbienceSource.volume = TargetVolume;
                        fadedIn = true;
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !triggerEnter && AmbienceSource.clip != Ambience)
            {
                fadedIn = false;
                fadedOut = false;
                triggerEnter = true;
                FindObjectsOfType<AmbienceZone>().Where(x => x != this).ToList().ForEach(x => x.triggerEnter = false);
            }
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>()
            {
                { "ZoneTriggered", triggerEnter }
            };
        }

        public void OnLoad(JToken token)
        {
            bool isTriggered = token["ZoneTriggered"].ToObject<bool>();

            if (isTriggered)
            {
                fadedIn = true;
                fadedOut = true;
                triggerEnter = true;
                AmbienceSource.clip = Ambience;
                AmbienceSource.volume = TargetVolume;
                AmbienceSource.Play();
            }
        }
    }
}