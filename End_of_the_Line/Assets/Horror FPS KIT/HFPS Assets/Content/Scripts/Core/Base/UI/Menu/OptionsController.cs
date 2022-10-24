/*
 * OptionsController.cs - by ThunderWire Studio
 * Version 1.0
*/

using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using ThunderWire.Input;
using ThunderWire.Helpers;
using HFPS.UI;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    /// <summary>
    /// Main Controller for Options
    /// </summary>
    public class OptionsController : Singleton<OptionsController>
    {
        public const string SETTINGS_FILE = "Settings.xml";

        public string SETTINGS_PATH
        {
            get
            {
                return SerializationHelper.Settings.GetSerializationPath();
            }
        }

        public string SETTINGS_FILEPATH
        {
            get
            {
                return Path.Combine(SETTINGS_PATH, SETTINGS_FILE);
            }
        }

        #region Structures
        public enum PlatformMode
        {
            Computer,
            Console
        }

        public enum OptionVisibleType
        {
            Both,
            GamepadOnly,
            KeyboardOnly
        }

        public enum OptionType 
        { 
            Custom,
            Volume,
            PostBloom,
            PostGrain,
            PostMotionBlur,
            PostAmbient,
            Resolution,
            Fullscreen,
            Antialiasing,
            TextureQuality,
            ShadowResolution,
            ShadowDistance,
            VSync,
            InputController,
            Language
        }

        [Serializable]
        public sealed class TabViewData
        {
            public GameObject TabContents;
            public UITabButton ButtonToSelect;
            public Selectable FirstOption;
            public Selectable FirstOptionAlt;
            public bool IsControls;
        }
        #endregion

        private XmlDocument xmlDocument = new XmlDocument();
        private ScreenResolution[] resolutions;
        private InputOption[] inputOptions;

        [Header("Main")]
        public List<OptionSection> GameOptions = new List<OptionSection>();

        [Header("Options Tabs")]
        public TabViewData[] optionTabs;

        [Header("Other")]
        public PlatformMode platformMode = PlatformMode.Computer;
        public PostProcessVolume postProcessing;
        public DropdownLink GameQuality;
        [Space]
        public bool chngeableInput;
        public bool showRefreshRate;
        public bool debugMode;

        public static event Action OnOptionsUpdated;

        private int currentTab = 0;
        private int resolutionIndex = -1;
        private int fullScreenMode = -1;

        private void Awake()
        {
            currentTab = 0;

            InputHandler.OnInputsInitialized += OnInputsInitialized;
            InputHandler.OnDevicesUpdated += OnDevicesUpdated;
            InputHandler.GetInputAction("NavigateTab", "UI").performed += OnNavigateTab;

#if TW_LOCALIZATION_PRESENT
            LocalizationSystem.OnLanguagesInit += OnLanguagesInit;
#endif

            OnUpdateOptions();
        }

        private void OnDestroy()
        {
            InputHandler.OnInputsInitialized -= OnInputsInitialized;
            InputHandler.OnDevicesUpdated -= OnDevicesUpdated;
            InputHandler.GetInputAction("NavigateTab", "UI").performed -= OnNavigateTab;

#if TW_LOCALIZATION_PRESENT
            LocalizationSystem.OnLanguagesInit -= OnLanguagesInit;
#endif
        }

        private void OnNavigateTab(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            float navigate = ctx.ReadValue<float>();

            if (MenuController.HasReference && MenuController.Instance.optionsShown && !MenuController.Instance.rebindPending)
            {
                if (navigate < 0)
                {
                    int tab = currentTab > 0 ? currentTab - 1 : optionTabs.Length - 1;
                    SelectTab(tab);
                }
                else if (navigate > 0)
                {
                    int tab = currentTab < optionTabs.Length - 1 ? currentTab + 1 : 0;
                    SelectTab(tab);
                }
            }
        }

        private void OnLanguagesInit(string[] langs)
        {
            var option = GetOptionBehaviour(OptionType.Language);
            if (option != null)
            {
                if (option != null && option.Instance != null && option.Instance is DropdownUI dropdown)
                {
                    dropdown.SetDropdownValues(langs.Select(x => new Dropdown.OptionData(x)).ToArray());
                }
            }
        }

        /// <summary>
        /// Select Options Tab.
        /// </summary>
        public void SelectTab(int tab)
        {
            if (optionTabs.Length - 1 < tab)
                return;

            for (int i = 0; i < optionTabs.Length; i++)
            {
                if (i != tab)
                {
                    optionTabs[i].TabContents.SetActive(false);
                }
            }

            if (currentTab >= 0 && currentTab != tab)
                optionTabs[currentTab].ButtonToSelect.Unhold();

            TabViewData selectTab = optionTabs[tab];
            if (selectTab.TabContents != null)
            {
                selectTab.ButtonToSelect.Select();
                selectTab.TabContents.SetActive(true);
                MenuController.FirstOrAltButton(selectTab.FirstOption, selectTab.FirstOptionAlt);
                currentTab = tab;
            }
        }

        public void ReSelectTab()
        {
            if (currentTab >= 0)
            {
                TabViewData selectTab = optionTabs[currentTab];
                if (selectTab.TabContents != null)
                {
                    selectTab.ButtonToSelect.Select();
                    selectTab.TabContents.SetActive(true);
                    MenuController.FirstOrAltButton(selectTab.FirstOption, selectTab.FirstOptionAlt);
                }
            }
        }

        private void OnDevicesUpdated(InputHandler.Device[] devices)
        {
            if (chngeableInput)
            {
                var option = GetOptionBehaviour(OptionType.InputController);

                if (option != null)
                {
                    inputOptions = devices.Select(x => new InputOption()
                    {
                        PrettyName = x.DeviceToPrettyName(),
                        Device = x
                    }).ToArray();
                    var dropdownValues = inputOptions.Select(x => new Dropdown.OptionData(x.PrettyName)).ToArray();

                    if (option.Instance != null && option.Instance is DropdownUI dropdown)
                    {
                        dropdown.SetDropdownValues(dropdownValues);
                    }
                }
            }
        }

        void OnInputsInitialized()
        {
            int inputType = InputHandler.CurrentDevice.IsGamepadDevice();
            ShowHideOptionsByDevice(inputType);
            MaskInputsByOption(false);
        }

        async void OnUpdateOptions()
        {
            if (!postProcessing)
            {
                if (ScriptManager.HasReference)
                {
                    postProcessing = ScriptManager.Instance.MainPostProcess;
                }
            }

            if (platformMode == PlatformMode.Computer)
            {
                if (showRefreshRate)
                {
                    resolutions = Screen.resolutions.Select(x => new ScreenResolution(x.width, x.height, x.refreshRate)).Reverse().ToArray();
                }
                else
                {
                    resolutions = Screen.resolutions.Select(x => new ScreenResolution(x.width, x.height, 0)).Distinct().Reverse().ToArray();
                }

                var resolutionOpt = GetOptionBehaviour(OptionType.Resolution);
                if (resolutionOpt != null && resolutionOpt.Instance is DropdownUI resDrop)
                {
                    resDrop.dropdown.ClearOptions();

                    if (showRefreshRate)
                    {
                        resDrop.dropdown.options.AddRange(resolutions.Select(x => new Dropdown.OptionData(x.width + "x" + x.height + "@" + x.refreshRate)).ToArray());
                    }
                    else
                    {
                        resDrop.dropdown.options.AddRange(resolutions.Select(x => new Dropdown.OptionData(x.width + "x" + x.height)).ToArray());
                    }

                    resDrop.dropdown.RefreshShownValue();
                }
            }

            await ReadOptions();
        }

        private void MaskInputsByOption(bool sendDeviceUpdateEvent)
        {
            if (InputHandler.HasReference && InputHandler.Instance.maskInputManually && chngeableInput)
            {
                if (GetOptionValueRef(OptionType.InputController, out int value, out OptionObject opt))
                {
                    InputHandler.Device device = InputHandler.Device.None;

                    if (inputOptions.Length > value)
                    {
                        device = inputOptions[value].Device;
                        opt.Instance.SetValue(value.ToString());
                    }
                    else if (inputOptions.Length > 0)
                    {
                        device = inputOptions[0].Device;
                        opt.Instance.SetValue("0");
                        opt.OverrideValue = "0";
                    }

                    if (device != InputHandler.Device.None)
                    {
                        InputHandler.MaskInputByDevice(device);
                        ShowHideOptionsByDevice(device.IsGamepadDevice());

                        if (sendDeviceUpdateEvent)
                            InputHandler.SendInputUpdateEvent();

                        SelectTab(currentTab);
                    }
                }
            }
        }

        private void ShowHideOptionsByDevice(int deviceType)
        {
            foreach (var entry in GameOptions)
            {
                foreach (var option in entry.OptionObjects)
                {
                    if(platformMode == PlatformMode.Console && !option.EnabledOnConsole)
                    {
                        option.Instance.gameObject.SetActive(false);
                    }
                    else if (deviceType == 1)
                    {
                        if (option.VisibleType == OptionVisibleType.GamepadOnly)
                        {
                            option.Instance.gameObject.SetActive(true);
                        }
                        else if (option.VisibleType == OptionVisibleType.KeyboardOnly)
                        {
                            option.Instance.gameObject.SetActive(false);
                        }
                    }
                    else if (deviceType == 0)
                    {
                        if (option.VisibleType == OptionVisibleType.GamepadOnly)
                        {
                            option.Instance.gameObject.SetActive(false);
                        }
                        else if (option.VisibleType == OptionVisibleType.KeyboardOnly)
                        {
                            option.Instance.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }

        async Task<XmlDocument> ReadOptionsXML()
        {
            string xmlData = string.Empty;

            using (StreamReader sr = new StreamReader(SETTINGS_FILEPATH))
            {
                xmlData = await sr.ReadToEndAsync();
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            return xmlDoc;
        }

        public void OnOptionChanged(OptionBehaviour option)
        {
            foreach (var section in GameOptions)
            {
                foreach (var entry in section.OptionObjects)
                {
                    if (entry.Instance == option)
                    {
                        entry.IsChanged = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Apply all changed option settings.
        /// </summary>
        public async void ApplyOptions()
        {
            if (xmlDocument.ChildNodes.Count > 0)
                xmlDocument.RemoveAll();

            XmlNode rootNode = xmlDocument.CreateElement("Settings");
            foreach (var option in GameOptions)
            {
                XmlNode sectionRoot = xmlDocument.CreateElement(option.Section);
                bool skip = false;

                foreach (var entry in option.OptionObjects)
                {
                    if (!entry.EnabledOnConsole && platformMode == PlatformMode.Console)
                    {
                        skip = true;
                        break;
                    }

                    string value = entry.Instance.GetValue().ToString();

                    if (entry.IsChanged)
                    {
                        ApplyOptionRealtime(entry.Option, value);
                        entry.OverrideValue = value;
                    }

                    entry.IsChanged = false;

                    XmlNode sectionElement = xmlDocument.CreateElement("Option");

                    XmlAttribute name_attr = xmlDocument.CreateAttribute("Name");
                    name_attr.Value = entry.Prefix;
                    sectionElement.Attributes.Append(name_attr);

                    XmlAttribute value_attr = xmlDocument.CreateAttribute("Value");
                    value_attr.Value = value;
                    sectionElement.Attributes.Append(value_attr);
                    sectionRoot.AppendChild(sectionElement);
                }

                if(!skip) rootNode.AppendChild(sectionRoot);
            }
            xmlDocument.AppendChild(rootNode);

            OnOptionsUpdated?.Invoke();
            MaskInputsByOption(true);

            if (platformMode == PlatformMode.Computer)
            {
                ApplyResolution(resolutionIndex, fullScreenMode);
                resolutionIndex = -1;
                fullScreenMode = -1;
            }

            if (xmlDocument.DocumentElement.HasChildNodes)
            {
                StringWriter sw = new StringWriter();
                XmlTextWriter xw = new XmlTextWriter(sw)
                {
                    Formatting = Formatting.Indented
                };
                xmlDocument.WriteTo(xw);

                if (!Directory.Exists(SETTINGS_PATH))
                {
                    Directory.CreateDirectory(SETTINGS_PATH);
                }

                using (StreamWriter file_sw = new StreamWriter(SETTINGS_FILEPATH))
                {
                    await file_sw.WriteAsync(sw.ToString());
                }

                if(debugMode) Debug.Log("[XML] Options was successfully writed!");
            }
        }

        void ApplyOptionRealtime(OptionType option, string value)
        {
            if (string.IsNullOrEmpty(value) && debugMode)
                Debug.LogError(new NullReferenceException("Cannot convert null value of \"" + option.ToString() + "\" option!").Message);

            if (option == OptionType.Volume)
            {
                AudioListener.volume = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (option == OptionType.PostBloom && postProcessing)
            {
                if (postProcessing != null && postProcessing.sharedProfile.HasSettings<Bloom>())
                {
                    bool val = bool.Parse(value);
                    postProcessing.profile.GetSetting<Bloom>().enabled.Override(val);
                }
            }
            else if (option == OptionType.PostGrain && postProcessing)
            {
                if (postProcessing != null && postProcessing.sharedProfile.HasSettings<Grain>())
                {
                    bool val = bool.Parse(value);
                    postProcessing.profile.GetSetting<Grain>().enabled.Override(val);
                }
            }
            else if (option == OptionType.PostMotionBlur && postProcessing)
            {
                if (postProcessing != null && postProcessing.sharedProfile.HasSettings<MotionBlur>())
                {
                    bool val = bool.Parse(value);
                    postProcessing.profile.GetSetting<MotionBlur>().enabled.Override(val);
                }
            }
            else if (option == OptionType.PostAmbient && postProcessing)
            {
                if (postProcessing != null && postProcessing.sharedProfile.HasSettings<AmbientOcclusion>())
                {
                    bool val = bool.Parse(value);
                    postProcessing.profile.GetSetting<AmbientOcclusion>().enabled.Override(val);
                }
            }
            else if (option == OptionType.Resolution)
            {
                resolutionIndex = int.Parse(value);
            }
            else if (option == OptionType.Fullscreen)
            {
                fullScreenMode = int.Parse(value);
            }
            else if (option == OptionType.Antialiasing)
            {
                int val = int.Parse(value);
                int antialiasing = val == 0 ? 0 : val == 1 ? 2 : val == 2 ? 4 : 8;
                QualitySettings.antiAliasing = antialiasing;
            }
            else if (option == OptionType.TextureQuality)
            {
                int val = int.Parse(value);
                int quality = val == 0 ? 3 : val == 1 ? 2 : val == 2 ? 1 : 0;
                QualitySettings.masterTextureLimit = quality;
            }
            else if (option == OptionType.ShadowResolution)
            {
                int val = int.Parse(value);
                QualitySettings.shadowResolution = (ShadowResolution)val;
            }
            else if (option == OptionType.ShadowDistance)
            {
                QualitySettings.shadowDistance = float.Parse(value);
            }
            else if (option == OptionType.VSync)
            {
                QualitySettings.vSyncCount = bool.Parse(value) == false ? 0 : 1;
            }
            else if(option == OptionType.Language)
            {
#if TW_LOCALIZATION_PRESENT
                if (LocalizationSystem.HasReference)
                {
                    LocalizationSystem.Instance.ChangeLocalizationFromOpt(value);
                }
#endif
            }
        }

        object GetOptionDefault(OptionType option)
        {
            if (option == OptionType.Volume)
            {
                return AudioListener.volume;
            }
            else if (option == OptionType.PostBloom)
            {
                if (postProcessing != null && postProcessing.sharedProfile.HasSettings<Bloom>())
                    return postProcessing.profile.GetSetting<Bloom>().enabled.value;

                return false;
            }
            else if (option == OptionType.PostGrain)
            {
                if (postProcessing != null && postProcessing.sharedProfile.HasSettings<Grain>())
                    return postProcessing.profile.GetSetting<Grain>().enabled.value;

                return false;
            }
            else if (option == OptionType.PostMotionBlur)
            {
                if (postProcessing != null && postProcessing.sharedProfile.HasSettings<MotionBlur>())
                    return postProcessing.profile.GetSetting<MotionBlur>().enabled.value;

                return false;
            }
            else if (option == OptionType.PostAmbient)
            {
                if (postProcessing != null && postProcessing.sharedProfile.HasSettings<AmbientOcclusion>())
                    return postProcessing.profile.GetSetting<AmbientOcclusion>().enabled.value;

                return false;
            }
            else if (option == OptionType.Resolution)
            {
                Resolution resolution = new Resolution
                {
                    width = Screen.width,
                    height = Screen.height,
                    refreshRate = Screen.currentResolution.refreshRate
                };

                return GetResolution(resolution);
            }
            else if (option == OptionType.Fullscreen)
            {
                int val = (int)Screen.fullScreenMode;
                return val == 0 ? 0 : val == 1 ? 1 : 2;
            }
            else if (option == OptionType.Antialiasing)
            {
                int val = QualitySettings.antiAliasing;
                return val == 0 ? 0 : val == 2 ? 1 : val == 4 ? 2 : 3;
            }
            else if (option == OptionType.TextureQuality)
            {
                int val = QualitySettings.masterTextureLimit;
                return val == 3 ? 0 : val == 2 ? 1 : val == 1 ? 2 : 3;
            }
            else if (option == OptionType.ShadowResolution)
            {
                return (int)QualitySettings.shadowResolution;
            }
            else if (option == OptionType.ShadowDistance)
            {
                return QualitySettings.shadowDistance;
            }
            else if (option == OptionType.VSync)
            {
                return QualitySettings.vSyncCount != 0;
            }
            else if (option == OptionType.Language)
            {
#if TW_LOCALIZATION_PRESENT
                if (LocalizationSystem.HasReference)
                {
                    return LocalizationSystem.GetLocalizationName();
                }
#endif
            }

            return string.Empty;
        }

        async Task ReadOptions()
        {
            if (File.Exists(SETTINGS_FILEPATH))
            {
                xmlDocument = await ReadOptionsXML();
            }

            foreach (var section in GameOptions)
            {
                foreach (var entry in section.OptionObjects)
                {
                    if (!entry.EnabledOnConsole && platformMode == PlatformMode.Console)
                        break;

                    string value = string.Empty;

                    if (xmlDocument.ChildNodes.Count > 0)
                    {
                        var node = xmlDocument.SelectSingleNode($"//Option[@Name='{entry.Prefix}']");
                        if(node != null) value = node.Attributes["Value"].Value;
                    }
                    else
                    {
                        if (entry.Option != OptionType.Custom)
                        {
                            value = GetOptionDefault(entry.Option).ToString();
                        }
                        else
                        {
                            value = entry.DefaultValue;
                        }
                    }

                    if (!string.IsNullOrEmpty(value))
                    {
                        entry.Instance.SetValue(value);
                        ApplyOptionRealtime(entry.Option, value);
                        entry.OverrideValue = value;
                    }
                }
            }

            if(platformMode == PlatformMode.Computer)
            {
                if (GameQuality) GameQuality.Refresh();
            }

            OnOptionsUpdated?.Invoke();
        }

        /// <summary>
        /// Find and Get Option Behaviour Object from <see cref="GameOptions"/> Array by OptionType.
        /// </summary>
        public static OptionObject GetOptionBehaviour(OptionType type)
        {
            if (HasReference)
            {
                foreach (var s in Instance.GameOptions)
                {
                    if (s.OptionObjects.Any(x => x.Option == type))
                    {
                        foreach (var opt in s.OptionObjects)
                        {
                            if (opt.Option == type)
                            {
                                return opt;
                            }
                        }

                        break;
                    }
                }

                if (Instance.debugMode)
                    Debug.LogError(new NullReferenceException("Could not find option with \"" + type.ToString() + "\" type!").Message);
            }

            return null;
        }

        /// <summary>
        /// Find and Get Option Behaviour Object from <see cref="GameOptions"/> Array by Section and Option Prefix.
        /// </summary>
        public static OptionObject GetOptionBehaviour(string section, string prefix)
        {
            foreach (var s in Instance.GameOptions)
            {
                if (s.Section.Equals(section))
                {
                    foreach (var opt in s.OptionObjects)
                    {
                        if (opt.Prefix.Equals(prefix))
                        {
                            return opt;
                        }
                    }

                    break;
                }
            }

            if (Instance.debugMode)
                Debug.LogError(new NullReferenceException("Could not find option with \"" + prefix + "\" prefix in \"" + section + "\" section!").Message);

            return null;
        }

        /// <summary>
        /// Get Option Behaviour Value and Option Object by OptionType.
        /// </summary>
        public static bool GetOptionValueRef<T>(OptionType option, out T value, out OptionObject opt)
        {
            opt = GetOptionBehaviour(option);

            if (opt != null)
            {
                if (!string.IsNullOrEmpty(opt.OverrideValue))
                {
                    value = Parser.Convert<T>(opt.OverrideValue);
                }
                else if (!string.IsNullOrEmpty(opt.DefaultValue))
                {
                    value = Parser.Convert<T>(opt.DefaultValue);
                }
                else
                {
                    value = default;
                    return false;
                }

                return true;
            }

            if (Instance.debugMode)
                Debug.LogError(new KeyNotFoundException("Could not find option with \"" + option.ToString() + "\" type!").Message);

            value = default;
            return false;
        }

        /// <summary>
        /// Get Option Behaviour Value by OptionType.
        /// </summary>
        public static bool GetOptionValue<T>(OptionType option, out T value)
        {
            var opt = GetOptionBehaviour(option);

            if(opt != null)
            {
                if (!string.IsNullOrEmpty(opt.OverrideValue))
                {
                    value = Parser.Convert<T>(opt.OverrideValue);
                }
                else if (!string.IsNullOrEmpty(opt.DefaultValue))
                {
                    value = Parser.Convert<T>(opt.DefaultValue);
                }
                else
                {
                    value = default;
                    return false;
                }

                return true;
            }

            if (Instance.debugMode)
                Debug.LogError(new KeyNotFoundException("Could not find option with \"" + option.ToString() + "\" type!").Message);

            value = default;
            return false;
        }

        /// <summary>
        /// Get Option Behaviour Value by Section and Option Prefix.
        /// </summary>
        public static bool GetOptionValue<T>(string prefix, out T value)
        {
            foreach (var entry in Instance.GameOptions)
            {
                foreach (var option in entry.OptionObjects)
                {
                    if (option.Prefix.Equals(prefix))
                    {
                        if (!string.IsNullOrEmpty(option.OverrideValue))
                        {
                            value = Parser.Convert<T>(option.OverrideValue);
                        }
                        else if (!string.IsNullOrEmpty(option.DefaultValue))
                        {
                            value = Parser.Convert<T>(option.DefaultValue);
                        }
                        else
                        {
                            value = default;
                            return false;
                        }

                        return true;
                    }
                }
            }

            if (Instance.debugMode)
                Debug.LogError(new KeyNotFoundException("Could not find option with \"" + prefix + "\" prefix!").Message);

            value = default;
            return false;
        }

        /// <summary>
        /// Reset Changed Status of the Option Objects.
        /// </summary>
        public void ResetChangedStatus()
        {
            foreach (var section in GameOptions)
            {
                for (int i = 0; i < section.OptionObjects.Count(); i++)
                {
                    OptionObject entry = section.OptionObjects[i];
                    entry.IsChanged = false;
                    section.OptionObjects[i] = entry;
                }
            }
        }

        /// <summary>
        /// Apply Desktop Resolution.
        /// </summary>
        public void ApplyResolution(int index, int fullscreen)
        {
            if (index > -1)
            {
                ScreenResolution resolution = resolutions[index];
                FullScreenMode fullScreenMode = fullscreen == 0 ? FullScreenMode.ExclusiveFullScreen : fullscreen == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

                if (showRefreshRate && resolution.refreshRate > 0)
                {
                    Screen.SetResolution(resolution.width, resolution.height, fullScreenMode, resolution.refreshRate);
                }
                else
                {
                    Screen.SetResolution(resolution.width, resolution.height, fullScreenMode);
                }
            }

            if (fullscreen > -1)
            {
                FullScreenMode fullScreenMode = fullscreen == 0 ? FullScreenMode.ExclusiveFullScreen : fullscreen == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                Screen.fullScreenMode = fullScreenMode;
            }
        }

        /// <summary>
        /// Get Resolution Index.
        /// </summary>
        public int GetResolution(Resolution res)
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == res.width && resolutions[i].height == res.height)
                {
                    return i;
                }
            }

            return default;
        }

        public struct ScreenResolution : IEquatable<ScreenResolution>
        {
            public int width;
            public int height;
            public int refreshRate;

            public ScreenResolution(int w, int h, int rate)
            {
                width = w;
                height = h;
                refreshRate = rate;
            }

            public bool Equals(ScreenResolution other)
            {
                return width == other.width && height == other.height;
            }
        }

        [Serializable]
        public struct InputOption
        {
            public string PrettyName;
            public InputHandler.Device Device;
        }

        [Serializable]
        public sealed class OptionSection
        {
            public string Section;
            public List<OptionObject> OptionObjects = new List<OptionObject>();
        }

        [Serializable]
        public sealed class OptionObject
        {
            public string Prefix;
            public OptionBehaviour Instance;
            public OptionType Option = OptionType.Custom;
            public OptionVisibleType VisibleType = OptionVisibleType.Both;
            public bool EnabledOnConsole = true;
            public string DefaultValue;

            [ReadOnly]
            public string OverrideValue;

            [HideInInspector]
            public bool IsChanged;
        }
    }
}