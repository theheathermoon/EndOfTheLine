/*
 * Keypad.cs - by ThunderWire Studio
 * Version 1.0
*/

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HFPS.Systems
{
    public class Keypad : MonoBehaviour, ISaveable
    {
        private MeshRenderer textRenderer;
        public MeshRenderer keypadRenderer;

        [Header("Materials")]
        public Material LedRedOn;
        public Material LedGreenOn;

        [Header("Setup")]
        public int AccessCode;
        public TextMesh AccessCodeText;

        [Header("Sounds")]
        public AudioClip enterCode;
        [Range(0, 2)] public float enterCodeVolume = 1f;

        public AudioClip accessGranted;
        [Range(0, 2)] public float grantedVolume = 1f;

        public AudioClip accessDenied;
        [Range(0, 2)] public float deniedVolume = 1f;

        [Space(15)]
        public UnityEvent OnAccessGranted;
        [Space(7)]
        public UnityEvent OnAccessDenied;

        private string numberInsert = "";
        private bool enableInsert = true;

        [HideInInspector]
        public bool m_accessGranted = false;

        void Awake()
        {
            textRenderer = AccessCodeText.gameObject.GetComponent<MeshRenderer>();
        }

        void Start()
        {
            if (!keypadRenderer)
            {
                Debug.LogError("[Keypad] Please assign the Keypad Renderer!");
            }

            if (!LedGreenOn || !LedRedOn)
            {
                Debug.LogError("[Keypad] Please assign the Led Materials!");
            }
        }

        public void InsertCode(int number)
        {
            if (enableInsert)
            {
                if (numberInsert.Length < AccessCode.ToString().Length && number != 10 && number != 11)
                {
                    if (enterCode) { AudioSource.PlayClipAtPoint(enterCode, transform.position, enterCodeVolume); }
                    numberInsert += number;
                }
                else if (!string.IsNullOrEmpty(numberInsert))
                {
                    if (number == 10)
                    {
                        // Back Button
                        if (numberInsert.Length > 0)
                        {
                            if (enterCode) { AudioSource.PlayClipAtPoint(enterCode, transform.position, enterCodeVolume); }
                            numberInsert = numberInsert.Remove(numberInsert.Length - 1);
                        }
                    }
                    else if (number == 11)
                    {
                        // Confirm Code
                        if (numberInsert == AccessCode.ToString())
                        {
                            if (accessGranted) { AudioSource.PlayClipAtPoint(accessGranted, transform.position, grantedVolume); }

                            textRenderer.material.SetColor("_Color", Color.green);
                            AccessCodeText.text = "GRANTED";
                            keypadRenderer.material = LedGreenOn;
                            OnAccessGranted.Invoke();

                            numberInsert = "";
                            m_accessGranted = true;
                            enableInsert = false;
                            StartCoroutine(WaitEnableInsert());
                        }
                        else
                        {
                            if (accessDenied) { AudioSource.PlayClipAtPoint(accessDenied, transform.position, deniedVolume); }

                            textRenderer.material.SetColor("_Color", Color.red);
                            AccessCodeText.text = "DENIED";
                            keypadRenderer.material = LedRedOn;
                            OnAccessDenied.Invoke();

                            numberInsert = "";
                            m_accessGranted = false;
                            enableInsert = false;
                            StartCoroutine(WaitEnableInsert());
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (enableInsert)
            {
                textRenderer.material.SetColor("_Color", Color.white);
                AccessCodeText.text = numberInsert;
            }
        }

        public void SetAccessGranted()
        {
            keypadRenderer.material = LedGreenOn;
            enableInsert = false;
            numberInsert = "";
            m_accessGranted = true;
        }

        IEnumerator WaitEnableInsert()
        {
            yield return new WaitForSeconds(1);
            enableInsert = true;
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>()
            {
                { "accessGranted", m_accessGranted }
            };
        }

        public void OnLoad(JToken token)
        {
            m_accessGranted = (bool)token["accessGranted"];

            if (m_accessGranted)
            {
                keypadRenderer.material = LedGreenOn;
                enableInsert = false;
                enableInsert = true;
                numberInsert = "";
                textRenderer.material.SetColor("_Color", Color.green);
                AccessCodeText.text = "GRANTED";
            }
        }
    }
}