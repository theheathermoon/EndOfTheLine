using UnityEngine;
using ThunderWire.Input;

namespace HFPS.Systems
{
    public class Screenshot : MonoBehaviour
    {
        const string path = "Assets/Screenshots/";

        public string screenshotAction;
        public KeyCode screesnhotKey;
        public bool crossPlatformInput;

        private bool isTaken;
        private int count;

        void Update()
        {
            if (crossPlatformInput)
            {
                if (InputHandler.InputIsInitialized)
                {
                    if (InputHandler.ReadButtonOnce(this, screenshotAction))
                    {
                        TakeScreenshot();
                    }
                }
            }
            else
            {
                if (Input.GetKeyDown(screesnhotKey) && !isTaken)
                {
                    TakeScreenshot();
                    isTaken = true;
                }
                else if (isTaken)
                {
                    isTaken = false;
                }
            }
        }

        void TakeScreenshot()
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            string name = path + "Screenshot_" + count + ".png";
            ScreenCapture.CaptureScreenshot(name);
            Debug.Log("Captured: " + name);
            count++;
        }
    }
}