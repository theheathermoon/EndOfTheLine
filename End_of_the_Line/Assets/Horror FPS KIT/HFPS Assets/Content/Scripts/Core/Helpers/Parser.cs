using System;
using UnityEngine;
using System.Globalization;

namespace ThunderWire.Helpers
{
    /// <summary>
    /// Provides methods for type conversion
    /// </summary>
    public static class Parser
    {

        /// <summary>
        /// Basic type conversion.
        /// </summary>
        public static T Convert<T>(string value)
        {
            return (T)ConvertType(typeof(T), value);
        }

        /// <summary>
        /// Vector2 type conversion.
        /// </summary>
        public static Vector2 Convert(string x, string y)
        {
            return (Vector2)ParseVector2(x, y);
        }

        /// <summary>
        /// Vector3 type conversion.
        /// </summary>
        public static Vector3 Convert(string x, string y, string z)
        {
            return (Vector3)ParseVector3(x, y, z);
        }

        /// <summary>
        /// Vector4 or Quaterion type conversion.
        /// </summary>
        public static T Convert<T>(string x, string y, string z, string w) where T : struct
        {
            if (typeof(T) == typeof(Vector4))
            {
                return (T)ParseVector4(x, y, z, w);
            }
            else if (typeof(T) == typeof(Quaternion))
            {
                return (T)ParseQuaternion(x, y, z, w);
            }
            else
            {
                Debug.LogError("Wrong Conversion Type");
            }

            return default;
        }

        private static object ConvertType(Type type, string value)
        {
            if (type == typeof(int)) { return int.Parse(value); }
            if (type == typeof(uint)) { return uint.Parse(value); }
            if (type == typeof(long)) { return long.Parse(value); }
            if (type == typeof(ulong)) { return ulong.Parse(value); }
            if (type == typeof(float)) { return ParseFloat(value); }
            if (type == typeof(double)) { return double.Parse(value); }
            if (type == typeof(bool)) { return bool.Parse(value); }
            if (type == typeof(char)) { return char.Parse(value); }
            if (type == typeof(short)) { return short.Parse(value); }
            if (type == typeof(byte)) { return byte.Parse(value); }
            if (type == typeof(Color)) { return ParseColor(value); }
            if (type == typeof(KeyCode)) { return (KeyCode)Enum.Parse(typeof(KeyCode), value); }

            return value;
        }

        private static object ParseFloat(string input)
        {
            float result;

            if (float.TryParse(input, out result))
                return result;

            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;

            return default;
        }

        private static object ParseColor(string color)
        {
            Color newColor;
            ColorUtility.TryParseHtmlString(color, out newColor);
            return newColor;
        }

        private static object ParseVector2(string x, string y)
        {
            return new Vector2(float.Parse(x), float.Parse(y));
        }

        private static object ParseVector3(string x, string y, string z)
        {
            return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
        }

        private static object ParseVector4(string x, string y, string z, string w)
        {
            return new Vector4(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
        }

        private static object ParseQuaternion(string x, string y, string z, string w)
        {
            return new Quaternion(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
        }
    }
}
