/*
 * JumpscareTrigger.cs - by ThunderWire Studio
 * Version 2.0
*/

using UnityEngine;
using ThunderWire.Utility;
using HFPS.Player;

namespace HFPS.Systems
{
	/// <summary>
	/// Jumpscare Trigger Script
	/// </summary>
	public class JumpscareTrigger : MonoBehaviour
	{
		private JumpscareEffects effects;

		[Header("Jumpscare Setup")]
		public Animation AnimationObject;
		public AudioClip JumpscareSound;
		public AudioClip ScaredBreath;
		[Range(0, 5)] public float scareVolume = 0.5f;
		[Tooltip("Value sets how long will be player scared.")]
		public float scaredBreathTime = 33f;
		public bool enableEffects = true;

		[Header("Scare Effects")]
		public float chromaticAberrationAmount = 0.8f;
		public float vignetteAmount = 0.3f;
		public float effectsTime = 5f;

		[Header("Scare Shake")]
		public bool shakeByPreset = false;
		public float magnitude = 3f;
		public float roughness = 3f;
		public float startTime = 0.1f;
		public float durationTime = 3f;

		[Header("Scare Position Influence")]
		public Vector3 PositionInfluence = new Vector3(0.15f, 0.15f, 0f);
		public Vector3 RotationInfluence = Vector3.one;

		[SaveableField, HideInInspector]
		public bool isPlayed;

		void Start()
		{
			effects = ScriptManager.Instance.C<JumpscareEffects>();
		}

		public void IsPlayed(bool state)
		{
			isPlayed = state;
		}

		void OnTriggerEnter(Collider other)
		{
			if (other.tag == "Player" && !isPlayed)
			{
				AnimationObject.Play();

				if (JumpscareSound)
				{
					Utilities.PlayOneShot2D(transform.position, JumpscareSound, scareVolume);
				}

				if (enableEffects)
				{
					if (shakeByPreset)
					{
						CameraShakeInstance shakeInstance = CameraShakePresets.Scare;
						effects.Scare(shakeInstance, chromaticAberrationAmount, vignetteAmount, scaredBreathTime, effectsTime, ScaredBreath);
					}
					else
					{
						CameraShakeInstance shakeInstance = new CameraShakeInstance(magnitude, roughness, startTime, durationTime);
						shakeInstance.PositionInfluence = PositionInfluence;
						shakeInstance.RotationInfluence = RotationInfluence;
						effects.Scare(shakeInstance, chromaticAberrationAmount, vignetteAmount, scaredBreathTime, effectsTime, ScaredBreath);
					}
				}

				isPlayed = true;
			}
		}
	}
}