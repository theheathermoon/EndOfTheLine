using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using EmeraldAI;

namespace EmeraldAI.CharacterController
{
    public class TopDownCharacterController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public LayerMask MovementMask = 2;
        public Camera PlayerCamera;
        public float StoppingDistance = 1;
        public float RotationSpeed = 12;
        public GameObject DestinationEffect;

        NavMeshAgent m_NavMeshAgent;
        Animator m_AnimatorController;
        float m_AttackTimer;
        float m_UpdateTargetPosition;
        bool m_ClickEffectUsed;
        Vector3 m_LastMousePosition;
        float m_UpdatePositionTimer;
        float m_MouseUpClickEffectTimer;

        //Initialize the player controller
        void Start()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_AnimatorController = GetComponent<Animator>();
            m_NavMeshAgent.updateRotation = false;
        }

        void RotatePlayer()
        {
            if ((m_NavMeshAgent.destination - transform.position).magnitude < 0.1f) return;

            Vector3 direction = (m_NavMeshAgent.steeringTarget - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion qDir = Quaternion.LookRotation(direction);
                qDir.x = 0;
                qDir.z = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, qDir, Time.deltaTime * RotationSpeed);
            }
        }

        void Update()
        {
            RotatePlayer();
            m_NavMeshAgent.speed = Mathf.Lerp(0, 4.8f, Vector3.Distance(m_LastMousePosition, transform.position) / 1.35f);
            m_AnimatorController.SetFloat("Speed", m_NavMeshAgent.velocity.magnitude / m_NavMeshAgent.speed - (0.4f), 0.01f, Time.deltaTime);

            if (Input.GetMouseButton(0))
            {
                m_UpdatePositionTimer += Time.deltaTime;
                m_MouseUpClickEffectTimer += Time.deltaTime;

                if (m_UpdatePositionTimer >= 0.01f)
                {
                    Ray m_Ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit m_hit;
                    if (Physics.Raycast(m_Ray, out m_hit, 1000.0f, MovementMask))
                    {
                        if (!m_ClickEffectUsed && DestinationEffect != null)
                        {
                            EmeraldAI.Utility.EmeraldAIObjectPool.SpawnEffect(DestinationEffect, m_hit.point + Vector3.up * 0.5f, Quaternion.identity, 2);
                            m_ClickEffectUsed = true;
                        }

                        if (Vector3.Distance(m_hit.point, transform.position) > 1f)
                        {
                            m_NavMeshAgent.isStopped = false;
                            m_NavMeshAgent.stoppingDistance = 0.5f;
                            m_LastMousePosition = m_hit.point;
                            m_NavMeshAgent.destination = m_hit.point;
                        }
                    }

                    m_UpdatePositionTimer = 0;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (DestinationEffect != null && m_MouseUpClickEffectTimer > 1)
                {
                    EmeraldAI.Utility.EmeraldAIObjectPool.SpawnEffect(DestinationEffect, m_LastMousePosition + Vector3.up * 0.5f, Quaternion.identity, 1);
                }

                m_ClickEffectUsed = false;
                m_MouseUpClickEffectTimer = 0;
            }
        }
    }
}