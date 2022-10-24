using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using ThunderWire.Helpers;
using ThunderWire.Utility;

namespace HFPS.Systems
{
    public class Radio : MonoBehaviour, ISaveable
    {
        [System.Serializable]
        public class RadioChannel
        {
            public AudioClip channelAudio;
            public bool loopable;
            [Tooltip("0 - random")]
            public float tunerPosition;
            [HideInInspector]
            public float playBackTime;
        }

        private readonly RandomHelper random = new RandomHelper();

        [Header("Sounds")]
        public AudioSource radioAudioSource;
        public AudioClip pushButton;
        public float pushVolume = 0.3f;

        [Header("Channels")]
        public RadioChannel[] radioChannels;
        public bool randomChannel;

        [Header("Effects")]
        public Renderer meshRenderer;
        public Light PowerLight;
        public Color OnColor = Color.green;
        public Color OffColor = Color.red;

        [Header("Tuner")]
        public Transform tunerGO;
        public Vector2 tunerMinMax;
        public float moveSpeed = 0.25f;

        [Header("Animation")]
        public Animation m_animation;
        public string OnAnimation;
        public string OffAnimation;
        public bool isOn;

        private int lastChannel = 0;

        void Start()
        {
            meshRenderer.material.EnableKeyword("_EMISSION");
            radioAudioSource.spatialBlend = 1f;
            lastChannel = 0;

            if (isOn)
            {
                radioAudioSource.Play();
                meshRenderer.material.SetColor("_EmissionColor", OnColor);

                if (PowerLight)
                {
                    PowerLight.color = OnColor;
                    PowerLight.enabled = true;
                }

                if (m_animation)
                {
                    m_animation.Play(OnAnimation);
                }
            }
            else if (PowerLight)
            {
                PowerLight.color = OffColor;
                PowerLight.enabled = true;
            }
        }

        public void UseObject()
        {
            AudioSource.PlayClipAtPoint(pushButton, transform.position, pushVolume);

            if (!isOn)
            {
                ReceiveTransmission(true);
                meshRenderer.material.SetColor("_EmissionColor", OnColor);

                if (m_animation)
                {
                    m_animation.Play(OnAnimation);
                }

                if (PowerLight)
                {
                    PowerLight.color = OnColor;
                }

                isOn = true;
            }
            else
            {
                ReceiveTransmission(false);
                meshRenderer.material.SetColor("_EmissionColor", OffColor);

                if (m_animation)
                {
                    m_animation.Play(OffAnimation);
                }

                if (PowerLight)
                {
                    PowerLight.color = OffColor;
                }

                isOn = false;
            }
        }

        void ReceiveTransmission(bool receive)
        {
            RadioChannel channel = radioChannels[lastChannel];

            if (receive)
            {
                radioAudioSource.clip = channel.channelAudio;
                radioAudioSource.time = channel.playBackTime;
                radioAudioSource.loop = channel.loopable;
                radioAudioSource.Play();
            }
            else
            {
                channel.playBackTime = radioAudioSource.time;
                radioAudioSource.Pause();
            }
        }

        public void ChangeChannel()
        {
            if (radioAudioSource.isPlaying)
            {
                radioChannels[lastChannel].playBackTime = radioAudioSource.time;
                radioAudioSource.Stop();
            }

            if (randomChannel)
                lastChannel = random.Range(0, radioChannels.Length);
            else
                lastChannel = lastChannel == radioChannels.Length - 1 ? 0 : lastChannel + 1;

            if (tunerGO)
            {
                RadioChannel channel = radioChannels[lastChannel];
                float position = channel.tunerPosition;

                if (position == 0)
                    position = tunerMinMax.Random();

                StopAllCoroutines();
                StartCoroutine(MoveTuner(position));
            }

            if(isOn) ReceiveTransmission(true);
        }

        IEnumerator MoveTuner(float tunerPos)
        {
            while (!Mathf.Approximately(tunerGO.localPosition.x, tunerPos))
            {
                Vector3 position = tunerGO.localPosition;
                position.x = Mathf.MoveTowards(position.x, tunerPos, Time.smoothDeltaTime * moveSpeed);
                tunerGO.localPosition = position;
                yield return null;
            }

            yield return null;
        }

        public Dictionary<string, object> OnSave()
        {
            var playbackTimes = radioChannels.Select(x => x.playBackTime);
            float tunerPos = 0;

            if(tunerGO)
                tunerPos = tunerGO.localPosition.x;

            return new Dictionary<string, object>()
            {
                { "isOn", isOn },
                { "tunerPos", tunerPos },
                { "lastChannel", lastChannel },
                { "playbackTimes", playbackTimes }
            };
        }

        public void OnLoad(JToken token)
        {
            isOn = (bool)token["isOn"];

            if (tunerGO)
            {
                Vector3 pos = tunerGO.localPosition;
                pos.x = (float)token["tunerPos"];
                tunerGO.localPosition = pos;
            }

            lastChannel = (int)token["lastChannel"];
            var playbackTimes = token["playbackTimes"].ToObject<float[]>();

            for (int i = 0; i < playbackTimes.Length; i++)
            {
                radioChannels[i].playBackTime = playbackTimes[i];
            }

            if (isOn)
            {
                ReceiveTransmission(true);
                meshRenderer.material.SetColor("_EmissionColor", OnColor);

                if (m_animation)
                {
                    m_animation.Play(OnAnimation);
                }

                if (PowerLight)
                {
                    PowerLight.color = OnColor;
                }
            }
        }
    }
}