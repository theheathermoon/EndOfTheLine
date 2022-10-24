using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    [ExecuteInEditMode]
    public class TargetPositionModifier : MonoBehaviour
    {
        public PositionSources PositionSource = PositionSources.Transform;
        public enum PositionSources
        {
            Transform,
            Collider,
        }

        public float PositionModifier = 0;
        public float GizmoRadius = 0.5f;
        public Color GizmoColor = new Color(1f, 0, 0, 0.8f);
        Collider m_Collider;
        float Offset;

        private void OnEnable()
        {
            m_Collider = GetComponent<Collider>();
            
            if (m_Collider == null)
            {
                PositionSource = PositionSources.Transform;
                Debug.Log("No Collider could be found on " + gameObject.name + ". The Transform Position Source will be used instead. Please add a collider to use this Position Source.");
            }

            if (PositionSource == PositionSources.Collider)
                Offset = PositionModifier;
        }

        private void Update()
        {
            if (PositionSource == PositionSources.Collider && Application.isPlaying)
            {
                PositionModifier = (Mathf.Abs(m_Collider.bounds.max.y - transform.position.y) + (transform.up * Offset).y) - 1.0f;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColor;
            if (PositionSource == PositionSources.Transform)
            {
                Gizmos.DrawSphere(transform.position + (transform.up * PositionModifier), GizmoRadius);
            }
            else if (PositionSource == PositionSources.Collider && !Application.isPlaying)
            {
                Gizmos.DrawSphere((new Vector3(m_Collider.bounds.center.x, m_Collider.bounds.max.y - 1.0f, m_Collider.bounds.center.z)) + ((transform.up) * PositionModifier), GizmoRadius);
            }
            else if (PositionSource == PositionSources.Collider && Application.isPlaying)
            {
                Gizmos.DrawSphere((new Vector3(m_Collider.bounds.center.x, m_Collider.bounds.max.y - 1.0f, m_Collider.bounds.center.z)) + ((transform.up) * Offset), GizmoRadius);
            }
        }
    }
}