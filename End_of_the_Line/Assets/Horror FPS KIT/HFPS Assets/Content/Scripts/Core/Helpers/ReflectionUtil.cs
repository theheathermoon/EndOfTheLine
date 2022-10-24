using System.Reflection;
using UnityEngine;

namespace ThunderWire.Utility
{
    /// <summary>
    /// Provides Basic Methods for Reflection
    /// </summary>
    public static class ReflectionUtil
    {
        public enum ReflectType { Field, Property, Method };

        [System.Serializable]
        public sealed class Reflection
        {
            public ReflectType ReflectType;
            public MonoBehaviour Instance;
            public string ReflectName;
            public bool IsSettedUp;

            public FieldInfo m_FieldInfo = null;
            public PropertyInfo m_PropertyInfo = null;
            public MethodInfo m_MethodInfo = null;
        }

        /// <summary>
        /// Setup the Reflection
        /// </summary>
        public static bool Setup(this Reflection reflection)
        {
            if (reflection.ReflectType == ReflectType.Field)
            {
                reflection.m_FieldInfo = reflection.Instance.GetType().GetField(reflection.ReflectName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            }
            else if (reflection.ReflectType == ReflectType.Property)
            {
                reflection.m_PropertyInfo = reflection.Instance.GetType().GetProperty(reflection.ReflectName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            }
            else
            {
                reflection.m_MethodInfo = reflection.Instance.GetType().GetMethod(reflection.ReflectName, BindingFlags.Public | BindingFlags.Instance);
            }

            if(reflection.m_FieldInfo != null || reflection.m_PropertyInfo != null || reflection.m_MethodInfo != null)
            {
                reflection.IsSettedUp = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get Value From Reflection
        /// </summary>
        public static object Get(this Reflection reflection)
        {
            if (reflection.ReflectType == ReflectType.Field)
            {
                return reflection.m_FieldInfo.GetValue(reflection.Instance);
            }
            else if (reflection.ReflectType == ReflectType.Property)
            {
                return reflection.m_PropertyInfo.GetValue(reflection.Instance);
            }

            return reflection.m_MethodInfo.Invoke(reflection.Instance, new object[0]);
        }

        /// <summary>
        /// Set Reflection Value
        /// </summary>
        public static bool Set(this Reflection reflection, params object[] value)
        {
            try
            {
                if (reflection.ReflectType == ReflectType.Field)
                {
                    reflection.m_FieldInfo.SetValue(reflection.Instance, value[0]);
                }
                else if (reflection.ReflectType == ReflectType.Property)
                {
                    reflection.m_PropertyInfo.SetValue(reflection.Instance, value[0]);
                }
                else
                {
                    reflection.m_MethodInfo.Invoke(reflection.Instance, value);
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return true;
        }
    }
}