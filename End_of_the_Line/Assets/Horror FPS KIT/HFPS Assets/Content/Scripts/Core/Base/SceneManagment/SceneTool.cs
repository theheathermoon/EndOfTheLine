using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderWire.Scene
{
    public static class SceneTool
    {
        public static ThreadPriority threadPriority = ThreadPriority.High;
        public static bool LoadingDone;
        private static AsyncOperation async;

        /// <summary>
        /// Loads the Scene asynchronously in the background.
        /// </summary>
        public static IEnumerator LoadSceneAsyncSwitch(string scene)
        {
            Time.timeScale = 1f;
            Application.backgroundLoadingPriority = threadPriority;

            async = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            async.allowSceneActivation = false;

            while (!async.isDone)
            {
                if (async.progress >= 0.9f)
                {
                    async.allowSceneActivation = true;
                    break;
                }
                yield return null;
            }

            yield return null;
        }

        /// <summary>
        /// Loads the Scene asynchronously in the background.
        /// </summary>
        public static IEnumerator LoadSceneAsyncSwitch(int scene)
        {
            Time.timeScale = 1f;
            Application.backgroundLoadingPriority = threadPriority;

            async = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            async.allowSceneActivation = false;

            while (!async.isDone)
            {
                if (async.progress >= 0.9f)
                {
                    async.allowSceneActivation = true;
                    break;
                }
                yield return null;
            }

            yield return null;
        }

        /// <summary>
        /// Loads the Scene asynchronously in the background without scene activation when loading is done.
        /// </summary>
        public static IEnumerator LoadSceneAsync(string scene, LoadSceneMode loadSceneMode)
        {
            Time.timeScale = 1f;
            Application.backgroundLoadingPriority = threadPriority;

            async = SceneManager.LoadSceneAsync(scene, loadSceneMode);
            async.allowSceneActivation = false;

            while (!async.isDone)
            {
                if (async.progress >= 0.9f)
                {
                    LoadingDone = true;
                    break;
                }
                yield return null;
            }

            yield return null;
        }

        /// <summary>
        /// Loads the Scene asynchronously in the background without scene activation when loading is done.
        /// </summary>
        public static IEnumerator LoadSceneAsync(int scene, LoadSceneMode loadSceneMode)
        {
            Time.timeScale = 1f;
            Application.backgroundLoadingPriority = threadPriority;

            async = SceneManager.LoadSceneAsync(scene, loadSceneMode);
            async.allowSceneActivation = false;

            while (!async.isDone)
            {
                if (async.progress >= 0.9f)
                {
                    LoadingDone = true;
                    break;
                }
                yield return null;
            }

            yield return null;
        }

        /// <summary>
        /// Activates loaded scene when loading is done.
        /// </summary>
        public static void AllowSceneActivation()
        {
            if (LoadingDone)
            {
                async.allowSceneActivation = true;
            }
        }

        public static string GetCurrentScene()
        {
            return SceneManager.GetActiveScene().name;
        }
    }
}
