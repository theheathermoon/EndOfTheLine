using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ThunderWire.Input;
using HFPS.Systems;

namespace HFPS.UI
{
    public class SliderUI : OptionBehaviour, ISelectHandler, IDeselectHandler
    {
        public Slider slider;
        public float repeatAfter;
        public float repeatEvery;
        public float moveValue = 0.1f;
        public bool roundValue = false;

        private bool repeat;
        private float time;
        private float repeater;

        private bool isSelected = false;

        private void Awake()
        {
            slider.onValueChanged.AddListener(delegate { OnChanged(slider); });
        }

        void Update()
        {
            Vector2 nav;
            if (isSelected)
            {
                if ((nav = InputHandler.ReadInput<Vector2>("Navigate", "UI")) != null)
                {
                    if (nav.x != 0)
                    {
                        if (!repeat)
                        {
                            if (nav.x > 0.1)
                            {
                                ChangeValue(true);
                            }
                            else if (nav.x < -0.1)
                            {
                                ChangeValue(false);
                            }

                            repeat = true;
                        }
                        else
                        {
                            if (time < repeatAfter)
                            {
                                time += Time.unscaledDeltaTime;
                            }
                            else
                            {
                                if (repeater < repeatEvery)
                                {
                                    repeater += Time.unscaledDeltaTime;
                                }
                                else
                                {
                                    if (nav.x > 0.1)
                                    {
                                        ChangeValue(true);
                                    }
                                    else if (nav.x < -0.1)
                                    {
                                        ChangeValue(false);
                                    }

                                    repeater = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        repeat = false;
                        time = 0;
                        repeater = 0;
                    }
                }
            }
        }

        private void ChangeValue(bool increase)
        {
            if (increase)
            {
                if (slider.wholeNumbers)
                {
                    slider.value += 1;
                }
                else
                {
                    slider.value += moveValue;
                    if (roundValue) slider.value = (float)Math.Round(slider.value, 1);
                }
            }
            else
            {
                if (slider.wholeNumbers)
                {
                    slider.value -= 1;
                }
                else
                {
                    slider.value -= moveValue;
                    if (roundValue) slider.value = (float)Math.Round(slider.value, 1);
                }
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            isSelected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isSelected = false;
        }

        void OnDisable()
        {
            isSelected = false;
        }

        public override object GetValue()
        {
            return slider.value.ToString(CultureInfo.InvariantCulture);
        }

        public override void SetValue(string value)
        {
            slider.value = float.Parse(value, CultureInfo.InvariantCulture);
        }

        public void OnChanged(Slider slider)
        {
            OptionsController.Instance.OnOptionChanged(this);
        }
    }
}