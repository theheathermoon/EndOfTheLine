using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ThunderWire.Input;
using ThunderWire.Utility;
using HFPS.UI;

namespace HFPS.Systems
{
    [RequireComponent(typeof(OptionsController))]
    public class MenuController : Singleton<MenuController>
    {
        #region Structures
        public enum ViewType { Panel, Options }

        [Serializable]
        public sealed class PanelViewData
        {
            public string PanelTag;
            public GameObject PanelObject;
            public TabPanelEvents Events;
            public GameObject[] GamepadButtons;
            public bool IsSettings;
        }
        #endregion

        private OptionsController optionsController;
        private HFPS_GameManager gameManager;

        [Header("Inputs UI")]
        public GameObject ControlPrefab;
        public Transform ControlsParent;

        [Header("On Duplicate Action")]
        public GameObject DuplicateInputPanel;
        public Text DuplicateInputText;
        public Button DuplicateFirstButton;
        public Selectable[] DuplicateRestrict;

        [Header("Device Variant")]
        public GameObject[] KeyboardMouse;
        public GameObject[] Gamepad;

        [Header("Panel View")]
        public GameObject GeneralPanel;
        public PanelViewData[] panelViewDatas;

        [Header("Menu/Pause Select")]
        public bool selectFirstButton;
        public Selectable FirstButton;
        public Selectable FirstAltButton;

        public static event Action OnBeforeRebind;
        public static event Action OnAfterRebind;

        private readonly List<InputActionProperties> actionProperties = new List<InputActionProperties>();
        private readonly List<GameObject> allControls = new List<GameObject>();

        private InputHandler.Device currentDevice;
        private InputActionProperties currentRebind;

        private bool isInputApplied = false;
        private string OnDuplicateTextPC;
        private string OnDuplicateTextCon;

        [HideInInspector]
        public string currentPanel;

        [HideInInspector]
        public bool optionsShown = false;

        [HideInInspector]
        public bool rebindPending = false;

        private void OnEnable()
        {
            if (!InputHandler.HasReference)
                throw new NullReferenceException("The InputHandler component was not found on the scene!");

            InputHandler.OnInputsUpdated += OnInputsUpdated;
            InputHandler.OnPrepareRebind += OnPrepareRebind;
            InputHandler.OnActionDuplicate += OnActionDuplicate;
            InputHandler.OnRebindCancelled += OnRebindCancelled;

            TextsSource.Subscribe(OnInitTexts);

            InputHandler.GetInputAction("Apply", "UI").performed += OnApply;
            InputHandler.GetInputAction("Cancel", "UI").performed += OnCancel;
        }

        private void OnDestroy()
        {
            InputHandler.OnInputsUpdated -= OnInputsUpdated;
            InputHandler.OnPrepareRebind -= OnPrepareRebind;
            InputHandler.OnActionDuplicate -= OnActionDuplicate;
            InputHandler.OnRebindCancelled -= OnRebindCancelled;

            InputHandler.GetInputAction("Apply", "UI").performed -= OnApply;
            InputHandler.GetInputAction("Cancel", "UI").performed -= OnCancel;
        }

        private void OnInitTexts()
        {
            OnDuplicateTextPC = TextsSource.GetText("MenuUI.PCActionDuplicate");
            OnDuplicateTextCon = TextsSource.GetText("MenuUI.ConActionDuplicate");
        }

        private void Awake()
        {
            gameManager = GetComponent<HFPS_GameManager>();
            optionsController = GetComponent<OptionsController>();
            optionsShown = false;
        }

        void FixedUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == null && currentDevice.IsGamepadDevice() == 1 && string.IsNullOrEmpty(currentPanel))
            {
                if (selectFirstButton)
                {
                    if (FirstButton && FirstButton.interactable)
                    {
                        FirstButton.Select();
                    }
                    else if (FirstAltButton && FirstAltButton.interactable)
                    {
                        FirstAltButton.Select();
                    }
                }
            }
        }

        #region InputHandler Events
        private void OnApply(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (rebindPending) return;

            if (!string.IsNullOrEmpty(currentPanel))
            {
                foreach (var panel in panelViewDatas)
                {
                    if (panel.PanelTag.Equals(currentPanel) && panel.Events)
                    {
                        panel.Events.Apply();
                        break;
                    }
                }
            }
        }

        private void OnCancel(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (rebindPending) return;

            if (!string.IsNullOrEmpty(currentPanel))
            {
                foreach (var panel in panelViewDatas)
                {
                    if (panel.PanelTag.Equals(currentPanel) && panel.Events)
                    {
                        panel.Events.Cancel();
                        break;
                    }
                }
            }
            else if (gameManager && gameManager.isPaused)
            {
                gameManager.Unpause();
            }
        }

        private void OnInputsUpdated(InputHandler.Device device, ActionBinding[] bindings)
        {
            currentDevice = device;

            foreach (var variant in GetComponentsInChildren<DeviceVariantObject>(true))
            {
                variant.ChangeDevice(device);
            }

            if (allControls.Count > 0)
            {
                allControls.ForEach(x =>
                {
                    if (x != null)
                    {
                        Destroy(x);
                    }
                });

                allControls.Clear();
                actionProperties.Clear();
            }

            string group = device.DeviceToGroup();
            foreach (var action in bindings)
            {
                foreach (var binding in action.actionBindings)
                {
                    foreach (var composite in binding[group])
                    {
                        GameObject control = Instantiate(ControlPrefab, ControlsParent);
                        allControls.Add(control);

                        InputActionProperties properties = control.GetComponent<InputActionProperties>();

                        properties.bindingIndex = composite.bindingIndex;
                        properties.realActionName = action.name;

                        string partName = composite.partString;
                        properties.ActionName = string.Format("{0} {1}", action.name, partName);
                        properties.notRebindable = composite.notRebindable;
                        SetPropertyDisplay(properties, composite);

                        actionProperties.Add(properties);
                    }
                }
            }

            foreach (var tab in optionsController.optionTabs)
            {
                if (tab.IsControls)
                {
                    Selectable selectable = allControls[0].GetComponent<Selectable>();
                    tab.FirstOption = selectable;
                    break;
                }
            }

            int isGpDevice = device.IsGamepadDevice();
            foreach (GameObject km in KeyboardMouse)
            {
                km.SetActive(isGpDevice != 1);
            }
            foreach (GameObject gp in Gamepad)
            {
                gp.SetActive(isGpDevice == 1);
            }
        }

        private void SetPropertyDisplay(InputActionProperties properties, ActionBinding.CompositePart composite, bool setOvrd = false)
        {
            int displayType = currentDevice.IsGamepadDevice();

            if (displayType == 1)
            {
                if (properties.consoleDisplay == InputActionProperties.DisplayType.Text)
                {
                    properties.SetDisplay(composite.displayString, setOvrd);
                }
                else if (properties.consoleDisplay == InputActionProperties.DisplayType.Image)
                {
                    string bindingPath = !string.IsNullOrEmpty(composite.overridePath) ?
                        composite.overridePath : composite.bindingPath;

                    Sprite sprite = InputHandler.Instance.inputSprites.GetSprite(bindingPath, currentDevice);
                    properties.SetDisplay(sprite, setOvrd);
                }
            }
            else if (displayType == 0)
            {
                if (properties.keybaordDisplay == InputActionProperties.DisplayType.Text)
                {
                    properties.SetDisplay(composite.displayString, setOvrd);
                }
                else if (properties.keybaordDisplay == InputActionProperties.DisplayType.Image)
                {
                    string bindingPath = !string.IsNullOrEmpty(composite.overridePath) ?
                        composite.overridePath : composite.bindingPath;

                    Sprite sprite = InputHandler.Instance.inputSprites.GetSprite(bindingPath, currentDevice);
                    properties.SetDisplay(sprite, setOvrd);
                }
            }
        }

        private void OnActionDuplicate(string action)
        {
            if (DuplicateInputText)
            {
                if(currentDevice.IsGamepadDevice() == 1 && !string.IsNullOrEmpty(OnDuplicateTextCon))
                {
                    DuplicateInputText.text = OnDuplicateTextCon.RegexReplaceTag('{', '}', "action", action);
                }
                else if(currentDevice.IsGamepadDevice() != 1 && !string.IsNullOrEmpty(OnDuplicateTextPC))
                {
                    DuplicateInputText.text = OnDuplicateTextPC.RegexReplaceTag('{', '}', "action", action);
                }
                else
                {
                    DuplicateInputText.text = DuplicateInputText.text.RegexReplaceTag('{', '}', "action", action);
                }

                DuplicateInputPanel.SetActive(true);

                if(DuplicateFirstButton)
                    DuplicateFirstButton.Select();
            }
            else
            {
                throw new NullReferenceException("The DuplicateInputText variable is not assigned!");
            }
        }

        private void OnPrepareRebind(ActionBinding.CompositePart composite)
        {
            var prop = FindActionPropertiesFor(composite.isPartOfAction, composite.bindingIndex);
            SetPropertyDisplay(prop, composite, true);

            DuplicateInputPanel.SetActive(false);
            RestrictSelections(true);
            optionsController.ReSelectTab();
        }

        private void OnRebindCancelled()
        {
            if (currentRebind)
            {
                currentRebind.ResetDisplay();
                currentRebind = null;
            }

            DuplicateInputPanel.SetActive(false);
            RestrictSelections(true);
            optionsController.ReSelectTab();

            rebindPending = false;
        }

        InputActionProperties FindActionPropertiesFor(string actionName, int bindingIndex)
            => actionProperties.Where(x => x.realActionName == actionName && x.bindingIndex == bindingIndex).FirstOrDefault();
        #endregion

        private void RestrictSelections(bool state)
        {
            foreach (var go in allControls)
            {
                if(go.HasComponent(out Button button, true))
                {
                    button.interactable = state;
                }
            }

            foreach (var selectable in DuplicateRestrict)
            {
                selectable.interactable = state;
            }

            if (!state)
                OnBeforeRebind?.Invoke();
            else
                OnAfterRebind?.Invoke();
        }

        public void StartInteractiveRebind(InputActionProperties prop, string actionName, int bindingIndex)
        {
            if (InputHandler.HasReference)
            {
                currentRebind = prop;
                rebindPending = true;
                isInputApplied = false;

                RestrictSelections(false);
                InputHandler.StartInteractiveRebind(actionName, bindingIndex);
            }
        }

        public void ManageDuplicateBinding(bool rewrite)
        {
            if (InputHandler.HasReference)
            {
                RestrictSelections(true);

                if(currentDevice.IsGamepadDevice() == 1)
                {
                    InputHandler.ManageRewriteDuplicate(rewrite, true);
                }
                else
                {
                    InputHandler.ManageRewriteDuplicate(rewrite);
                }

                rebindPending = false;
            }
        }

        public void ApplyChanges()
        {
            if (InputHandler.HasReference)
            {
                InputHandler.ApplyChanges();
                optionsController.ApplyOptions();
                isInputApplied = true;
            }
        }

        public void ResetPanels()
        {
            if (!string.IsNullOrEmpty(currentPanel))
            {
                foreach (var panel in panelViewDatas)
                {
                    if (panel.PanelTag.Equals(currentPanel))
                    {
                        foreach (var obj in panel.GamepadButtons)
                        {
                            obj.SetActive(false);
                        }

                        panel.PanelObject.SetActive(false);
                        break;
                    }
                }
            }

            currentPanel = string.Empty;
            optionsShown = false;
            rebindPending = false;

            if (InputHandler.HasReference && InputHandler.Instance.applyManually && !isInputApplied)
            {
                foreach (var item in actionProperties)
                {
                    item.ResetDisplay();
                }
            }

            if(GeneralPanel) GeneralPanel.SetActive(true);
        }

        public void ShowPanel(string tag)
        {
            foreach (var panel in panelViewDatas)
            {
                if (panel.PanelTag.Equals(tag))
                {
                    currentPanel = panel.PanelTag;
                    panel.PanelObject.SetActive(true);

                    foreach (var obj in panel.GamepadButtons)
                    {
                        obj.SetActive(true);
                    }

                    optionsShown = panel.IsSettings;
                    if (panel.IsSettings)
                    {
                        optionsController.SelectTab(0);
                    }
                    break;
                }
            }
        }

        public void ShowGeneralMenu()
        {
            if (currentDevice.IsGamepadDevice() == 1)
            {
                FirstOrAltButton(FirstButton, FirstAltButton);
            }
        }

        public static void FirstOrAltButton(Selectable first, Selectable alt, bool sendMessage = false, bool activeCheck = true)
        {
            EventSystem.current.SetSelectedGameObject(null);

            if (first != null && (first.gameObject.activeInHierarchy || !activeCheck) && first.interactable)
            {
                first.Select();

                if (sendMessage)
                {
                    first.gameObject.SendMessage("Select", SendMessageOptions.DontRequireReceiver);
                }
            }
            else if (alt != null && (alt.gameObject.activeInHierarchy || !activeCheck) && alt.interactable)
            {
                alt.Select();

                if (sendMessage)
                {
                    alt.gameObject.SendMessage("Select", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}