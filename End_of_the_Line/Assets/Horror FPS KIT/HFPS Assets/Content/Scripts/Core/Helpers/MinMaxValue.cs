using System;
using UnityEngine;

namespace ThunderWire.Helpers
{
    [Serializable]
    public struct MinMaxValue
    {
        public float min;
        public float max;
        public float value;

        public bool Flipped => max < min;

        public float RealMin => Flipped ? max : min;
        public float RealMax => Flipped ? min : max;

        public MinMaxValue(float min, float max)
        {
            this.min = min;
            this.max = max;
            value = min;
        }

        public MinMaxValue(float min, float max, float value)
        {
            this.min = min;
            this.max = max;
            this.value = value;
        }

        public static implicit operator Vector2(MinMaxValue minMax)
        {
            return new Vector2(minMax.RealMin, minMax.RealMax);
        }

        public static implicit operator MinMaxValue(Vector2 vector)
        {
            MinMaxValue result = default;
            result.min = vector.x;
            result.max = vector.y;
            return result;
        }

        public MinMaxValue Flip() => new MinMaxValue(max, min);
    }
}