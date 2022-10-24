using UnityEngine;

namespace HFPS.Systems
{
    public abstract class OptionBehaviour : MonoBehaviour
    {
        public abstract object GetValue();
        public abstract void SetValue(string value);
    }
}