/*
 * InputHandler.cs - by ThunderWire Studio
 * Ver. 2.0
*/

using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using ThunderWire.Utility;
using ThunderWire.Helpers;

namespace ThunderWire.Input
{
    /// <summary>
    /// Handler for Unity Input System.
    /// Requires* (Unity Input System Package)
    /// Supports: Keyboard, Mouse, PS4, Xbox
    /// </summary>
    public class InputHandler : Singleton<InputHandler>
    {
        public enum Device
        {
            None,
            MouseKeyboard,
            DualshockGamepad,
            XboxGamepad
        };

        public const string INPUT_FILENAME = "UserInputs.xml";
        public const string NULL_INPUT = "null";

        public string SerializationFolder
        {
            get => SerializationHelper.Settings.GetSerializationPath();
        }

        public string InputPath
        {
            get => Path.Combine(SerializationFolder, INPUT_FILENAME);
        }

        public const string KEYBOARD_BINDING = "<Keyboard>";
        public const string MOUSE_BINDING = "<Mouse>";
        public const string DUALSHOCK_BINDING = "<DualShockGamepad>";
        public const string XINPUT_BINDING = "<XInputController>";

        public const string DEFAULTGROUP_PC = "PC";
        public const string DEFAULTGROUP_PS4 = "PS4";
        public const string DEFAULTGROUP_XBOX = "Xbox";

        private const string PREFS_INPUT = "InputController";

        public static bool InputIsInitialized => Instance.isReaded;

        public InputActionAsset inputActionAsset;
        public CrossPlatformSprites inputSprites;
        public ActionSchemeList actionSchemes = new ActionSchemeList();

        public Device prefferedDevice = Device.None;

        public string defaultMapString;
        public int defaultMapID;
        public bool debugMode;
        public bool applyManually;
        public bool maskInputManually;

        public static string DefaultMap
        {
            get
            {
                if (HasReference)
                {
                    return Instance.defaultMapString;
                }

                return default;
            }
        }

        public static int DefaultMapID
        {
            get
            {
                if (HasReference)
                {
                    return Instance.defaultMapID;
                }

                return default;
            }
        }

        /// <summary>
        /// Event will be called, when the inputs will be initialized.
        /// </summary>
        public static event Action OnInputsInitialized;

        /// <summary>
        /// Event will be called, when the inputs will be initialized or updated.
        /// </summary>
        public static event Action<Device, ActionBinding[]> OnInputsUpdated;

        /// <summary>
        /// Event will be called, when the current input device is masked and the new input device is active.
        /// </summary>
        public static event Action<Device> OnDeviceMasked;

        /// <summary>
        /// Event will be called, when the input device status will be changed.
        /// </summary>
        public static event Action<Device, InputDeviceChange> OnDeviceStatusChanged;

        /// <summary>
        /// Event will be called, when the input devices will be updated or changed.
        /// </summary>
        public static event Action<Device[]> OnDevicesUpdated;

        /// <summary>
        /// Event will be called, before rebind of the action.
        /// </summary>
        public static event Action<ActionBinding.CompositePart> OnPrepareRebind;

        /// <summary>
        /// Event will be called, when the action with the same input will be found.
        /// </summary>
        public static event Action<string> OnActionDuplicate;

        /// <summary>
        /// Event will be called, after rebind cancellation.
        /// </summary>
        public static event Action OnRebindCancelled;

        /// <summary>
        /// The current active Input Device Type
        /// </summary>
        public static Device CurrentDevice
        {
            get
            {
                if(Instance.activeDevice == Device.None)
                {
                    Instance.GetConnectedDevices();
                }

                return Instance.activeDevice;
            }
        }

        public List<Device> connectedDevices = new List<Device>();

        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        public List<RebindContext> preparedRebinds = new List<RebindContext>();
        private static readonly List<ButtonInstance> buttonInstances = new List<ButtonInstance>();
        private static RebindContext[] duplicateContexts = new RebindContext[2];

        private int prefsInputDevice = 0;
        private Device activeDevice = Device.None;
        private bool isReaded = false;

        void OnEnable()
        {
            InputSystem.onDeviceChange += (device, state) =>
            {
                Device deviceType = GetDevice(device);

                switch (state)
                {
                    case InputDeviceChange.Added:
                    case InputDeviceChange.Reconnected:
                        if (!connectedDevices.Contains(deviceType))
                        {
                            connectedDevices.Add(deviceType);
                        }
                        break;
                    case InputDeviceChange.Disconnected:
                    case InputDeviceChange.Removed:
                    case InputDeviceChange.Disabled:
                        if (connectedDevices.Contains(deviceType))
                        {
                            connectedDevices.Remove(deviceType);
                        }
                        break;
                }

                GetConnectedDevices();
                OnDevicesUpdated?.Invoke(connectedDevices.ToArray());
                OnDeviceStatusChanged?.Invoke(deviceType, state);
            };
        }

        private void OnDestroy()
        {
            if (rebindOperation != null)
            {
                Instance.rebindOperation?.Cancel();
                Instance.CleanRebindOperation();
            }
        }

        private void OnDisable()
        {
            if (rebindOperation != null)
            {
                Instance.rebindOperation?.Cancel();
                Instance.CleanRebindOperation();
            }
        }

        void Awake()
        {
            InitializeInputs();
        }

        async void InitializeInputs()
        {
            foreach (var actionMap in inputActionAsset.actionMaps)
            {
                ActionBinding[] bindings = new ActionBinding[actionMap.actions.Count];

                for (int i = 0; i < actionMap.actions.Count; i++)
                {
                    bindings[i] = new ActionBinding(actionMap.actions[i]);
                }

                actionSchemes.list.Add(new ActionScheme(actionMap.name, bindings));
            }

            prefsInputDevice = (int)Device.None;
            if (PlayerPrefs.HasKey(PREFS_INPUT))
            {
                prefsInputDevice = PlayerPrefs.GetInt(PREFS_INPUT);
            }

            await Task.Run(() => GetConnectedDevices());
            OnDevicesUpdated?.Invoke(connectedDevices.ToArray());
            if (debugMode) Debug.Log("Controller: " + activeDevice.ToString());

            if (File.Exists(InputPath))
            {
                isReaded = await ReadInputOverrides(InputPath);
                if(debugMode) Debug.Log("[Input] Inputs readed successfully.");
            }
            else isReaded = true;

            if (!maskInputManually)
            {
                string bindingMask = activeDevice.DeviceToGroup();

                if (!string.IsNullOrEmpty(bindingMask))
                {
                    inputActionAsset.bindingMask = InputBinding.MaskByGroup(bindingMask);
                }
            }

            inputActionAsset.Enable();
            if (debugMode) Debug.Log("[Input] Inputs initialized.");
        }

        IEnumerator Start()
        {
            yield return new WaitUntil(() => isReaded);
            OnInputsInitialized?.Invoke();
            OnInputsUpdated?.Invoke(activeDevice, actionSchemes[defaultMapID].ActionBindings);
        }

        Task<XmlDocument> WriteOverridesToXML()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("InputScheme");

            foreach (var action in actionSchemes[defaultMapID].ActionBindings)
            {
                XmlNode node_action = xmlDoc.CreateElement("Action");

                foreach (var binding in action.actionBindings)
                {
                    foreach (ActionBinding.CompositePart composite in binding.compositeParts)
                    {
                        string currPath = composite.GetPrettyPath();

                        XmlNode node_binding = xmlDoc.CreateElement("Binding");
                        XmlAttribute attr_index = xmlDoc.CreateAttribute("Index");
                        XmlAttribute attr_name = xmlDoc.CreateAttribute("Name");
                        XmlAttribute attr_bind = xmlDoc.CreateAttribute("Bind");

                        attr_name.Value = action.name;
                        node_action.Attributes.Append(attr_name);

                        attr_index.Value = composite.bindingIndex.ToString();
                        node_binding.Attributes.Append(attr_index);

                        attr_bind.Value = currPath;
                        node_binding.Attributes.Append(attr_bind);

                        node_action.AppendChild(node_binding);
                    }
                }

                if (node_action.HasChildNodes)
                {
                    rootNode.AppendChild(node_action);
                }
            }

            xmlDoc.AppendChild(rootNode);
            return Task.FromResult(xmlDoc);
        }

        async Task<bool> ReadInputOverrides(string inputPath)
        {
            string xmlData = string.Empty;

            using (StreamReader sr = new StreamReader(inputPath))
            {
                xmlData = await sr.ReadToEndAsync();
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            foreach (XmlNode actionNode in xmlDoc.DocumentElement.ChildNodes)
            {
                string name = actionNode.Attributes["Name"].Value;

                foreach (XmlNode bindingNode in actionNode.ChildNodes)
                {
                    int bindingIndex = int.Parse(bindingNode.Attributes["Index"].Value);
                    string ovrdControl = bindingNode.Attributes["Bind"].Value;
                    string ovrdPath = FormatBindingPath(ovrdControl);
                    var composite = CompositeOf(name, bindingIndex: bindingIndex);

                    if (composite.GetPrettyPath() != ovrdControl)
                    {
                        InputAction action = GetInputAction(name);
                        string path = ovrdControl != NULL_INPUT ? ovrdPath : ovrdControl;
                        string displayString = path != NULL_INPUT ? RealDisplayString(ovrdPath) : "None";

                        ApplyBindingOverride(new RebindContext(action, bindingIndex, path)
                        {
                            displayString = displayString
                        });
                    }
                }
            }

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Disable current and activate new device type.
        /// </summary>
        public static void MaskInputByDevice(Device device)
        {
            string bindingMask = "None";
            bool error_flag = false;

            if (Instance.connectedDevices.Contains(device) && device != Device.None)
            {
                bindingMask = device.DeviceToGroup();
            }
            else if (Instance.connectedDevices.Count == 1 && Instance.connectedDevices[0] != Device.None)
            {
                bindingMask = Instance.connectedDevices[0].DeviceToGroup();
                if (Instance.debugMode) Debug.Log("The specified device is not connected! The input will be masked by the first connected device.");
            }
            else
            {
                error_flag = true;
            }

            if (!error_flag && !string.IsNullOrEmpty(bindingMask) && !bindingMask.Equals("None"))
            {
                PlayerPrefs.SetInt(PREFS_INPUT, (int)device);
                Instance.inputActionAsset.bindingMask = InputBinding.MaskByGroup(bindingMask);
                Instance.activeDevice = device;
                OnDeviceMasked?.Invoke(device);
                return;
            }

            throw new NullReferenceException("Input cannot be masked! The specified device is not connected!");
        }

        /// <summary>
        /// Send an event with information about the current active input device and the action bindings.
        /// </summary>
        public static void SendInputUpdateEvent()
        {
            OnInputsUpdated?.Invoke(Instance.activeDevice, Instance.actionSchemes[DefaultMapID].ActionBindings);
        }

        public void GetConnectedDevices()
        {
            if (Gamepad.current != null && Gamepad.current is DualShockGamepad)
            {
                if (!connectedDevices.Contains(Device.DualshockGamepad))
                {
                    connectedDevices.Add(Device.DualshockGamepad);
                }
            }
            else if(connectedDevices.Contains(Device.DualshockGamepad))
            {
                connectedDevices.Remove(Device.DualshockGamepad);
            }

            if (Gamepad.current != null && Gamepad.current is XInputController xInput &&
                xInput.subType == XInputController.DeviceSubType.Gamepad)
            {
                if (!connectedDevices.Contains(Device.XboxGamepad))
                {
                    connectedDevices.Add(Device.XboxGamepad);
                }
            }
            else if (connectedDevices.Contains(Device.XboxGamepad))
            {
                connectedDevices.Remove(Device.XboxGamepad);
            }

            if (Keyboard.current != null && Mouse.current != null)
            {
                if (!connectedDevices.Contains(Device.MouseKeyboard))
                {
                    connectedDevices.Add(Device.MouseKeyboard);
                }
            }
            else if (connectedDevices.Contains(Device.MouseKeyboard))
            {
                connectedDevices.Remove(Device.MouseKeyboard);
            }

            if (connectedDevices.Count > 0)
            {
                if (prefsInputDevice > 0 && connectedDevices.Contains((Device)prefsInputDevice))
                {
                    activeDevice = (Device)prefsInputDevice;
                }
                else if (prefferedDevice != Device.None && connectedDevices.Contains(prefferedDevice))
                {
                    activeDevice = prefferedDevice;
                }
                else
                {
                    activeDevice = connectedDevices[0];
                }
            }
            else
            {
                if(debugMode) Debug.LogError("[Device Init] Supported Input Device is not connected!");
            }
        }

        /// <summary>
        /// Read Input Value as Type.
        /// </summary>
        public static T ReadInput<T>(string ActionName, string ActionMap = "Default") where T : struct
        {
            InputAction inputAction = GetInputAction(ActionName, ActionMap);

            if (inputAction != null)
            {
                return inputAction.ReadValue<T>();
            }

            return default;
        }

        /// <summary>
        /// Read Input Value as Object.
        /// </summary>
        public static object ReadInput(string ActionName, string ActionMap = "Default")
        {
            InputAction inputAction = GetInputAction(ActionName, ActionMap);

            if (inputAction != null)
            {
                return inputAction.ReadValueAsObject();
            }

            return default;
        }

        /// <summary>
        /// Read Input Value as Button.
        /// </summary>
        public static bool ReadButton(string ActionName, string ActionMap = "Default")
        {
            InputAction inputAction = GetInputAction(ActionName, ActionMap);

            if (inputAction != null)
            {
                if (inputAction.type == InputActionType.Button)
                {
                    return Convert.ToBoolean(inputAction.ReadValueAsObject());
                }
                else
                {
                    throw new NotSupportedException("The Input Action must be a button type!");
                }
            }

            return default;
        }

        /// <summary>
        /// Read input as button once per script instance.
        /// </summary>
        public static bool ReadButtonOnce(MonoBehaviour Instance, string ActionName, string ActionMap = "Default")
        {
            if (ReadButton(ActionName, ActionMap))
            {
                if (!buttonInstances.Any(x => x.caller == Instance && x.action == ActionName))
                {
                    buttonInstances.Add(new ButtonInstance(Instance, ActionName));
                    return true;
                }
            }
            else
            {
                buttonInstances.RemoveAll(x => x.caller == Instance && x.action == ActionName);
            }

            return false;
        }

        /// <summary>
        /// Get Input System Action Reference.
        /// </summary>
        public static InputAction GetInputAction(string ActionName, string ActionMap = "Default")
        {
            if (ActionMap.Equals("Default"))
            {
                ActionMap = DefaultMap;
                if (string.IsNullOrEmpty(ActionMap)) return default;
            }

            InputAction inputAction;
            if (!string.IsNullOrEmpty(ActionMap))
            {
                inputAction = Instance.inputActionAsset.FindActionMap(ActionMap, true).FindAction(ActionName, true);
            }
            else
            {
                inputAction = Instance.inputActionAsset.FindAction(ActionName, true);
            }

            return inputAction;
        }

        /// <summary>
        /// Get Binding Data of Action.
        /// </summary>
        public static ActionBinding BindingOf(string ActionName, string ActionMap = "Default")
        {
            if (ActionMap.Equals("Default"))
            {
                ActionMap = DefaultMap;
                if (string.IsNullOrEmpty(ActionMap)) return default;
            }

            if (Instance.actionSchemes.list.Count > 0)
            {
                ActionScheme actionScheme = Instance.actionSchemes.list.Where(x => x.SchemeName.Equals(ActionMap)).FirstOrDefault();

                if (actionScheme != null)
                {
                    return actionScheme[ActionName] ?? null;
                }
            }

            return default;
        }

        /// <summary>
        /// Get Action Composite Data by Binding Index.
        /// </summary>
        /// <param name="ActionName"></param>
        public static ActionBinding.CompositePart CompositeOf(string ActionName, int bindingIndex, string ActionMap = "Default")
        {
            if (ActionMap.Equals("Default"))
            {
                ActionMap = DefaultMap;
                if (string.IsNullOrEmpty(ActionMap)) return default;
            }

            foreach (var binding in Instance.actionSchemes[ActionMap][ActionName].actionBindings)
            {
                foreach (var composite in binding.compositeParts)
                {
                    if (composite.bindingIndex == bindingIndex)
                    {
                        return new ActionBinding.CompositePart(composite);
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Get Action Composite Data by Current Device
        /// </summary>
        /// <param name="ActionName"></param>
        public static ActionBinding.CompositePart CompositeOf(string ActionName, string ActionMap = "Default")
        {
            if (ActionMap.Equals("Default"))
            {
                ActionMap = DefaultMap;
                if (string.IsNullOrEmpty(ActionMap)) return default;
            }

            string group = CurrentDevice.DeviceToGroup();

            foreach (var binding in Instance.actionSchemes[ActionMap][ActionName].actionBindings)
            {
                foreach (var composite in binding.compositeParts)
                {
                    if (composite.groups.Any(x => x.Equals(group)))
                    {
                        return new ActionBinding.CompositePart(composite);
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Get Action Composite Data by Group.
        /// </summary>
        public static ActionBinding.CompositePart CompositeOfGroup(string ActionName, string Group, int bindingIndex = -1, string ActionMap = "Default")
        {
            if (ActionMap.Equals("Default"))
            {
                ActionMap = DefaultMap;
                if (string.IsNullOrEmpty(ActionMap)) return default;
            }

            foreach (var binding in Instance.actionSchemes[ActionMap][ActionName].actionBindings)
            {
                foreach (var composite in binding.compositeParts)
                {
                    if (composite.groups.Any(x => x.Equals(Group)) && (bindingIndex < 0 || composite.bindingIndex == bindingIndex))
                    {
                        return new ActionBinding.CompositePart(composite);
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Check if the action or its composite is rebindable.
        /// </summary>
        public static bool IsActionNotRebindable(string ActionName, int bindingIndex = -1, string ActionMap = "Default")
        {
            if (ActionMap.Equals("Default"))
            {
                ActionMap = DefaultMap;
                if (string.IsNullOrEmpty(ActionMap)) return default;
            }

            foreach (var action in Instance.actionSchemes[ActionMap].ActionBindings)
            {
                if (action.name == ActionName)
                {
                    bool notRebindable = action.globalNotRebindable;

                    if (bindingIndex >= 0)
                    {
                        foreach (var binding in action.actionBindings)
                        {
                            foreach (var composite in binding.compositeParts)
                            {
                                if (composite.bindingIndex == bindingIndex)
                                {
                                    return notRebindable || composite.notRebindable;
                                }
                            }
                        }
                    }

                    return notRebindable;
                }
            }

            return false;
        }

        /// <summary>
        /// Compares the first action with the second action.
        /// </summary>
        public static bool IsCompositesSame(string FirstAction, string SecondAction, string Group = "Default", string ActionMap = "Default")
        {
            if (ActionMap.Equals("Default"))
            {
                ActionMap = DefaultMap;
                if (string.IsNullOrEmpty(ActionMap)) return default;
            }

            if (Group.Equals("Default"))
            {
                ActionBinding.CompositePart FirstComposite = CompositeOf(FirstAction, ActionMap);
                ActionBinding.CompositePart SecondComposite = CompositeOf(SecondAction, ActionMap);

                return FirstComposite.bindingPath == SecondComposite.bindingPath;
            }
            else
            {
                ActionBinding.CompositePart FirstComposite = CompositeOfGroup(FirstAction, Group, -1, ActionMap);
                ActionBinding.CompositePart SecondComposite = CompositeOfGroup(SecondAction, Group, -1, ActionMap);

                return FirstComposite.bindingPath == SecondComposite.bindingPath;
            }
        }

        /// <summary>
        /// Check if the specified action exists in the action scheme.
        /// </summary>
        public static bool IsActionExist(string Action, string ActionMap = "Default")
        {
            if (ActionMap.Equals("Default"))
            {
                ActionMap = DefaultMap;
                if (string.IsNullOrEmpty(ActionMap)) return default;
            }

            return Instance.actionSchemes[ActionMap][Action] != null;
        }

        /// <summary>
        /// Get Mouse Pointer position.
        /// </summary>
        public static Vector2 GetMousePosition()
        {
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }
            else
            {
                Debug.LogError("[Mouse Init] Mouse is not connected!");
            }

            return default;
        }

        /// <summary>
        /// Check if any input key is pressed.
        /// </summary>
        public static bool AnyInputPressed()
        {
            Device device = CurrentDevice;

            if (device == Device.MouseKeyboard)
            {
                Mouse m = Mouse.current;
                Keyboard k = Keyboard.current;

                bool m_pressed = m != null && (m.leftButton.isPressed || m.middleButton.isPressed || m.rightButton.isPressed);
                return k != null && k.allControls.Any(x => x.IsPressed()) || m_pressed;
            }
            else if (device != Device.None)
            {
                foreach (var button in (GamepadButton[])Enum.GetValues(typeof(GamepadButton)))
                {
                    if (Gamepad.current[button].isPressed)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Convert Action Display String to Control String.
        /// </summary>
        public static string DisplayToControlString(InputAction action, int bindingIndex)
        {
            try
            {
                string displayString = action.GetBindingDisplayString(bindingIndex);
                return displayString.Replace("Press ", string.Empty).Trim();
            }
            catch
            {
                throw new NotSupportedException($"Cannot get display string from \"{action.name}\" action with \"Any\" binding!");
            }
        }

        /// <summary>
        /// Convert Action Binding Path to Control String.
        /// </summary>
        public static string RealDisplayString(string bindingPath)
        {
            if (bindingPath.Contains("/"))
            {
                string split = bindingPath.Split('/')[1].Trim();

                var tmpDisplay = Regex.Replace(split, "([^A-Z ])([A-Z])", "$1 $2");
                return Regex.Replace(tmpDisplay, "([A-Z]+)([A-Z][^A-Z$])", "$1 $2").Trim().TitleCase();
            }

            return bindingPath.Trim().TitleCase();
        }

        /// <summary>
        /// Get asset that contains input sprites.
        /// </summary>
        public static CrossPlatformSprites GetSprites() => Instance.inputSprites;

        /// <summary>
        /// Start Rebind Operation.
        /// </summary>
        /// <param name="ActionName">Action reference name.</param>
        /// <param name="BindingIndex">Action composite binding index.</param>
        public static void StartInteractiveRebind(string ActionName, int BindingIndex)
        {
            InputAction input = Instance.inputActionAsset.FindAction(ActionName, true);

            if (IsActionNotRebindable(ActionName, BindingIndex))
            {
                OnRebindCancelled?.Invoke();
                if (Instance.debugMode) Debug.Log("[Input] Rebind Canceled - Action is not rebindable!");
                return;
            }

            if (Instance.debugMode) Debug.Log("[Input] Rebind Started - Press any control.");
            Instance.inputActionAsset.Disable();
            Instance.PerformInteractiveRebind(input, BindingIndex);
        }

        /// <summary>
        /// Accept or Reject the rewriting of an action with the same control.
        /// </summary>
        public static void ManageRewriteDuplicate(bool accept, bool keepDuplicate = false)
        {
            if (accept)
            {
                if (duplicateContexts[0] != null && duplicateContexts[1] != null)
                {
                    Instance.PrepareRebind(duplicateContexts[0]);
                    if(!keepDuplicate)
                        Instance.PrepareRebind(duplicateContexts[1]);

                    duplicateContexts = new RebindContext[2];

                    if (!Instance.applyManually) ApplyChanges();
                    if (Instance.debugMode) Debug.Log("[Input] Rebind Completed.");
                }
            }
            else
            {
                OnRebindCancelled?.Invoke();
                Instance.preparedRebinds.Clear();

                if (Instance.debugMode) Debug.Log("[Input] Rebind Cancelled.");
            }
        }

        /// <summary>
        /// Apply and Serialize Input Changes.
        /// </summary>
        public static void ApplyChanges()
        {
            if(Instance.preparedRebinds.Count > 0)
            {
                try
                {
                    foreach (var rebind in Instance.preparedRebinds)
                    {
                        if(rebind.action.bindings[rebind.bindingIndex].path == rebind.overridePath)
                        {
                            Instance.ApplyRemoveOverride(rebind);
                        }
                        else
                        {
                            Instance.ApplyBindingOverride(rebind);
                        }
                    }
                }
                finally 
                {
                    Instance.preparedRebinds.Clear();
                    Instance.PackAndWriteOverrides();

                    if (Instance.debugMode) Debug.Log("[Input] Changes Applied.");
                }
            }
        }

        string FormatBindingPath(string bindingPath)
        {
            string[] pathSplit = bindingPath.Split('.');
            string newBindingPath = string.Format("<{0}>/", pathSplit[0]);

            for (int i = 1; i < pathSplit.Length; i++)
            {
                newBindingPath += pathSplit[i];
            }

            return newBindingPath;
        }

        void PerformInteractiveRebind(InputAction action, int bindingIndex)
        {
            bool hasDuplicate = false;

            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .OnCancel(operation =>
                {
                    if (!hasDuplicate)
                    {
                        OnRebindCancelled?.Invoke();
                    }

                    inputActionAsset.Enable();
                    CleanRebindOperation();
                })
                .OnComplete(operation =>
                {
                    if (!hasDuplicate && !applyManually)
                    {
                        ApplyChanges();
                    }

                    inputActionAsset.Enable();
                    CleanRebindOperation();
                })
                .OnApplyBinding((operation, path) =>
                {
                    if(PreparedContainsBindingPath(path, action, bindingIndex, out RebindContext dupContext))
                    {
                        duplicateContexts[0] = new RebindContext(action, bindingIndex, path);
                        duplicateContexts[1] = new RebindContext(dupContext.action, dupContext.bindingIndex, NULL_INPUT);

                        var composite = CompositeOf(dupContext.action.name, dupContext.bindingIndex);
                        string displayAction = !string.IsNullOrEmpty(composite.partString) ?
                            string.Format("{0} {1}", dupContext.action.name, composite.partString) :
                            dupContext.action.name;

                        OnActionDuplicate?.Invoke(displayAction);

                        hasDuplicate = true;
                        rebindOperation.Cancel();
                    }
                    else if(ContainsBindingPath(path, action, bindingIndex, out InputAction dupAction, out int dupIndex))
                    {
                        if (!IsActionNotRebindable(dupAction.name, dupIndex))
                        {
                            if (!preparedRebinds.Any(x => x.bindingIndex == dupIndex))
                            {
                                duplicateContexts[0] = new RebindContext(action, bindingIndex, path);
                                duplicateContexts[1] = new RebindContext(dupAction, dupIndex, NULL_INPUT);

                                var composite = CompositeOf(dupAction.name, dupIndex);
                                string displayAction = !string.IsNullOrEmpty(composite.partString) ?
                                    string.Format("{0} {1}", dupAction.name, composite.partString) :
                                    dupAction.name;

                                OnActionDuplicate?.Invoke(displayAction);

                                hasDuplicate = true;
                                rebindOperation.Cancel();
                            }
                            else
                            {
                                PrepareRebind(new RebindContext(action, bindingIndex, path));
                                OnRebindCancelled?.Invoke();
                                if (Instance.debugMode) Debug.Log("[Input] Rebind Completed.");
                            }
                        }
                        else
                        {
                            OnRebindCancelled?.Invoke();
                            if (Instance.debugMode) Debug.Log("[Input] Rebind Canceled - An action with the same binding path is not rebindable!");
                        }
                    }
                    else
                    {
                        PrepareRebind(new RebindContext(action, bindingIndex, path));
                        OnRebindCancelled?.Invoke();
                        if (Instance.debugMode) Debug.Log("[Input] Rebind Completed.");
                    }
                }).WithCancelingThrough("<Keyboard>/escape"); // Input System Package BUG: 'escape' also cancels rebinding with 'e' key. (will be fixed in 1.4 version)

            rebindOperation.Start();
        }

        void PrepareRebind(RebindContext context)
        {
            if(context != null)
            {
                var resultComposite = CompositeOf(context.action.name, context.bindingIndex);
                string path = string.IsNullOrEmpty(context.overridePath) ? resultComposite.bindingPath : context.overridePath;               
                string displayString = path != NULL_INPUT ? RealDisplayString(path) : "None";

                if (preparedRebinds.Any(x => x.action == context.action && x.bindingIndex == context.bindingIndex))
                {
                    foreach (var item in preparedRebinds)
                    {
                        if(item.action == context.action && item.bindingIndex == context.bindingIndex)
                        {
                            item.overridePath = path;
                            item.displayString = displayString;
                            resultComposite.overridePath = path;
                            resultComposite.displayString = displayString;
                            break;
                        }
                    }
                }
                else
                {
                    resultComposite.overridePath = path;
                    resultComposite.displayString = displayString;
                    context.displayString = displayString;
                    preparedRebinds.Add(context);
                }
                
                OnPrepareRebind?.Invoke(resultComposite);
            }
        }

        void ApplyBindingOverride(RebindContext context)
        {
            context.action.ApplyBindingOverride(context.bindingIndex, context.overridePath);

            var composite = CompositeOfRef(context.action.name, context.bindingIndex);
            if(composite != null)
            {
                composite.overridePath = context.overridePath;
                composite.displayString = context.displayString;
            }
        }

        void ApplyRemoveOverride(RebindContext context)
        {
            context.action.RemoveBindingOverride(context.bindingIndex);

            var composite = CompositeOfRef(context.action.name, context.bindingIndex);
            if (composite != null)
            {
                composite.overridePath = context.overridePath;
                composite.displayString = context.displayString;
            }
        }

        private ActionBinding.CompositePart CompositeOfRef(string ActionName, int bindingIndex, string ActionMap = "Default")
        {
            if (ActionMap.Equals("Default"))
            {
                ActionMap = DefaultMap;
                if (string.IsNullOrEmpty(ActionMap)) return default;
            }

            foreach (var binding in Instance.actionSchemes[ActionMap][ActionName].actionBindings)
            {
                foreach (var composite in binding.compositeParts)
                {
                    if (composite.bindingIndex == bindingIndex)
                    {
                        return composite;
                    }
                }
            }

            return default;
        }

        void CleanRebindOperation()
        {
            rebindOperation?.Dispose();
            rebindOperation = null;
        }

        async void PackAndWriteOverrides()
        {
            XmlDocument xml = await WriteOverridesToXML();
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw)
            {
                Formatting = Formatting.Indented
            };
            xml.WriteTo(xw);

            if (!Directory.Exists(SerializationFolder))
            {
                Directory.CreateDirectory(SerializationFolder);
            }

            using (StreamWriter stream = new StreamWriter(InputPath))
            {
                await stream.WriteAsync(sw.ToString());
            }

            if (debugMode) Debug.Log("[XML] Inputs was successfully writed!");
        }

        Device GetDevice(InputDevice device)
        {
            if (device is DualShockGamepad)
            {
                return Device.DualshockGamepad;
            }
            else if (device is XInputController xInput)
            {
                if (xInput.subType == XInputController.DeviceSubType.Gamepad)
                {
                    return Device.XboxGamepad;
                }
            }
            else if (device is Keyboard || device is Mouse)
            {
                return Device.MouseKeyboard;
            }

            return Device.None;
        }

        string FormatBindingPathWithDevice(string bindingPath)
        {
            if (activeDevice.IsGamepadDevice() == 1) {

                Queue<string> pathSplit = new Queue<string>(bindingPath.Split('/'));
                pathSplit.Dequeue();

                string controller = activeDevice == Device.DualshockGamepad ? DUALSHOCK_BINDING :
                    activeDevice == Device.XboxGamepad ? XINPUT_BINDING : NULL_INPUT;

                if (controller != NULL_INPUT)
                {
                    return controller + "/" + string.Join("/", pathSplit);
                }
            }

            return bindingPath;
        }

        bool PreparedContainsBindingPath(string path, InputAction currentAction, int currentIndex, out RebindContext duplicate)
        {
            string devicePath = FormatBindingPathWithDevice(path);

            foreach (var context in preparedRebinds)
            {
                string prepDevicePath = FormatBindingPathWithDevice(context.overridePath);

                if (devicePath == prepDevicePath && (context.action != currentAction || context.action == currentAction && context.bindingIndex != currentIndex))
                {
                    duplicate = context;
                    return true;
                }
            }

            duplicate = null;
            return false;
        }

        bool ContainsBindingPath(string bindingPath, InputAction currentAction, int currentIndex, out InputAction duplicateAction, out int duplicateIndex)
        {
            foreach (var action in actionSchemes[defaultMapID].ActionBindings)
            {
                foreach (var binding in action.actionBindings)
                {
                    foreach (var composite in binding.compositeParts)
                    {
                        if (action.action == currentAction && composite.bindingIndex == currentIndex)
                            continue;

                        string devicePath = FormatBindingPathWithDevice(bindingPath);
                        string mainPath = composite.bindingPath;
                        string ovrdPath = composite.overridePath;

                        bool isMainPath = (mainPath == bindingPath || mainPath == devicePath) && ovrdPath != NULL_INPUT && string.IsNullOrEmpty(ovrdPath);
                        bool isOvrdPath = ovrdPath == bindingPath || ovrdPath == devicePath;

                        if (isMainPath || isOvrdPath)
                        {
                            duplicateAction = action.action;
                            duplicateIndex = composite.bindingIndex;
                            return true;
                        }
                    }
                }
            }

            duplicateAction = new InputAction();
            duplicateIndex = -1;
            return false;
        }

        private sealed class ButtonInstance
        {
            public MonoBehaviour caller;
            public string action;

            public ButtonInstance(MonoBehaviour caller, string action)
            {
                this.caller = caller;
                this.action = action;
            }
        }

        [Serializable]
        public sealed class RebindContext
        {
            public InputAction action;
            public int bindingIndex;
            public string overridePath;
            public string displayString;

            public RebindContext(InputAction action, int bindingIndex, string overridePath)
            {
                this.action = action;
                this.bindingIndex = bindingIndex;
                this.overridePath = overridePath;
            }
        }
    }

    public static class InputHandlerUtility
    {
        public static string DeviceToGroup(this InputHandler.Device device)
        {
            return device == InputHandler.Device.MouseKeyboard ? InputHandler.DEFAULTGROUP_PC :
                device == InputHandler.Device.DualshockGamepad ? InputHandler.DEFAULTGROUP_PS4 :
                device == InputHandler.Device.XboxGamepad ? InputHandler.DEFAULTGROUP_XBOX :
                string.Empty;
        }

        /// <summary>
        /// Check if the Device is a Gamepad Type Device.
        /// </summary>
        /// <returns>
        /// 1 = Gamepad, 0 = Mouse & Keyboard, -1 = None
        /// </returns>
        public static int IsGamepadDevice(this InputHandler.Device device)
        {
            if (device == InputHandler.Device.DualshockGamepad || device == InputHandler.Device.XboxGamepad)
            {
                return 1;
            }
            else if (device == InputHandler.Device.MouseKeyboard)
            {
                return 0;
            }

            return -1;
        }

        public static string DeviceToPrettyName(this InputHandler.Device device)
        {
            return device == InputHandler.Device.MouseKeyboard ? "Mouse & Keyboard" :
                device == InputHandler.Device.DualshockGamepad ? "DualShock Gamepad" :
                device == InputHandler.Device.XboxGamepad ? "Xbox Gamepad" :
                "None";
        }
    }

    [Serializable]
    public sealed class ActionSchemeList
    {
        public List<ActionScheme> list = new List<ActionScheme>();

        /// <summary>
        /// Get Action Scheme by Scheme Name.
        /// </summary>
        public ActionScheme this[string schemeName]
        {
            get
            {
                var scheme = list.Where(x => x.SchemeName == schemeName).FirstOrDefault();
                if (scheme == null)
                {
                    throw new KeyNotFoundException($"Cannot find action scheme '{schemeName}' in '{this}'");
                }
                return scheme;
            }
        }

        public ActionScheme this[int index]
        {
            get => list[index];
        }
    }

    [Serializable]
    public sealed class ActionScheme
    {
        public string SchemeName;
        public ActionBinding[] ActionBindings;

        public ActionScheme(string scheme, ActionBinding[] bindings)
        {
            SchemeName = scheme;
            ActionBindings = bindings;
        }

        /// <summary>
        /// Get Action Binding by Action Name.
        /// </summary>
        public ActionBinding this[string actionName]
        {
            get
            {
                var action = ActionBindings.Where(x => x.name == actionName).SingleOrDefault();
                if (action == null)
                {
                    throw new KeyNotFoundException($"Cannot find action '{actionName}' in Action Scheme!");
                }
                return action;
            }
        }
    }

    [Serializable]
    public sealed class ActionBinding
    {
        [Serializable]
        public sealed class BindingList
        {
            public List<CompositePart> compositeParts = new List<CompositePart>();
            public bool isComposite;

            public BindingList(bool isComposite, CompositePart[] parts)
            {
                this.isComposite = isComposite;
                compositeParts = new List<CompositePart>(parts);
            }

            /// <summary>
            /// Get the composite parts assigned to the group.
            /// </summary>
            public CompositePart[] this[string group]
            {
                get
                {
                    var compositeParts = this.compositeParts.Where(x => x.groups.Any(y => y.Equals(group))).ToArray();
                    if (compositeParts == null || compositeParts.Length < 0)
                    {
                        throw new KeyNotFoundException($"Cannot find '{group}' group in Composite Parts!");
                    }

                    return compositeParts;
                }
            }
        }

        [Serializable]
        public sealed class CompositePart
        {
            public string isPartOfAction;
            public string bindingPath;
            public string overridePath;
            public string displayString;
            public string partString;
            public string[] groups;
            public bool notRebindable;
            public int bindingIndex;

            /// <summary>
            /// Get current binding path of the action.
            /// </summary>
            /// <returns>Format: &lt;Device&gt;/Input</returns>
            public string GetBindingPath()
            {
                if (!string.IsNullOrEmpty(overridePath))
                    return overridePath;

                return bindingPath;
            }

            /// <summary>
            /// Get pretty current binding path of the action.
            /// </summary>
            /// <returns>Format: Device.Input</returns>
            public string GetPrettyPath()
            {
                if (!string.IsNullOrEmpty(overridePath))
                    return overridePath.Replace("<", "").Replace(">", "").Replace("/", ".");

                return bindingPath.Replace("<", "").Replace(">", "").Replace("/", ".");
            }

            /// <summary>
            /// Get control key of the current binding path.
            /// </summary>
            /// <returns>Format: Input</returns>
            public string GetControlKeyPath()
            {
                string pathToQueue = bindingPath;

                if (!string.IsNullOrEmpty(overridePath))
                    pathToQueue = overridePath;

                string[] split = pathToQueue.Split('/');
                Queue<string> pathQueue = new Queue<string>(split);
                pathQueue.Dequeue();
                return string.Join("/", pathQueue.ToArray());
            }

            public CompositePart() { }

            public CompositePart(CompositePart other)
            {
                isPartOfAction = other.isPartOfAction;
                bindingPath = other.bindingPath;
                overridePath = other.overridePath;
                displayString = other.displayString;
                partString = other.partString;
                groups = other.groups;
                notRebindable = other.notRebindable;
                bindingIndex = other.bindingIndex;
            }
        }

        public string name;
        public InputAction action;

        public bool globalNotRebindable;
        public List<BindingList> actionBindings = new List<BindingList>();

        public ActionBinding(InputAction inputAction)
        {
            name = inputAction.name;
            action = inputAction;

            var bindings = inputAction.bindings;
            int bindingsCount = inputAction.bindings.Count;
            int bindingIndex;

            globalNotRebindable = inputAction.interactions.Contains("NotRebindable");

            for (bindingIndex = 0; bindingIndex < bindingsCount; bindingIndex++)
            {
                bool isComposite = inputAction.bindings[bindingIndex].isComposite;

                if (isComposite)
                {
                    int firstPartIndex = bindingIndex + 1;
                    int lastPartIndex = firstPartIndex;
                    while (lastPartIndex < bindingsCount && bindings[lastPartIndex].isPartOfComposite) ++lastPartIndex;
                    int partCount = lastPartIndex - firstPartIndex;

                    CompositePart[] parts = new CompositePart[partCount];

                    for (int i = 0; i < partCount; i++)
                    {
                        string[] groups = bindings[firstPartIndex + i].groups.Split(InputBinding.Separator).ToArray();
                        string bindingPath = bindings[firstPartIndex + i].path;
                        string displayString = InputHandler.RealDisplayString(bindings[firstPartIndex + i].path);

                        string partString = string.Empty;
                        string bindingName = bindings[firstPartIndex + i].name;
                        if (!string.IsNullOrEmpty(bindingName))
                        {
                            NameAndParameters nameParameters = NameAndParameters.Parse(bindingName);
                            partString = nameParameters.name.TitleCase();
                        }

                        bool notRebindable = globalNotRebindable ||
                            bindings[firstPartIndex + i].interactions.Contains("NotRebindable");

                        parts[i] = new CompositePart
                        {
                            isPartOfAction = name,
                            bindingPath = bindingPath,
                            overridePath = null,
                            displayString = displayString,
                            partString = partString,
                            groups = groups,
                            notRebindable = notRebindable,
                            bindingIndex = firstPartIndex + i
                        };
                    }

                    actionBindings.Add(new BindingList(true, parts));
                    bindingIndex += partCount;
                }
                else
                {
                    string[] groups = bindings[bindingIndex].groups.Split(InputBinding.Separator).ToArray();
                    string bindingPath = bindings[bindingIndex].path;
                    string displayString = InputHandler.RealDisplayString(bindings[bindingIndex].path);

                    string partString = string.Empty;
                    string bindingName = bindings[bindingIndex].name;
                    if (!string.IsNullOrEmpty(bindingName))
                    {
                        NameAndParameters nameParameters = NameAndParameters.Parse(bindingName);
                        partString = nameParameters.name.TitleCase();
                    }

                    bool notRebindable = globalNotRebindable ||
                        bindings[bindingIndex].interactions.Contains("NotRebindable");

                    CompositePart[] part = new CompositePart[1]
                    {
                        new CompositePart
                        {
                            isPartOfAction = name,
                            bindingPath = bindingPath,
                            overridePath = null,
                            displayString = displayString,
                            partString = partString,
                            groups = groups,
                            notRebindable = notRebindable,
                            bindingIndex = bindingIndex
                        }
                    };

                    actionBindings.Add(new BindingList(false, part));
                }
            }
        }

        public CompositePart this[int bindingIndex]
        {
            get
            {
                foreach (var binding in actionBindings)
                {
                    foreach (var composite in binding.compositeParts)
                    {
                        if (composite.bindingIndex == bindingIndex)
                            return composite;
                    }
                }

                throw new KeyNotFoundException($"Cannot find binding index '{bindingIndex}' in bindings!");
            }
        }
    }
}