using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    [System.Serializable]
    public class HandIKProfile : ScriptableObject
    {
        [HideInInspector] public Vector3 RightHandPosition;
        [HideInInspector] public Vector3 RightHandRotation = new Vector3(0, 90, -180);
        [HideInInspector] public Vector3 LeftHandPosition;
        [HideInInspector] public Vector3 LeftHandRotation = new Vector3(0, 90, 0);
        [HideInInspector] public bool ValuesModified = false;
    }
}