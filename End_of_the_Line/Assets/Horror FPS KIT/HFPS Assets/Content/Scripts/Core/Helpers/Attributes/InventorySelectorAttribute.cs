using System;
using UnityEngine;

namespace HFPS.Systems
{
    /// <summary>
    /// Convert <see cref="int"/> field to a Inventory Selector field.
    /// Valid On: <see cref="int"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InventorySelectorAttribute : PropertyAttribute
    {

    }
}