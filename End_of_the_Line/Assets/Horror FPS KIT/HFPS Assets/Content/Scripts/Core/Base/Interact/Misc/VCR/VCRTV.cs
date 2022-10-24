/*
 * VCRTV.cs - by ThunderWire Studio
 * Version 1.1
*/

using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace HFPS.Systems
{
    public class VCRTV : MonoBehaviour
    {
        public enum OSD { NoInput, InsertTape, Zero, Pause, Stop, Rewind }

        [HideInInspector]
        public VCRPlayer VCR;

        [Header("Main")]
        public VideoPlayer videoPlayer;
        public AudioSource videoAudio;
        public AudioSource soundsAudio;

        [Space(10)]
        public Vector2Int renderTextureSize = new Vector2Int(512, 512);

        [Header("VCR Textures")]
        public Texture2D NoInput;
        public Texture2D InsertTape;
        public Texture2D ZeroZero;
        public Texture2D Paused;
        public Texture2D Stop;
        public Texture2D Rewind;

        [Header("Renderers")]
        public MeshRenderer PowerButton;
        public MeshRenderer Display;
        public Light PowerLight;

        [Header("Display")]
        public Light DisplayLight;
        public Color DisplayNoTape = Color.blue;
        public Color DisplayPlaying = Color.white;

        [Header("Materials")]
        public Material OffMaterial;
        public Material RenderMaterial;

        [Header("Sounds")]
        public AudioClip OnSound;
        public AudioClip OffSound;

        [Space(10)]
        public bool TurnOn;

        [HideInInspector]
        public bool isOn;
        [HideInInspector]
        public long lastFrame;

        private RenderTexture renderTexture;
        private Texture2D osdTex;

        private bool hasInput = false;
        private bool hasTape = false;
        private bool isRender = false;
        private bool isTapeSet = false;
        private bool isStarted = false;

        private VideoClip lastClip;

        void Awake()
        {
            renderTexture = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 16);
            renderTexture.Create();
            osdTex = NoInput;
        }

        void Start()
        {
            videoAudio.spatialBlend = 1;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.source = VideoSource.VideoClip;

            PowerButton.material.DisableKeyword("_EMISSION");

            if (TurnOn)
            {
                PowerOnOff(false);
            }
        }

        void Update()
        {
            if (videoPlayer.isPlaying)
            {
                VCR.UpdateClipTime(videoPlayer.time);
            }
        }

        public void SetOsdScreen(OSD screen)
        {
            if (DisplayLight) DisplayLight.color = DisplayNoTape;

            switch (screen)
            {
                case OSD.NoInput:
                    osdTex = NoInput;
                    break;
                case OSD.InsertTape:
                    osdTex = InsertTape;
                    break;
                case OSD.Zero:
                    osdTex = ZeroZero;
                    break;
                case OSD.Pause:
                    osdTex = Paused;
                    break;
                case OSD.Stop:
                    osdTex = Stop;
                    break;
                case OSD.Rewind:
                    osdTex = Rewind;
                    break;
            }

            if (isOn)
            {
                Display.material = RenderMaterial;
                Display.material.SetTexture("_MainTex", osdTex);
            }
        }

        void EnableScreen(bool enable)
        {
            if (enable)
            {
                PowerButton.material.EnableKeyword("_EMISSION");
                Display.material = RenderMaterial;

                if (!isRender)
                {
                    Display.material.SetTexture("_MainTex", osdTex);
                }
                else
                {
                    Display.material.SetTexture("_MainTex", renderTexture);
                }
            }
            else
            {
                isRender = Display.material.GetTexture("_MainTex") is RenderTexture;
                PowerButton.material.DisableKeyword("_EMISSION");
                Display.material = OffMaterial;
            }
        }

        #region Callbacks
        public void PowerOnOff(bool clickSound = true)
        {
            if (!isOn)
            {
                if (hasTape && videoPlayer.isPlaying)
                {
                    videoAudio.volume = 1;
                    videoPlayer.targetTexture = renderTexture;
                    if (DisplayLight) DisplayLight.color = DisplayPlaying;
                }
                else
                {
                    if (DisplayLight) DisplayLight.color = DisplayNoTape;
                }

                if (clickSound && OnSound)
                {
                    soundsAudio.clip = OnSound;
                    soundsAudio.Play();
                }

                if (PowerLight) PowerLight.enabled = true;
                if (DisplayLight) DisplayLight.enabled = true;

                EnableScreen(true);
                isOn = true;
            }
            else
            {
                if (hasTape)
                {
                    videoAudio.volume = 0;
                }

                if (clickSound && OffSound)
                {
                    soundsAudio.clip = OffSound;
                    soundsAudio.Play();
                }

                if (PowerLight) PowerLight.enabled = false;
                if (DisplayLight) DisplayLight.enabled = false;

                EnableScreen(false);
                isOn = false;
            }
        }

        public void PlayVideo(VideoClip clip)
        {
            lastClip = clip;
            videoAudio.loop = false;
            videoPlayer.isLooping = false;

            StopAllCoroutines();

            if (!isTapeSet)
            {
                videoPlayer.clip = clip;
                lastFrame = 0;
                hasInput = true;
                hasTape = true;
                isRender = true;
                isStarted = true;
                isTapeSet = true;
            }

            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, videoAudio);
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.targetTexture = renderTexture;
            if (DisplayLight) DisplayLight.color = DisplayPlaying;

            if (isOn)
            {
                Display.material = RenderMaterial;
                Display.material.SetTexture("_MainTex", renderTexture);
            }

            StartCoroutine(PrepareAndPlay(lastFrame));
        }

        public void PauseVideo()
        {
            lastFrame = videoPlayer.frame;
            videoPlayer.Pause();
        }

        public void PauseVideoAT(long frame, VideoClip clip, bool on)
        {
            videoPlayer.clip = clip;
            lastClip = clip;

            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, videoAudio);
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.frame = frame;
            lastFrame = frame;

            hasInput = true;
            hasTape = true;
            isRender = false;
            isStarted = true;

            if (on)
            {
                isOn = true;
                SetOsdScreen(OSD.Pause);
                EnableScreen(true);
            }
        }

        public void SetVideoSource(bool state, bool changeOSD = false)
        {
            if (!state)
            {
                StopAllCoroutines();
                videoPlayer.Stop();
                videoPlayer.frame = 0;
                renderTexture?.Release();
                isRender = false;
                isTapeSet = false;
                isStarted = false;
                hasTape = false;
                lastFrame = 0;

                if (isOn && changeOSD)
                {
                    SetOsdScreen(OSD.InsertTape);
                }
            }
            else
            {
                hasTape = true;
                lastFrame = 0;

                if (!videoPlayer.isPlaying && changeOSD)
                {
                    SetOsdScreen(OSD.Zero);
                }
            }
        }

        public void SetInput(bool state)
        {
            hasInput = state;

            if (!state)
            {
                StopAllCoroutines();
                lastFrame = videoPlayer.frame;
                videoPlayer.Stop();
                SetOsdScreen(OSD.NoInput);
            }
            else if (hasTape)
            {
                videoPlayer.clip = lastClip;
                videoPlayer.frame = lastFrame;
                videoPlayer.Pause();
                if (isStarted)
                {
                    SetOsdScreen(OSD.Pause);
                }
                else
                {
                    SetOsdScreen(OSD.Zero);
                }
            }
            else
            {
                SetOsdScreen(OSD.InsertTape);
            }
        }
        #endregion

        IEnumerator PrepareAndPlay(long lastFrame = 0)
        {
            videoPlayer.Prepare();

            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            videoAudio.volume = 1;
            videoPlayer.Play();
            videoAudio.Play();

            if (lastFrame > 0)
            {
                videoPlayer.frame = lastFrame;
            }

            while (videoPlayer.time < (videoPlayer.clip.length - 0.1) && hasInput)
            {
                yield return null;
            }

            if (hasInput)
            {
                VCR.OnTapeEnd();
                SetOsdScreen(OSD.Stop);
                videoPlayer.Stop();
                videoPlayer.frame = 0;
                this.lastFrame = 0;
                StopAllCoroutines();
            }
        }
    }
}