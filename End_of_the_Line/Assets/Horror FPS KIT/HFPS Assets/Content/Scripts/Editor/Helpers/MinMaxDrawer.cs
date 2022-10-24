using UnityEngine;
using UnityEditor;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer
    {
        const float kFloatFieldWidth = 40f;
        const float kSpacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var minMaxAttribute = (MinMaxAttribute)attribute;
            var propertyType = property.propertyType;

            label.tooltip = minMaxAttribute.MinValue.ToString("F2") + " to " + minMaxAttribute.MaxValue.ToString("F2");

            Rect controlRect = EditorGUI.PrefixLabel(position, label);
            Rect[] splittedRect = SplitRect(controlRect);

            EditorGUI.indentLevel = 0;

            if (propertyType == SerializedPropertyType.Vector2)
            {
                EditorGUI.BeginChangeCheck();

                Vector2 sliderValue = property.vector2Value;
                EditorGUI.MinMaxSlider(splittedRect[1], ref sliderValue.x, ref sliderValue.y, minMaxAttribute.MinValue, minMaxAttribute.MaxValue);

                sliderValue.x = EditorGUI.DelayedFloatField(splittedRect[0], float.Parse(sliderValue.x.ToString("F2")));
                sliderValue.x = Mathf.Clamp(sliderValue.x, minMaxAttribute.MinValue, Mathf.Min(minMaxAttribute.MaxValue, sliderValue.y));

                sliderValue.y = EditorGUI.DelayedFloatField(splittedRect[2], float.Parse(sliderValue.y.ToString("F2")));
                sliderValue.y = Mathf.Clamp(sliderValue.y, Mathf.Max(minMaxAttribute.MinValue, sliderValue.x), minMaxAttribute.MaxValue);

                if (EditorGUI.EndChangeCheck())
                {
                    property.vector2Value = sliderValue;
                }
            }
            else if (propertyType == SerializedPropertyType.Vector2Int)
            {
                EditorGUI.BeginChangeCheck();

                Vector2Int sliderValue = property.vector2IntValue;
                float minVal = sliderValue.x;
                float maxVal = sliderValue.y;

                EditorGUI.MinMaxSlider(splittedRect[1], ref minVal, ref maxVal, minMaxAttribute.MinValue, minMaxAttribute.MaxValue);

                sliderValue.x = EditorGUI.DelayedIntField(splittedRect[0], Mathf.FloorToInt(minVal));
                sliderValue.x = Mathf.FloorToInt(Mathf.Clamp(sliderValue.x, minMaxAttribute.MinValue, Mathf.Min(minMaxAttribute.MaxValue, sliderValue.y)));

                sliderValue.y = EditorGUI.DelayedIntField(splittedRect[2], Mathf.FloorToInt(maxVal));
                sliderValue.y = Mathf.FloorToInt(Mathf.Clamp(sliderValue.y, Mathf.Max(minMaxAttribute.MinValue, sliderValue.x), minMaxAttribute.MaxValue));

                if (EditorGUI.EndChangeCheck())
                {
                    property.vector2IntValue = sliderValue;
                }
            }
        }

        Rect[] SplitRect(Rect rectToSplit)
        {
            Rect[] rects = new Rect[3];
            float ppp = EditorGUIUtility.pixelsPerPoint;
            float spacing = kSpacing * ppp;
            float fieldWidth = kFloatFieldWidth * ppp;

            rects[0] = rectToSplit;
            rects[0].width = fieldWidth - spacing;

            rects[1] = rectToSplit;
            rects[1].x += fieldWidth + spacing;
            rects[1].width -= (fieldWidth + spacing * 2) * 2;

            rects[2] = rectToSplit;
            rects[2].x += rects[1].width + fieldWidth + (spacing * 4);
            rects[2].width = fieldWidth - spacing;

            return rects;
        }
    }
}