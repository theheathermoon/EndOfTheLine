using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace HFPS.Systems
{
    /// <summary>
    /// Manages HFPS Scripts and Variables
    /// </summary>
    public class ScriptManager : Singleton<ScriptManager>
    {
        [Header("Main Scripts")]
        public HFPS_GameManager m_GameManager;

        [Header("Cameras")]
        public Camera MainCamera;
        public Camera ArmsCamera;

        [Header("Post-Processing")]
        public PostProcessVolume MainPostProcess;
        public PostProcessVolume ArmsPostProcess;

        [Header("Other")]
        public AudioSource AmbienceSource;
        public AudioSource SoundEffects;

        [HideInInspector] public bool ScriptEnabledGlobal;
        [HideInInspector] public bool ScriptGlobalState;

        [HideInInspector] public bool IsExamineRaycast;
        [HideInInspector] public bool IsGrabRaycast;

        void Start()
        {
            ScriptEnabledGlobal = true;
            ScriptGlobalState = true;
        }

        public T C<T>() where T : MonoBehaviour
        {
            if (GetComponent<T>() != null)
                return GetComponent<T>();

            if (GetComponentInChildren<T>() != null)
                return GetComponentInChildren<T>();

            return null;
        }

        public bool TryGetC<T>(out T script) where T : MonoBehaviour
        {
            if ((script = GetComponent<T>()) != null)
                return true;

            if ((script = GetComponentInChildren<T>()) != null)
                return true;

            return false;
        }
    }
}