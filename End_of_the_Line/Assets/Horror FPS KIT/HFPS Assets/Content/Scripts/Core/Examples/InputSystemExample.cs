using UnityEngine;
using ThunderWire.Input;

namespace HFPS.Examples
{
    [RequireComponent(typeof(InputHandler))]
    public class InputSystemExample : MonoBehaviour
    {
        [Header("Control Scheme Action")]
        public string ActionName;

        [Header("Settings")]
        public bool pressOnce;
        public bool isButton;

        void Update()
        {
            if (InputHandler.InputIsInitialized)
            {
                if (InputHandler.IsActionExist(ActionName))
                {
                    if (isButton)
                    {
                        if (!pressOnce)
                        {
                            if (InputHandler.ReadButton(ActionName))
                            {
                                Debug.Log($"Action {ActionName} is pressed!");
                            }
                        }
                        else
                        {
                            if (InputHandler.ReadButtonOnce(this, ActionName))
                            {
                                Debug.Log($"Action {ActionName} is pressed once!");
                            }
                        }
                    }
                    else
                    {

                        Vector2 value;
                        if ((value = InputHandler.ReadInput<Vector2>(ActionName)) != null)
                        {
                            Debug.Log($"Action {ActionName} is pressed! Value: {value}");
                        }
                    }
                }
                else
                {
                    Debug.Log($"Action {ActionName} does not exist!");
                }
            }
        }
    }
}