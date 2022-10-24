using UnityEngine;
using HFPS.Systems;

namespace HFPS.Player
{
    public class DelayEffect : MonoBehaviour
    {
        private ScriptManager scriptManager;
        private MouseLook mouseLook;

        public float amount = 0.02f;
        public float maxAmount = 0.03f;
        public float smooth = 3;

        private Vector3 def;
        private Vector3 vel;
        private float factorX;
        private float factorY;

        [HideInInspector]
        public bool isEnabled;

        void Start()
        {
            scriptManager = ScriptManager.Instance;
            mouseLook = scriptManager.GetComponent<MouseLook>();

            isEnabled = true;
            def = transform.localPosition;
        }

        void Update()
        {
            if (Cursor.lockState == CursorLockMode.None) return;

            Vector2 input = mouseLook.GetInputDelta();
            factorX = input.x;
            factorY = input.y;

            factorX *= -1 * amount;
            factorY *= -1 * amount;

            if (factorX > maxAmount)
                factorX = maxAmount;

            if (factorX < -maxAmount)
                factorX = -maxAmount;

            if (factorY > maxAmount)
                factorY = maxAmount;

            if (factorY < -maxAmount)
                factorY = -maxAmount;

            if (isEnabled)
            {
                Vector3 final = new Vector3(def.x + factorX, def.y + factorY, def.z);
                //transform.localPosition = Vector3.Lerp(transform.localPosition, final, Time.deltaTime * smooth);
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, final, ref vel, Time.deltaTime * smooth);
            }
        }
    }
}