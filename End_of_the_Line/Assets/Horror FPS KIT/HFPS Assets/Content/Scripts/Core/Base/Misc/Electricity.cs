/*
 * Electricity.cs - wirted by ThunderWire Games
 * ver. 2.0
*/

using UnityEngine;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    public class Electricity : MonoBehaviour
    {
        [Header("Hint")]
        public string offHint;
        public string offHintKey;
        public float hintTime;

        [Header("Indicator")]
        public GameObject LampIndicator;

        [SaveableField]
        public bool isPoweredOn = true;

#if TW_LOCALIZATION_PRESENT
        void OnEnable()
        {
            if (HFPS_GameManager.LocalizationEnabled)
            {
                LocalizationSystem.SubscribeAndGet(OnLocalizationUpdate, offHintKey);
            }
        }

        public void OnLocalizationUpdate(string[] trs)
        {
            offHint = trs[0];
        }
#endif

        public void ShowOffHint()
        {
            if (HFPS_GameManager.HasReference)
            {
                HFPS_GameManager.Instance.ShowHintPopup(offHint, hintTime);
            }
        }

        public void SwitchElectricity(bool power)
        {
            isPoweredOn = power;

            if (LampIndicator)
            {
                if (power)
                {
                    LampIndicator.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(1f, 1f, 1f));
                    LampIndicator.GetComponentInChildren<Light>().enabled = true;
                }
                else
                {
                    LampIndicator.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
                    LampIndicator.GetComponentInChildren<Light>().enabled = false;
                }
            }
        }
    }
}