using UnityEngine;

namespace FlashlightSystem
{
    public class FlashlightPickupTrigger : MonoBehaviour
    {
        [Space(10)] [SerializeField] private ObjectType _objectType = ObjectType.None;

        private enum ObjectType { None, Flashlight, Battery }

        [Header("Battery Number")]
        [SerializeField] private int batteryNumber = 1;

        private const string myPlayer = "Player";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(myPlayer))
            {
                if (_objectType == ObjectType.Flashlight)
                {
                    FlashlightController.instance.EnableInventory();
                    gameObject.SetActive(false);
                }

                else if (_objectType == ObjectType.Battery)
                {
                    FlashlightController.instance.CollectBattery(batteryNumber);
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
