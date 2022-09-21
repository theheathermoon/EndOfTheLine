using UnityEngine;

namespace FlashlightSystem
{
    [RequireComponent(typeof(Camera))]
    public class FlashlightRaycast : MonoBehaviour
    {
        [Header("Raycast Features")]
        [SerializeField] private float rayLength = 5;
        private FlashlightItem examinableItem;
        private Camera _camera;

        public bool IsLookingAtExaminable
        {
            get { return examinableItem != null; }
        }

        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        void Update()
        {
            if (Physics.Raycast(_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), transform.forward, out RaycastHit hit, rayLength))
            {
                var examineItem = hit.collider.GetComponent<FlashlightItem>();
                if (examineItem != null)
                {
                    examinableItem = examineItem;
                    HighlightCrosshair(true);
                }
                else
                {
                    ClearExaminable();
                }
            }
            else
            {
                ClearExaminable();
            }

            if (IsLookingAtExaminable)
            {
                if (Input.GetKeyDown(FLInputManager.instance.pickupKey))
                {
                    examinableItem.ObjectInteract();
                }
            }
        }

        private void ClearExaminable()
        {
            if (examinableItem != null)
            {
                HighlightCrosshair(false);
                examinableItem = null;
            }
        }

        void HighlightCrosshair(bool on)
        {
            FLUIManager.instance.HighlightCrosshair(on);
        }
    }
}
