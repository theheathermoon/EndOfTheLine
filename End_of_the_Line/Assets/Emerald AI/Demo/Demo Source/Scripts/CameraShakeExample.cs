using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Example
{
    public class CameraShakeExample : MonoBehaviour
    {
        public void TriggerCameraShake ()
        {
            CameraShake.Instance.ShakeCamera(0.3f, 0.2f);
        }
    }
}