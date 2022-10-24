using System;
using UnityEngine;
using UnityEngine.UI;

namespace HFPS.UI
{
	public class SliderValue : MonoBehaviour
	{
		public Text ValueText;

		[HideInInspector]
		public float value;

		void Update()
		{
			value = GetComponent<Slider>().value;
			double a = Math.Round(value, 2);
			ValueText.text = a.ToString();
			GetComponent<Slider>().value = (float)a;
		}
	}
}