using UnityEngine;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    public class InventoryFixedContainer : MonoBehaviour
    {        
        public string ContainerName = "Container";
        public string ContainerNameKey;

        public AudioClip OpenSound;
        [Range(0, 1)] public float Volume = 1f;

#if TW_LOCALIZATION_PRESENT
        void Awake()
        {
            if (HFPS_GameManager.LocalizationEnabled)
            {
                LocalizationSystem.SubscribeAndGet((result) =>
                {
                    if (!string.IsNullOrEmpty(result[0]))
                    {
                        ContainerName = result[0];
                    }
                }, ContainerNameKey);
            }
        }
#endif

        public void UseObject()
        {
            if (OpenSound) AudioSource.PlayClipAtPoint(OpenSound, transform.position, Volume);

            if (Inventory.HasReference)
                Inventory.Instance.ShowFixedInventoryContainer(ContainerName);
        }
    }
}