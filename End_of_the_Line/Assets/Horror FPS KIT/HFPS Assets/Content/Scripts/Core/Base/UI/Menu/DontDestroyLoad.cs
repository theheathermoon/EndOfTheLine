using UnityEngine;

namespace HFPS.Systems
{
	public class DontDestroyLoad : MonoBehaviour
	{
		void Start()
		{
			DontDestroyOnLoad(gameObject);
		}
	}
}