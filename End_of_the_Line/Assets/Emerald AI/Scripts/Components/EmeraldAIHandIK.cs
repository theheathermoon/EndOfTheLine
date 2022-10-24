using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    public class EmeraldAIHandIK : MonoBehaviour
    {
        public HandIKProfile HandIKProfileData;
        public HandIKProfile ImportedHandIKProfileData;
        public Transform RightHandPoint;
        public float RightHandPosWeight = 0;
        public float RightHandRotWeight = 0;
        public Transform LeftHandPoint;
        public float LeftHandPosWeight = 0;
        public float LeftHandRotWeight = 0;
        public string FilePath;
        EmeraldAISystem EmeraldComponent;

        void Start()
        {
            EmeraldComponent = GetComponent<EmeraldAISystem>();

            RightHandPoint = new GameObject("RightHandPoint").transform;
            LeftHandPoint = new GameObject("LeftHandPoint").transform;

            RightHandPoint.SetParent(EmeraldComponent.AIAnimator.GetBoneTransform(HumanBodyBones.RightHand));
            LeftHandPoint.SetParent(EmeraldComponent.AIAnimator.GetBoneTransform(HumanBodyBones.RightHand));

            if (HandIKProfileData == null)
            {
                Debug.LogError("The " + gameObject.name + " is missing its Hand IK Profile so the Hand IK System for it has been disabled. " +
                    "Ensure that you have set up the Hand IK System correctly. You can press the 'See Tutorial' button on the Hand IK System for a tutorial on setting up.");
                enabled = false;
                return;
            }

            RightHandPoint.localPosition = HandIKProfileData.RightHandPosition;
            RightHandPoint.localEulerAngles = HandIKProfileData.RightHandRotation;
            LeftHandPoint.localPosition = HandIKProfileData.LeftHandPosition;
            LeftHandPoint.localEulerAngles = HandIKProfileData.LeftHandRotation;
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.DrawSphere(LeftHandPoint.position, 0.03f);
                Gizmos.DrawSphere(RightHandPoint.position, 0.03f);
            }
        }

        /// <summary>
        /// Instantly fade in weights (used for AI who aren't using equip animations).
        /// </summary>
        public void InstantlyFadeInWeights()
        {
            RightHandPosWeight = 1;
            LeftHandPosWeight = 1;
            RightHandRotWeight = 1;
            LeftHandRotWeight = 1;
        }

        public void FadeInHandWeights()
        {
            StartCoroutine(FadeInHandWeightsInternal());
        }

        IEnumerator FadeInHandWeightsInternal()
        {
            float T = 0;
            float RightStartingHandPosWeight = RightHandPosWeight;
            float LeftStartingHandPosWeight = LeftHandPosWeight;
            float RightStartingHandRotWeight = RightHandRotWeight;
            float LeftStartingHandRotWeight = LeftHandRotWeight;

            yield return new WaitForSeconds(EmeraldComponent.RangedPullOutWeaponAnimation.length * 0.25f);

            while (T < 1)
            {
                T += Time.deltaTime * 4;
                RightHandPosWeight = Mathf.Lerp(RightStartingHandPosWeight, 1, T);
                LeftHandPosWeight = Mathf.Lerp(LeftStartingHandPosWeight, 1, T);
                RightHandRotWeight = Mathf.Lerp(RightStartingHandRotWeight, 1, T);
                LeftHandRotWeight = Mathf.Lerp(LeftStartingHandRotWeight, 1, T);
                yield return null;
            }
        }

        public void FadeOutHandWeights()
        {
            StartCoroutine(FadeOutHandWeightsInternal());
        }

        IEnumerator FadeOutHandWeightsInternal()
        {
            float T = 0;
            float RightStartingHandWeight = RightHandPosWeight;
            float LeftStartingHandWeight = LeftHandPosWeight;
            float RightStartingHandRotWeight = RightHandRotWeight;
            float LeftStartingHandRotWeight = LeftHandRotWeight;

            while (T < 1)
            {
                T += Time.deltaTime * 1;
                RightHandPosWeight = Mathf.Lerp(RightStartingHandWeight, 0, T);
                LeftHandPosWeight = Mathf.Lerp(LeftStartingHandWeight, 0, T);
                RightHandRotWeight = Mathf.Lerp(RightStartingHandRotWeight, 0, T);
                LeftHandRotWeight = Mathf.Lerp(LeftStartingHandRotWeight, 0, T);
                yield return null;
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            EmeraldComponent.AIAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandPosWeight);
            EmeraldComponent.AIAnimator.SetIKPosition(AvatarIKGoal.RightHand, RightHandPoint.position);
            EmeraldComponent.AIAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, RightHandRotWeight);
            EmeraldComponent.AIAnimator.SetIKRotation(AvatarIKGoal.RightHand, RightHandPoint.rotation);

            EmeraldComponent.AIAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, LeftHandPosWeight);
            EmeraldComponent.AIAnimator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandPoint.position);
            EmeraldComponent.AIAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, LeftHandRotWeight);
            EmeraldComponent.AIAnimator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandPoint.rotation);
        }
    }
}