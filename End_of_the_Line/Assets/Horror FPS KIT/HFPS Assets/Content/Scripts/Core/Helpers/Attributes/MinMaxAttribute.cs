using System;
using UnityEngine;

namespace HFPS.Systems
{
    /// <summary>
    /// Attribute which shows MinMax value slider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class MinMaxAttribute : PropertyAttribute
    {
        public float MinValue;
        public float MaxValue;

        public MinMaxAttribute(float min, float max)
        {
            MinValue = min;
            MaxValue = max;
        }
    }
}