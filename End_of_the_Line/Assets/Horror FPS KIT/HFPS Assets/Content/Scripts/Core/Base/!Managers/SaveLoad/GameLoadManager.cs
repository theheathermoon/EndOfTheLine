/*
 * GameLoadManager.cs - by ThunderWire Studio
 * Version 1.0
*/

using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ThunderWire.Helpers;

namespace HFPS.Systems
{
    /// <summary>
    /// Provides additional methods for Save/Load System.
    /// </summary>
    public class GameLoadManager : MonoBehaviour
    {
        private SaveGameHandler saveHandler;
        private HFPS_GameManager gameManager;

        [Header("Save Game Panel")]
        public string sceneLoaderName = "SceneLoader";
        public GameObject SavedGamePrefab;
        public Transform SavedGameContent;
        public Text EmptyText;
        public bool SelectFirstSave;

        [Header("Main Menu")]
        public bool isMainMenu;
        public bool setStartNewGame;
        public string NewGameBuildName = "Scene";

        public Button SaveDeleteButton;
        public Button ContinueButton;
        public Button NewGameButton;

        protected List<SavedGame> m_SavesCache = new List<SavedGame>();
        private SavedGame selectedSave;
        private string lastSave;
        private bool isSaveGame;

        private string SerializationPath
        {
            get
            {
                string folderPath = SerializationHelper.Settings.GetSerializationPath();
                return Path.Combine(folderPath, "SavedGame");
            }
        }

        void Awake()
        {
            if (HFPS_GameManager.HasReference)
            {
                gameManager = HFPS_GameManager.Instance;
                sceneLoaderName = gameManager.sceneLoaderName;
            }

            if (SaveGameHandler.HasReference)
            {
                saveHandler = SaveGameHandler.Instance;
            }
        }

        void Start()
        {
            LoadSaves();
        }

        void Update()
        {
            if (SaveDeleteButton) SaveDeleteButton.interactable = selectedSave != null;
        }

        public async void LoadSaves()
        {
            List<SavedData> rawSaves = await SaveGameExtension.RetrieveSavedGames();

            if (rawSaves != null && rawSaves.Count > 0)
            {
                EmptyText.gameObject.SetActive(false);

                foreach (var save in rawSaves)
                {
                    SavedGame sgCache = m_SavesCache.SingleOrDefault(x => x.save == save.SaveName);

                    if (sgCache == null)
                    {
                        GameObject obj = Instantiate(SavedGamePrefab);
                        SavedGame sg = obj.GetComponentInChildren<SavedGame>();
                        obj.transform.SetParent(SavedGameContent);
                        obj.transform.localScale = new Vector3(1, 1, 1);
                        obj.transform.SetSiblingIndex(0);

                        sg.SetSavedGame(save.SaveName, save.Scene, save.LevelName, save.SaveTime);
                        obj.GetComponentInChildren<Button>().onClick.AddListener(delegate { OnSelect(sg); });

                        m_SavesCache.Add(sg);
                    }
                    else
                    {
                        sgCache.SetSavedGame(save.SaveName, save.Scene, save.LevelName, save.SaveTime);
                    }
                }
            }
            else
            {
                EmptyText.gameObject.SetActive(true);
            }

            if (isMainMenu)
            {
                InitializeContinue();
            }

            if (SelectFirstSave)
            {
                if (m_SavesCache.Count > 0)
                {
                    SavedGame save = m_SavesCache[m_SavesCache.Count - 1];
                    save.GetComponentInChildren<Button>().Select();
                    OnSelect(save);
                }
            }
        }

        public void OnSelect(SavedGame save)
        {
            if (save != null) selectedSave = m_SavesCache.SingleOrDefault(x => x == save);
        }

        public void LoadSelectedSave()
        {
            if (selectedSave != null)
            {
                if (gameManager && !gameManager.isPaused)
                {
                    gameManager.LockPlayerControls(false, false, false);
                }

                if (saveHandler && saveHandler.fadeControl)
                {
                    saveHandler.fadeControl.FadeIn(false);
                }

                StartCoroutine(LoadScene());
            }
        }

        IEnumerator LoadScene()
        {
            if (saveHandler && saveHandler.fadeControl)
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitUntil(() => saveHandler.fadeControl.IsFadedIn);
            }

            Prefs.Game_LoadState(1);
            Prefs.Game_SaveName(selectedSave.save);
            Prefs.Game_LevelName(selectedSave.scene);

            SceneManager.LoadScene(sceneLoaderName);
        }

        public void DeleteSelectedSave()
        {
            if (selectedSave != null)
            {
                string pathToFile = Path.Combine(SerializationPath, selectedSave.save);

                File.Delete(pathToFile);
                m_SavesCache.Remove(selectedSave);
                Destroy(selectedSave.gameObject);

                if (SelectFirstSave)
                {
                    if (m_SavesCache.Count > 0)
                    {
                        SavedGame save = m_SavesCache[m_SavesCache.Count - 1];
                        save.GetComponentInChildren<Button>().Select();
                        OnSelect(save);
                    }
                }
            }
        }

        void InitializeContinue()
        {
            if (Prefs.Exist(Prefs.LOAD_SAVE_NAME))
            {
                lastSave = Prefs.Game_SaveName();

                if (m_SavesCache.Count > 0 && m_SavesCache.Any(x => x.GetComponentInChildren<SavedGame>().save.Equals(lastSave)))
                {
                    ContinueButton.interactable = true;
                    isSaveGame = true;
                }
                else
                {
                    string pathToFile = Path.Combine(SerializationPath, lastSave);

                    if (File.Exists(pathToFile))
                    {
                        ContinueButton.interactable = true;
                    }
                    else
                    {
                        ContinueButton.interactable = false;
                    }

                    isSaveGame = false;
                }
            }

            if (isMainMenu && setStartNewGame)
            {
                if (!isSaveGame)
                {
                    NewGameButton.onClick.RemoveAllListeners();
                    NewGameButton.onClick = new Button.ButtonClickedEvent();
                    NewGameButton.onClick.AddListener(() => NewGame());
                }
            }
        }

        public void Continue()
        {
            if (isSaveGame)
            {
                SavedGame continueSave = m_SavesCache.Where(x => x.GetComponentInChildren<SavedGame>().save.Equals(lastSave)).Select(x => x.GetComponentInChildren<SavedGame>()).FirstOrDefault();

                Prefs.Game_LoadState(1);
                Prefs.Game_SaveName(continueSave.save);
                Prefs.Game_LevelName(continueSave.scene);
            }
            else
            {
                Prefs.Game_LoadState(2);
                Prefs.Game_SaveName(lastSave);
            }

            SceneManager.LoadScene(sceneLoaderName);
        }

        public void NewGame()
        {
            if (!string.IsNullOrEmpty(NewGameBuildName))
            {
                Prefs.Game_LoadState(0);
                Prefs.Game_SaveName(string.Empty);
                Prefs.Game_LevelName(NewGameBuildName);

                SceneManager.LoadScene(sceneLoaderName);
            }
            else
            {
                Debug.LogError("[GameLoadManager] New Game scene is empty!");
            }
        }

        public void NewGameScene(string sceneBuildName)
        {
            if (!string.IsNullOrEmpty(sceneBuildName))
            {
                Prefs.Game_LoadState(0);
                Prefs.Game_SaveName(string.Empty);
                Prefs.Game_LevelName(sceneBuildName);

                SceneManager.LoadScene(sceneLoaderName);
            }
            else
            {
                Debug.LogError("[GameLoadManager] New Game scene is empty!");
            }
        }

        public void Quit()
        {
            Application.Quit();
        }
    }

    public struct SavedData
    {
        public string SaveName;
        public string LevelName;
        public string Scene;
        public string SaveTime;
    }
}