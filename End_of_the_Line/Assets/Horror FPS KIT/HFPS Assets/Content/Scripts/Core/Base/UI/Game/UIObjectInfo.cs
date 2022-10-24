using UnityEngine;
using ThunderWire.Utility;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    public class UIObjectInfo : MonoBehaviour
    {
        public string ObjectTitle;

        public ReflectionUtil.Reflection DynamicTitle = new ReflectionUtil.Reflection();

        public string UseText = "Use";
        public string DynamicTrue = "Close";
        public string DynamicFalse = "Open";

        public string titleKey;
        public string useKey;
        public string dTrueKey;
        public string dFalseKey;

        public bool isUppercased;
        public bool dynamicUseText;
        public bool overrideDoorText;

#if TW_LOCALIZATION_PRESENT
        void OnEnable()
        {
            if (HFPS_GameManager.LocalizationEnabled)
                LocalizationSystem.SubscribeAndGet(OnChangeLocalization, titleKey, useKey, dTrueKey, dFalseKey);
        }

        void OnChangeLocalization(string[] texts)
        {
            ObjectTitle = texts[0];
            UseText = texts[1];
            DynamicTrue = texts[2];
            DynamicFalse = texts[3];
        }
#endif

        void Awake()
        {
            if (DynamicTitle.Instance != null && !string.IsNullOrEmpty(DynamicTitle.ReflectName))
            {
                DynamicTitle.Setup();
            }
        }

        void Update()
        {
            if (DynamicTitle.Instance != null && !string.IsNullOrEmpty(DynamicTitle.ReflectName))
            {
                if ((bool)DynamicTitle.Get())
                {
                    if (!dynamicUseText)
                    {
                        ObjectTitle = isUppercased ? DynamicTrue.ToUpper() : DynamicTrue;
                    }
                    else
                    {
                        UseText = isUppercased ? DynamicTrue.ToUpper() : DynamicTrue;
                    }
                }
                else
                {
                    if (!dynamicUseText)
                    {
                        ObjectTitle = isUppercased ? DynamicFalse.ToUpper() : DynamicFalse;
                    }
                    else
                    {
                        UseText = isUppercased ? DynamicFalse.ToUpper() : DynamicFalse;
                    }
                }
            }
        }
    }
}