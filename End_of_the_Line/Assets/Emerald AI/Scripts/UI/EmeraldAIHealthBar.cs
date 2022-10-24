using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EmeraldAI.Utility
{
    public class EmeraldAIHealthBar : MonoBehaviour
    {
        public static string CameraTag = "MainCamera";
        Image HealthBar;
        [HideInInspector]
        public EmeraldAISystem EmeraldComponent;
        public static Camera m_Camera;
        float ObjectScaleDifference;
        [HideInInspector]
        public Canvas canvas;
        [HideInInspector]
        public float MaxScalingSize = 2f;
        CanvasGroup CG;
        Text AINameUI;
        Text AILevelUI;
        Coroutine C;

        void Start()
        {
            if (m_Camera == null)
            {
                m_Camera = GameObject.FindGameObjectWithTag(CameraTag).GetComponent<Camera>();
            }

            InvokeRepeating("UpdateUI", 0f, 0.1f);
        }

        public void UpdateUI()
        {
            if (HealthBar != null
                || HealthBar != null && EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion
                || HealthBar != null && EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Pet)
            {
                HealthBar.fillAmount = ((float)EmeraldComponent.CurrentHealth / (float)EmeraldComponent.StartingHealth);
            }
            else
            {
                CG = GetComponent<CanvasGroup>();
                HealthBar = transform.Find("AI Health Bar Background/AI Health Bar").GetComponent<Image>();
                AINameUI = transform.Find("AI Name Text").GetComponent<Text>();
                AILevelUI = transform.Find("AI Level Text").GetComponent<Text>();
            }
        }

        void Update()
        {
            CalculateUI();
        }

        public void CalculateUI()
        {
            if (m_Camera != null)
            {
                if (HealthBar != null
                    || HealthBar != null && EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion
                    || HealthBar != null && EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Pet
                    || HealthBar != null && EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Passive && EmeraldComponent.PassiveTargetRef != null)
                {
                    canvas.transform.parent.LookAt(canvas.transform.parent.position + m_Camera.transform.rotation * Vector3.forward,
                    m_Camera.transform.rotation * Vector3.up);

                    float dist = Vector3.Distance(m_Camera.transform.position, transform.position);
                    if (dist < EmeraldComponent.MaxUIScaleSize)
                    {
                        canvas.transform.localScale = new Vector3(dist * 0.085f, dist * 0.085f, dist * 0.085f);
                    }
                    else
                    {
                        canvas.transform.localScale = new Vector3(EmeraldComponent.MaxUIScaleSize * 0.085f, EmeraldComponent.MaxUIScaleSize * 0.085f, EmeraldComponent.MaxUIScaleSize * 0.085f);
                    }
                }
            }
        }

        public void FadeOut()
        {
            if (gameObject.activeSelf)
            {
                if (C != null) { StopCoroutine(C); }
                C = StartCoroutine(FadeTo(0.0f, 1.5f));
            }
        }

        void OnDisable()
        {
            if (CG != null)
            {
                Color newColor1 = new Color(1, 1, 1, 1);
                CG.alpha = newColor1.a;
                AINameUI.color = new Color(AINameUI.color.r, AINameUI.color.g, AINameUI.color.b, newColor1.a);
                AILevelUI.color = new Color(AILevelUI.color.r, AILevelUI.color.g, AILevelUI.color.b, newColor1.a);
            }
        }

        IEnumerator FadeTo (float DesiredValue, float TransitionTime)
        {
            HealthBar.fillAmount = ((float)EmeraldComponent.CurrentHealth / (float)EmeraldComponent.StartingHealth);
            float alpha = CG.alpha;
            float t = 0;

            while ((t / TransitionTime) < 1)
            {
                t += Time.deltaTime;
                Color newColor1 = new Color(1, 1, 1, Mathf.Lerp(alpha, DesiredValue, t));
                CG.alpha = newColor1.a;
                AINameUI.color = new Color(AINameUI.color.r, AINameUI.color.g, AINameUI.color.b, newColor1.a);
                AILevelUI.color = new Color(AILevelUI.color.r, AILevelUI.color.g, AILevelUI.color.b, newColor1.a);
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}