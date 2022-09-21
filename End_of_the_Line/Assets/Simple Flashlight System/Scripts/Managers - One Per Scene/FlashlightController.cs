using UnityEngine;

namespace FlashlightSystem
{
    public class FlashlightController : MonoBehaviour
    {
        [Header("Flashlight On Start")]
        [SerializeField] private bool hasFlashlight = false;

        [Header("Inventory Toggle")] //If this is true, allows the user to toggle inventory on / off
        [SerializeField] private bool toggleableInventory = false;

        [Header("Infinite Flashlight")]
        [SerializeField] private bool infiniteFlashlight = false;

        [Header("Battery Parameters")]
        [SerializeField] private float batteryDrainAmount = 0.01f;
        [SerializeField] private int batteryCount = 1;

        [Header("Battery Reload Timers")]
        [SerializeField] private float replaceBatteryTimer = 1.0f;
        private float maxReplaceBatteryTimer = 1.0f;

        [Header("Flashlight Parameters")]
        [Range(0, 10)] [SerializeField] private float maxFlashlightIntensity = 1.0f;
        [Range(1, 10)] [SerializeField] private int flashlightRotationSpeed = 2;
        private bool isFlashlightOn;

        [Header("Main Flashlight References")]
        [SerializeField] private Light flashlightSpot = null;
        [SerializeField] private FlashlightMovement flashlightMovement = null;

        [Header("Flashlight Sound Names")]
        [SerializeField] private ScriptableObject flashlightPickup;
        [SerializeField] private ScriptableObject flashlightClick;
        [SerializeField] private ScriptableObject flashlightReload;

        private bool shouldUpdate = false;

        public static FlashlightController instance;

        private void Awake()
        {
            if (instance != null) { Destroy(gameObject); }
            else { instance = this; DontDestroyOnLoad(gameObject); }
        }

        void Start()
        {
            flashlightSpot.intensity = maxFlashlightIntensity;
            FLUIManager.instance.UpdateBatteryUI(batteryCount);
            flashlightMovement.speed = flashlightRotationSpeed;
            maxReplaceBatteryTimer = replaceBatteryTimer;
        }

        public void EnableInventory()
        {
            hasFlashlight = true;
            FlashlightPickupSound();
        }

        void ToggleInventory()
        {
            if (toggleableInventory)
            {
                FLUIManager.instance.ToggleFlashlightInventory();
            }
        }

        void ToggleRadialIndicator(bool on)
        {
            FLUIManager.instance.ToggleRadialIndicator(on);
        }

        void UpdateRadialIndicator(float amount)
        {
            FLUIManager.instance.UpdateRadialIndicatorUI(amount);
        }

        public void CollectBattery(int batteries)
        {
            batteryCount = batteryCount + batteries;
            FLUIManager.instance.UpdateBatteryUI(batteryCount);
            FlashlightPickupSound();
        }

        void DegradingFlashlightLogic()
        {
            if (isFlashlightOn)
            {
                if (!infiniteFlashlight)
                {
                    if (flashlightSpot.intensity <= maxFlashlightIntensity && flashlightSpot.intensity > 0)
                    {
                        flashlightSpot.intensity -= (batteryDrainAmount * Time.deltaTime) * maxFlashlightIntensity;
                        FLUIManager.instance.UpdateBatteryLevelUI(batteryDrainAmount);
                    }

                    if (flashlightSpot.intensity >= maxFlashlightIntensity)
                    {
                        flashlightSpot.intensity = maxFlashlightIntensity;
                    }

                    else if (flashlightSpot.intensity <= 0)
                    {
                        flashlightSpot.intensity = 0;
                    }
                }
            }
        }

        void Update()
        {
            if (hasFlashlight)
            {
                PlayerInput();
                DegradingFlashlightLogic();
            }
        }

        void PlayerInput()
        {
            if (Input.GetKeyDown(FLInputManager.instance.flashlightSwitch)) //TURNING FLASHLIGHT ON/OFF
            {
                isFlashlightOn = !isFlashlightOn;

                flashlightSpot.enabled = isFlashlightOn;
                FLUIManager.instance.FlashlightIndicatorColor(isFlashlightOn);
                FlashlightClickSound();
            }

            if (Input.GetKey(FLInputManager.instance.reloadBattery) && batteryCount >= 1)
            {
                shouldUpdate = false;
                replaceBatteryTimer -= Time.deltaTime;
                ToggleRadialIndicator(true);
                UpdateRadialIndicator(replaceBatteryTimer);

                if (replaceBatteryTimer <= 0)
                {
                    batteryCount--;
                    FLUIManager.instance.UpdateBatteryUI(batteryCount);
                    flashlightSpot.intensity += maxFlashlightIntensity;
                    FLUIManager.instance.MaximumBatteryLevel(maxFlashlightIntensity);
                    FlashlightReloadSound();

                    replaceBatteryTimer = maxReplaceBatteryTimer;
                    UpdateRadialIndicator(maxReplaceBatteryTimer);
                    ToggleRadialIndicator(false);
                }
            }
            else
            {
                if (shouldUpdate)
                {
                    replaceBatteryTimer += Time.deltaTime;
                    UpdateRadialIndicator(replaceBatteryTimer);

                    if (replaceBatteryTimer >= maxReplaceBatteryTimer)
                    {
                        replaceBatteryTimer = maxReplaceBatteryTimer;
                        UpdateRadialIndicator(maxReplaceBatteryTimer);
                        ToggleRadialIndicator(false);
                        shouldUpdate = false;
                    }
                }
            }

            if (Input.GetKeyUp(FLInputManager.instance.reloadBattery))
            {
                shouldUpdate = true;
            }

            if (Input.GetKeyDown(FLInputManager.instance.toggleFlashlightInv))
            {
                ToggleInventory();
            }
        }

        void FlashlightPickupSound()
        {
            FLAudioManager.instance.Play(flashlightPickup.name);
        }

        void FlashlightClickSound()
        {
            FLAudioManager.instance.Play(flashlightClick.name);
        }

        void FlashlightReloadSound()
        {
            FLAudioManager.instance.Play(flashlightReload.name);
        }
    }
}