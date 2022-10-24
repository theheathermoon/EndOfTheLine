using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI;

namespace EmeraldAI.Example
{
    /// <summary>
    /// A simple example of canceling an AI's active combat state when it loses sight of its target after the ObstructedSeconds amount of seconds
    /// (AI using this script must be using the Line of Sight Detection Type).
    /// </summary>
    public class AdvancedLineOfSight : MonoBehaviour
    {
        [Tooltip("The amount of seconds that the target needs to be obstructed for before the AI cancels its combat state and resumes wandering.")]
        public float ObstructedSeconds = 5;
        EmeraldAISystem EmeraldComponent;
        float m_ObstructedTimer;

        void Start()
        {
            EmeraldComponent = GetComponent<EmeraldAISystem>();
        }

        void Update()
        {
            if (EmeraldComponent.CurrentTarget != null)
            {
                if (EmeraldComponent.TargetObstructed)
                {
                    m_ObstructedTimer += Time.deltaTime;

                    if (m_ObstructedTimer >= ObstructedSeconds)
                    {
                        CancelCurrentTarget();
                    }
                }
                else
                {
                    m_ObstructedTimer = 0;
                }
            }
        }

        /// <summary>
        /// Called when the AI has lost its current target for the ObstructedSeconds amount of seconds.
        /// </summary>
        void CancelCurrentTarget()
        {
            EmeraldComponent.EmeraldEventsManagerComponent.ClearTarget();
            EmeraldComponent.EmeraldEventsManagerComponent.ReturnToDefaultState();
            m_ObstructedTimer = 0;
        }
    }
}