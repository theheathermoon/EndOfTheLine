using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
using ThunderWire.Input;
using TWTools = ThunderWire.Utility.Utilities;
using HFPS.Systems;
using HFPS.Player;
using ThunderWire.Helpers;
using ThunderWire.Utility;

namespace HFPS.Editors
{
    public class HFPSMenu : EditorWindow
    {
        private bool encrypt;
        private SerializationPath filePath;
        private string key;

        public const string MMANAGER = "_MAINMENU";
        public const string GMANAGER = "_GAMEUI";
        public const string HERO = "HEROPLAYER";
        public const string PLAYER = "FPSPLAYER";

        public const string PATH_MMANAGER = "Setup/MainMenu/" + MMANAGER;
        public const string PATH_GMANAGER = "Setup/Game/" + GMANAGER;
        public const string PATH_HERO = "Setup/Game/" + HERO;
        public const string PATH_PLAYER = "Setup/Game/" + PLAYER;

        public static string ScriptablesPath
        {
            get
            {
                string hfpsPath = FindAssetPath("HFPS Assets");
                if (hfpsPath == "Assets")
                    hfpsPath = Path.Combine(hfpsPath, "Horror FPS KIT", "HFPS Assets");

                string scriptablesPath = Path.Combine(hfpsPath, "Scriptables").Replace("\\", "/"); ;

                if (!Directory.Exists(scriptablesPath))
                {
                    Directory.CreateDirectory(scriptablesPath);
                    AssetDatabase.Refresh();
                }

                return scriptablesPath;
            }
        }

        public static string GameScriptablesPath
        {
            get
            {
                string hfpsPath = FindAssetPath("HFPS Assets");
                if (hfpsPath == "Assets")
                    hfpsPath = Path.Combine(hfpsPath, "Horror FPS KIT", "HFPS Assets");

                string scriptablesPath = Path.Combine(hfpsPath, "Scriptables", "Game Scriptables").Replace("\\", "/");

                if (!Directory.Exists(scriptablesPath))
                {
                    Directory.CreateDirectory(scriptablesPath);
                    AssetDatabase.Refresh();
                }

                return scriptablesPath;
            }
        }

        public static string FindAssetPath(string searchPattern)
        {
            string[] result = AssetDatabase.FindAssets(searchPattern);

            if (result.Length > 0)
                return AssetDatabase.GUIDToAssetPath(result[0]);

            return "Assets";
        }

        public static string FindHFPSAssetsPath()
        {
            string path = FindAssetPath("HFPS Assets");

            if (path == "Assets")
                path = Path.Combine(path, "Horror FPS KIT", "HFPS Assets");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }

            return path;
        }

        [MenuItem("Tools/HFPS KIT/Setup/Game/FirstPerson")]
        static void SetupFPS()
        {
            GameObject GameManager = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(PATH_GMANAGER)) as GameObject;
            GameObject Player = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(PATH_PLAYER)) as GameObject;

            Player.transform.position = new Vector3(0, 0, 0);
            GameManager.GetComponent<HFPS_GameManager>().m_PlayerObj = Player;
            GameManager.GetComponent<SaveGameHandler>().constantSaveables = new List<SaveableDataPair>();
            Player.GetComponentInChildren<ScriptManager>().m_GameManager = GameManager.GetComponent<HFPS_GameManager>();
        }

        [MenuItem("Tools/HFPS KIT/Setup/Game/FirstPerson", true)]
        static bool CheckSetupFPS()
        {
            if (GameObject.Find(MMANAGER))
            {
                return false;
            }

            if (GameObject.Find(GMANAGER))
            {
                return false;
            }

            if (GameObject.Find(PLAYER))
            {
                return false;
            }

            return true;
        }

        [MenuItem("Tools/HFPS KIT/Setup/Game/FirstPerson Body")]
        static void SetupFPSB()
        {
            GameObject GameManager = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(PATH_GMANAGER)) as GameObject;
            GameObject Player = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(PATH_HERO)) as GameObject;

            Player.transform.position = new Vector3(0, 0, 0);
            GameManager.GetComponent<HFPS_GameManager>().m_PlayerObj = Player;
            GameManager.GetComponent<SaveGameHandler>().constantSaveables = new List<SaveableDataPair>();
            Player.GetComponentInChildren<ScriptManager>().m_GameManager = GameManager.GetComponent<HFPS_GameManager>();
        }

        [MenuItem("Tools/HFPS KIT/Setup/Game/FirstPerson Body", true)]
        static bool CheckSetupFPSB()
        {
            if (GameObject.Find(MMANAGER))
            {
                return false;
            }

            if (GameObject.Find(GMANAGER))
            {
                return false;
            }

            if (GameObject.Find(HERO))
            {
                return false;
            }

            return true;
        }

        [MenuItem("Tools/HFPS KIT/Setup/Refresh GameManager")]
        static void SetupRefreshGM()
        {
            GameObject gameManager = GameObject.Find(GMANAGER);

            if (gameManager != null)
            {
                List<SaveableDataPair> _tempSaveables = new List<SaveableDataPair>();
                if (gameManager.TryGetComponent(out SaveGameHandler sgh))
                    _tempSaveables = sgh.constantSaveables;

                ObjectivesScriptable _tempObjAsset = null;
                if (gameManager.TryGetComponent(out ObjectiveManager objManager))
                    _tempObjAsset = objManager.SceneObjectives;

                List<CutsceneManager.Cutscene> _tempCutscenes = new List<CutsceneManager.Cutscene>();
                if (gameManager.TryGetComponent(out CutsceneManager cutscene))
                    _tempCutscenes = cutscene.Cutscenes;

                List<GameObject> _tempFloatingIcons = new List<GameObject>();
                if (gameManager.TryGetComponent(out FloatingIconManager floatingIcon))
                    _tempFloatingIcons = floatingIcon.FloatingIcons;

                DestroyImmediate(gameManager);

                GameObject newGameManager = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(PATH_GMANAGER)) as GameObject;
                GameObject player = PlayerController.Instance.gameObject;

                newGameManager.GetComponent<HFPS_GameManager>().m_PlayerObj = player;
                player.GetComponentInChildren<ScriptManager>().m_GameManager = newGameManager.GetComponent<HFPS_GameManager>();

                if (newGameManager.TryGetComponent(out SaveGameHandler gmSgh))
                    gmSgh.constantSaveables = _tempSaveables;

                if (newGameManager.TryGetComponent(out ObjectiveManager gmObjManager))
                    gmObjManager.SceneObjectives = _tempObjAsset;

                if (newGameManager.TryGetComponent(out CutsceneManager gmCutscene))
                    gmCutscene.Cutscenes = _tempCutscenes;

                if (newGameManager.TryGetComponent(out FloatingIconManager gmFloatingIcon))
                    gmFloatingIcon.FloatingIcons = _tempFloatingIcons;

                Debug.Log("<color=green>GameManager has been refreshed!</color>");
            }
        }

        [MenuItem("Tools/HFPS KIT/Setup/Refresh GameManager", true)]
        static bool CheckSetupRefreshGM()
        {
            if (GameObject.Find(GMANAGER))
            {
                return true;
            }

            return false;
        }

        [MenuItem("Tools/HFPS KIT/Setup/MainMenu")]
        static void SetupMainMenu()
        {
            if (DestroyAll())
            {
                PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(PATH_MMANAGER));
            }
        }

        [MenuItem("Tools/HFPS KIT/Setup/MainMenu", true)]
        static bool CheckSetupMainMenu()
        {
            if (GameObject.Find(MMANAGER))
            {
                return false;
            }

            if (GameObject.Find(GMANAGER))
            {
                return false;
            }

            if (PlayerController.HasReference)
            {
                return false;
            }

            return true;
        }

        [MenuItem("Tools/HFPS KIT/Setup/Fix/FirstPerson")]
        static void FixFirstPerson()
        {
            GameObject GameManager;
            GameObject Player;

            if (HFPS_GameManager.HasReference)
            {
                GameManager = HFPS_GameManager.Instance.gameObject;
            }
            else
            {
                GameManager = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(PATH_GMANAGER)) as GameObject;
            }

            if (PlayerController.HasReference)
            {
                Player = PlayerController.Instance.gameObject;
            }
            else
            {
                Player = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(PATH_PLAYER)) as GameObject;
            }

            GameManager.GetComponent<HFPS_GameManager>().m_PlayerObj = Player;
            Player.GetComponentInChildren<ScriptManager>().m_GameManager = GameManager.GetComponent<HFPS_GameManager>();

            EditorUtility.SetDirty(GameManager.GetComponent<HFPS_GameManager>());
            EditorUtility.SetDirty(Player.GetComponentInChildren<ScriptManager>());

            Debug.Log("<color=green>Everything should be OK!</color>");
        }

        [MenuItem("Tools/HFPS KIT/Setup/Fix/FirstPerson Body")]
        static void FixFirstPersonBody()
        {
            GameObject GameManager;
            GameObject Player;

            if (HFPS_GameManager.HasReference)
            {
                GameManager = HFPS_GameManager.Instance.gameObject;
            }
            else
            {
                GameManager = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(PATH_GMANAGER)) as GameObject;
            }

            if (PlayerController.HasReference)
            {
                Player = PlayerController.Instance.gameObject;
            }
            else
            {
                Player = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(PATH_HERO)) as GameObject;
            }

            GameManager.GetComponent<HFPS_GameManager>().m_PlayerObj = Player;
            Player.GetComponentInChildren<ScriptManager>().m_GameManager = GameManager.GetComponent<HFPS_GameManager>();

            EditorUtility.SetDirty(GameManager.GetComponent<HFPS_GameManager>());
            EditorUtility.SetDirty(Player.GetComponentInChildren<ScriptManager>());

            Debug.Log("<color=green>Everything should be OK!</color>");
        }

        static bool DestroyAll()
        {
            if (FindObjectsOfType<GameObject>().Length > 0)
            {
                foreach (GameObject o in FindObjectsOfType<GameObject>().Select(obj => obj.transform.root.gameObject).ToArray())
                {
                    DestroyImmediate(o);
                }

                if (FindObjectsOfType<GameObject>().Length < 1)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            return true;
        }

        [MenuItem("Tools/HFPS KIT/" + "Scriptables" + "/New Inventory Database")]
        static void CreateInventoryDatabase()
        {
            CreateAssetFile<InventoryScriptable>("InventoryDatabase");
        }

        [MenuItem("Tools/HFPS KIT/" + "Scriptables" + "/New Scene Objectives")]
        static void CreateObjectiveDatabase()
        {
            CreateAssetFile<ObjectivesScriptable>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + " Objectives", "Objectives 2");
        }

        [MenuItem("Tools/HFPS KIT/" + "Scriptables" + "/New Cross-Platform Sprites")]
        static void CreateCrossPlatformSprites()
        {
            CreateAssetFile<CrossPlatformSprites>("CrossPlatformSprites");
        }

        [MenuItem("Tools/HFPS KIT/" + "Scriptables" + "/New Surface Details")]
        static void CreateSurfaceDetails()
        {
            CreateAssetFile<SurfaceDetailsScriptable>("Surface Details");
        }

        [MenuItem("Tools/HFPS KIT/" + "Scriptables" + "/New TextTable")]
        static void CreateTextTable()
        {
            CreateAssetFile<TextTableScriptable>("TextTable");
        }

        [MenuItem("Tools/HFPS KIT/" + "Scriptables" + "/New ObjectReferences")]
        static void CreateObjectReferencese()
        {
            CreateAssetFile<ObjectReferences>("ObjectReferences");
        }

        [MenuItem("Tools/HFPS KIT/" + "Scriptables" + "/New Serialization Settings")]
        static void ShowWindow()
        {
            EditorWindow window = GetWindow<HFPSMenu>(false, "Create Serialization Settings", true);
            window.minSize = new Vector2(500, 130);
            window.maxSize = new Vector2(500, 130);
            window.Show();
        }

        [MenuItem("Tools/HFPS KIT/Delete Saved Games")]
        static void DeleteSavedGames()
        {
            string serialization = GetSerializationPath();
            string path = Path.Combine(serialization, "SavedGame");

            if (Directory.Exists(path))
            {
                DirectoryInfo dinfo = new DirectoryInfo(path);
                FileInfo[] finfo = dinfo.GetFiles("*.sav");

                if (finfo.Length > 0)
                {
                    for (int i = 0; i < finfo.Length; i++)
                    {
                        File.Delete(finfo[i].FullName);
                    }

                    if (finfo.Length > 1)
                    {
                        EditorUtility.DisplayDialog("SavedGames Deleted", $"{finfo.Length} Saved Games was deleted successfully.", "Okay");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("SavedGames Deleted", "Saved Game was deleted successfully.", "Okay");
                    }

                    AssetDatabase.Refresh();
                }
                else
                {
                    EditorUtility.DisplayDialog("Directory empty", "Folder is empty.", "Okay");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Directory not found", "Failed to find Directory:  " + path, "Okay");
            }
        }

        [MenuItem("Tools/HFPS KIT/Add FloatingIcons")]
        static void AddFloatingIcon()
        {
            if (Selection.gameObjects.Length > 0)
            {
                FloatingIconManager uIFloatingItem = FindObjectOfType<FloatingIconManager>();
                int count = 0;

                if (uIFloatingItem)
                {
                    foreach (var obj in Selection.gameObjects)
                    {
                        if (obj.HasComponent(out InteractiveItem interactiveItem))
                        {
                            interactiveItem.floatingIcon = true;
                        }

                        if (!uIFloatingItem.FloatingIcons.Contains(obj))
                        {
                            uIFloatingItem.FloatingIcons.Add(obj);
                            count++;
                        }
                    }

                    EditorUtility.SetDirty(uIFloatingItem);
                    Debug.Log("<color=green>" + count + " objects are added to the Floating Icon Manager.</color>");
                }
                else
                {
                    Debug.Log("<color=red>Could not find the Floating Icon Manager script!</color>");
                }
            }
            else
            {
                Debug.Log("<color=red>Please select one or more items which will be marked as Floating Icon</color>");
            }
        }

        void OnGUI()
        {
            encrypt = EditorGUILayout.Toggle("Enable Encryption:", encrypt);
            filePath = (SerializationPath)EditorGUILayout.EnumPopup("Serialization Path:", SerializationPath.GameDataPath);
            key = EditorGUILayout.TextField("Encryption Key:", key);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create", GUILayout.Width(100), GUILayout.Height(30)))
            {
                SerializationSettings asset = CreateInstance<SerializationSettings>();

                asset.EncryptData = encrypt;
                asset.SerializePath = filePath;
                asset.EncryptionKey = MD5Hash(key);

                AssetDatabase.CreateAsset(asset, GameScriptablesPath + "SerializationSettings" + ".asset");
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();

                Selection.activeObject = asset;
            }
        }

        public static T CreateAssetFile<T>(string AssetName, string Folder = "") where T : ScriptableObject
        {
            var asset = CreateInstance<T>();
            string folderpath = Path.Combine(GameScriptablesPath, Folder);

            if (!Directory.Exists(folderpath))
            {
                Directory.CreateDirectory(folderpath);
                AssetDatabase.Refresh();
            }

            ProjectWindowUtil.CreateAsset(asset, Path.Combine(folderpath,  "New " + AssetName + ".asset"));
            return asset;
        }

        public static string MD5Hash(string Data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(Data));

            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }

            return stringBuilder.ToString();
        }

        private static string GetSerializationPath()
        {
            string gameScriptables = GameScriptablesPath;

            if (Directory.Exists(gameScriptables))
            {
                if (Directory.GetFiles(gameScriptables).Length > 0)
                {
                    string path = Path.Combine(gameScriptables, "SerializationSettings.asset");
                    SerializationSettings settings = AssetDatabase.LoadAssetAtPath<SerializationSettings>(path);

                    if (settings != null)
                        return SerializationTool.GetSerializationPath(settings.SerializePath);

                    return SerializationTool.GetSerializationPath(SerializationPath.GameDataPath);
                }

                return SerializationTool.GetSerializationPath(SerializationPath.GameDataPath);
            }
            else
            {
                return SerializationTool.GetSerializationPath(SerializationPath.GameDataPath);
            }
        }
    }

    public static class ScriptableFinder
    {
        public static T GetScriptable<T>(string AssetName) where T : ScriptableObject
        {
            string path = HFPSMenu.GameScriptablesPath;

            if (Directory.Exists(path))
            {
                if (Directory.GetFiles(path).Length > 0)
                {
                    return AssetDatabase.LoadAssetAtPath<T>(path + AssetName + ".asset");
                }
                return null;
            }
            else
            {
                return null;
            }
        }
    }
}