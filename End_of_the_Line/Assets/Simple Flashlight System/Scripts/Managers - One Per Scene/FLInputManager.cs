using UnityEngine;

namespace FlashlightSystem
{
    public class FLInputManager : MonoBehaviour
    {
        [Header("Raycast Pickup Input")]
        public KeyCode pickupKey;

        [Header("Flashlight System Inputs")]
        public KeyCode flashlightSwitch;
        public KeyCode reloadBattery;
        public KeyCode toggleFlashlightInv;

        public static FLInputManager instance;

        private void Awake()
        {
            if (instance != null) { Destroy(gameObject); }
            else { instance = this; DontDestroyOnLoad(gameObject); }
        }
    }
}
