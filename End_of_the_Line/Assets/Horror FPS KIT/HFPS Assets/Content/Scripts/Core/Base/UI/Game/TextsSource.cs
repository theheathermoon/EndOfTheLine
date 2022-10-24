using System;
using System.Collections.Generic;
using ThunderWire.Helpers;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    public class TextsSource : Singleton<TextsSource>
    {
        public TextTableAsset textTableAsset;

        public static event Action OnInitTexts;

        protected Dictionary<string, TextSourceTable> textTable;

        private static bool IsInitialized;

#if TW_LOCALIZATION_PRESENT
        private void Awake()
        {
            if (HFPS_GameManager.LocalizationEnabled)
                LocalizationSystem.OnIntOrUpdate += OnTextsUpdated;
        }

        private void OnDestroy()
        {
            LocalizationSystem.OnIntOrUpdate -= OnTextsUpdated;

            foreach (Delegate d in OnInitTexts.GetInvocationList())
            {
                OnInitTexts -= (Action)d;
            }
        }
#endif

        /// <summary>
        /// Subscribe to listen for changes to the text table, even if subscription is delayed.
        /// </summary>
        public static void Subscribe(Action action)
        {
            if (HasReference)
            {
                OnInitTexts += action;
                if (IsInitialized) action.Invoke();
            }
            else
            {
                action.Invoke();
            }
        }

        private void OnEnable()
        {
            if (!HFPS_GameManager.LocalizationEnabled)
            {
                OnTextsUpdated();
            }
        }

        private void OnTextsUpdated()
        {
            textTable = textTableAsset.GetTextTable();
            OnInitTexts?.Invoke();
            IsInitialized = true;
        }

        public static string GetText(string key)
        {
            if (HasReference && Instance.textTable != null && Instance.textTable.ContainsKey(key))
            {
                return Instance.textTable[key].Text;
            }

            return string.Empty;
        }

        public static T GetText<T>(string key)
        {
            if (HasReference && Instance.textTable != null && Instance.textTable.ContainsKey(key))
            {
                return Parser.Convert<T>(Instance.textTable[key].Text);
            }

            return default;
        }

        public static string GetText(string key, string defaultText)
        {
            if (HasReference && Instance.textTable != null && Instance.textTable.ContainsKey(key))
            {
                string text = Instance.textTable[key].Text;

                if (!string.IsNullOrEmpty(text))
                    return text;
                else
                    return defaultText;
            }

            return defaultText;
        }

        public static T GetText<T>(string key, object defaultValue)
        {
            if (HasReference && Instance.textTable != null && Instance.textTable.ContainsKey(key))
            {
                string text = Instance.textTable[key].Text;

                if (typeof(T) != typeof(string))
                    return Parser.Convert<T>(text);
                else if (!string.IsNullOrEmpty(text))
                    return (T)(object)text;
                else
                    return (T)defaultValue;
            }

            return (T)defaultValue;
        }
    }

    [Serializable]
    public struct TextSourceTable
    {
        public string Text;
        public bool IsUppercase;
    }
}