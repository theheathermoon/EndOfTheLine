using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HFPS.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectAutoScroll : MonoBehaviour
    {
        public float scrollMargin = 1f;

        private ScrollRect scrollRect;
        private GameObject oldSelected;

        private Transform[] contents;

        void Awake()
        {
            scrollRect = gameObject.GetComponent<ScrollRect>();
        }

        private void OnEnable()
        {
            contents = scrollRect.content.GetComponentsInChildren<Transform>();
        }

        void Update()
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;

            if (selected != null && contents != null && contents.Length > 0 && contents.Any(x => x == selected.transform))
            {
                if (selected != oldSelected)
                {
                    OnUpdateSelected(selected);
                }

                oldSelected = selected;
            }
        }

        void OnUpdateSelected(GameObject obj)
        {
            // helper vars
            float contentHeight = scrollRect.content.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;

            // what bounds must be visible?
            float centerLine = obj.transform.localPosition.y; // selected item's center
            float upperBound = centerLine + (obj.GetComponent<RectTransform>().rect.height / 2f); // selected item's upper bound
            float lowerBound = centerLine - (obj.GetComponent<RectTransform>().rect.height / 2f); // selected item's lower bound

            // what are the bounds of the currently visible area?
            float lowerVisible = (contentHeight - viewportHeight) * scrollRect.normalizedPosition.y - contentHeight;
            float upperVisible = lowerVisible + viewportHeight;

            // is our item visible right now?
            float desiredLowerBound;
            if (upperBound > upperVisible)
            {
                // need to scroll up to upperBound
                desiredLowerBound = upperBound - viewportHeight + obj.GetComponent<RectTransform>().rect.height * scrollMargin;
            }
            else if (lowerBound < lowerVisible)
            {
                // need to scroll down to lowerBound
                desiredLowerBound = lowerBound - obj.GetComponent<RectTransform>().rect.height * scrollMargin;
            }
            else
            {
                // item already visible - all good
                return;
            }

            // normalize and set the desired viewport
            float normalizedDesired = (desiredLowerBound + contentHeight) / (contentHeight - viewportHeight);
            scrollRect.normalizedPosition = new Vector2(0f, Mathf.Clamp01(normalizedDesired));
        }
    }
}