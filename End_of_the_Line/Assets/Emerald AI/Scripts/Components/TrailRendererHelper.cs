using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// Clears all points from a Trailer Renderer to stop the previous position from being visible when respawning projectiles that use a TrailRenderer.
    /// </summary>
    public class TrailRendererHelper : MonoBehaviour
    {
        TrailRenderer m_TrailRenderer;

        void Awake()
        {
            m_TrailRenderer = GetComponent<TrailRenderer>();
        }

        void OnDisable()
        {
            m_TrailRenderer.Clear();
        }
    }
}