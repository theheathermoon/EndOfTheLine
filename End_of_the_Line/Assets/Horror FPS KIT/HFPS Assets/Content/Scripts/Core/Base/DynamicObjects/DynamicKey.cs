using UnityEngine;

namespace HFPS.Systems
{
    public class DynamicKey : MonoBehaviour, IItemEvent
    {
        public DynamicObject dynamicObject;

        public void OnItemEvent()
        {
            UseObject();
        }

        public void UseObject()
        {
            dynamicObject.hasKey = true;
        }
    }
}