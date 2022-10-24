using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    public class LayerMaskDrawer : Editor
    {
        /// <summary>
        /// Converts a LayerMask to a field value
        /// </summary>
        public static int LayerMaskToField(LayerMask mask)
        {
            int field = 0;
            var layers = InternalEditorUtility.layers;
            for (int c = 0; c < layers.Length; c++)
            {
                if ((mask & (1 << LayerMask.NameToLayer(layers[c]))) != 0)
                {
                    field |= 1 << c;
                }
            }
            return field;
        }

        /// <summary>
        /// Converts the field value to a LayerMask
        /// </summary>
        public static LayerMask FieldToLayerMask(int field)
        {
            LayerMask mask = 0;
            var layers = InternalEditorUtility.layers;
            for (int c = 0; c < layers.Length; c++)
            {
                if ((field & (1 << c)) != 0)
                {
                    mask |= 1 << LayerMask.NameToLayer(layers[c]);
                }
            }
            return mask;
        }
    }
}