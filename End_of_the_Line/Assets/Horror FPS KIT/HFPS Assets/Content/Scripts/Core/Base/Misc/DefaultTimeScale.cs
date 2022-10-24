using UnityEngine;

namespace HFPS.Systems
{
	public class DefaultTimeScale : MonoBehaviour
	{

		void Awake()
		{
			Time.timeScale = 1;
		}
	}
}