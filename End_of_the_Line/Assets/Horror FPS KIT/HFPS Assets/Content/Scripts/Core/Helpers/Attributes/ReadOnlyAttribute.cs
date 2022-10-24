using UnityEngine;

namespace HFPS.Systems
{
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public bool IsLabel = false;

        public ReadOnlyAttribute(bool label = false)
        {
            IsLabel = label;
        }
    }
}