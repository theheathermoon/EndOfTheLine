using UnityEngine;
using ThunderWire.Helpers;

namespace HFPS.UI
{
    public class Spinner : MonoBehaviour
    {

        public float RotateSpeed;
        [HideInInspector] public bool isSpinning;

        private RectTransform rectTransform;
        private Timekeeper timekeeper = new Timekeeper();

        void Awake()
        {
            isSpinning = true;
            rectTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            timekeeper.UpdateTime();

            if (isSpinning)
            {
                rectTransform.Rotate(0, 0, -RotateSpeed * timekeeper.deltaTime);
            }
        }
    }
}