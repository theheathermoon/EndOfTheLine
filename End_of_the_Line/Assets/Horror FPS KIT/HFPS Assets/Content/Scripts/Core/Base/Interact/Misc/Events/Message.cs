using UnityEngine;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    public class Message : MonoBehaviour
    {
        public enum MessageType { Hint, PickupHint, Message, ItemName }

        public MessageType messageType = MessageType.Message;
        public string message;
        public string messageKey;
        public float messageTime;

#if TW_LOCALIZATION_PRESENT
        void OnEnable()
        {
            if (HFPS_GameManager.LocalizationEnabled)
            {
                LocalizationSystem.Subscribe(OnLocalizationUpdate, messageKey);
            }
        }

        public void OnLocalizationUpdate(string[] trs)
        {
            message = trs[0];
        }
#endif
    }
}