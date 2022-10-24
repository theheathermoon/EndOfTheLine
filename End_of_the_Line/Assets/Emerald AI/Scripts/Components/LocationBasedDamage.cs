﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    public class LocationBasedDamage : MonoBehaviour
    {
        EmeraldAISystem EmeraldComponent;
        public bool ColliderListFoldout = true;
        [SerializeField]
        public List<LocationBasedDamageClass> ColliderList = new List<LocationBasedDamageClass>();
        [System.Serializable]
        public class LocationBasedDamageClass
        {
            public Collider ColliderObject;
            public float DamageMultiplier = 1;

            public LocationBasedDamageClass(Collider m_ColliderObject, int m_DamageMultiplier)
            {
                ColliderObject = m_ColliderObject;
                DamageMultiplier = m_DamageMultiplier;
            }

            public static bool Contains(List<LocationBasedDamageClass> m_LocationBasedDamageList, LocationBasedDamageClass m_LocationBasedDamageClass)
            {
                foreach (LocationBasedDamageClass lbdc in m_LocationBasedDamageList)
                {
                    if (lbdc.ColliderObject == m_LocationBasedDamageClass.ColliderObject)
                    { return true; }
                }
                return false;
            }
        }

        public void InitializeLocationBasedDamage()
        {
            EmeraldComponent = GetComponent<EmeraldAISystem>();
            EmeraldComponent.DeathTypeRef = EmeraldAISystem.DeathType.Ragdoll;
            EmeraldComponent.LocationBasedDamageComp = this;
            EmeraldComponent.AIBoxCollider.size = new Vector3(0.015f, 0.015f, 0.015f);
            EmeraldComponent.AIBoxCollider.center = Vector3.zero;
            EmeraldComponent.AIBoxCollider.center = Vector3.up * EmeraldComponent.HitPointTransform.localPosition.y;
            EmeraldComponent.AIBoxCollider.isTrigger = true;

            for (int i = 0; i < ColliderList.Count; i++)
            {
                if (ColliderList[i].ColliderObject.GetComponent<Rigidbody>() != null)
                {
                    Rigidbody ColliderRigidbody = ColliderList[i].ColliderObject.GetComponent<Rigidbody>();
                    ColliderRigidbody.useGravity = true;
                    ColliderRigidbody.isKinematic = true;

                    LocationBasedDamageArea DamageComponent = ColliderList[i].ColliderObject.gameObject.AddComponent<LocationBasedDamageArea>();
                    DamageComponent.EmeraldComponent = EmeraldComponent;
                    DamageComponent.DamageMultiplier = ColliderList[i].DamageMultiplier;
                }
            }
        }
    }
}