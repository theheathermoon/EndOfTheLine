using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    public class EmeraldAILookAtController : MonoBehaviour
    {
        [HideInInspector] public bool LookAtInProgress;
        [HideInInspector] public float HeadFade;
        [HideInInspector] public bool ForceUnityIK;
        [HideInInspector] public float UnityHeadIK = 0.75f;
        [HideInInspector] public float UnityBodyIK;
        [HideInInspector] public float BodyWeight = 0;
        [HideInInspector] public bool DeathDelay;
        [HideInInspector] public List<HumanBodyBones> BoneObjects = new List<HumanBodyBones>();
        [HideInInspector] public List<Transform> BoneTransformList = new List<Transform>();

        float CurrentLookAtSpeed = 12;
        float LookAtSpeedGoal = 12;
        Vector3 CurrentLookPosition; 
        Vector3 LerpedLookPosition;
        Vector3 UnityIKLookPos;
        Vector3 CurrentTargetPosition;       
        float UnityHeadIKGoal = 0.75f;        
        float UnityBodyIKGoal;
        float CurrentDistance;
        int Interations = 5;        
        Vector3 CurrentOffsetPos;
        float LookWeight;
        float LookWeightGoal;
        float DelayTurnTimer;
        float TargetAngle;
        float ForwardLerp;
        bool FadingIKOutActive;
        Vector3 LookVelocity = Vector3.zero;
        EmeraldAISystem EmeraldComponent;
        Coroutine FadeLookWeightCoroutine;

        public void Initialize ()
        {
            EmeraldComponent = GetComponent<EmeraldAISystem>();
            BoneObjects = new List<HumanBodyBones>(EmeraldComponent.BoneObjects);

            //If the BoneObejcts count are equal to 0 and the user is not using Unity IK, they forgot to assign bones so force Unity's built-in IK.
            if (BoneObjects.Count == 0 && EmeraldComponent.IKType == EmeraldAISystem.IKTypes.EmeraldIK)
            {
                Debug.Log("'" + gameObject.name + "' has no BoneObjects assigned, but is using the Emerald IK Type. Unity IK will be used instead. Please apply at least 1 BoneObject if you want to use this feature.");
                EmeraldComponent.IKType = EmeraldAISystem.IKTypes.UnityIK;
                ForceUnityIK = true;
            }

            if (EmeraldComponent.IKType == EmeraldAISystem.IKTypes.UnityIK)
            {
                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes)
                    ForceUnityIK = true;
            }

            if (EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.No)
                return;

            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && !ForceUnityIK)
            {
                BodyWeight = 0;
                UnityBodyIK = 0;
                HeadFade = 1;            
                LerpedLookPosition = (EmeraldComponent.RangedAttackTransform.position + EmeraldComponent.RangedAttackTransform.forward * 15);
            }
            else if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee || ForceUnityIK)
            {
                BodyWeight = 0;
                HeadFade = 0;
                LerpedLookPosition = EmeraldComponent.HeadTransform.position + EmeraldComponent.HeadTransform.forward * 5;
            }

            for (int i = 0; i < BoneObjects.Count; i++)
            {
                BoneTransformList.Add(EmeraldComponent.AIAnimator.GetBoneTransform(BoneObjects[i]));
            }
        }

        /// <summary>
        /// Resets the IK settings so the AI can be respawned.
        /// </summary>
        public void ResetSettings ()
        {
            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && !ForceUnityIK)
            {
                BodyWeight = 0;
                UnityBodyIK = 0;
                HeadFade = 1;
                LerpedLookPosition = (EmeraldComponent.RangedAttackTransform.position + EmeraldComponent.RangedAttackTransform.forward * 15);
            }
            else if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee || ForceUnityIK)
            {
                BodyWeight = 0;
                HeadFade = 0;
                LerpedLookPosition = EmeraldComponent.HeadTransform.position + EmeraldComponent.HeadTransform.forward * 5;
            }
        }

        /// <summary>
        /// Gets the distance between the AI and its current target or non-combat look at reference.
        /// </summary>
        void CalculateDistances ()
        {
            if (EmeraldComponent.CurrentTarget != null && EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.Active)
                CurrentDistance = Vector3.Distance(new Vector3(EmeraldComponent.CurrentTarget.position.x, 0, EmeraldComponent.CurrentTarget.position.z), new Vector3(transform.position.x, 0, transform.position.z));
            else if (EmeraldComponent.LookAtTarget != null && EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive)
                CurrentDistance = Vector3.Distance(new Vector3(EmeraldComponent.LookAtTarget.position.x, 0, EmeraldComponent.LookAtTarget.position.z), new Vector3(transform.position.x, 0, transform.position.z));
        }

        private void OnDrawGizmos()
        {
            if (EmeraldComponent == null)
                return;

            if (EmeraldComponent.DrawRaycastsEnabled == EmeraldAISystem.YesOrNo.Yes && !EmeraldComponent.IsDead && EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.Active || 
                EmeraldComponent.DrawRaycastsEnabled == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive && EmeraldComponent.LookAtTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(LerpedLookPosition, 0.15f);
                Gizmos.color = Color.white;
            }
        }

        /// <summary>
        /// Gets the angle of the AI's current look position and the AI's current target.
        /// </summary>
        public float LookAtDirectionAngle()
        {
            Vector3 TargetDir = CurrentTargetPosition - transform.position;
            Vector3 LookDir = LerpedLookPosition - transform.position;
            return Vector3.Angle(TargetDir, LookDir);
        }

        /// <summary>
        /// Gets the angle of the AI's current look position and the AI's current target when using Unity's IK
        /// </summary>
        public float UnityIKAngle()
        {
            Vector3 relative2 = transform.InverseTransformPoint(CurrentTargetPosition);
            return Mathf.Abs(Mathf.Atan2(relative2.x, relative2.z) * Mathf.Rad2Deg);
        }

        private void OnAnimatorIK(int layerIndex)
        {     
            if (EmeraldComponent == null || EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.No)
                return;

            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive || ForceUnityIK)
            {
                //Used for delaying the head look between targets by a second after each current target dies or is switched.
                //This helps further smooth out the lerping process between targets so there's no rapid changing, which can result in unrealistic looking speeds.
                if (DeathDelay)
                {
                    DelayTurnTimer += Time.deltaTime;

                    if (DelayTurnTimer >= 1)
                    {
                        DelayTurnTimer = 0;
                        DeathDelay = false;
                    }
                }

                if (EmeraldComponent.CurrentTarget != null)
                {
                    float LookAngle = UnityIKAngle();
                    TargetAngle = EmeraldComponent.TargetAngle();

                    //Set the combat weight goals for both the head and body.
                    UnityHeadIKGoal = EmeraldComponent.HeadLookWeightCombat;
                    UnityBodyIKGoal = EmeraldComponent.BodyLookWeightCombat;

                    if (LookAngle <= EmeraldComponent.LookAtLimit && TargetAngle <= EmeraldComponent.LookAtLimit)
                    {
                        LookAtSpeedGoal = 20 * EmeraldComponent.CombatLookSpeedMultiplier;

                        CurrentOffsetPos = (EmeraldComponent.CurrentTarget.up * EmeraldComponent.CurrentPositionModifier);
                        LookWeightGoal = 1;
                        if (EmeraldComponent.TargetTypeRef != EmeraldAISystem.TargetType.AI)
                            CurrentLookPosition = EmeraldComponent.CurrentTarget.position + CurrentOffsetPos;
                        else
                            CurrentLookPosition = EmeraldComponent.TargetEmerald.HitPointTransform.position + CurrentOffsetPos;
                    }
                    else
                    {
                        LookAtSpeedGoal = 8 * EmeraldComponent.CombatLookSpeedMultiplier;

                        if (LookAngle < 120 && !DeathDelay)
                        {
                            LookWeightGoal = 0;

                            float LastHeight = CurrentLookPosition.y;
                            if (EmeraldComponent.IsTurningRight)
                                CurrentLookPosition = EmeraldComponent.CurrentTarget.position + (EmeraldComponent.HeadTransform.right * CurrentDistance);
                            else if (EmeraldComponent.IsTurningLeft)
                                CurrentLookPosition = EmeraldComponent.CurrentTarget.position + (-EmeraldComponent.HeadTransform.right * CurrentDistance);
                            else
                                CurrentLookPosition = EmeraldComponent.HeadTransform.position + (EmeraldComponent.HeadTransform.forward * 5);
                            CurrentLookPosition.y = LastHeight;
                        }
                        else
                        {
                            LookWeightGoal = 0;
                            float LastHeight = CurrentLookPosition.y;
                            CurrentLookPosition = EmeraldComponent.CurrentTarget.position + (EmeraldComponent.HeadTransform.forward * 5);
                            CurrentLookPosition.y = LastHeight;
                        }
                    }
                }
                else if (EmeraldComponent.LookAtTarget != null && !EmeraldComponent.DeathDelayActive && !FadingIKOutActive && CurrentDistance <= EmeraldComponent.MaxLookAtDistance)
                {
                    float LookAngle = UnityIKAngle();
                    TargetAngle = EmeraldComponent.HeadLookAngle();

                    if (!EmeraldComponent.IsMoving)
                        LookAtSpeedGoal = 4 * EmeraldComponent.NonCombatLookSpeedMultiplier;
                    else
                        LookAtSpeedGoal = 25 * EmeraldComponent.NonCombatLookSpeedMultiplier;

                    //Set the non-combat weight goals for both the head and body.
                    UnityHeadIKGoal = EmeraldComponent.HeadLookWeightNonCombat;
                    UnityBodyIKGoal = EmeraldComponent.BodyLookWeightNonCombat;

                    if (LookAngle <= EmeraldComponent.LookAtLimit && TargetAngle <= EmeraldComponent.LookAtLimit)
                    {
                        CurrentOffsetPos = (EmeraldComponent.LookAtTarget.up * EmeraldComponent.CurrentPositionModifier);
                        LookWeightGoal = 1;
                        if (EmeraldComponent.TargetTypeRef != EmeraldAISystem.TargetType.AI)
                            CurrentLookPosition = EmeraldComponent.LookAtTarget.position + CurrentOffsetPos;
                        else
                            CurrentLookPosition = EmeraldComponent.TargetEmerald.HitPointTransform.position + CurrentOffsetPos;
                    }
                    else
                    {
                        if (LookAngle < 120 && !DeathDelay)
                        {
                            LookWeightGoal = 0;
                            if (EmeraldComponent.IsTurningRight)
                                CurrentLookPosition = EmeraldComponent.LookAtTarget.position + (EmeraldComponent.HeadTransform.right * CurrentDistance);
                            else if (EmeraldComponent.IsTurningLeft)
                                CurrentLookPosition = EmeraldComponent.LookAtTarget.position + (-EmeraldComponent.HeadTransform.right * CurrentDistance);
                            else
                                CurrentLookPosition = EmeraldComponent.HeadTransform.position + (EmeraldComponent.HeadTransform.forward * 5);
                        }
                        else
                        {
                            LookWeightGoal = 0;
                            CurrentLookPosition = EmeraldComponent.HeadTransform.position + (EmeraldComponent.HeadTransform.forward * 5);
                        }
                    }
                }
                else if (EmeraldComponent.CurrentTarget == null && EmeraldComponent.LookAtTarget == null || CurrentDistance > EmeraldComponent.MaxLookAtDistance)
                {
                    if (LookWeight <= 0.001f)
                        LerpedLookPosition = EmeraldComponent.HeadTransform.position + (EmeraldComponent.HeadTransform.forward * 5);

                    LookWeightGoal = 0;
                }

                if (EmeraldComponent.CurrentTarget != null)
                    CurrentTargetPosition = EmeraldComponent.CurrentTarget.position;
                else if (EmeraldComponent.LookAtTarget != null)
                    CurrentTargetPosition = EmeraldComponent.LookAtTarget.position;

                //Track the distance between the look at position and the target's current position. This is used to determine when the AI is aim is near its target.
                //This is used to avoid the AI firing too early and only firing when its aim is near its target.
                float LookPointDistance = Vector3.Distance(new Vector3(LerpedLookPosition.x, 0, LerpedLookPosition.z), (new Vector3(CurrentTargetPosition.x, 0, CurrentTargetPosition.z)));

                if (LookPointDistance <= 1f)
                    LookAtInProgress = false;
                else
                    LookAtInProgress = true;

                CalculateDistances();
                CurrentLookAtSpeed = Mathf.LerpAngle(CurrentLookAtSpeed, LookAtSpeedGoal, Time.deltaTime);
                LerpedLookPosition = Vector3.MoveTowards(LerpedLookPosition, CurrentLookPosition, Time.deltaTime * CurrentLookAtSpeed);

                //Lerp the weight goal with the current weight value for both the head and body.
                UnityHeadIK = Mathf.Lerp(UnityHeadIK, UnityHeadIKGoal, Time.deltaTime);
                UnityBodyIK = Mathf.Lerp(UnityBodyIK, UnityBodyIKGoal, Time.deltaTime);
            }

            if (!EmeraldComponent.IsGettingHit)
                LookWeight = Mathf.LerpAngle(LookWeight, LookWeightGoal, Time.deltaTime * 2);

            EmeraldComponent.AIAnimator.SetLookAtWeight(LookWeight, UnityBodyIK, EmeraldComponent.HeadLookWeightCombat, 0.85f);

            //Used for Melee AI or AI that don't use aimable weapons. This IK is fully controlled by Unity's IK system. 
            if (ForceUnityIK || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive)
            {
                UnityIKLookPos = Vector3.Lerp(UnityIKLookPos, LerpedLookPosition, Time.deltaTime * 5);
                EmeraldComponent.AIAnimator.SetLookAtPosition(UnityIKLookPos);
            }
            //Used by AI who have aimable weapons. Most of this is controlled by Emerald AI's custom IK system, but some Unity IK is used to keep AI's head looking forward or at targets when needed.
            else
            {
                UnityIKLookPos = Vector3.Lerp(LerpedLookPosition, EmeraldComponent.HeadTransform.position + EmeraldComponent.HeadTransform.forward * (CurrentDistance * 3), HeadFade);
                UnityIKLookPos.y = LerpedLookPosition.y;
                EmeraldComponent.AIAnimator.SetLookAtPosition(UnityIKLookPos);
            }
        }

        IEnumerator FadeLookWeightInternal()
        {
            float T = 0;
            float StartingLookWeight = LookWeight;

            while (T < 1)
            {
                T += Time.deltaTime * 4;
                LookWeight = Mathf.LerpAngle(StartingLookWeight, 0, T);
                yield return null;
            }
        }

        /// <summary>
        /// Fades the IK when called so it doesn't interfere with certain functionality.
        /// </summary>
        public void FadeLookWeight ()
        {
            if (FadeLookWeightCoroutine != null) { StopCoroutine(FadeLookWeightCoroutine); }
            FadeLookWeightCoroutine = StartCoroutine(FadeLookWeightInternal());
        }

        /// <summary>
        /// Fade in the custom IK. This is only used by ranged AI who use weapon objects. Because the forward vector of the ranged weapon is used for accurate weapon aiming, 
        /// it must be faded out previously to avoid the AI aiming with its equip animation active.
        /// </summary>
        public void FadeInBodyIK()
        {
            StartCoroutine(FadeInBodyIKInternal());
        }

        /// <summary>
        /// Fade out the custom IK. This is only used by ranged AI who use weapon objects. Because the forward vector of the ranged weapon is used for accurate weapon aiming, 
        /// it must be faded out to avoid the AI aiming with its unequip animation active.
        /// </summary>
        public void FadeOutBodyIK()
        {
            StartCoroutine(FadeOutBodyIKInternal());
        }

        private void LateUpdate()
        {
            if (EmeraldComponent == null || EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.No || ForceUnityIK || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                return;

            //Used for delaying the head look between targets by a second after each current target dies or is switched.
            //This helps further smooth out the lerping process between targets so there's no rapid changing, which can result in unrealistic looking speeds.
            if (DeathDelay)
            {
                DelayTurnTimer += Time.deltaTime;

                if (DelayTurnTimer >= 1)
                {
                    DelayTurnTimer = 0;
                    DeathDelay = false;
                }
            }

            //Switch between aiming transform sources when needed. This is to prevent AI from using their weapon transform when moving or equipping weapons that results in an unwanted bobbing effect of the AI's head.
            if (!EmeraldComponent.IsDead)
            {
                //Switch to head transform
                if (CurrentDistance > EmeraldComponent.AttackDistance || EmeraldComponent.IsMoving || EmeraldComponent.IsSwitchingWeapons || EmeraldComponent.IsEquipping || EmeraldComponent.IsBackingUp)
                    ForwardLerp = Mathf.Lerp(ForwardLerp, 1, Time.deltaTime * 3);
                //Switch to ranged weapon transform
                else if (CurrentDistance <= EmeraldComponent.AttackDistance)
                    ForwardLerp = Mathf.Lerp(ForwardLerp, 0, Time.deltaTime * 6);
            }

            //Get the aiming position and lerp towards it. If the AI's max look angle is exceeded, aim in the last detected direction while turning towards the target.
            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged  || BodyWeight > 0)
            {
                if (EmeraldComponent.CurrentTarget != null)
                    CurrentTargetPosition = EmeraldComponent.CurrentTarget.position;

                float LookAngle = LookAtDirectionAngle();
                TargetAngle = EmeraldComponent.TargetAngle();

                CalculateDistances();
                LookWeightGoal = 1;

                if (EmeraldComponent.CurrentTarget != null)
                {
                    //If the AI's target is within range, look at its current position with the CurrentPositionModifier variable height difference.
                    if (LookAngle <= EmeraldComponent.LookAtLimit && TargetAngle <= EmeraldComponent.LookAtLimit)
                    {
                        CurrentOffsetPos = (EmeraldComponent.CurrentTarget.up * EmeraldComponent.CurrentPositionModifier);
                        if (EmeraldComponent.TargetTypeRef != EmeraldAISystem.TargetType.AI)
                            CurrentLookPosition = EmeraldComponent.CurrentTarget.position + CurrentOffsetPos;
                        else
                            CurrentLookPosition = EmeraldComponent.TargetEmerald.HitPointTransform.position + CurrentOffsetPos;
                    }
                    else
                    {
                        //If the AI is within 120 degrees of its target, use a modified look at position to blend with the AI's current turning direction.
                        if (LookAngle < EmeraldComponent.LookAtLimit && TargetAngle > EmeraldComponent.LookAtLimit && TargetAngle <= 120 && !EmeraldComponent.IsEquipping)
                        {
                            if (EmeraldComponent.IsTurningRight)
                                CurrentLookPosition = (EmeraldComponent.transform.position + (EmeraldComponent.transform.forward * 10) + EmeraldComponent.HeadTransform.right * 10);
                            else if (EmeraldComponent.IsTurningLeft)
                                CurrentLookPosition = (EmeraldComponent.transform.position + (EmeraldComponent.transform.forward * 10) + -EmeraldComponent.HeadTransform.right * 10);
                            else
                                CurrentLookPosition = (EmeraldComponent.HeadTransform.position + EmeraldComponent.HeadTransform.forward * 10);
                            CurrentLookPosition.y = EmeraldComponent.RangedAttackTransform.position.y;
                        }
                        //If the AI angle of its target exceeds 120 degrees, modify the look at position to avoid the lerping position getting too close to the AI.
                        else
                        {
                            CurrentLookPosition = (EmeraldComponent.HeadTransform.position + EmeraldComponent.HeadTransform.forward * 10);
                            CurrentLookPosition.y = EmeraldComponent.RangedAttackTransform.position.y;

                        }
                    }
                }

                LerpedLookPosition = Vector3.SmoothDamp(LerpedLookPosition, CurrentLookPosition, ref LookVelocity, 0.35f, 20);
            }

            //Track the distance between the look at position and the target's current position. This is used to determine when the AI is aim is near its target.
            //This is used to avoid the AI firing too early and only firing when its aim is near its target.
            float LookPointDistance = Vector3.Distance(LerpedLookPosition, CurrentTargetPosition);

            if (LookPointDistance <= 2.75f && !DeathDelay)
                LookAtInProgress = false;
            else
                LookAtInProgress = true;

            //Set the look at position for each of the set bones.
            if (BodyWeight > 0)
            {
                for (int i = 0; i < Interations; i++)
                {
                    for (int j = 0; j < BoneTransformList.Count; j++)
                    {
                        LookAtTarget(LerpedLookPosition, BoneTransformList[j], BodyWeight);
                    }
                }
            }

            //When enabled, draw the lines of the AI's current head look direcion and the AI's weapon aiming direction towards its target.
            if (EmeraldComponent.EnableDebugging == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.DrawRaycastsEnabled == EmeraldAISystem.YesOrNo.Yes && BodyWeight > 0)
            {
                Debug.DrawLine(EmeraldComponent.RangedAttackTransform.position, EmeraldComponent.RangedAttackTransform.position + EmeraldComponent.RangedAttackTransform.forward * CurrentDistance, new Color(0.5f, 0.5f, 0));
                Debug.DrawRay(new Vector3(EmeraldComponent.HeadTransform.position.x, EmeraldComponent.HeadTransform.position.y, EmeraldComponent.HeadTransform.position.z), EmeraldComponent.EmeraldDetectionComponent.TargetDirection, EmeraldComponent.EmeraldDetectionComponent.DebugLineColor);
            }
        }

        IEnumerator FadeInBodyIKInternal ()
        {
            if (ForceUnityIK || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee && EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No)
                yield break;

            float T = 0;
            float StartingBodyWeight = BodyWeight;
            float StartingUnityBodyIK = UnityBodyIK;
            float StartingHeadFade = HeadFade;

            while (T < 1)
            {
                if (EmeraldComponent.IsDead || FadingIKOutActive)
                    yield break;

                T += Time.deltaTime * 0.5f;
                UnityBodyIK = Mathf.LerpAngle(StartingUnityBodyIK, 0, T);
                HeadFade = Mathf.LerpAngle(StartingHeadFade, 1, T);
                BodyWeight = Mathf.LerpAngle(StartingBodyWeight, 1, T);
                yield return null;
            }
        }

        IEnumerator FadeOutBodyIKInternal()
        {
            float T = 0;
            float StartingBodyWeight = BodyWeight;
            float StartingUnityBodyIK = UnityBodyIK;
            float StartingHeadFade = HeadFade;
            float BodyLookWeightGoal = EmeraldComponent.BodyLookWeightCombat;
            FadingIKOutActive = true;

            //If the AI's target is destroyed, change the body look weight to non-combat.
            if (EmeraldComponent.CurrentTarget == null)
                BodyLookWeightGoal = EmeraldComponent.BodyLookWeightNonCombat;

            while (T < 1)
            {
                if (EmeraldComponent.IsDead)
                    yield break;

                T += Time.deltaTime * 0.5f;
                UnityBodyIK = Mathf.LerpAngle(StartingUnityBodyIK, BodyLookWeightGoal, T);
                HeadFade = Mathf.LerpAngle(StartingHeadFade, 0, T);
                BodyWeight = Mathf.LerpAngle(StartingBodyWeight, 0, T);
                yield return null;
            }

            BodyWeight = 0;
            FadingIKOutActive = false;
        }

        /// <summary>
        /// Roate the passed BoneTransforms to according to TargetPos which is a lerped Vector3 called CurrentLookAtPosition.
        /// </summary>
        void LookAtTarget(Vector3 TargetPos, Transform BoneTranform, float Weight)
        {
            //Only use the Weapon forward position if the AI is within attack range (Controlled by ForwardLerp above).
            Vector3 WeaponDir = Vector3.Lerp(EmeraldComponent.RangedAttackTransform.forward, EmeraldComponent.HeadTransform.forward, ForwardLerp); 

            //Offset the WeaponDir by -3 to allow the AI to aim past the actual target position. This helps avoid the AI's RangedAttackTransform from being past the target's position.
            Vector3 ModifiedWeaponPoint = (Vector3.Lerp(EmeraldComponent.RangedAttackTransform.position, EmeraldComponent.HeadTransform.position, ForwardLerp)) + WeaponDir * -3;

            //Get the target's position from the ModifiedWeaponPoint.
            Vector3 TargetDir = TargetPos - ModifiedWeaponPoint;

            //Get the aim direction and add another offset to avoid the WeaponDir from being too close to the actual target.
            Quaternion AimDir = Quaternion.FromToRotation(WeaponDir * 4, TargetDir);

            //Slerp based on the Weight parameter and apply the rotation to the passed bone transform.
            Quaternion BlendRot = Quaternion.Slerp(Quaternion.identity, AimDir, Weight);
            BoneTranform.rotation = BlendRot * BoneTranform.rotation;
        }

        /// <summary>
        /// When switching a target sources, set DeathDelay to true, this triggeres a second delay to allow for smoother blending between targets. 
        /// </summary>
        public void SetPreviousLookAtInfo ()
        {
            DeathDelay = true;
            DelayTurnTimer = 0;
        }
    }
}
 