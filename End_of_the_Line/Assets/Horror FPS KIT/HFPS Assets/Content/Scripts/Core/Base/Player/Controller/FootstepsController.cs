/*
 * FootstepsController.cs - by ThunderWire Studio
 * ver. 2.0
*/

using System.Collections;
using UnityEngine;
using ThunderWire.Helpers;
using ThunderWire.Utility;
using HFPS.Systems;

namespace HFPS.Player
{
    /// <summary>
    /// Script for playing surface based Player Footstep Sounds
    /// </summary>
    public class FootstepsController : MonoBehaviour
    {
        private PlayerController playerController;
        protected RandomHelper rand = new RandomHelper();

        [Header("Main")]
        public AudioSource footstepsAudio;
        public LayerMask footstepsMask;

        [Header("Player Footsteps")]
        public SurfaceID surfaceID = SurfaceID.Texture;
        public SurfaceDetailsScriptable surfaceDetails;
        [Space(5)]
        public AudioClip[] defaultFootsteps;
        public AudioClip[] ladderFootsteps;
        public AudioClip[] waterFootsteps;
        [Space(5)]
        public float playVelocity = 0.1f;
        public bool allowDefaultFootsteps;
        public bool eventBasedFootsteps;

        [Header("Sliding")]
        public AudioClip slidingSound;
        public float slidePlayVelocity = 1f;
        public float slideVolumeSpeed = 5f;
        public float slideEndVolumeSpeed = 5f;
        public float slidingVolume = 0.75f;
        public bool enableSlidingSound = true;

        [Header("Footsteps Volume")]
        public float volumeJump = 0.6f;
        public float volumeWalk = 0.6f;
        public float volumeCrouch = 0.5f;
        public float volumeRun = 0.7f;
        public float volumeLadder = 0.6f;
        public float volumeWater = 0.6f;

        [Header("Next Footstep Wait")]
        public float idleTime = 0.5f;
        public float walkNextWait = 0.1f;
        public float crouchNextWait = 0.1f;
        public float runNextWait = 0.1f;
        public float ladderNextWait = 0.1f;
        public float waterNextWait = 0.1f;

        private float defaultVolume;
        private float nextWaitTime = 0.0f;
        private float waitTime = 0.0f;

        private float playerVelocity;
        private int playerState;

        private bool isRunning;
        private bool isOnLadder;
        private bool isInWater;

        private bool isSliding;
        private bool slidePlaying;
        private bool slideVolState;

        private Terrain terrainUnder;
        private GameObject surfaceUnder;

        void Awake()
        {
            playerController = GetComponent<PlayerController>();
            defaultVolume = footstepsAudio.volume;
        }

        void Update()
        {
            playerVelocity = playerController.velMagnitude;

            playerState = (int)playerController.characterState;
            isRunning = playerController.isRunning;
            isInWater = playerController.isInWater;
            isOnLadder = playerController.ladderReady;
            isSliding = playerController.sliding;

            if (playerVelocity > playVelocity)
            {
                waitTime += Time.deltaTime;

                if (playerState < 2 && waitTime >= nextWaitTime && (isOnLadder || isInWater))
                {
                    if (isOnLadder && ladderFootsteps.Length > 0)
                    {
                        footstepsAudio.PlayOneShot(ladderFootsteps[rand.Range(0, ladderFootsteps.Length)], volumeLadder);
                        nextWaitTime = ladderNextWait;
                    }
                    else if (isInWater && waterFootsteps.Length > 0)
                    {
                        footstepsAudio.PlayOneShot(waterFootsteps[rand.Range(0, waterFootsteps.Length)], volumeWater);
                        nextWaitTime = waterNextWait;
                    }

                    waitTime = 0f;
                }
            }
            else
            {
                waitTime = idleTime;
            }

            if(playerVelocity > slidePlayVelocity)
            {
                if (enableSlidingSound && slidingSound)
                {
                    if (isSliding && !slidePlaying)
                    {
                        footstepsAudio.clip = slidingSound;
                        footstepsAudio.volume = 0f;
                        footstepsAudio.loop = true;
                        footstepsAudio.Play();
                        slidePlaying = true;
                        slideVolState = false;
                    }
                    else if (!isSliding && slidePlaying)
                    {
                        slidePlaying = false;
                    }
                }
            }
            else
            {
                slidePlaying = false;
            }

            if (slidePlaying)
            {
                footstepsAudio.volume = Mathf.MoveTowards(footstepsAudio.volume, slidingVolume, Time.deltaTime * slideVolumeSpeed);
                slideVolState = true;
            }
            else if (slideVolState)
            {
                footstepsAudio.volume = Mathf.MoveTowards(footstepsAudio.volume, 0, Time.deltaTime * slideEndVolumeSpeed);

                if (Mathf.Approximately(footstepsAudio.volume, 0))
                {
                    footstepsAudio.Stop();
                    footstepsAudio.clip = null;
                    footstepsAudio.loop = false;
                    footstepsAudio.volume = defaultVolume;
                    slideVolState = false;
                }
            }
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            surfaceUnder = hit.gameObject;
            terrainUnder = hit.collider.GetComponent<Terrain>();

            if (!footstepsMask.CompareLayer(surfaceUnder.layer) || eventBasedFootsteps) return;

            if (playerVelocity > playVelocity)
            {
                if (playerState < 2 && waitTime >= nextWaitTime)
                {
                    if (!isInWater && !isOnLadder && !isSliding && playerController.IsGrounded())
                    {
                        AudioClip[] footsteps = GetSurfaceFootsteps();

                        if (footsteps.Length > 0)
                        {
                            if (playerState == 0)
                            {
                                footstepsAudio.PlayOneShot(footsteps[rand.Range(0, footsteps.Length)], isRunning ? volumeRun : volumeWalk);
                                nextWaitTime = isRunning ? runNextWait : walkNextWait;
                            }
                            else if (playerState == 1)
                            {
                                footstepsAudio.PlayOneShot(footsteps[rand.Range(0, footsteps.Length)], volumeCrouch);
                                nextWaitTime = crouchNextWait;
                            }
                        }
                        else if (defaultFootsteps.Length > 0 && allowDefaultFootsteps)
                        {
                            if (playerState == 0)
                            {
                                footstepsAudio.PlayOneShot(defaultFootsteps[rand.Range(0, defaultFootsteps.Length)], isRunning ? volumeRun : volumeWalk);
                                nextWaitTime = isRunning ? runNextWait : walkNextWait;
                            }
                            else if (playerState == 1)
                            {
                                footstepsAudio.PlayOneShot(defaultFootsteps[rand.Range(0, defaultFootsteps.Length)], volumeCrouch);
                                nextWaitTime = crouchNextWait;
                            }
                        }

                        waitTime = 0f;
                    }
                }
            }
        }


        /// <summary>
        /// Event function to play one footstep.
        /// </summary>
        public void PlayFootstep()
        {
            if (!eventBasedFootsteps || isInWater || isOnLadder) return;

            AudioClip[] footsteps = GetSurfaceFootsteps();

            if (footsteps.Length > 0)
            {
                if (playerState == 0)
                {
                    footstepsAudio.PlayOneShot(footsteps[rand.Range(0, footsteps.Length)], isRunning ? volumeRun : volumeWalk);
                }
                else if (playerState == 1)
                {
                    footstepsAudio.PlayOneShot(footsteps[rand.Range(0, footsteps.Length)], volumeCrouch);
                }
            }
            else if (defaultFootsteps.Length > 0 && allowDefaultFootsteps)
            {
                if (playerState == 0)
                {
                    footstepsAudio.PlayOneShot(defaultFootsteps[rand.Range(0, defaultFootsteps.Length)], isRunning ? volumeRun : volumeWalk);
                }
                else if (playerState == 1)
                {
                    footstepsAudio.PlayOneShot(defaultFootsteps[rand.Range(0, defaultFootsteps.Length)], volumeCrouch);
                }
            }
        }

        /// <summary>
        /// Play jump footsteps.
        /// </summary>
        public void OnJump()
        {
            if (!isOnLadder && !isInWater)
                StartCoroutine(JumpFootsteps());
        }

        IEnumerator JumpFootsteps()
        {
            AudioClip[] footsteps = GetSurfaceFootsteps();

            if (footsteps.Length > 0)
            {
                footstepsAudio.PlayOneShot(footsteps[rand.Range(0, footsteps.Length)], volumeJump);
                yield return new WaitForSeconds(0.075f);
                footstepsAudio.PlayOneShot(footsteps[rand.Range(0, footsteps.Length)], volumeJump);
            }
            else if (defaultFootsteps.Length > 0 && allowDefaultFootsteps)
            {
                footstepsAudio.PlayOneShot(defaultFootsteps[rand.Range(0, defaultFootsteps.Length)], volumeJump);
                yield return new WaitForSeconds(0.075f);
                footstepsAudio.PlayOneShot(defaultFootsteps[rand.Range(0, defaultFootsteps.Length)], volumeJump);
            }
        }

        AudioClip[] GetSurfaceFootsteps()
        {
            if (surfaceDetails)
            {
                if (terrainUnder != null)
                {
                    SurfaceDetails surface = surfaceDetails.GetTerrainSurfaceDetails(terrainUnder, transform.position);

                    if (surface != null && surface.HasFootsteps() && surface.SurfaceProperties.AllowFootsteps)
                    {
                        return surface.FootstepProperties.SurfaceFootsteps;
                    }
                }
                else if (surfaceUnder != null && surfaceUnder.GetComponent<MeshRenderer>())
                {
                    SurfaceDetails surface = surfaceDetails.GetSurfaceDetails(surfaceUnder, surfaceID);

                    if (surface != null && surface.HasFootsteps() && surface.SurfaceProperties.AllowFootsteps)
                    {
                        return surface.FootstepProperties.SurfaceFootsteps;
                    }
                }
            }

            return new AudioClip[0];
        }
    }
}