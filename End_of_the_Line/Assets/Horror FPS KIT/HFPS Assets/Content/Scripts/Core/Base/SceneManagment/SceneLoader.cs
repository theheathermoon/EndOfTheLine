using System.Collections;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ThunderWire.Scene;
using ThunderWire.Input;
using HFPS.UI;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    public class SceneLoader : MonoBehaviour
    {
        [Serializable]
        public struct SceneInfo
        {
            public string SceneBuildName;

            public string LevelName;
            public string LevelNameKey;

            [Multiline] public string LevelDescription;
            public string LevelDescriptionKey;

            public Sprite Background;
        }

        public static SceneInfo CurrentInfo;
        public SceneInfo[] SceneInfos;

        [Header("UI")]
        public TipsManager TipsManager;
        public UIFadePanel BlackScreenFade;
        public GameObject Spinner;
        public GameObject ManualSwitchText;
        public Text LevelNameText;
        public Text LevelDescriptionText;
        public Image LevelBackground;

        [Header("Settings")]
        public bool SwitchManually;
        public bool WaitBeforeLoad = true;
        public ThreadPriority LoadPriority = ThreadPriority.High;
        public int TimeBeforeLoad;

#if TW_LOCALIZATION_PRESENT
        void Awake()
        {
            string[] keys = new string[SceneInfos.Length * 2];

            int index = 0;
            for (int i = 0; i < SceneInfos.Length; i++, index += 2)
            {
                keys[index] = SceneInfos[i].LevelNameKey;
                keys[index + 1] = SceneInfos[i].LevelDescriptionKey;
            }

            LocalizationSystem.SubscribeAndGet(OnLocalizationUpdate, keys);
        }
#endif

        void OnLocalizationUpdate(string[] values)
        {
            int index = 0;
            for (int i = 0; i < values.Length / 2; i++, index += 2)
            {
                SceneInfos[i].LevelName = values[index];
                SceneInfos[i].LevelDescription = values[index + 1];
            }
        }

        void Start()
        {
            Time.timeScale = 1f;
            SceneTool.threadPriority = LoadPriority;

            Spinner.SetActive(true);
            ManualSwitchText.SetActive(false);

            if (Prefs.Exist(Prefs.LOAD_LEVEL_NAME))
            {
                LoadLevelAsync(Prefs.Game_LevelName());
            }
            else
            {
                Spinner.SetActive(false);
                throw new NullReferenceException("Loading Error: Could not retrieve the scene from registry that should be loaded!");
            }
        }

        public void LoadLevelAsync(string scene)
        {
            if (SceneInfos.Length > 0)
            {
                if (SceneInfos.Any(x => x.SceneBuildName == scene))
                {
                    foreach (var info in SceneInfos)
                    {
                        if (info.SceneBuildName == scene)
                        {
                            LevelNameText.text = info.LevelName;
                            LevelDescriptionText.text = info.LevelDescription;
                            LevelBackground.sprite = info.Background;
                            CurrentInfo = info;
                            break;
                        }
                    }
                }
                else
                {
                    LevelNameText.text = scene;
                    LevelDescriptionText.text = $"No info for \"{scene}\" scene!";
                }
            }
            else
            {
                LevelNameText.text = scene;
                LevelDescriptionText.text = "No scene infos!";
            }

            StartCoroutine(LoadScene(scene, TimeBeforeLoad));
        }

        IEnumerator LoadScene(string scene, int timeWait)
        {
            if (WaitBeforeLoad)
                yield return new WaitForSeconds(timeWait);

            if (!SwitchManually)
            {
                StartCoroutine(SceneTool.LoadSceneAsyncSwitch(scene));
            }
            else
            {
                StartCoroutine(SceneTool.LoadSceneAsync(scene, LoadSceneMode.Single));

                yield return new WaitUntil(() => SceneTool.LoadingDone);

                Spinner.SetActive(false);
                ManualSwitchText.SetActive(true);

                if (TipsManager)
                {
                    TipsManager.TipsText.gameObject.SetActive(false);
                }

                yield return new WaitUntil(() => InputHandler.AnyInputPressed());

                RemoveDontDestroyOnLoad();

                if (!BlackScreenFade)
                {
                    SceneTool.AllowSceneActivation();
                }
                else
                {
                    BlackScreenFade.FadeIn(true);
                    yield return new WaitUntil(() => BlackScreenFade.IsFadedIn);

                    SceneTool.AllowSceneActivation();
                }
            }
        }

        void RemoveDontDestroyOnLoad()
        {
            DontDestroyLoad[] objs = FindObjectsOfType<DontDestroyLoad>();

            if (objs.Length > 0)
            {
                GameObject go = new GameObject("_Temp");

                foreach (var obj in objs)
                {
                    obj.transform.SetParent(go.transform);
                }
            }
        }
    }
}