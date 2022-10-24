/*
 * KeycardLock.cs - written by ThunderWire Games
 * Version 1.0
*/

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HFPS.Systems
{
    public class KeycardLock : MonoBehaviour, ISaveable
    {
        private Inventory inventory;
        private MeshRenderer textRenderer;

        [Header("Setup")]
        [Tooltip("Keycard Inventory ID")]
        [InventorySelector]
        public int keycardID;

        [Tooltip("Remove Keycard after access granted.")]
        public bool removeCard;
        public TextMesh resultText;

        [Header("Colors")]
        public Color NormalColor = Color.white;
        public Color GrantedColor = Color.green;
        public Color DeniedColor = Color.red;

        [Header("Text")]
        public string NormalText;
        public string GrantedText;
        public string DeniedText;

        [Header("Sounds")]
        public AudioClip accessGranted;
        public AudioClip accessDenied;
        public float volume = 1f;

        [Header("Events")]
        public UnityEvent OnAccessGranted;
        public UnityEvent OnAccessDenied;

        private bool granted;
        private bool denied;

        void Awake()
        {
            inventory = Inventory.Instance;
            textRenderer = resultText.gameObject.GetComponent<MeshRenderer>();

            textRenderer.material.SetColor("_Color", NormalColor);
            resultText.text = NormalText;
        }

        public void UseObject()
        {
            if (!denied && !granted)
            {
                if (inventory.CheckItemInventory(keycardID))
                {
                    if (accessGranted) { AudioSource.PlayClipAtPoint(accessGranted, transform.position, volume); }
                    textRenderer.material.SetColor("_Color", GrantedColor);
                    resultText.text = GrantedText;
                    OnAccessGranted.Invoke();

                    if (removeCard)
                    {
                        inventory.RemoveItem(keycardID);
                    }

                    granted = true;
                    denied = false;
                }
                else
                {
                    if (accessDenied) { AudioSource.PlayClipAtPoint(accessDenied, transform.position, volume); }
                    textRenderer.material.SetColor("_Color", DeniedColor);
                    resultText.text = DeniedText;
                    OnAccessDenied.Invoke();
                    StartCoroutine(AccessDenied());
                    denied = true;
                }
            }
        }

        IEnumerator AccessDenied()
        {
            yield return new WaitForSeconds(2);
            textRenderer.material.SetColor("_Color", NormalColor);
            resultText.text = NormalText;
            denied = false;
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>
        {
            {"granted", granted }
        };
        }

        public void OnLoad(JToken token)
        {
            granted = (bool)token["granted"];

            if (granted)
            {
                textRenderer.material.SetColor("_Color", GrantedColor);
                resultText.text = GrantedText;
                OnAccessGranted.Invoke();
            }
        }
    }
}