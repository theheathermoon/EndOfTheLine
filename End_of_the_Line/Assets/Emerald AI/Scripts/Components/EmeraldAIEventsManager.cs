﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    public class EmeraldAIEventsManager : MonoBehaviour
    {
        EmeraldAISystem EmeraldComponent;
        Coroutine m_RotateTowards;

        void Awake()
        {
            EmeraldComponent = GetComponent<EmeraldAISystem>();
        }

        /// <summary>
        /// Plays a sound clip according to the Clip parameter.
        /// </summary>
        public void PlaySoundClip(AudioClip Clip)
        {
            if (!EmeraldComponent.m_AudioSource.isPlaying)
            {
                EmeraldComponent.m_AudioSource.volume = 1;
                EmeraldComponent.m_AudioSource.PlayOneShot(Clip);
            }
            else
            {
                EmeraldComponent.m_SecondaryAudioSource.volume = 1;
                EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(Clip);
            }
        }

        /// <summary>
        /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayIdleSound()
        {
            if (EmeraldComponent.IdleSounds.Count > 0)
            {
                if (!EmeraldComponent.m_AudioSource.isPlaying)
                {
                    AudioClip m_RandomIdleSoundClip = EmeraldComponent.IdleSounds[Random.Range(0, EmeraldComponent.IdleSounds.Count)];
                    if (m_RandomIdleSoundClip != null)
                    {
                        EmeraldComponent.m_AudioSource.volume = EmeraldComponent.IdleVolume;
                        EmeraldComponent.m_AudioSource.PlayOneShot(m_RandomIdleSoundClip);
                        EmeraldComponent.IdleSoundsSeconds = Random.Range(EmeraldComponent.IdleSoundsSecondsMin, EmeraldComponent.IdleSoundsSecondsMax);
                        EmeraldComponent.IdleSoundsSeconds = (int)m_RandomIdleSoundClip.length + EmeraldComponent.IdleSoundsSeconds;
                    }
                }
                else
                {
                    AudioClip m_RandomIdleSoundClip = EmeraldComponent.IdleSounds[Random.Range(0, EmeraldComponent.IdleSounds.Count)];
                    if (m_RandomIdleSoundClip != null)
                    {
                        EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.IdleVolume;
                        EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(m_RandomIdleSoundClip);
                        EmeraldComponent.IdleSoundsSeconds = Random.Range(EmeraldComponent.IdleSoundsSecondsMin, EmeraldComponent.IdleSoundsSecondsMax);
                        EmeraldComponent.IdleSoundsSeconds = (int)m_RandomIdleSoundClip.length + EmeraldComponent.IdleSoundsSeconds;
                    }
                }
            }
        }

        /// <summary>
        /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayAttackSound()
        {
            if (EmeraldComponent.AttackSounds.Count > 0)
            {
                if (!EmeraldComponent.m_AudioSource.isPlaying)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.AttackVolume;
                    EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.AttackSounds[Random.Range(0, EmeraldComponent.AttackSounds.Count)]);
                }
                else
                {
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.AttackVolume;
                    EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.AttackSounds[Random.Range(0, EmeraldComponent.AttackSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayWarningSound()
        {
            if (EmeraldComponent.WarningSounds.Count > 0)
            {
                if (!EmeraldComponent.m_AudioSource.isPlaying)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.WarningVolume;
                    EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.WarningSounds[Random.Range(0, EmeraldComponent.WarningSounds.Count)]);
                }
                else
                {
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.WarningVolume;
                    EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.WarningSounds[Random.Range(0, EmeraldComponent.WarningSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a random impact sound based on your AI's Impact Sounds list.
        /// </summary>
        public void PlayImpactSound()
        {
            if (EmeraldComponent.ImpactSounds.Count > 0)
            {
                if (!EmeraldComponent.m_AudioSource.isPlaying)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.ImpactVolume;
                    EmeraldComponent.m_AudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.ImpactSounds[Random.Range(0, EmeraldComponent.ImpactSounds.Count)]);
                }
                else if (!EmeraldComponent.m_SecondaryAudioSource.isPlaying)
                {
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.ImpactVolume;
                    EmeraldComponent.m_AudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.ImpactSounds[Random.Range(0, EmeraldComponent.ImpactSounds.Count)]);
                }
                else
                {
                    EmeraldComponent.m_EventAudioSource.volume = EmeraldComponent.ImpactVolume;
                    EmeraldComponent.m_AudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    EmeraldComponent.m_EventAudioSource.PlayOneShot(EmeraldComponent.ImpactSounds[Random.Range(0, EmeraldComponent.ImpactSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a random critical hit sound based on your AI's Critical Hit Sounds list.
        /// </summary>
        public void PlayCriticalHitSound()
        {
            if (EmeraldComponent.CriticalHitSounds.Count > 0)
            {
                if (!EmeraldComponent.m_AudioSource.isPlaying)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.CriticalHitVolume;
                    EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.CriticalHitSounds[Random.Range(0, EmeraldComponent.CriticalHitSounds.Count)]);
                }
                else if (!EmeraldComponent.m_SecondaryAudioSource.isPlaying)
                {
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.CriticalHitVolume;
                    EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.CriticalHitSounds[Random.Range(0, EmeraldComponent.CriticalHitSounds.Count)]);
                }
                else
                {
                    EmeraldComponent.m_EventAudioSource.volume = EmeraldComponent.CriticalHitVolume;
                    EmeraldComponent.m_EventAudioSource.PlayOneShot(EmeraldComponent.CriticalHitSounds[Random.Range(0, EmeraldComponent.CriticalHitSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a random block sound based on your AI's Block Sounds list.
        /// </summary>
        public void PlayBlockSound()
        {
            if (EmeraldComponent.BlockingSounds.Count > 0)
            {
                if (!EmeraldComponent.m_AudioSource.isPlaying)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.BlockVolume;
                    EmeraldComponent.m_AudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.BlockingSounds[Random.Range(0, EmeraldComponent.BlockingSounds.Count)]);
                }
                else if (!EmeraldComponent.m_SecondaryAudioSource.isPlaying)
                {
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.BlockVolume;
                    EmeraldComponent.m_AudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.BlockingSounds[Random.Range(0, EmeraldComponent.BlockingSounds.Count)]);
                }
                else
                {
                    EmeraldComponent.m_EventAudioSource.volume = EmeraldComponent.BlockVolume;
                    EmeraldComponent.m_AudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    EmeraldComponent.m_EventAudioSource.PlayOneShot(EmeraldComponent.BlockingSounds[Random.Range(0, EmeraldComponent.BlockingSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a random injured sound based on your AI's Injured Sounds list.
        /// </summary>
        public void PlayInjuredSound()
        {
            if (EmeraldComponent.InjuredSounds.Count > 0)
            {
                if (!EmeraldComponent.m_AudioSource.isPlaying)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.InjuredVolume;
                    EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.InjuredSounds[Random.Range(0, EmeraldComponent.InjuredSounds.Count)]);
                }
                else if (!EmeraldComponent.m_SecondaryAudioSource.isPlaying)
                {
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.InjuredVolume;
                    EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.InjuredSounds[Random.Range(0, EmeraldComponent.InjuredSounds.Count)]);
                }
                else
                {
                    EmeraldComponent.m_EventAudioSource.volume = EmeraldComponent.InjuredVolume;
                    EmeraldComponent.m_EventAudioSource.PlayOneShot(EmeraldComponent.InjuredSounds[Random.Range(0, EmeraldComponent.InjuredSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a random death sound based on your AI's Death Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayDeathSound()
        {
            if (EmeraldComponent.DeathSounds.Count > 0)
            {
                if (!EmeraldComponent.m_AudioSource.isPlaying)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.DeathVolume;
                    EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.DeathSounds[Random.Range(0, EmeraldComponent.DeathSounds.Count)]);
                }
                else
                {
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.DeathVolume;
                    EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.DeathSounds[Random.Range(0, EmeraldComponent.DeathSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a footstep sound from the AI's Footstep Sounds list to use when the AI is walking. This should be setup through an Animation Event.
        /// </summary>
        public void WalkFootstepSound()
        {
            if (EmeraldComponent.AnimatorType == EmeraldAISystem.AnimatorTypeState.RootMotion && EmeraldComponent.AIAnimator.GetFloat("Speed") > 0.05f && EmeraldComponent.AIAnimator.GetFloat("Speed") <= 0.1f
                || EmeraldComponent.AnimatorType == EmeraldAISystem.AnimatorTypeState.NavMeshDriven && EmeraldComponent.m_NavMeshAgent.velocity.magnitude > 0.05f && EmeraldComponent.m_NavMeshAgent.velocity.magnitude <= EmeraldComponent.WalkSpeed + 0.25f || EmeraldComponent.IsTurning)
            {
                if (EmeraldComponent.FootStepSounds.Count > 0)
                {
                    if (!EmeraldComponent.m_AudioSource.isPlaying)
                    {
                        EmeraldComponent.m_AudioSource.volume = EmeraldComponent.WalkFootstepVolume;
                        EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.FootStepSounds[Random.Range(0, EmeraldComponent.FootStepSounds.Count)]);
                    }
                    else
                    {
                        EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.WalkFootstepVolume;
                        EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.FootStepSounds[Random.Range(0, EmeraldComponent.FootStepSounds.Count)]);
                    }
                }
            }
        }

        /// <summary>
        /// Plays a footstep sound from the AI's Footstep Sounds list to use when the AI is running. This should be setup through an Animation Event.
        /// </summary>
        public void RunFootstepSound()
        {
            if (EmeraldComponent.AnimatorType == EmeraldAISystem.AnimatorTypeState.RootMotion && EmeraldComponent.AIAnimator.GetFloat("Speed") > 0.1f
                || EmeraldComponent.AnimatorType == EmeraldAISystem.AnimatorTypeState.NavMeshDriven && EmeraldComponent.m_NavMeshAgent.velocity.magnitude > EmeraldComponent.WalkSpeed + 0.25f || EmeraldComponent.IsTurning)
            {
                if (EmeraldComponent.FootStepSounds.Count > 0)
                {
                    if (!EmeraldComponent.m_AudioSource.isPlaying)
                    {
                        EmeraldComponent.m_AudioSource.volume = EmeraldComponent.RunFootstepVolume;
                        EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.FootStepSounds[Random.Range(0, EmeraldComponent.FootStepSounds.Count)]);
                    }
                    else
                    {
                        EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.RunFootstepVolume;
                        EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.FootStepSounds[Random.Range(0, EmeraldComponent.FootStepSounds.Count)]);
                    }
                }
            }
        }

        /// <summary>
        /// Plays a random sound effect from the AI's General Sounds list.
        /// </summary>
        public void PlayRandomSoundEffect()
        {
            if (EmeraldComponent.InteractSoundList.Count > 0)
            {
                if (!EmeraldComponent.m_AudioSource.isPlaying)
                {
                    EmeraldComponent.m_AudioSource.volume = 1;
                    EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.InteractSoundList[Random.Range(0, EmeraldComponent.InteractSoundList.Count)].SoundEffectClip);
                }
                else if (!EmeraldComponent.m_SecondaryAudioSource.isPlaying)
                {
                    EmeraldComponent.m_SecondaryAudioSource.volume = 1;
                    EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.InteractSoundList[Random.Range(0, EmeraldComponent.InteractSoundList.Count)].SoundEffectClip);
                }
                else
                {
                    EmeraldComponent.m_EventAudioSource.volume = 1;
                    EmeraldComponent.m_EventAudioSource.PlayOneShot(EmeraldComponent.InteractSoundList[Random.Range(0, EmeraldComponent.InteractSoundList.Count)].SoundEffectClip);
                }
            }
        }

        /// <summary>
        /// Plays a sound effect from the AI's General Sounds list using the Sound Effect ID as the parameter.
        /// </summary>
        public void PlaySoundEffect(int SoundEffectID)
        {
            if (EmeraldComponent.InteractSoundList.Count > 0)
            {
                for (int i = 0; i < EmeraldComponent.InteractSoundList.Count; i++)
                {
                    if (EmeraldComponent.InteractSoundList[i].SoundEffectID == SoundEffectID)
                    {
                        if (!EmeraldComponent.m_AudioSource.isPlaying)
                        {
                            EmeraldComponent.m_AudioSource.volume = 1;
                            EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.InteractSoundList[i].SoundEffectClip);
                        }
                        else if (!EmeraldComponent.m_SecondaryAudioSource.isPlaying)
                        {
                            EmeraldComponent.m_SecondaryAudioSource.volume = 1;
                            EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.InteractSoundList[i].SoundEffectClip);
                        }
                        else
                        {
                            EmeraldComponent.m_EventAudioSource.volume = 1;
                            EmeraldComponent.m_EventAudioSource.PlayOneShot(EmeraldComponent.InteractSoundList[i].SoundEffectClip);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enables an item from your AI's Item list using the Item ID.
        /// </summary>
        public void EnableItem(int ItemID)
        {
            //Look through each item in the ItemList for the appropriate ID.
            //Once found, enable the item of the same index as the found ID.
            for (int i = 0; i < EmeraldComponent.ItemList.Count; i++)
            {
                if (EmeraldComponent.ItemList[i].ItemID == ItemID)
                {
                    if (EmeraldComponent.ItemList[i].ItemObject != null)
                    {
                        EmeraldComponent.ItemList[i].ItemObject.SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// Disables an item from your AI's Item list using the Item ID.
        /// </summary>
        public void DisableItem(int ItemID)
        {
            //Look through each item in the ItemList for the appropriate ID.
            //Once found, enable the item of the same index as the found ID.
            for (int i = 0; i < EmeraldComponent.ItemList.Count; i++)
            {
                if (EmeraldComponent.ItemList[i].ItemID == ItemID)
                {
                    if (EmeraldComponent.ItemList[i].ItemObject != null)
                    {
                        EmeraldComponent.ItemList[i].ItemObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Disables all items from your AI's Item list.
        /// </summary>
        public void DisableAllItems()
        {
            //Disable all of an AI's items
            for (int i = 0; i < EmeraldComponent.ItemList.Count; i++)
            {
                if (EmeraldComponent.ItemList[i].ItemObject != null)
                {
                    EmeraldComponent.ItemList[i].ItemObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Enables the AI's weapon object and plays the AI's equip sound effect, if one is applied.
        /// </summary>
        public void EquipWeapon(string WeaponTypeToEnable)
        {
            if (WeaponTypeToEnable == "Melee")
            {
                if (EmeraldComponent.UnsheatheWeapon != null)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.EquipVolume;
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.EquipVolume;

                    if (!EmeraldComponent.m_AudioSource.isPlaying)
                    {
                        EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.UnsheatheWeapon);
                    }
                    else
                    {
                        EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.UnsheatheWeapon);
                    }
                }

                if (EmeraldComponent.HeldMeleeWeaponObject != null)
                {
                    EmeraldComponent.HeldMeleeWeaponObject.SetActive(true);
                }

                if (EmeraldComponent.HolsteredMeleeWeaponObject != null)
                {
                    EmeraldComponent.HolsteredMeleeWeaponObject.SetActive(false);
                }
            }
            else if (WeaponTypeToEnable == "Ranged")
            {
                if (EmeraldComponent.RangedUnsheatheWeapon != null)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.RangedEquipVolume;
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.RangedEquipVolume;

                    if (!EmeraldComponent.m_AudioSource.isPlaying)
                    {
                        EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.RangedUnsheatheWeapon);
                    }
                    else
                    {
                        EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.RangedUnsheatheWeapon);
                    }
                }

                if (EmeraldComponent.HeldRangedWeaponObject != null)
                {
                    EmeraldComponent.HeldRangedWeaponObject.SetActive(true);
                }

                if (EmeraldComponent.HolsteredRangedWeaponObject != null)
                {
                    EmeraldComponent.HolsteredRangedWeaponObject.SetActive(false);
                }

                //Fade in the hand IK
                if (EmeraldComponent.EmeraldHandIKComp != null)
                    EmeraldComponent.EmeraldHandIKComp.FadeInHandWeights();

                //Fade in the look at controller after a 1 second delay.
                if (EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.Yes)
                    EmeraldComponent.EmeraldLookAtComponent.Invoke("FadeInBodyIK", EmeraldComponent.RangedPullOutWeaponAnimation.length / 1.5f); //Was  / 1.5f
            }
            else
            {
                Debug.Log("This string withing the EquipWeapon Animation Event is blank or incorrect. Ensure that it's either Ranged or Melee.");
            }
        }

        /// <summary>
        /// Disables the AI's weapon object and plays the AI's unequip sound effect, if one is applied.
        /// </summary>
        public void UnequipWeapon(string WeaponTypeToDisable)
        {
            if (WeaponTypeToDisable == "Melee")
            {
                if (EmeraldComponent.SheatheWeapon != null)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.UnequipVolume;
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.UnequipVolume;

                    if (!EmeraldComponent.m_AudioSource.isPlaying)
                    {
                        EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.SheatheWeapon);
                    }
                    else
                    {
                        EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.SheatheWeapon);
                    }
                }

                if (EmeraldComponent.HeldMeleeWeaponObject != null)
                {
                    EmeraldComponent.HeldMeleeWeaponObject.SetActive(false);
                }

                if (EmeraldComponent.HolsteredMeleeWeaponObject != null)
                {
                    EmeraldComponent.HolsteredMeleeWeaponObject.SetActive(true);
                }
            }
            else if (WeaponTypeToDisable == "Ranged")
            {
                if (EmeraldComponent.RangedSheatheWeapon != null)
                {
                    EmeraldComponent.m_AudioSource.volume = EmeraldComponent.RangedUnequipVolume;
                    EmeraldComponent.m_SecondaryAudioSource.volume = EmeraldComponent.RangedUnequipVolume;

                    if (!EmeraldComponent.m_AudioSource.isPlaying)
                    {
                        EmeraldComponent.m_AudioSource.PlayOneShot(EmeraldComponent.RangedSheatheWeapon);
                    }
                    else
                    {
                        EmeraldComponent.m_SecondaryAudioSource.PlayOneShot(EmeraldComponent.RangedSheatheWeapon);
                    }
                }

                if (EmeraldComponent.HeldRangedWeaponObject != null)
                {
                    EmeraldComponent.HeldRangedWeaponObject.SetActive(false);
                }

                if (EmeraldComponent.HolsteredRangedWeaponObject != null)
                {
                    EmeraldComponent.HolsteredRangedWeaponObject.SetActive(true);
                }
            }
            else
            {
                Debug.Log("This string withing the UnequipWeapon Animation Event is blank or incorrect. Ensure that it's either Ranged or Melee.");
            }
        }

        /// <summary>
        /// Cancels the AI's current attack animation from playing.
        /// </summary>
        public void CancelAttackAnimation ()
        {
            EmeraldComponent.IsRunAttack = false;
            EmeraldComponent.AttackTimer = 0;
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            EmeraldComponent.AIAnimator.SetTrigger("Attack Cancelled");
            EmeraldComponent.AIAnimator.ResetTrigger("Attack Cancelled");
        }

        /// <summary>
        /// Plays an emote animation according to the Animation Clip parameter. Note: This function will only work if
        /// an AI is not in active combat mode.
        /// </summary>
        public void PlayEmoteAnimation(int EmoteAnimationID)
        {
            //Look through each animation in the EmoteAnimationList for the appropriate ID.
            //Once found, play the animaition of the same index as the found ID.
            for (int i = 0; i < EmeraldComponent.EmoteAnimationList.Count; i++)
            {
                if (EmeraldComponent.EmoteAnimationList[i].AnimationID == EmoteAnimationID)
                {
                    if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive)
                    {
                        EmeraldComponent.AIAnimator.SetInteger("Emote Index", EmoteAnimationID);
                        EmeraldComponent.AIAnimator.SetTrigger("Emote Trigger");
                        EmeraldComponent.IsMoving = false;
                    }
                }
            }
        }

        /// <summary>
        /// Loops an emote animation according to the Animation Clip parameter until it is called to stop. Note: This function will only work if
        /// an AI is not in active combat mode.
        /// </summary>
        public void LoopEmoteAnimation(int EmoteAnimationID)
        {
            //Look through each animation in the EmoteAnimationList for the appropriate ID.
            //Once found, play the animaition of the same index as the found ID.
            for (int i = 0; i < EmeraldComponent.EmoteAnimationList.Count; i++)
            {
                if (EmeraldComponent.EmoteAnimationList[i].AnimationID == EmoteAnimationID)
                {
                    if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive)
                    {
                        EmeraldComponent.AIAnimator.SetInteger("Emote Index", EmoteAnimationID);
                        EmeraldComponent.AIAnimator.SetBool("Emote Loop", true);
                        EmeraldComponent.IsMoving = false;
                    }
                }
            }
        }

        /// <summary>
        /// Loops an emote animation according to the Animation Clip parameter until it is called to stop. Note: This function will only work if
        /// an AI is not in active combat mode.
        /// </summary>
        public void StopLoopEmoteAnimation(int EmoteAnimationID)
        {
            //Look through each animation in the EmoteAnimationList for the appropriate ID.
            //Once found, play the animaition of the same index as the found ID.
            for (int i = 0; i < EmeraldComponent.EmoteAnimationList.Count; i++)
            {
                if (EmeraldComponent.EmoteAnimationList[i].AnimationID == EmoteAnimationID)
                {
                    if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive)
                    {
                        EmeraldComponent.AIAnimator.SetInteger("Emote Index", EmoteAnimationID);
                        EmeraldComponent.AIAnimator.SetBool("Emote Loop", false);
                        EmeraldComponent.IsMoving = false;
                    }
                }
            }
        }

        /// <summary>
        /// Spawns an additional effect object at the position of the AI's Blood Spawn Offset position.
        /// </summary>
        public void SpawnAdditionalEffect(GameObject EffectObject)
        {
            GameObject Effect = EmeraldAIObjectPool.Spawn(EffectObject, transform.position + EmeraldComponent.BloodPosOffset, Quaternion.identity);
            Effect.transform.SetParent(EmeraldAISystem.ObjectPool.transform);

            if (Effect.GetComponent<EmeraldAIProjectileTimeout>() == null)
            {
                Effect.AddComponent<EmeraldAIProjectileTimeout>().TimeoutSeconds = 3;
            }
        }

        /// <summary>
        /// Spawns an effect object at the position of the AI's target.
        /// </summary>
        public void SpawnEffectOnTarget(GameObject EffectObject)
        {
            if (EmeraldComponent.CurrentTarget != null)
            {
                GameObject Effect = EmeraldAIObjectPool.Spawn(EffectObject, new Vector3(EmeraldComponent.CurrentTarget.position.x,
                    EmeraldComponent.CurrentTarget.position.y + EmeraldComponent.CurrentTarget.localScale.y / 2, EmeraldComponent.CurrentTarget.position.z), Quaternion.identity);
                Effect.transform.SetParent(EmeraldAISystem.ObjectPool.transform);

                if (Effect.GetComponent<EmeraldAIProjectileTimeout>() == null)
                {
                    Effect.AddComponent<EmeraldAIProjectileTimeout>().TimeoutSeconds = 2;
                }
            }
        }

        /// <summary>
        /// Spawns a blood splat effect object at the position of the AI's Blood Spawn Offset position. 
        /// The rotation of this object is then randomized and adjusted based off of your attacker's current location.
        /// </summary>
        public void SpawnBloodSplatEffect(GameObject BloodSplatObject)
        {
            if (EmeraldComponent.CurrentTarget != null)
            {
                GameObject Effect = EmeraldAIObjectPool.Spawn(BloodSplatObject, transform.position + EmeraldComponent.BloodPosOffset, Quaternion.Euler(Random.Range(110, 160),
                    EmeraldComponent.CurrentTarget.localEulerAngles.y - Random.Range(120, 240), Random.Range(0, 360)));
                Effect.transform.SetParent(EmeraldAISystem.ObjectPool.transform);

                if (Effect.GetComponent<EmeraldAIProjectileTimeout>() == null)
                {
                    Effect.AddComponent<EmeraldAIProjectileTimeout>().TimeoutSeconds = 2;
                }
            }
        }

        /// <summary>
        /// Instantly kills this AI
        /// </summary>
        public void KillAI()
        {
            if (!EmeraldComponent.IsDead)
            {
                EmeraldComponent.Damage(9999999, EmeraldAISystem.TargetType.AI);
            }
        }

        /// <summary>
        /// Manually sets the AI's next Idle animation instead of being generated randomly. This is useful for functionality such as playing a particular idle animation
        /// at a certain location such as for an AI's schedule. Note: The animation numbers are from 1 to 6 and must exist in your AI's Idle Animation list. You must call 
        /// DisableOverrideIdleAnimation() to have idle animations randomly generate again and to disable this feature.
        /// </summary>
        public void OverrideIdleAnimation(int IdleIndex)
        {
            EmeraldComponent.m_IdleAnimaionIndexOverride = true;
            EmeraldComponent.AIAnimator.SetInteger("Idle Index", IdleIndex);
        }

        /// <summary>
        /// Disables the OverrideIdleAnimation feature.
        /// </summary>
        public void DisableOverrideIdleAnimation()
        {
            EmeraldComponent.m_IdleAnimaionIndexOverride = false;
        }

        /// <summary>
        /// Changes the AI's Behavior
        /// </summary>
        public void ChangeBehavior(EmeraldAISystem.CurrentBehavior NewBehavior, bool ChangeStartingBehavior = false)
        {
            EmeraldComponent.BehaviorRef = NewBehavior;

            if (ChangeStartingBehavior)
                EmeraldComponent.StartingBehaviorRef = (int)EmeraldComponent.BehaviorRef;
        }

        /// <summary>
        /// Changes the AI's Confidence
        /// </summary>
        public void ChangeConfidence(EmeraldAISystem.ConfidenceType NewConfidence, bool ChangeStartingConfidence = false)
        {
            EmeraldComponent.ConfidenceRef = NewConfidence;

            if (ChangeStartingConfidence)
                EmeraldComponent.StartingConfidenceRef = (int)EmeraldComponent.ConfidenceRef;
        }

        /// <summary>
        /// Changes the AI's Wander Type
        /// </summary>
        public void ChangeWanderType(EmeraldAISystem.WanderType NewWanderType)
        {
            EmeraldComponent.WanderTypeRef = NewWanderType;
        }

        /// <summary>
        /// Instantiates an AI's Droppable Weapon Object on death. 
        /// </summary>
        public void CreateDroppableWeapon()
        {
            //If using one weapon type, use either the 
            if (EmeraldComponent.UseDroppableWeapon == EmeraldAISystem.YesOrNo.Yes)
            {
                if (EmeraldComponent.HeldMeleeWeaponObject != null)
                {
                    if (EmeraldComponent.HeldMeleeWeaponObject.activeSelf && EmeraldComponent.DroppableMeleeWeapon != null)
                    {
                        //Unparent the created weapon object.
                        EmeraldComponent.DroppableMeleeWeapon.transform.SetParent(EmeraldAISystem.ObjectPool.transform);

                        //Check for a collider on the WeaponObject, if there isn't one, add one. 
                        if (EmeraldComponent.DroppableMeleeWeapon.GetComponent<Collider>() == null)
                            EmeraldComponent.DroppableMeleeWeapon.AddComponent<BoxCollider>();

                        if (EmeraldComponent.DroppableMeleeWeapon.GetComponent<Rigidbody>() == null)
                            EmeraldComponent.DroppableMeleeWeapon.AddComponent<Rigidbody>();

                        //Apply the AI's current velocity to the weapon object.
                        Rigidbody WeapnRigidbody = EmeraldComponent.DroppableMeleeWeapon.GetComponent<Rigidbody>();
                        WeapnRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

                        if (EmeraldComponent.EmeraldEventsManagerComponent.GetAttacker() != null && EmeraldComponent.EmeraldEventsManagerComponent.GetAttacker() != EmeraldComponent.CurrentTarget)
                            WeapnRigidbody.AddForce((EmeraldComponent.EmeraldEventsManagerComponent.GetAttacker().position - transform.position).normalized * -EmeraldComponent.ReceivedRagdollForceAmount, ForceMode.Impulse);
                        else
                            WeapnRigidbody.AddForce((EmeraldComponent.CurrentTarget.position - transform.position).normalized * -EmeraldComponent.ReceivedRagdollForceAmount, ForceMode.Impulse);

                        //Disable the reference weapon object and enable the copy.
                        EmeraldComponent.DroppableMeleeWeapon.gameObject.SetActive(true);
                        EmeraldComponent.HeldMeleeWeaponObject.SetActive(false);
                    }
                }

                if (EmeraldComponent.HeldRangedWeaponObject != null)
                {
                    if (EmeraldComponent.HeldRangedWeaponObject.activeSelf && EmeraldComponent.DroppableRangedWeapon != null)
                    {
                        //Unparent the created weapon object.
                        EmeraldComponent.DroppableRangedWeapon.transform.SetParent(EmeraldAISystem.ObjectPool.transform);

                        //Check for a collider on the WeaponObject, if there isn't one, add one. 
                        if (EmeraldComponent.DroppableRangedWeapon.GetComponent<Collider>() == null)
                            EmeraldComponent.DroppableRangedWeapon.AddComponent<BoxCollider>();

                        if (EmeraldComponent.DroppableRangedWeapon.GetComponent<Rigidbody>() == null)
                            EmeraldComponent.DroppableRangedWeapon.AddComponent<Rigidbody>();

                        //Apply the AI's current velocity to the weapon object.
                        Rigidbody WeapnRigidbody = EmeraldComponent.DroppableRangedWeapon.GetComponent<Rigidbody>();

                        if (EmeraldComponent.EmeraldEventsManagerComponent.GetAttacker() != null && EmeraldComponent.EmeraldEventsManagerComponent.GetAttacker() != EmeraldComponent.CurrentTarget)
                            WeapnRigidbody.AddForce((EmeraldComponent.EmeraldEventsManagerComponent.GetAttacker().position - transform.position).normalized * -EmeraldComponent.ReceivedRagdollForceAmount, ForceMode.Impulse);
                        else
                            WeapnRigidbody.AddForce((EmeraldComponent.CurrentTarget.position - transform.position).normalized * -EmeraldComponent.ReceivedRagdollForceAmount, ForceMode.Impulse);

                        //Disable the reference weapon object and enable the copy.
                        EmeraldComponent.DroppableRangedWeapon.gameObject.SetActive(true);
                        EmeraldComponent.HeldRangedWeaponObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Clears the AI's target
        /// </summary>
        /// <param name="ClearFollower">Also clears a Companion or Pet AI's follower.</param>
        public void ClearTarget(bool? ClearFollower = false)
        {
            if ((bool)ClearFollower)
            {
                EmeraldComponent.CurrentFollowTarget = null;
            }
            EmeraldComponent.CurrentTarget = null;
            EmeraldComponent.LineOfSightTargets.Clear();
            EmeraldComponent.potentialTargets.Clear();
            EmeraldComponent.TargetEmerald = null;
        }

        /// <summary>
        /// Clears all ignored targets from the static EmeraldAISystem IgnoredTargetsList.
        /// </summary>
        public void ClearAllIgnoredTargets()
        {
            EmeraldAISystem.IgnoredTargetsList.Clear();
        }

        /// <summary>
        /// Adds the specified ignored target to the static EmeraldAISystem IgnoredTargetsList.
        /// </summary>
        public void SetIgnoredTarget(Transform TargetTransform)
        {
            if (!EmeraldAISystem.IgnoredTargetsList.Contains(TargetTransform))
            {
                EmeraldAISystem.IgnoredTargetsList.Add(TargetTransform);
            }
        }

        /// <summary>
        /// Removes the specified ignored target from the static EmeraldAISystem IgnoredTargetsList.
        /// </summary>
        public void ClearIgnoredTarget(Transform TargetTransform)
        {
            if (!EmeraldAISystem.IgnoredTargetsList.Contains(TargetTransform))
            {
                Debug.Log("The TargetTransform did not exist in the EmeraldAISystem IgnoreTargetsList list.");
                return;
            }
 
            EmeraldAISystem.IgnoredTargetsList.Remove(TargetTransform);
        }

        /// <summary>
        /// Returns the AI to its starting destination
        /// </summary>
        public void ReturnToStart()
        {
            EmeraldComponent.ReturningToStartInProgress = true;
            Invoke("DelayTargetReset ", 0.1f);
        }

        void DelayTargetReset ()
        {
            EmeraldComponent.m_NavMeshAgent.ResetPath();
            EmeraldComponent.EmeraldBehaviorsComponent.DefaultState();
            EmeraldComponent.CombatStateRef = EmeraldAISystem.CombatState.NotActive;
        }

        /// <summary>
        /// Returns the current distance between the AI and their current target (Returns -1 if the Current Target is null).
        /// </summary>
        /// <returns></returns>
        public float GetDistanceFromTarget ()
        {
            if (EmeraldComponent.CurrentTarget != null)
            {
                return EmeraldComponent.DistanceFromTarget;
            }
            else
            {
                Debug.Log("This AI's Current Target is null");
                return -1;
            }
        }

        /// <summary>
        /// Returns the AI's current target.
        /// </summary>
        public Transform GetCombatTarget()
        {
            return EmeraldComponent.CurrentTarget;
        }

        /// <summary>
        /// Assigns a specified combat target for your AI to attack within the AI's Detection Radius. Note: Targets outside of an AI's Detection Radius will be ignored. If you want no distance limitations, use OverrideCombatTarget(Transform Target).
        /// </summary>
        public void SetCombatTarget(Transform Target)
        {
            if (EmeraldComponent.ConfidenceRef != EmeraldAISystem.ConfidenceType.Coward && Target != null)
            {
                EmeraldComponent.EmeraldDetectionComponent.SetDetectedTarget(Target);
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.AttackDistance;
                EmeraldComponent.m_NavMeshAgent.destination = Target.position;
            }
            else if (Target == null)
            {
                Debug.Log("The SetCombatTarget paramter is null. Ensure that the target exists before calling this function.");
            }
        }

        /// <summary>
        /// Assigns a specified combat target for your AI to attack ignoring any distance limitations. If the target is not within attacking range, the AI will move to the target's position and attack based on its attack distance.
        /// </summary>
        public void OverrideCombatTarget(Transform Target)
        {
            if (EmeraldComponent.ConfidenceRef != EmeraldAISystem.ConfidenceType.Coward && Target != null)
            {
                EmeraldComponent.EmeraldDetectionComponent.SetDetectedTarget(Target);
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.AttackDistance;
                EmeraldComponent.m_NavMeshAgent.destination = Target.position;
                EmeraldComponent.MaxChaseDistance = 4000;
                EmeraldComponent.EmeraldBehaviorsComponent.ActivateCombatState();
            }
            else if (Target == null)
            {
                Debug.Log("The OverrideCombatTarget paramter is null. Ensure that the target exists before calling this function.");
            }
        }

        /// <summary>
        /// Makes an AI flee from the specified target by overiding their behavior and confidence levels. ChangeTemperament can be set to false to not overide an AI's behavior and confidence levels.
        /// </summary>
        public void FleeFromTarget(Transform FleeTarget, bool ChangeTemperament = true)
        {
            if (FleeTarget != null)
            {
                if (ChangeTemperament)
                {
                    ChangeBehavior(EmeraldAISystem.CurrentBehavior.Cautious, true);
                    ChangeConfidence(EmeraldAISystem.ConfidenceType.Coward, true);
                }

                EmeraldComponent.CurrentTarget = FleeTarget;
                EmeraldComponent.EmeraldDetectionComponent.DetectTargetType(EmeraldComponent.CurrentTarget, true);
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldComponent.EmeraldBehaviorsComponent.ActivateCombatState();
            }
            else if (FleeTarget == null)
            {
                Debug.Log("The FleeTarget paramter is null. Ensure that the target exists before calling this function.");
            }
        }

        /// <summary>
        /// Assigns a new follow target for your companion AI to follow.
        /// </summary>
        public void SetFollowerTarget(Transform Target)
        {
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.FollowingStoppingDistance;
            EmeraldComponent.CurrentFollowTarget = Target;
            EmeraldComponent.CurrentMovementState = EmeraldAISystem.MovementState.Run;
            EmeraldComponent.UseAIAvoidance = EmeraldAISystem.YesOrNo.No;
        }

        /// <summary>
        /// Tames the AI to become the Target's companion. Note: The tameable AI must have a Cautious Behavior Type and 
        /// a Brave or Foolhardy Confidence Type. The AI must be tamed before the AI turns Aggressive to be successful.
        /// </summary>
        public void TameAI(Transform Target)
        {
            if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Cautious)
            {
                if (EmeraldComponent.ConfidenceRef == EmeraldAISystem.ConfidenceType.Brave ||
                    EmeraldComponent.ConfidenceRef == EmeraldAISystem.ConfidenceType.Foolhardy)
                {
                    EmeraldComponent.CurrentTarget = null;
                    EmeraldComponent.CombatStateRef = EmeraldAISystem.CombatState.NotActive;
                    EmeraldComponent.BehaviorRef = EmeraldAISystem.CurrentBehavior.Companion;
                    EmeraldComponent.StartingBehaviorRef = (int)EmeraldComponent.BehaviorRef;
                    EmeraldComponent.CurrentMovementState = EmeraldAISystem.MovementState.Run;
                    EmeraldComponent.StartingMovementState = EmeraldAISystem.MovementState.Run;
                    EmeraldComponent.UseAIAvoidance = EmeraldAISystem.YesOrNo.No;
                    EmeraldComponent.CurrentFollowTarget = Target;
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.FollowingStoppingDistance;
                }
            }
        }

        /// <summary>
        /// Returns the transform that last attacked the AI.
        /// </summary>
        public Transform GetAttacker ()
        {
            return EmeraldComponent.LastAttacker;
        }

        /// <summary>
        /// Changes the AI's Detection Type.
        /// </summary>
        public void ChangeDetectionType (EmeraldAISystem.DetectionType DetectionType)
        {
            EmeraldComponent.DetectionTypeRef = DetectionType;
        }

        /// <summary>
        /// Updates the AI's Health Bar color
        /// </summary>
        public void UpdateUIHealthBarColor(Color NewColor)
        {
            if (EmeraldComponent.CreateHealthBarsRef == EmeraldAISystem.YesOrNo.Yes)
            {
                GameObject HealthBarChild = EmeraldComponent.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                UnityEngine.UI.Image HealthBarRef = HealthBarChild.transform.Find("AI Health Bar").GetComponent<UnityEngine.UI.Image>();
                HealthBarRef.color = NewColor;
                UnityEngine.UI.Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<UnityEngine.UI.Image>();
                HealthBarBackgroundImageRef.color = EmeraldComponent.HealthBarBackgroundColor;
            }
        }

        /// <summary>
        /// Updates the AI's Health Bar Background color
        /// </summary>
        public void UpdateUIHealthBarBackgroundColor(Color NewColor)
        {
            if (EmeraldComponent.CreateHealthBarsRef == EmeraldAISystem.YesOrNo.Yes)
            {
                GameObject HealthBarChild = EmeraldComponent.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                UnityEngine.UI.Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<UnityEngine.UI.Image>();
                HealthBarBackgroundImageRef.color = NewColor;
            }
        }

        /// <summary>
        /// Updates the AI's Name color
        /// </summary>
        public void UpdateUINameColor(Color NewColor)
        {
            if (EmeraldComponent.CreateHealthBarsRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.DisplayAINameRef == EmeraldAISystem.YesOrNo.Yes)
            {
                EmeraldComponent.AINameUI.color = NewColor;
            }
        }

        /// <summary>
        /// Updates the AI's Name text
        /// </summary>
        public void UpdateUINameText(string NewName)
        {
            if (EmeraldComponent.CreateHealthBarsRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.DisplayAINameRef == EmeraldAISystem.YesOrNo.Yes)
            {
                EmeraldComponent.AINameUI.text = NewName;
            }
        }

        /// <summary>
        /// Updates the AI's Max Health, Current Health, and health bar if it's enabled
        /// </summary>
        public void UpdateHealth(int MaxHealth, int CurrentHealth)
        {
            EmeraldComponent.CurrentHealth = CurrentHealth;
            EmeraldComponent.StartingHealth = MaxHealth;
        }

        /// <summary>
        /// Updates the AI's dynamic wandering position to the AI's current positon.
        /// </summary>
        public void UpdateDynamicWanderPosition()
        {
            EmeraldComponent.StartingDestination = this.transform.position;
        }

        /// <summary>
        /// Sets the AI's dynamic wandering position to the position of the Destination transform. 
        /// This is useful for functionality such as custom AI schedules. Note: This will automatically change
        /// your AI's Wander Type to Dynamic.
        /// </summary>
        public void SetDynamicWanderPosition(Transform Destination)
        {
            ChangeWanderType(EmeraldAISystem.WanderType.Dynamic);
            EmeraldComponent.StartingDestination = Destination.position;
        }

        /// <summary>
        /// Updates the AI's starting position to the AI's current position.
        /// </summary>
        public void UpdateStartingPosition()
        {
            EmeraldComponent.StartingDestination = this.transform.position;
        }

        /// <summary>
        /// Sets the AI's destination using the transform's position.
        /// </summary>
        public void SetDestination(Transform Destination)
        {
            EmeraldComponent.LockTurning = false;
            EmeraldComponent.AIReachedDestination = false;
            EmeraldComponent.m_NavMeshAgent.destination = Destination.position;
            EmeraldComponent.SingleDestination = Destination.position;
            EmeraldComponent.AIAnimator.SetBool("Idle Active", false);
            EmeraldComponent.StartingDestination = Destination.position;
        }

        /// <summary>
        /// Sets the AI's destination using a Vector3 position.
        /// </summary>
        public void SetDestinationPosition(Vector3 DestinationPosition)
        {
            EmeraldComponent.LockTurning = false;
            EmeraldComponent.AIReachedDestination = false;
            EmeraldComponent.m_NavMeshAgent.destination = DestinationPosition;
            EmeraldComponent.SingleDestination = DestinationPosition;
            EmeraldComponent.AIAnimator.SetBool("Idle Active", false);
            EmeraldComponent.StartingDestination = DestinationPosition;
        }

        /// <summary>
        /// Generates a new position to move to within the specified radius based on the AI's current position.
        /// </summary>
        public void GenerateNewWaypointCurrentPosition(int Radius)
        {
            Vector3 NewDestination = transform.position + new Vector3(Random.insideUnitSphere.y, 0, Random.insideUnitSphere.z) * Radius;
            RaycastHit HitDown;
            if (Physics.Raycast(new Vector3(NewDestination.x, NewDestination.y + 5, NewDestination.z), -transform.up, out HitDown, 10, EmeraldComponent.DynamicWanderLayerMask, QueryTriggerInteraction.Ignore))
            {
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(NewDestination, out hit, 4, EmeraldComponent.m_NavMeshAgent.areaMask))
                {
                    EmeraldComponent.m_NavMeshAgent.SetDestination(NewDestination);
                }
            }
        }

        /// <summary>
        /// Adds a waypoint to an AI's Waypoint List.
        /// </summary>
        public void AddWaypoint(Transform Waypoint)
        {
            EmeraldComponent.WaypointsList.Add(Waypoint.position);
        }

        /// <summary>
        /// Removes a waypoint from the AI's Wapoint List according to the specified index.
        /// </summary>
        public void RemoveWaypoint(int WaypointIndex)
        {
            EmeraldComponent.WaypointsList.RemoveAt(WaypointIndex);
        }

        /// <summary>
        /// Clears all of an AI's current waypoints. Note: When an AI's waypoints are cleared, it will be set to the Stationary wander type to avoid an error. 
        /// If you want the AI to follow newly created waypoints, you will need to set it's Wander Type back to Waypoint with the ChangeWanderType functio (located within the EmeraldAIEventsManager script).
        /// </summary>
        public void ClearAllWaypoints()
        {
            EmeraldComponent.WanderTypeRef = EmeraldAISystem.WanderType.Stationary;
            EmeraldComponent.WaypointsList.Clear();
        }

        /// <summary>
        /// Refills the AI's health to full instantly
        /// </summary>
        public void InstantlyRefillAIHealth()
        {
            EmeraldComponent.CurrentHealth = EmeraldComponent.StartingHealth;
        }

        /// <summary>
        /// Stops an AI from moving. This is useful for functionality like dialogue.
        /// </summary>
        public void StopMovement()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = true;
        }

        /// <summary>
        /// Resumes an AI's movement after using the StopMovement function.
        /// </summary>
        public void ResumeMovement()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = false;
        }

        /// <summary>
        /// Stops a Companion AI from moving.
        /// </summary>
        public void StopFollowing()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = true;
        }

        /// <summary>
        /// Allows a Companion AI to resume following its follower.
        /// </summary>
        public void ResumeFollowing()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = false;
        }

        /// <summary>
        /// Allows a Companion AI to guard the assigned position.
        /// </summary>
        public void CompanionGuardPosition(Vector3 PositionToGuard)
        {
            Transform TempFollower = new GameObject(EmeraldComponent.AIName + "'s position to guard").transform;
            TempFollower.position = PositionToGuard;
            SetFollowerTarget(TempFollower);
        }

        /// <summary>
        /// Searches for a new target within the AI's Attacking Range clostest to the AI.
        /// </summary>
        public void SearchForClosestTarget()
        {
            EmeraldComponent.EmeraldDetectionComponent.SearchForTarget();
            EmeraldComponent.EmeraldDetectionComponent.SetDetectedTarget(EmeraldComponent.CurrentTarget);
        }

        /// <summary>
        /// Searches for a new random target within the AI's Attacking Range.
        /// </summary>
        public void SearchForRandomTarget()
        {
            EmeraldComponent.EmeraldDetectionComponent.SearchForRandomTarget = true;
            EmeraldComponent.EmeraldDetectionComponent.SearchForTarget();
        }

        /// <summary>
        /// Updates the AI's Min and Max Melee Damage using the sent Melee Attack Number from the AI's Melee Attack List starting from 1. If the attack is not using randomized damage, the Min Damage will be used.
        /// </summary>
        public void UpdateAIMeleeDamage(int MeleeAttackNumber, int MinDamage, int MaxDamage)
        {
            EmeraldComponent.MeleeAttacks[MeleeAttackNumber - 1].MinDamage = MinDamage;
            EmeraldComponent.MeleeAttacks[MeleeAttackNumber - 1].MaxDamage = MaxDamage;
        }

        /// <summary>
        /// Updates the AI's Minimum and Maximum Melee Attack Speed
        /// </summary>
        public void UpdateAIMeleeAttackSpeed (int MinAttackSpeed, int MaxAttackSpeed)
        {
            EmeraldComponent.MinMeleeAttackSpeed = MinAttackSpeed;
            EmeraldComponent.MaxMeleeAttackSpeed = MaxAttackSpeed;
        }

        /// <summary>
        /// Updates the AI's Minimum and Maximum Ranged Attack Speed
        /// </summary>
        public void UpdateAIRangedAttackSpeed(int MinAttackSpeed, int MaxAttackSpeed)
        {
            EmeraldComponent.MinRangedAttackSpeed = MinAttackSpeed;
            EmeraldComponent.MaxRangedAttackSpeed = MaxAttackSpeed;
        }

        /// <summary>
        /// Rotates the AI towards the specified target that utilizes the AI's turning animations. The angle in which the AI will stop rotating is based off of an AI's Turning Angle set within the Emerald AI editor. 
        /// This will also stop the AI's movement for the Duration parameter.
        /// If a Duration of -1 is used, the duration period will be indefinite and can be canceled with CancelRotateAITowardsTarget().
        /// </summary>
        /// <param name="Target">The target the AI will rotate towards. This can be an object, a player, or another AI.</param>
        /// <param name="Duration">The length in seconds until the AI resumes wandering according to its Wander Type.</param>
        public void RotateAITowardsTarget(Transform Target, int Duration)
        {
            EmeraldComponent.RotateTowardsTarget = true;            
            StopMovement();
            if (m_RotateTowards != null) {StopCoroutine(m_RotateTowards);}
            m_RotateTowards = StartCoroutine(RotateTowards(Target, Duration));
        }

        /// <summary>
        /// Cancels the indefinite rotation towards the target after RotateAITowardsTarget has been called.
        /// </summary>
        public void CancelRotateAITowardsTarget()
        {
            EmeraldComponent.RotateTowardsTarget = false;
            ResumeMovement();
        }

        /// <summary>
        /// Rotates the AI away from the specified target that utilizes the AI's turning animations. The angle in which the AI will stop rotating is based off of an AI's Turning Angle set within the Emerald AI editor. 
        /// This will also stop the AI's movement for the Duration parameter. 
        /// If a Duration of -1 is used, the duration period will be indefinite and can be canceled with CancelRotateAIAwayFromTarget().
        /// </summary>
        /// <param name="Target">The target the AI will rotate away from. This can be an object, a player, or another AI.</param>
        /// <param name="Duration">The length in seconds until the AI resumes wandering according to its Wander Type.</param>
        public void RotateAIAwayFromTarget(Transform Target, int Duration)
        {
            EmeraldComponent.RotateTowardsTarget = true;
            StopMovement();
            if (m_RotateTowards != null) { StopCoroutine(m_RotateTowards); }
            m_RotateTowards = StartCoroutine(RotateTowards(Target, Duration, Vector3.zero, true));
        }

        /// <summary>
        /// Cancels the indefinite rotation away from the target after RotateAIAwayFromTarget has been called.
        /// </summary>
        public void CancelRotateAIAwayFromTarget()
        {
            EmeraldComponent.RotateTowardsTarget = false;
            ResumeMovement();
        }

        /// <summary>
        /// Rotates the AI towards the specified position that utilizes the AI's turning animations. The angle in which the AI will stop rotating is based off of an AI's Turning Angle set within the Emerald AI editor. 
        /// This will also stop the AI's movement for the Duration parameter. 
        /// If a Duration of -1 is used, the duration period will be indefinite and can be canceled with CancelRotateAITowardsPosition().
        /// </summary>
        /// <param name="TargetPosition">The position the AI will rotate towards. This can be an object, a player, or another AI.</param>
        /// <param name="Duration">The length in seconds until the AI resumes wandering according to its Wander Type.</param>
        public void RotateAITowardsPosition(Vector3 TargetPosition, int Duration)
        {
            EmeraldComponent.RotateTowardsTarget = true;
            StopMovement();
            if (m_RotateTowards != null) { StopCoroutine(m_RotateTowards); }
            m_RotateTowards = StartCoroutine(RotateTowards(transform, Duration, TargetPosition));
        }

        /// <summary>
        /// Cancels the indefinite rotation towards the position after RotateAITowardsPosition has been called.
        /// </summary>
        public void CancelRotateAITowardsPosition()
        {
            EmeraldComponent.RotateTowardsTarget = false;
            ResumeMovement();
        }

        /// <summary>
        /// Rotates the AI away from the specified position that utilizes the AI's turning animations. The angle in which the AI will stop rotating is based off of an AI's Turning Angle set within the Emerald AI editor. 
        /// This will also stop the AI's movement for the Duration parameter. 
        /// If a Duration of -1 is used, the duration period will be indefinite and can be canceled with CancelRotateAIAwayFromPosition().
        /// </summary>
        /// <param name="TargetPosition">The position the AI will rotate away from. This can be an object, a player, or another AI.</param>
        /// <param name="Duration">The length in seconds until the AI resumes wandering according to its Wander Type.</param>
        public void RotateAIAwayFromPosition(Vector3 TargetPosition, int Duration)
        {
            EmeraldComponent.RotateTowardsTarget = true;
            StopMovement();
            if (m_RotateTowards != null) { StopCoroutine(m_RotateTowards); }
            m_RotateTowards = StartCoroutine(RotateTowards(transform, Duration, TargetPosition, true));
        }

        /// <summary>
        /// Cancels the indefinite rotation away from the position after RotateAIAwayFromPosition has been called.
        /// </summary>
        public void CancelRotateAIAwayFromPosition()
        {
            EmeraldComponent.RotateTowardsTarget = false;
            ResumeMovement();
        }

        IEnumerator RotateTowards (Transform Target, int Duration, Vector3 TargetPosition = new Vector3(), bool RotateAwayFrom = false)
        {
            float angle = 150;
            float RayCastUpdateTimer = 0;
            Vector3 Direction = new Vector3(0, 90, 0);
            Vector3 DestinationDirection = Vector3.zero;
            EmeraldComponent.DestinationAdjustedAngle = 150;

            while (EmeraldComponent.DestinationAdjustedAngle >= EmeraldComponent.NonCombatAngleToTurn)
            {
                EmeraldComponent.AIAnimator.SetFloat("Speed", 0, 0.1f, Time.deltaTime);
                RayCastUpdateTimer += Time.deltaTime;

                if (RayCastUpdateTimer >= EmeraldComponent.RayCastUpdateSeconds && EmeraldComponent.AlignAIWithGroundRef == EmeraldAISystem.YesOrNo.Yes)
                {
                    RaycastHit HitDown;
                    if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), -transform.up, out HitDown, 2, EmeraldComponent.AlignmentLayerMask))
                    {
                        if (HitDown.transform != this.transform)
                        {
                            EmeraldComponent.SurfaceNormal = HitDown.normal;
                            EmeraldComponent.SurfaceNormal.x = Mathf.Clamp(EmeraldComponent.SurfaceNormal.x, -EmeraldComponent.MaxNormalAngle, EmeraldComponent.MaxNormalAngle);
                            EmeraldComponent.SurfaceNormal.z = Mathf.Clamp(EmeraldComponent.SurfaceNormal.z, -EmeraldComponent.MaxNormalAngle, EmeraldComponent.MaxNormalAngle);
                            RayCastUpdateTimer = 0;
                        }
                    }
                }

                //Get the angle between the current target and the AI. If using the alignment feature,
                //adjust the angle to include the rotation difference of the AI's current surface angle.
                if (TargetPosition == Vector3.zero)
                {
                    if (!RotateAwayFrom)
                    {
                        Direction = new Vector3(Target.position.x, 0, Target.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
                        angle = Vector3.Angle(transform.forward, Direction);
                        DestinationDirection = Direction;
                    }
                    else
                    {
                        Direction = new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(Target.position.x, 0, Target.position.z);
                        angle = Vector3.Angle(transform.forward, Direction);
                        DestinationDirection = Direction;
                    }
                }
                else
                {
                    if (!RotateAwayFrom)
                    {
                        Direction = new Vector3(TargetPosition.x, 0, TargetPosition.z) - new Vector3(transform.position.x, 0, transform.position.z);
                        angle = Vector3.Angle(transform.forward, Direction);
                        DestinationDirection = Direction;
                    }
                    else
                    {
                        Direction = new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(TargetPosition.x, 0, TargetPosition.z);
                        angle = Vector3.Angle(transform.forward, Direction);
                        DestinationDirection = Direction;
                    }
                }

                if (EmeraldComponent.AlignAIWithGroundRef == EmeraldAISystem.YesOrNo.Yes)
                {
                    float RoationDifference = transform.localEulerAngles.x;
                    RoationDifference = (RoationDifference > 180) ? RoationDifference - 360 : RoationDifference;
                    EmeraldComponent.DestinationAdjustedAngle = Mathf.Abs(angle) - Mathf.Abs(RoationDifference);
                }
                else if (EmeraldComponent.AlignAIWithGroundRef == EmeraldAISystem.YesOrNo.No)
                {
                    EmeraldComponent.DestinationAdjustedAngle = Mathf.Abs(angle);
                }

                if (EmeraldComponent.DestinationAdjustedAngle >= EmeraldComponent.AngleToTurn && DestinationDirection != Vector3.zero && !EmeraldComponent.IsAttacking)
                {
                    if (EmeraldComponent.AlignAIWithGroundRef == EmeraldAISystem.YesOrNo.Yes)
                    {
                        Quaternion qTarget = Quaternion.LookRotation(DestinationDirection, Vector3.up);
                        Quaternion qGround = Quaternion.FromToRotation(Vector3.up, EmeraldComponent.SurfaceNormal) * qTarget;
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, qGround, Time.deltaTime * EmeraldComponent.StationaryTurningSpeedCombat);
                    }
                    else if (EmeraldComponent.AlignAIWithGroundRef == EmeraldAISystem.YesOrNo.No)
                    {
                        Quaternion qTarget = Quaternion.LookRotation(DestinationDirection, Vector3.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, qTarget, Time.deltaTime * EmeraldComponent.StationaryTurningSpeedCombat);
                    }
                }

                if (EmeraldComponent.DestinationAdjustedAngle >= EmeraldComponent.AngleToTurn && DestinationDirection != Vector3.zero && EmeraldComponent.AIAgentActive)
                {
                    Vector3 cross = Vector3.Cross(transform.rotation * Vector3.forward, Quaternion.LookRotation(DestinationDirection, Vector3.up) * Vector3.forward);

                    if (cross.y < -EmeraldComponent.AngleToTurn * 0.01f)
                    {
                        EmeraldComponent.AIAnimator.SetBool("Idle Active", false);
                        EmeraldComponent.AIAnimator.SetBool("Turn Left", true);
                        EmeraldComponent.AIAnimator.SetBool("Turn Right", false);
                    }
                    else if (cross.y > EmeraldComponent.AngleToTurn * 0.01f)
                    {
                        EmeraldComponent.AIAnimator.SetBool("Idle Active", false);
                        EmeraldComponent.AIAnimator.SetBool("Turn Right", true);
                        EmeraldComponent.AIAnimator.SetBool("Turn Left", false);
                    }
                    else if (cross.y > -EmeraldComponent.AngleToTurn * 0.01f)
                    {
                        EmeraldComponent.AIAnimator.SetBool("Idle Active", false);
                        EmeraldComponent.AIAnimator.SetBool("Turn Left", true);
                        EmeraldComponent.AIAnimator.SetBool("Turn Right", false);
                    }
                    else
                    {
                        EmeraldComponent.AIAnimator.SetBool("Turn Left", false);
                        EmeraldComponent.AIAnimator.SetBool("Turn Right", false);
                    }
                }

                yield return null;
            }

            EmeraldComponent.DestinationAdjustedAngle = 0;
            EmeraldComponent.RotateTowardsTarget = true;
            EmeraldComponent.AIAnimator.SetBool("Turn Left", false);
            EmeraldComponent.AIAnimator.SetBool("Turn Right", false);

            if (Duration != -1)
            {
                yield return new WaitForSeconds(Duration);

                EmeraldComponent.RotateTowardsTarget = false;
                ResumeMovement();
            }
        }

        /// <summary>
        /// Changes the relation of the given faction. Note: The faction must be available in the AI's faction list.
        /// </summary>
        /// <param name="Faction"> The name of the faction to change.</param>
        /// <param name="FactionLevel">The level to set the faction to typed as a string. The options are Enemy, Neutral, or Friendly</param>
        public void SetFactionLevel(string Faction, string FactionLevel)
        {
            EmeraldAIFactionData FactionData = Resources.Load("Faction Data") as EmeraldAIFactionData;

            if (FactionLevel == "Enemy")
            {
                for (int i = 0; i < EmeraldComponent.FactionRelationsList.Count; i++)
                {
                    if (EmeraldComponent.FactionRelationsList[i].FactionIndex == FactionData.FactionNameList.IndexOf(Faction))
                    {
                        EmeraldComponent.FactionRelationsList[i].RelationTypeRef = 0;
                    }
                }
            }
            else if (FactionLevel == "Neutral")
            {
                for (int i = 0; i < EmeraldComponent.FactionRelationsList.Count; i++)
                {
                    if (EmeraldComponent.FactionRelationsList[i].FactionIndex == FactionData.FactionNameList.IndexOf(Faction))
                    {
                        EmeraldComponent.FactionRelationsList[i].RelationTypeRef = (EmeraldAISystem.FactionsList.RelationType)1;
                    }
                }
            }
            else if (FactionLevel == "Friendly")
            {
                for (int i = 0; i < EmeraldComponent.FactionRelationsList.Count; i++)
                {
                    if (EmeraldComponent.FactionRelationsList[i].FactionIndex == FactionData.FactionNameList.IndexOf(Faction))
                    {
                        EmeraldComponent.FactionRelationsList[i].RelationTypeRef = (EmeraldAISystem.FactionsList.RelationType)2;
                    }
                }
            }
        }

        /// <summary>
        /// Adds the Faction and Faction Relation to the AI's Faction Relations List. Note: The faction must exist within the Faction Manager's Current Faction List.
        /// </summary>
        /// <param name="Faction"> The name of the faction to change.</param>
        /// <param name="FactionLevel">The level to set the faction to typed as a string. The options are Enemy, Neutral, or Friendly</param>
        public void AddFactionRelation(string Faction, string FactionLevel)
        {
            int FactionEnumLevel = 0;
            EmeraldAIFactionData FactionData = Resources.Load("Faction Data") as EmeraldAIFactionData;
            if (!FactionData.FactionNameList.Contains(Faction))
            {
                Debug.Log("The faction: " + Faction + " does not exist in the Faction Manager. Please add it using the Emerald AI Faction Manager.");
                return;
            }

            if (FactionLevel == "Enemy")
            {
                FactionEnumLevel = 0;
            }
            else if (FactionLevel == "Neutral")
            {
                FactionEnumLevel = 1;
            }
            else if (FactionLevel == "Friendly")
            {
                FactionEnumLevel = 2;
            }

            for (int i = 0; i < EmeraldComponent.FactionRelationsList.Count; i++)
            {
                if (EmeraldComponent.FactionRelationsList[i].FactionIndex == FactionData.FactionNameList.IndexOf(Faction))
                {
                    Debug.Log("This AI already contains the faction: " + Faction + ". If you would like to modify an AI's existing faction, please use SetFactionLevel(string Faction, string FactionLevel) instead.");
                    return;
                }
            }

            EmeraldComponent.FactionRelationsList.Add(new EmeraldAISystem.FactionsList(FactionData.FactionNameList.IndexOf(Faction), FactionEnumLevel));
            SetFactionLevel(Faction, FactionLevel);
        }

        /// <summary>
        /// Returns the relation of the EmeraldTarget with this AI.
        /// </summary>
        public EmeraldAISystem.RelationType GetAIRelation(EmeraldAISystem EmeraldTarget)
        {
            EmeraldComponent.ReceivedFaction = EmeraldTarget.CurrentFaction;

            if (EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldComponent.ReceivedFaction)] == 0)
            {
                return EmeraldAISystem.RelationType.Enemy;
            }
            else if (EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldComponent.ReceivedFaction)] == 1)
            {
                return EmeraldAISystem.RelationType.Neutral;
            }
            else
            {
                return EmeraldAISystem.RelationType.Friendly;
            }
        }

        /// <summary>
        /// Returns the relation of this AI and the player.
        /// </summary>
        public EmeraldAISystem.RelationType GetPlayerRelation()
        {
            if (EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Enemy)
            {
                return EmeraldAISystem.RelationType.Enemy;
            }
            else if (EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Neutral)
            {
                return EmeraldAISystem.RelationType.Neutral;
            }
            else
            {
                return EmeraldAISystem.RelationType.Friendly;
            }
        }

        /// <summary>
        /// Sets the relation of this AI and the player.
        /// </summary>
        public void SetPlayerRelation (EmeraldAISystem.PlayerFactionClass.RelationType Relation)
        {
            EmeraldComponent.PlayerFaction[0].RelationTypeRef = Relation;
        }

        /// <summary>
        /// Changes the AI's faction. (Note: The FactionName must exists within the Faction Manager's Current Faction list)
        /// </summary>
        public void ChangeFaction(string FactionName)
        {
            EmeraldAIFactionData FactionData = Resources.Load("Faction Data") as EmeraldAIFactionData;

            if (FactionData.FactionNameList.Contains(FactionName))
            {
                EmeraldComponent.CurrentFaction = FactionData.FactionNameList.IndexOf(FactionName);
            }
            else
            {
                Debug.Log("Faction not Found");
            }
        }

        /// <summary>
        /// Checks to see if the player is currently within the AI's detection radius by returning true or false.
        /// </summary>
        public bool CheckForPlayerDetection ()
        {
            if (EmeraldComponent.CurrentTarget != null && EmeraldComponent.TargetTypeRef == EmeraldAISystem.TargetType.Player)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns this AI's current Faction as a string. (This is the Faction that is set through the Emerald AI Editor under Faction Options)
        /// </summary>
        public string GetFaction ()
        {
            EmeraldAIFactionData FactionData = Resources.Load("Faction Data") as EmeraldAIFactionData;
            var CurrentFaction = FactionData.FactionNameList[EmeraldComponent.CurrentFaction];
            return CurrentFaction;
        }

        /// <summary>
        /// Change the specified index, within the AI's Offensive Abilities List, with a new Ability Object. This can be useful for weapon or spell swaping of an AI.
        /// </summary>
        /// <param name="AbilityIndex">The index of the Ability Object within an AI's Offensive Abilities List.</param>
        /// <param name="AbilityObject">The new Ability Object that will be used.</param>
        public void ChangeOffsensiveAbilityObject (int AbilityIndex, EmeraldAIAbility AbilityObject)
        {
            EmeraldComponent.OffensiveAbilities[AbilityIndex].OffensiveAbility = AbilityObject;
        }

        /// <summary>
        /// Change the specified index, within the AI's Support Abilities List, with a new Ability Object.
        /// </summary>
        /// <param name="AbilityIndex">The index of the Ability Object within an AI's Support Abilities List.</param>
        /// <param name="AbilityObject">The new Ability Object that will be used.</param>
        public void ChangeSupportAbilityObject(int AbilityIndex, EmeraldAIAbility AbilityObject)
        {
            EmeraldComponent.SupportAbilities[AbilityIndex].SupportAbility = AbilityObject;
        }

        /// <summary>
        /// Change the specified index, within the AI's Summoning Abilities List, with a new Ability Object.
        /// </summary>
        /// <param name="AbilityIndex">The index of the Ability Object within an AI's Summoning Abilities List.</param>
        /// <param name="AbilityObject">The new Ability Object that will be used.</param>
        public void ChangeSummoningAbilityObject(int AbilityIndex, EmeraldAIAbility AbilityObject)
        {
            EmeraldComponent.SummoningAbilities[AbilityIndex].SummoningAbility = AbilityObject;
        }

        /// <summary>
        /// Sets an AI back to its wandering Default State. Note: If you want to respawn an AI, use ResetAI instead this function.
        /// </summary>
        public void ReturnToDefaultState ()
        {
            EmeraldComponent.EmeraldBehaviorsComponent.DefaultState();
        }

        /// <summary>
        /// Debug logs a message to the Unity Console for testing purposes.
        /// </summary>
        public void DebugLogMessage (string Message)
        {
            Debug.Log(Message);
        }

        /// <summary>
        /// Enables the passed gameobject.
        /// </summary>
        public void EnableObject(GameObject Object)
        {
            Object.SetActive(true);
        }

        /// <summary>
        /// Disables the passed gameobject.
        /// </summary>
        public void DisableObject(GameObject Object)
        {
            Object.SetActive(false);
        }

        /// <summary>
        /// Resets an AI to its default state. This is useful if an AI is being respawned. 
        /// </summary>
        public void ResetAI()
        {
            //Re-enable all of the AI's components.
            EmeraldComponent.EmeraldInitializerComponent.DisableRagdoll();
            EmeraldComponent.EmeraldInitializerComponent.enabled = true;
            EmeraldComponent.EmeraldEventsManagerComponent.enabled = true;
            EmeraldComponent.EmeraldDetectionComponent.enabled = true;
            EmeraldComponent.EmeraldLookAtComponent.enabled = true;
            EmeraldComponent.CurrentHealth = EmeraldComponent.StartingHealth;
            EmeraldComponent.TotalSummonedAI = 0;
            gameObject.tag = EmeraldComponent.StartingTag;
            gameObject.layer = EmeraldComponent.StartingLayer;
            EmeraldComponent.IsDead = false;
            EmeraldComponent.AIBoxCollider.enabled = true;
            EmeraldComponent.TargetDetectionActive = true;
            EmeraldComponent.AIAnimator.enabled = true;
            EmeraldComponent.m_NavMeshAgent.enabled = true;
            EmeraldComponent.TurnDirectionMet = false;
            EmeraldComponent.StartingDestination = transform.position;
            EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.StartingDestination;
            EmeraldComponent.enabled = true;
            EmeraldComponent.FinalImpactSound = false;
            EmeraldComponent.EmeraldBehaviorsComponent.DefaultState();
            EmeraldComponent.EmeraldInitializerComponent.CustomIKFaded = false;
            EmeraldComponent.OnEnabledEvent.Invoke();
            EmeraldComponent.AIAnimator.Rebind();
            if (EmeraldComponent.EmeraldHandIKComp != null) EmeraldComponent.EmeraldHandIKComp.enabled = true;
            if (EmeraldComponent.HolsteredMeleeWeaponObject != null) EmeraldComponent.HolsteredMeleeWeaponObject.SetActive(true);
            if (EmeraldComponent.HolsteredRangedWeaponObject != null) EmeraldComponent.HolsteredRangedWeaponObject.SetActive(true);

            //Reapply the AI's Animator Controller settings applied on Start because, when the
            //Animator Controller is disabled, they're reset to their default settings. 
            if (EmeraldComponent.UseEquipAnimation == EmeraldAISystem.YesOrNo.Yes)
            {
                EmeraldComponent.AIAnimator.SetBool("Animate Weapon State", true);
            }
            else if (EmeraldComponent.UseEquipAnimation == EmeraldAISystem.YesOrNo.No
                || EmeraldComponent.PutAwayWeaponAnimation == null
                || EmeraldComponent.PullOutWeaponAnimation == null)
            {
                EmeraldComponent.AIAnimator.SetBool("Animate Weapon State", false);
            }

            if (EmeraldComponent.UseHitAnimations == EmeraldAISystem.YesOrNo.Yes)
            {
                EmeraldComponent.AIAnimator.SetBool("Use Hit", true);
            }
            else
            {
                EmeraldComponent.AIAnimator.SetBool("Use Hit", false);
            }

            if (EmeraldComponent.ReverseWalkAnimation)
            {
                EmeraldComponent.AIAnimator.SetFloat("Backup Speed", -1);
            }
            else
            {
                EmeraldComponent.AIAnimator.SetFloat("Backup Speed", 1);
            }

            EmeraldComponent.AIAnimator.SetBool("Idle Active", false);

            EmeraldComponent.EmeraldBehaviorsComponent.DefaultState();
            EmeraldComponent.EmeraldInitializerComponent.InitializeWeaponTypeAnimationAndSettings();
        }

        public void HealOverTimeAbility (EmeraldAIAbility AbilityObject)
        {
            //Only allow one healing over time ability to be active at once.
            if (EmeraldComponent.HealingOverTimeCoroutine != null)
            {
                StopCoroutine(EmeraldComponent.HealingOverTimeCoroutine);
            }

            EmeraldComponent.HealingOverTimeCoroutine = StartCoroutine(HealOverTimeCoroutine(AbilityObject.AbilityLength, AbilityObject.AbilitySupportAmount, AbilityObject));
        }

        IEnumerator HealOverTimeCoroutine (int HealLength, int HealPointsPerSecond, EmeraldAIAbility AbilityObject)
        {
            float Length = 0;
            float Seconds = 0;

            while (Length < HealLength && EmeraldComponent.CurrentHealth < EmeraldComponent.StartingHealth)
            {
                Length += Time.deltaTime;
                Seconds += Time.deltaTime;

                if (Seconds >= 1)
                {
                    if (AbilityObject.UseDamageOverTimeEffectRef == EmeraldAIAbility.Yes_No.Yes)
                    {
                        GameObject DamageOverTimeEffect = EmeraldAIObjectPool.SpawnEffect(AbilityObject.DamageOverTimeEffect, transform.position, Quaternion.identity, AbilityObject.DamageOvertimeTimeout);
                        DamageOverTimeEffect.transform.SetParent(EmeraldAISystem.ObjectPool.transform);
                    }

                    if (AbilityObject.UseDamageOverTimeSoundRef == EmeraldAIAbility.Yes_No.Yes)
                    {
                        EmeraldComponent.EmeraldEventsManagerComponent.PlaySoundClip(AbilityObject.DamageOverTimeSound);
                    }

                    EmeraldComponent.CurrentHealth += HealPointsPerSecond;
                    Seconds = 0;
                }    

                yield return null;
            }

            if (EmeraldComponent.CurrentHealth > EmeraldComponent.StartingHealth)
            {
                EmeraldComponent.CurrentHealth = EmeraldComponent.StartingHealth;
            }
        }
    }
}
