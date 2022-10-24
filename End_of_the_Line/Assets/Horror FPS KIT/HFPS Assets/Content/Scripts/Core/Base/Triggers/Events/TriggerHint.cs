using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using ThunderWire.Utility;
using ThunderWire.Input;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    public class TriggerHint : MonoBehaviour, ISaveable
    {
        private HFPS_GameManager gameManager;
        private InputHandler inputHandler;
        private AudioSource soundEffects;

        public string Hint;
        public string HintKey;
        public float TimeShow;
        public float ShowAfter = 0f;
        public AudioClip HintSound;

        private float timer;
        private bool timedShow;
        private bool isShown;

        void Awake()
        {
            if (InputHandler.HasReference)
                inputHandler = InputHandler.Instance;
            else
                throw new System.NullReferenceException($"[TriggerHint] {gameManager.name}: Cannot find Input handler script!");
        }

#if TW_LOCALIZATION_PRESENT
        void OnEnable()
        {
            if (HFPS_GameManager.LocalizationEnabled)
            {
                LocalizationSystem.SubscribeAndGet(OnLocalizationUpdate, HintKey);
            }
        }

        public void OnLocalizationUpdate(string[] trs)
        {
            Hint = trs[0];
        }
#endif

        void Start()
        {
            gameManager = HFPS_GameManager.Instance;
            soundEffects = GetComponent<AudioSource>() ? GetComponent<AudioSource>() : null;

            if (HintSound && !soundEffects)
            {
                Debug.LogError("[TriggerHint] HintSound require an a AudioSource Component!");
            }

            if (soundEffects)
            {
                soundEffects.spatialBlend = 0f;
            }
        }

        public void SetTrigger(bool state)
        {
            isShown = state;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isShown && gameManager && inputHandler != null)
            {
                char[] hintChars = Hint.ToCharArray();

                if (hintChars.Contains('{') && hintChars.Contains('}'))
                {
                    string key = InputHandler.CompositeOf(Hint.GetBetween('{', '}')).displayString;
                    Hint = Hint.ReplacePart('{', '}', key);
                }

                if (ShowAfter > 0)
                {
                    timedShow = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(Hint))
                    {
                        gameManager.ShowHintPopup(Hint, TimeShow);
                    }

                    if (HintSound && soundEffects)
                    {
                        soundEffects.clip = HintSound;
                        soundEffects.Play();
                    }

                    isShown = true;
                }
            }
        }

        void Update()
        {
            if (timedShow && !isShown)
            {
                timer += Time.unscaledDeltaTime;

                if (timer >= ShowAfter)
                {
                    if (!string.IsNullOrEmpty(Hint))
                    {
                        gameManager.ShowHintPopup(Hint, TimeShow);
                    }

                    if (HintSound && soundEffects)
                    {
                        soundEffects.clip = HintSound;
                        soundEffects.Play();
                    }
                    isShown = true;
                }
            }
        }

        void OnDisable()
        {
            isShown = true;
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>()
            {
                { "enabled", enabled },
                { "isShown", isShown }
            };
        }

        public void OnLoad(JToken token)
        {
            isShown = (bool)token["isShown"];
            enabled = (bool)token["enabled"];
        }
    }
}