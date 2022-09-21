using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace FlashlightSystem
{
    public class FLAudioManager : MonoBehaviour
    {
        public static FLAudioManager instance;
        public AudioMixerGroup mixerGroup;
        public Sound[] sounds;

        void Awake()
        {
            if (instance != null) { Destroy(gameObject); }
            else { instance = this; DontDestroyOnLoad(gameObject); }

            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.loop = s.loop;

                s.source.outputAudioMixerGroup = mixerGroup;
            }
        }

        public void Play(string sound)
        {
            Sound s = sounds.FirstOrDefault(item => item.name == sound);

            if (s == null)
            {
                Debug.LogWarning("Sound: " + sound + " not found!");
                return;
            }

            s.source.volume = s.volume * (1f + Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

            s.source.Play();
        }

        public void StopPlaying(string sound)
        {
            Sound s = sounds.FirstOrDefault(item => item.name == sound);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.source.volume = s.volume * (1f + Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            s.source.Stop();
        }
    }
}
