/*
 * GraphicFader.cs - by ThunderWire Studio
 * ver. 1.0
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HFPS.UI
{
    /// <summary>
    /// Script for Fading Selectable Graphic UI.
    /// </summary>
    public class GraphicFader : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Selectable ParentSelectable;
        public Graphic FadeObject;
        public float fadeDuration;

        [Header("Colors")]
        public Color Normal;
        public Color Selected;
        public Color Disabled;

        private bool isInteractable;
        private bool isSelected;

        private void Update()
        {
            if (ParentSelectable)
            {
                if (!ParentSelectable.interactable)
                {
                    isInteractable = false;
                    FadeObject.CrossFadeColor(Disabled, fadeDuration, true, true);
                }
                else
                {
                    isInteractable = true;
                }
            }
        }

        public void Select()
        {
            FadeObject.CrossFadeColor(Selected, fadeDuration, true, true);
            isSelected = true;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!isInteractable) return;
            FadeObject.CrossFadeColor(Selected, fadeDuration, true, true);
            isSelected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!isInteractable) return;
            FadeObject.CrossFadeColor(Color.white, fadeDuration, true, true);
            isSelected = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable) return;
            FadeObject.CrossFadeColor(Selected, fadeDuration, true, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable) return;
            if (!isSelected)
            {
                FadeObject.CrossFadeColor(Color.white, fadeDuration, true, true);
            }
            else
            {
                FadeObject.CrossFadeColor(Selected, fadeDuration, true, true);
            }
        }

        void OnDisable()
        {
            isSelected = false;
            FadeObject.color = Normal;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}