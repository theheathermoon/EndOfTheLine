using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Input;
using ThunderWire.Utility;

namespace HFPS.Systems
{
    public class DeviceVariantObject : MonoBehaviour
    {
        [System.Serializable]
        public sealed class DeviceChange
        {
            public InputHandler.Device device;
            public int bindingIndex = -1;
        }

        public GameObject actionImageObj;
        public bool disableOnPC;

        public DeviceChange[] deviceChangeActions;
        public InputActionRef actionReference;

        public void ChangeDevice(InputHandler.Device device)
        {
            if (!actionImageObj)
                actionImageObj = gameObject;

            if (device == InputHandler.Device.MouseKeyboard && disableOnPC)
            {
                gameObject.SetActive(false);
            }

            foreach (var entry in deviceChangeActions)
            {
                if(entry.device == device)
                {
                    if (entry.bindingIndex >= 0)
                    {
                        if (actionImageObj.HasComponent(out Image image))
                        {
                            try
                            {
                                var binding = InputHandler.BindingOf(actionReference.ActionName, actionReference.ActionMap);
                                var composite = binding[entry.bindingIndex];
                                var icon = InputHandler.Instance.inputSprites.GetSprite(composite.GetBindingPath(), device);
                                image.sprite = icon;
                            }
                            catch
                            {
                                Debug.LogError("There was an error setting up device variant object on " + actionImageObj.name);
                            }
                        }
                        else throw new MissingComponentException("Cannot find Image Component for Icon Change!");
                    }
                }
            }
        }
    }
}