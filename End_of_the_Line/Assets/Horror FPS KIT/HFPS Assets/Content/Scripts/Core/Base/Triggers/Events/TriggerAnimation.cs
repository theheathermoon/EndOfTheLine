using UnityEngine;
using ThunderWire.Utility;

namespace HFPS.Systems
{
	public class TriggerAnimation : MonoBehaviour
	{
		public GameObject AnimationObject;
		public AudioClip AnimationSound;
		public float Volume = 0.5f;
		public bool is2D;

		[SaveableField, HideInInspector]
		public bool isPlayed;

		void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player") && !isPlayed)
			{
				AnimationObject.GetComponent<Animation>().Play();

				if (AnimationSound)
				{
					if (!is2D)
					{
						AudioSource.PlayClipAtPoint(AnimationSound, AnimationObject.transform.position, Volume);
					}
					else
					{
						Utilities.PlayOneShot2D(transform.position, AnimationSound, Volume);
					}
				}

				isPlayed = true;
			}
		}
	}
}