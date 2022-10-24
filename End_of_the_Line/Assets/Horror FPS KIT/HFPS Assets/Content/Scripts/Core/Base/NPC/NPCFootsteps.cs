/*
 * NPCFootsteps.cs by ThunderWire Studio
 * ver. 1.0
*/

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using ThunderWire.Helpers;
using ThunderWire.Utility;

namespace HFPS.Systems
{
    /// <summary>
    /// Script for playing surface based NPC Footstep Sounds
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCFootsteps : MonoBehaviour
    {
        [Serializable]
        public struct Footstep
        {
            public Texture2D[] GroundTextures;
            public AudioClip[] StepSounds;
        }

        private RandomHelper rand = new RandomHelper();
        private NavMeshAgent agent;

        [Header("Main")]
        public LayerMask footstepsMask;
        public AudioSource footstepsAudio;
        public float groundRay = 5f;

        [Header("NPC Footsteps")]
        public Footstep[] npcFootsteps;
        public AudioClip[] defaultFootsteps;
        [Space(10)]
        public float playVelocity = 0.1f;
        public bool allowDefaultFootsteps;
        public bool eventBasedFootsteps;

        [Header("Footstep Control")]
        public float walkVelocity;

        [Header("Footsteps Volume")]
        public float volumeWalk = 0.6f;
        public float volumeRun = 0.7f;

        [Header("Next Footstep Wait")]
        public float idleTime = 0.5f;
        public float walkNextWait = 0.1f;
        public float runNextWait = 0.1f;

        private bool isWalking;
        private float waitTime;
        private float nextWait;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        void Update()
        {
            float velocity = agent.velocity.magnitude;
            isWalking = velocity <= walkVelocity;

            if (velocity > playVelocity && !eventBasedFootsteps)
            {
                waitTime += Time.deltaTime;

                if (waitTime >= nextWait)
                {
                    AudioClip[] footsteps = GetSurfaceFootsteps();

                    if (footsteps.Length > 0)
                    {
                        if (isWalking)
                        {
                            footstepsAudio.PlayOneShot(footsteps[rand.Range(0, footsteps.Length)], volumeWalk);
                            nextWait = walkNextWait;
                        }
                        else
                        {
                            footstepsAudio.PlayOneShot(footsteps[rand.Range(0, footsteps.Length)], volumeRun);
                            nextWait = runNextWait;
                        }
                    }
                    else if (defaultFootsteps.Length > 0 && allowDefaultFootsteps)
                    {
                        if (isWalking)
                        {
                            footstepsAudio.PlayOneShot(defaultFootsteps[rand.Range(0, defaultFootsteps.Length)], volumeWalk);
                            nextWait = walkNextWait;
                        }
                        else
                        {
                            footstepsAudio.PlayOneShot(defaultFootsteps[rand.Range(0, defaultFootsteps.Length)], volumeRun);
                            nextWait = runNextWait;
                        }
                    }

                    waitTime = 0f;
                }
            }
        }

        /// <summary>
        /// Event function to play one footstep.
        /// </summary>
        public void PlayFootstep()
        {
            if (!eventBasedFootsteps) return;

            AudioClip[] footsteps = GetSurfaceFootsteps();

            if (footsteps.Length > 0)
            {
                if (isWalking)
                {
                    footstepsAudio.PlayOneShot(footsteps[rand.Range(0, footsteps.Length)], volumeWalk);
                }
                else
                {
                    footstepsAudio.PlayOneShot(footsteps[rand.Range(0, footsteps.Length)], volumeRun);
                }
            }
            else if (defaultFootsteps.Length > 0 && allowDefaultFootsteps)
            {
                if (isWalking)
                {
                    footstepsAudio.PlayOneShot(defaultFootsteps[rand.Range(0, defaultFootsteps.Length)], volumeWalk);
                }
                else
                {
                    footstepsAudio.PlayOneShot(defaultFootsteps[rand.Range(0, defaultFootsteps.Length)], volumeRun);
                }
            }
        }

        AudioClip[] GetSurfaceFootsteps()
        {
            Ray ray = new Ray(transform.position, -transform.up);

            if (Physics.Raycast(ray, out RaycastHit hit, groundRay, footstepsMask))
            {
                GameObject surfaceUnder = hit.collider.gameObject;
                Terrain terrainUnder = hit.collider.GetComponent<Terrain>();

                if (footstepsMask.CompareLayer(surfaceUnder.layer))
                {
                    foreach (var step in npcFootsteps)
                    {
                        if (terrainUnder != null)
                        {
                            Texture2D texBelow = Utilities.TerrainPosToTex(terrainUnder, transform.position);

                            if (step.GroundTextures.Any(x => x == texBelow))
                            {
                                return step.StepSounds;
                            }
                        }
                        else if (surfaceUnder.GetComponent<MeshRenderer>())
                        {
                            Texture2D[] texBelow = surfaceUnder.GetComponent<MeshRenderer>().materials.Select(x => x.mainTexture).Cast<Texture2D>().ToArray();

                            if (step.GroundTextures.Any(x => texBelow.Any(y => x == y)))
                            {
                                return step.StepSounds;
                            }
                        }
                    }
                }
            }

            return new AudioClip[0];
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, -transform.up * groundRay);
        }
    }
}