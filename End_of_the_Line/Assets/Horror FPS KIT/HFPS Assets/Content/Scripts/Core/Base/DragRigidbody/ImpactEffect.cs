/*
 * ImpactEffect.cs - by ThunderWire Studio
 * ver. 2.0
*/

using System.Linq;
using UnityEngine;
using ThunderWire.Utility;

namespace HFPS.Systems
{
    /// <summary>
    /// Script for surface based Impact Sounds
    /// </summary>
    [RequireComponent(typeof(AudioSource), typeof(Rigidbody))]
    public class ImpactEffect : MonoBehaviour
    {
        public enum ImpactDetect { Texture, Tag }

        [Header("Setup")]
        [MinMax(0, 1)]
        public Vector2 volumeRange = new Vector2(0.2f, 1);
        public float volumeModifer = 10f;
        public float nextImpactTime;
        public bool randomUnique;
        public bool useDefaultImpact;

        [Header("Sound Reaction")]
        public LayerMask soundReactionMask;
        public float reactionRadius;
        public bool soundReaction;

        [Header("Impacts")]
        public DefaultImpact defaultImpact = new DefaultImpact();
        public ImpactSound[] impactSounds;

        private AudioSource audioSource;
        private float time;

        private int lastSound = 0;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (time > 0)
            {
                time -= Time.deltaTime;
            }
            else
            {
                time = 0;
            }
        }

        void OnCollisionEnter(Collision col)
        {
            float volumeMultipler = Mathf.Clamp(col.relativeVelocity.magnitude / volumeModifer, volumeRange.x, volumeRange.y);

            if (nextImpactTime > 0)
            {
                if (time <= 0)
                {
                    OnImpact(volumeMultipler, col);
                    if (soundReaction && volumeRange.IsInRange(volumeMultipler)) SendSoundReaction();
                    time = nextImpactTime;
                }
            }
            else
            {
                OnImpact(volumeMultipler, col);
                if (soundReaction && volumeRange.IsInRange(volumeMultipler)) SendSoundReaction();
            }
        }

        void OnImpact(float volume, Collision col)
        {
            if (useDefaultImpact && defaultImpact.impactSounds.Length > 0)
            {
                if (defaultImpact.triggerVolume.IsInRange(volume))
                {
                    lastSound = randomUnique ? Utilities.RandomUnique(0, defaultImpact.impactSounds.Length, lastSound) : Random.Range(0, defaultImpact.impactSounds.Length);
                    audioSource.PlayOneShot(defaultImpact.impactSounds[lastSound], volume);
                }
            }
            else if (!useDefaultImpact)
            {
                Terrain terrain;
                MeshRenderer meshRenderer;

                foreach (var impact in impactSounds)
                {
                    if (impact.impactSounds.Length > 0)
                    {
                        if ((terrain = col.collider.gameObject.GetComponent<Terrain>()) != null)
                        {
                            Texture2D terrainTex = Utilities.TerrainPosToTex(terrain, transform.position);

                            if (impact.textures.Any(x => x == terrainTex) && impact.triggerVolume.IsInRange(volume))
                            {
                                lastSound = randomUnique ? Utilities.RandomUnique(0, defaultImpact.impactSounds.Length, lastSound) : Random.Range(0, defaultImpact.impactSounds.Length);
                                audioSource.PlayOneShot(impact.impactSounds[lastSound], volume);
                                break;
                            }
                        }
                        else if ((meshRenderer = col.collider.gameObject.GetComponent<MeshRenderer>()) != null)
                        {
                            Texture2D[] meshTex = meshRenderer.materials.Select(x => x.mainTexture).Cast<Texture2D>().ToArray();

                            if (impact.impactDetect == ImpactDetect.Tag && meshRenderer.gameObject.tag.Equals(impact.Tag))
                            {
                                lastSound = randomUnique ? Utilities.RandomUnique(0, defaultImpact.impactSounds.Length, lastSound) : Random.Range(0, defaultImpact.impactSounds.Length);
                                audioSource.PlayOneShot(impact.impactSounds[lastSound], volume);
                                break;
                            }
                            else if (impact.textures.Any(x => meshTex.Any(y => x == y)) && impact.triggerVolume.IsInRange(volume))
                            {
                                lastSound = randomUnique ? Utilities.RandomUnique(0, defaultImpact.impactSounds.Length, lastSound) : Random.Range(0, defaultImpact.impactSounds.Length);
                                audioSource.PlayOneShot(impact.impactSounds[lastSound], volume);
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("[Impact Effect] There are no impact sounds!");
                        break;
                    }
                }
            }
            else
            {
                Debug.LogError("[Impact Effect] There are no impact sounds!");
            }
        }

        void SendSoundReaction()
        {
            Collider[] colliderHit = Physics.OverlapSphere(transform.position, reactionRadius, soundReactionMask);

            foreach (var hit in colliderHit)
            {
                INPCReaction reaction;

                if ((reaction = hit.GetComponentInChildren<INPCReaction>()) != null)
                {
                    reaction.SoundReaction(transform.position, false);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, reactionRadius);
        }

        [System.Serializable]
        public class ImpactSound
        {
            public ImpactDetect impactDetect = ImpactDetect.Tag;
            public string Tag;
            public Vector2 triggerVolume = new Vector2(0.2f, 1);
            public Texture2D[] textures;
            public AudioClip[] impactSounds;
        }

        [System.Serializable]
        public class DefaultImpact
        {
            public Vector2 triggerVolume = new Vector2(0.2f, 1);
            public AudioClip[] impactSounds;
        }
    }
}