/* 
 * SaveGameHandler.cs - by ThunderWire Games
 * Version 3.1
 */

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using ThunderWire.Json;
using ThunderWire.Helpers;
using ThunderWire.Utility;
using HFPS.Player;
using HFPS.UI;

namespace HFPS.Systems
{
    [Serializable]
    public sealed class SaveableDataPair
    {
        public enum DataBlockType { ISaveable, Attribute }

        public DataBlockType BlockType;
        public string BlockKey;
        public MonoBehaviour Instance;
        public string[] FieldData;

        public SaveableDataPair(DataBlockType type, string key, MonoBehaviour instance, string[] fileds)
        {
            BlockType = type;
            BlockKey = key;
            Instance = instance;
            FieldData = fileds;
        }
    }

    /// <summary>
    /// Main script for Save/Load System
    /// </summary>
    public class SaveGameHandler : Singleton<SaveGameHandler>
    {
        [Serializable]
        public class RuntimeSaveable
        {
            public string PrefabPath;
            public GameObject Obj;
            public SaveableDataPair[] Data;

            public RuntimeSaveable(string path, GameObject obj, SaveableDataPair[] data)
            {
                PrefabPath = path;
                Obj = obj;
                Data = data;
            }
        }

        private string SerializationPath
        {
            get
            {
                string folderPath = SerializationHelper.Settings.GetSerializationPath();
                return Path.Combine(folderPath, "SavedGame");
            }
        }

        public const string RUNTIME_SAVEABLE_PREFIX = "RS";

        public enum SaveableType { Constant, Runtime, None };

        [Tooltip("Object References asset where are stored references to the instantiable objects.")]
        public ObjectReferences objectReferences;

        [Tooltip("Serialize player data between scenes.")]
        public bool crossSceneSaving;
        [Tooltip("Load/Save data, although some instances are missing.")]
        public bool forceSaveLoad;

        [Tooltip("You can leave this field blank if you don't want to fade screen when you start or switch scenes.")]
        public UIFadePanel fadeControl;

        private static JObject _dataBetweenScenes;
        public List<SaveableDataPair> constantSaveables = new List<SaveableDataPair>();
        public List<RuntimeSaveable> runtimeSaveables = new List<RuntimeSaveable>();

        private JsonManager jsonManager;
        private ItemSwitcher switcher;
        private Inventory inventory;
        private ObjectiveManager objectives;
        private GameObject player;

        [HideInInspector]
        public string lastSave;

        /// <summary>
        /// If the game is saved, an OnGameSaved event will be called.
        /// </summary>
        public static event Action OnGameSaved;

        /// <summary>
        /// If the game is loaded, an OnGameLoaded event will be called.
        /// </summary>
        public static event Action OnGameLoaded;

        /// <summary>
        /// Checks if the game will load
        /// </summary>
        public static bool GameBeingLoaded
        {
            get
            {
                if (Prefs.Exist(Prefs.LOAD_STATE))
                {
                    int loadstate = Prefs.Game_LoadState();
                    return Instance.CheckLoadState(loadstate);
                }

                return false;
            }
        }

        private bool CheckLoadState(int loadState)
        {
            bool flag = false;

            if (loadState > 0 && Prefs.Exist(Prefs.LOAD_SAVE_NAME))
            {
                if (loadState == 1)
                {
                    string filename = Prefs.Game_SaveName();
                    bool flag1 = File.Exists(Path.Combine(SerializationPath, filename));
                    bool flag2 = SceneManager.GetActiveScene().name == Prefs.Game_LevelName();
                    return flag1 && flag2;
                }
                else if (loadState == 2)
                {
                    flag = crossSceneSaving;
                }
            }

            return flag;
        }

        async void Awake()
        {
            jsonManager = new JsonManager(SerializationHelper.Settings, SerializationPath, true);
            inventory = GetComponent<Inventory>();
            objectives = GetComponent<ObjectiveManager>();
            player = PlayerController.Instance.gameObject;
            switcher = ScriptManager.Instance.C<ItemSwitcher>();

            if (constantSaveables.Any(pair => pair.Instance == null))
            {
                if (!forceSaveLoad)
                {
                    Debug.LogError("[SaveGameHandler] Some of Saveable Instances are missing or destroyed. To fix this message, click the Find Saveables button again. Loading is prevented!");
                    return;
                }
                else
                {
                    Debug.LogError("[SaveGameHandler] Some of Saveable Instances are missing or destroyed. To fix this message, click the Find Saveables button again.");
                }
            }

            if (Prefs.Exist(Prefs.LOAD_STATE))
            {
                int loadstate = Prefs.Game_LoadState();

                if(loadstate == 0)
                {
                    _dataBetweenScenes = null;
                }
                else if(loadstate == 1)
                {
                    string filename = Prefs.Game_SaveName();
                    if (CheckLoadState(loadstate))
                    {
                        await Task.Run(() => jsonManager.DeserializeDataAsync(filename));
                        lastSave = filename;
                        LoadSerializedData(null);
                    }
                    else
                    {
                        Debug.Log("<color=yellow>[SaveGameHandler]</color> Could not load file: " + filename);
                        Prefs.Game_LoadState(0);
                    }
                }
                else if(loadstate == 2 && CheckLoadState(loadstate))
                {
                    jsonManager.ClearArray();
                    Prefs.Game_LoadState(0);
                    LoadSerializedData(_dataBetweenScenes);
                    _dataBetweenScenes = null;
                }
            }
        }

        /// <summary>
        /// Instantiate Runtime Saveable.
        /// </summary>
        public GameObject InstantiateSaveableReference(ObjectReference reference, Vector3 position, Vector3 rotation, string name = "")
        {
            GameObject go = Instantiate(reference.Object, position, Quaternion.Euler(rotation));

            if (!string.IsNullOrEmpty(name)) go.name = name;
            else go.name = RUNTIME_SAVEABLE_PREFIX + "_" + reference.Object.name;

            AddSaveableObject(go, reference.GUID);
            return go;
        }

        /// <summary>
        /// Instantiate Runtime Saveable.
        /// </summary>
        public GameObject InstantiateSaveable(GameObject gameObject, Vector3 position, Vector3 rotation, string name = "")
        {
            var result = objectReferences.Instantiate(gameObject, position, Quaternion.Euler(rotation));

            if (!string.IsNullOrEmpty(name)) result.Instantiation.name = name;
            else result.Instantiation.name = RUNTIME_SAVEABLE_PREFIX + "_" + gameObject.name;

            AddSaveableObject(result.Instantiation, result.Reference.GUID);
            return result.Instantiation;
        }

        /// <summary>
        /// Add Saveable Object at Runtime.
        /// </summary>
        public void AddSaveableObject(GameObject obj, string prefab_path)
        {
            var saveables = Utilities.GetSaveables(obj);

            if (saveables.Length > 0)
            {
                runtimeSaveables.Add(new RuntimeSaveable(prefab_path, obj, saveables));
            }
        }

        /// <summary>
        /// Get object Saveable Type.
        /// </summary>
        public SaveableType GetSaveableType(GameObject obj)
        {
            if (constantSaveables.Count > 0 && constantSaveables.Any(x => x.Instance.gameObject == obj))
            {
                return SaveableType.Constant;
            }
            else if (runtimeSaveables.Count > 0 && runtimeSaveables.Any(x => x.Obj == obj))
            {
                return SaveableType.Runtime;
            }

            return SaveableType.None;
        }

        /// <summary>
        /// Remove Runtime Saveable Object.
        /// </summary>
        public void RemoveSaveableObject(GameObject obj, bool destroy = false, bool forceDestroy = true)
        {
            if (runtimeSaveables.Any(x => x.Obj == obj))
            {
                runtimeSaveables.RemoveAll(x => x.Obj == obj);

                if (destroy)
                {
                    Destroy(obj);
                }
            }
            else if (forceDestroy)
            {
                Destroy(obj);
            }
        }

        /// <summary>
        /// Save Scene Data
        /// </summary>
        public void SaveSerializedData(bool allData)
        {
            if (!forceSaveLoad)
            {
                if (constantSaveables.Any(pair => pair.Instance == null) || runtimeSaveables.Any(pair => pair.Data.Any(x => x.Instance == null)))
                {
                    return;
                }
            }

            jsonManager.ClearArray();
            Dictionary<string, object> playerData = new Dictionary<string, object>();
            Dictionary<string, object> slotData = new Dictionary<string, object>();
            Dictionary<string, object> shortcutData = new Dictionary<string, object>();

            /* PLAYER PAIRS */
            if (allData)
            {
                string lvlName = SceneLoader.CurrentInfo.LevelName ?? SceneManager.GetActiveScene().name;
                string lvlNameKey = SceneLoader.CurrentInfo.LevelNameKey;

                jsonManager.Add("scene", SceneManager.GetActiveScene().name);
                jsonManager.Add("levelName", lvlName);
                jsonManager.Add("levelNameKey", lvlNameKey);
                jsonManager.Add("dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                playerData.Add("playerPosition", player.transform.position);
                playerData.Add("cameraRotation", player.GetComponentInChildren<MouseLook>().GetRotation());
                playerData.Add("characterState", player.GetComponent<PlayerController>().characterState);
            }

            playerData.Add("playerHealth", player.GetComponent<HealthManager>().Health);
            /* END PLAYER PAIRS */

            /* ITEMSWITCHER PAIRS */
            Dictionary<string, object> switcherData = new Dictionary<string, object>
            {
                { "switcherActiveItem", switcher.currentItem },
                { "switcherWeaponItem", switcher.weaponItem }
            };

            foreach (var Item in switcher.ItemList)
            {
                Dictionary<string, object> ItemInstances = new Dictionary<string, object>();

                foreach (var Instance in Item.GetComponents<MonoBehaviour>().Where(x => typeof(ISaveableArmsItem).IsAssignableFrom(x.GetType())).ToArray())
                {
                    ItemInstances.Add(Instance.GetType().Name.Replace(" ", "_"), (Instance as ISaveableArmsItem).OnSave());
                    switcherData.Add("switcher_item_" + Item.name.ToLower().Replace(" ", "_"), ItemInstances);
                }
            }
            /* END ITEMSWITCHER PAIRS */

            /* INVENTORY PAIRS */
            foreach (var slot in inventory.Slots)
            {
                if (slot.GetComponent<InventorySlot>().itemData != null)
                {
                    InventoryItemData itemData = slot.GetComponent<InventorySlot>().itemData;
                    Dictionary<string, object> itemDataArray = new Dictionary<string, object>
                    {
                        { "slotID", itemData.slotID },
                        { "itemID", itemData.item.ID },
                        { "itemAmount", itemData.itemAmount },
                        { "itemData", itemData.data }
                    };

                    slotData.Add("inv_slot_" + inventory.Slots.IndexOf(slot), itemDataArray);
                }
                else
                {
                    slotData.Add("inv_slot_" + inventory.Slots.IndexOf(slot), "null");
                }
            }

            Dictionary<string, object> inventoryData = new Dictionary<string, object>
            {
                { "inv_slots_count", inventory.Slots.Count },
                { "slotsData", slotData }
            };

            /* INVENTORY SHORTCUTS PAIRS */
            if (inventory.Shortcuts.Count > 0)
            {
                Dictionary<string, object> shortcutsData = new Dictionary<string, object>();

                foreach (var shortcut in inventory.Shortcuts)
                {
                    Dictionary<string, object> shortcutsDataPairs = new Dictionary<string, object>
                    {
                        { "itemID", shortcut.item.ID },
                        { "shortcutKey", shortcut.shortcut }
                    };

                    shortcutsData.Add("shortcut_" + shortcut.slot, shortcutsDataPairs);
                }

                inventoryData.Add("shortcutsData", shortcutsData);
            }

            /* INVENTORY FIXED CONTAINER PAIRS */
            if (inventory.FixedContainerData.Count > 0)
            {
                inventoryData.Add("fixedContainerData", inventory.GetFixedContainerData());
            }
            /* END INVENTORY PAIRS */

            /* OBJECTIVE PAIRS */
            Dictionary<string, object> activeObjectives = new Dictionary<string, object>();

            if (objectives.activeObjectives.Count > 0)
            {
                foreach (var obj in objectives.activeObjectives)
                {
                    Dictionary<string, object> objectiveData = new Dictionary<string, object>
                    {
                        { "completion", obj.completion },
                        { "isCompleted", obj.isCompleted }
                    };

                    activeObjectives.Add(obj.identifier.ToString(), objectiveData);
                }
            }

            Dictionary<string, object> touchedObjectives = new Dictionary<string, object>();

            if (objectives.objectives.Count > 0)
            {
                foreach (var obj in objectives.objectives)
                {
                    if (obj.isTouched)
                    {
                        Dictionary<string, object> objectiveData = new Dictionary<string, object>
                        {
                            { "completion", obj.completion },
                            { "isCompleted", obj.isCompleted }
                        };

                        touchedObjectives.Add(obj.identifier.ToString(), objectiveData);
                    }
                }
            }

            Dictionary<string, object> objectivesData = new Dictionary<string, object>()
            {
                {"activeObjectives", activeObjectives},
                {"touchedObjectives", touchedObjectives}
            };
            /* END OBJECTIVE PAIRS */

            //Add data pairs to serialization buffer
            jsonManager.Add("playerData", playerData);
            jsonManager.Add("itemSwitcherData", switcherData);
            jsonManager.Add("inventoryData", inventoryData);
            jsonManager.Add("objectivesData", objectivesData);

            //Add all saveables to buffer
            if (allData)
            {
                //Add predefined saveables
                if (constantSaveables.Count > 0)
                {
                    var saveables = InitializeSaveables(constantSaveables.ToArray(), false);
                    jsonManager.Add("saveablesData", saveables);
                }

                //Add newly added saveables from runtime
                if (runtimeSaveables.Count > 0)
                {
                    var saveables = InitializeRuntimeSaveables(runtimeSaveables.ToArray());
                    jsonManager.Add("runtime_saveablesData", saveables);
                }
            }

            //Serialize all pairs from buffer
            SerializeSaveData(!allData);
        }

        Dictionary<string, object> InitializeSaveables(SaveableDataPair[] Saveables, bool runtime)
        {
            Dictionary<string, object> Pairs = new Dictionary<string, object>();

            foreach (var Pair in Saveables)
            {
                if (Pair.Instance == null) continue;

                Dictionary<string, object> ParentPairs = new Dictionary<string, object>();

                if (Pair.BlockType == SaveableDataPair.DataBlockType.ISaveable)
                {
                    var data = (Pair.Instance as ISaveable).OnSave();

                    if (data != null)
                    {
                        if (!runtime)
                        {
                            Pairs.Add(Pair.BlockKey, data);
                        }
                        else
                        {
                            ParentPairs.Add("block_type", SaveableDataPair.DataBlockType.ISaveable);
                            ParentPairs.Add("block_data", data);
                            Pairs.Add(Pair.BlockKey, ParentPairs);
                        }
                    }
                }
                else if (Pair.BlockType == SaveableDataPair.DataBlockType.Attribute)
                {
                    Dictionary<string, object> attributeFieldPairs = new Dictionary<string, object>();

                    if (Pair.FieldData.Length > 0)
                    {
                        foreach (var Field in Pair.FieldData)
                        {
                            FieldInfo fieldInfo = Pair.Instance.GetType().GetField(Field);

                            if (fieldInfo.FieldType == typeof(Color) || fieldInfo.FieldType == typeof(KeyCode))
                            {
                                if (fieldInfo.FieldType == typeof(Color))
                                {
                                    attributeFieldPairs.Add(GetAttributeKey(fieldInfo), string.Format("#{0}", ColorUtility.ToHtmlStringRGBA((Color)Pair.Instance.GetType().InvokeMember(Field, BindingFlags.GetField, null, Pair.Instance, null))));
                                }
                                else
                                {
                                    attributeFieldPairs.Add(GetAttributeKey(fieldInfo), Pair.Instance.GetType().InvokeMember(Field, BindingFlags.GetField, null, Pair.Instance, null).ToString());
                                }
                            }
                            else
                            {
                                attributeFieldPairs.Add(GetAttributeKey(fieldInfo), Pair.Instance.GetType().InvokeMember(Field, BindingFlags.GetField, null, Pair.Instance, null));
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Empty Fields Data: " + Pair.BlockKey);
                    }

                    if (!runtime)
                    {
                        Pairs.Add(Pair.BlockKey, attributeFieldPairs);
                    }
                    else
                    {
                        ParentPairs.Add("block_type", SaveableDataPair.DataBlockType.Attribute);
                        ParentPairs.Add("block_data", attributeFieldPairs);
                        Pairs.Add(Pair.BlockKey, ParentPairs);
                    }
                }
            }

            return Pairs;
        }

        Dictionary<string, object> InitializeRuntimeSaveables(RuntimeSaveable[] Saveables)
        {
            Dictionary<string, object> Pairs = new Dictionary<string, object>();

            for (int i = 0; i < Saveables.Length; i++)
            {
                if (Saveables[i].Data.Any(x => x.Instance == null)) continue;

                if (!string.IsNullOrEmpty(Saveables[i].PrefabPath))
                {
                    string key = string.Format("{0}_{1}", Saveables[i].Obj.name, Guid.NewGuid().ToString("N"));
                    var saveableData = InitializeSaveables(Saveables[i].Data, true);
                    var value = new 
                    {
                        prefabPath = Saveables[i].PrefabPath,
                        runtimeData = saveableData
                    };

                    Pairs.Add(key, value);
                }
            }

            if (Pairs.Count > 0)
            {
                return Pairs;
            }

            return null;
        }

        /// <summary>
        /// Load Scene Data
        /// </summary>
        void LoadSerializedData(JObject json = null)
        {
            bool loadWhole = json == null;

            if (json == null)
                json = jsonManager.Json();

            if (loadWhole)
            {
                var posToken = json["playerData"]["playerPosition"];
                Vector3 newPosition = posToken.ToObject<Vector3>();
                player.transform.SetPositionAndRotation(newPosition, Quaternion.identity);

                var rotToken = json["playerData"]["cameraRotation"];
                player.GetComponentInChildren<MouseLook>().SetRotation(rotToken.ToObject<Vector2>());

                var stateToken = json["playerData"]["characterState"];
                player.GetComponent<PlayerController>().SetPlayerState(stateToken.ToObject<PlayerController.CharacterState>());
            }

            var healthToken = json["playerData"]["playerHealth"];
            player.GetComponent<HealthManager>().Health = (float)healthToken;

            switcher.weaponItem = (int)json["itemSwitcherData"]["switcherWeaponItem"];

            //Deserialize ItemSwitcher Item Data
            foreach (var Item in switcher.ItemList)
            {
                JToken ItemToken = json["itemSwitcherData"]["switcher_item_" + Item.name.ToLower().Replace(" ", "_")];

                foreach (var Instance in Item.GetComponents<MonoBehaviour>().Where(x => typeof(ISaveableArmsItem).IsAssignableFrom(x.GetType())).ToArray())
                {
                    (Instance as ISaveableArmsItem).OnLoad(ItemToken[Instance.GetType().Name.Replace(" ", "_")]);
                }
            }

            //Deserialize ItemSwitcher ActiveItem
            int switchID = (int)json["itemSwitcherData"]["switcherActiveItem"];
            if (switchID != -1)
            {
                switcher.ActivateItem(switchID);
            }

            //Deserialize Inventory Data
            StartCoroutine(DeserializeInventory(json["inventoryData"]));

            //Deserialize Objectives
            if (json.ContainsKey("objectivesData"))
            {
                if (json["objectivesData"]["activeObjectives"].HasValues)
                {
                    Dictionary<int, JToken> activeObjectives = jsonManager.Json<Dictionary<int, JToken>>(json["objectivesData"]["activeObjectives"].ToString());

                    foreach (var obj in activeObjectives)
                    {
                        int id = obj.Key;
                        int completion = int.Parse(obj.Value["completion"].ToString());
                        bool completed = bool.Parse(obj.Value["isCompleted"].ToString());

                        objectives.AddObjectiveModel(new ObjectiveModel() 
                        {
                            identifier = id,
                            completion = completion,
                            isCompleted = completed
                        });
                    }
                }

                if (json["objectivesData"]["touchedObjectives"].HasValues)
                {
                    StartCoroutine(UpdateTouchedObjectives(json["objectivesData"]["touchedObjectives"]));
                }

                objectives.RefreshLocalization();
            }

            //Deserialize saveables 
            if (loadWhole)
            {
                foreach (var pair in constantSaveables)
                {
                    JToken token = json["saveablesData"][pair.BlockKey];

                    if (token == null || pair.Instance == null) continue;

                    if (pair.BlockType == SaveableDataPair.DataBlockType.ISaveable)
                    {
                        (pair.Instance as ISaveable).OnLoad(token);
                    }
                    else if (pair.BlockType == SaveableDataPair.DataBlockType.Attribute)
                    {
                        foreach (var Field in pair.FieldData)
                        {
                            SetValue(pair.Instance, pair.Instance.GetType().GetField(Field), json["saveablesData"][pair.BlockKey][GetAttributeKey(pair.Instance.GetType().GetField(Field))]);
                        }
                    }
                }

                if (json.ContainsKey("runtime_saveablesData"))
                {
                    Dictionary<string, JToken> runtimeTokens = jsonManager.Json<Dictionary<string, JToken>>(jsonManager.JString("runtime_saveablesData"));

                    foreach (var token in runtimeTokens)
                    {
                        string prefabPath = (string)token.Value["prefabPath"];
                        var tokenBlocks = token.Value["runtimeData"].ToObject<Dictionary<string, JToken>>();
                        if (tokenBlocks.Count < 1) continue;

                        var reference = objectReferences.GetObjectReference(prefabPath);
                        if (reference != null)
                        {
                            GameObject obj = Instantiate(reference.Object, Vector3.zero, Quaternion.identity);
                            AddSaveableObject(obj, reference.GUID);

                            foreach (var block in tokenBlocks)
                            {
                                SaveableDataPair.DataBlockType runtimeType = (SaveableDataPair.DataBlockType)(int)block.Value["block_type"];
                                JToken blockData = block.Value["block_data"];
                                string script_name = block.Key.Split('_')[0];

                                if (blockData.HasValues)
                                {
                                    MonoBehaviour Instance = null;

                                    if (runtimeType == SaveableDataPair.DataBlockType.ISaveable)
                                    {
                                        Instance = obj.GetComponents<MonoBehaviour>().Where(x => typeof(ISaveable).IsAssignableFrom(x.GetType()) && x.GetType().Name.Equals(script_name)).SingleOrDefault();
                                        (Instance as ISaveable).OnLoad(blockData);
                                    }
                                    else if (runtimeType == SaveableDataPair.DataBlockType.Attribute)
                                    {
                                        var fieldData = blockData.ToObject<Dictionary<string, JToken>>();

                                        var script = from Script in obj.GetComponents<MonoBehaviour>()
                                                     let attr = Script.GetType().GetFields().Where(field => field.GetCustomAttributes(typeof(SaveableField), false).Count() > 0 && !field.IsLiteral && field.IsPublic).Select(fls => fls.Name).ToArray()
                                                     where attr.Count() > 0 && Script.GetType().Name.Equals(script_name)
                                                     select Script;

                                        if (script.Count() > 0)
                                        {
                                            Instance = script.SingleOrDefault();

                                            foreach (var Field in fieldData)
                                            {
                                                SetValue(Instance, Instance.GetType().GetField(Field.Key), fieldData[GetAttributeKey(Instance.GetType().GetField(Field.Key))]);
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError(script_name + " was not found!");
                                        }
                                    }
                                }
                            }

                            if (obj.HasComponent(out InteractiveItem item))
                            {
                                item.disableType = InteractiveItem.DisableType.Destroy;
                            }
                        }
                        else
                        {
                            Debug.LogError("[Runtime Saveable Load] Cannot instantiate null object.");
                        }
                    }
                }
            }

            StartCoroutine(GameLoaded());
        }

        private IEnumerator GameLoaded()
        {
            yield return new WaitForFixedUpdate();
            OnGameLoaded?.Invoke();
        }

        /* LOAD SECTION INVENTORY */
        private IEnumerator DeserializeInventory(JToken token)
        {
            yield return new WaitUntil(() => inventory.Slots.Count > 0);

            int slotsCount = (int)token["inv_slots_count"];
            int neededSlots = slotsCount - inventory.Slots.Count;

            if (neededSlots != 0)
            {
                inventory.ExpandSlots(neededSlots);
            }

            for (int i = 0; i < inventory.Slots.Count; i++)
            {
                JToken slotToken = token["slotsData"]["inv_slot_" + i];
                string slotString = slotToken.ToString();

                if (slotString != "null")
                {
                    int slot = (int)slotToken["slotID"];
                    int id = (int)slotToken["itemID"];
                    int amnout = (int)slotToken["itemAmount"];
                    ItemData data = slotToken["itemData"].ToObject<ItemData>();
                    inventory.AddItemToSlot(slot, id, amnout, data);
                }
            }

            //Deserialize Shortcuts
            if (token["shortcutsData"] != null && token["shortcutsData"].HasValues)
            {
                Dictionary<string, Dictionary<string, string>> shortcutsData = token["shortcutsData"].ToObject<Dictionary<string, Dictionary<string, string>>>();

                foreach (var shortcut in shortcutsData)
                {
                    int slot = int.Parse(shortcut.Key.Split('_')[1]);
                    inventory.ShortcutBind(int.Parse(shortcut.Value["itemID"]), slot, shortcut.Value["shortcutKey"].ToString());
                }
            }

            //Deserialize FixedContainer
            if (token["fixedContainerData"] != null && token["fixedContainerData"].HasValues)
            {
                var fixedContainerData = token["fixedContainerData"].ToObject<Dictionary<int, JToken>>();

                foreach (var item in fixedContainerData)
                {
                    inventory.FixedContainerData.Add(new ContainerItemData(inventory.GetItem(item.Key), (int)item.Value["item_amount"], item.Value["item_custom"].ToObject<ItemData>()));
                }
            }
        }

        IEnumerator UpdateTouchedObjectives(JToken token)
        {
            Dictionary<int, Dictionary<string, string>> touchedObjectives = jsonManager.Json<Dictionary<int, Dictionary<string, string>>>(token.ToString());

            yield return new WaitUntil(() => objectives.objectives.Count > 0);

            foreach (var touch in touchedObjectives)
            {
                foreach (var obj in objectives.objectives)
                {
                    if (touch.Key == obj.identifier)
                    {
                        obj.completion = int.Parse(touch.Value["completion"]);
                        obj.isCompleted = bool.Parse(touch.Value["isCompleted"]);
                        obj.isTouched = true;
                    }
                }
            }
        }

        string GetAttributeKey(FieldInfo Field)
        {
            SaveableField saveableAttr = Field.GetCustomAttributes(typeof(SaveableField), false).Cast<SaveableField>().FirstOrDefault();

            if (string.IsNullOrEmpty(saveableAttr.CustomKey))
            {
                return Field.Name.Replace(" ", string.Empty);
            }
            else
            {
                return saveableAttr.CustomKey;
            }
        }

        void SetValue(object instance, FieldInfo fInfo, JToken token)
        {
            Type type = fInfo.FieldType;
            string value = token.ToString();
            if (type == typeof(string)) fInfo.SetValue(instance, value);
            if (type == typeof(int)) fInfo.SetValue(instance, int.Parse(value));
            if (type == typeof(uint)) fInfo.SetValue(instance, uint.Parse(value));
            if (type == typeof(long)) fInfo.SetValue(instance, long.Parse(value));
            if (type == typeof(ulong)) fInfo.SetValue(instance, ulong.Parse(value));
            if (type == typeof(float)) fInfo.SetValue(instance, float.Parse(value));
            if (type == typeof(double)) fInfo.SetValue(instance, double.Parse(value));
            if (type == typeof(bool)) fInfo.SetValue(instance, bool.Parse(value));
            if (type == typeof(char)) fInfo.SetValue(instance, char.Parse(value));
            if (type == typeof(short)) fInfo.SetValue(instance, short.Parse(value));
            if (type == typeof(byte)) fInfo.SetValue(instance, byte.Parse(value));
            if (type == typeof(Vector2)) fInfo.SetValue(instance, token.ToObject(type));
            if (type == typeof(Vector3)) fInfo.SetValue(instance, token.ToObject(type));
            if (type == typeof(Vector4)) fInfo.SetValue(instance, token.ToObject(type));
            if (type == typeof(Quaternion)) fInfo.SetValue(instance, token.ToObject(type));
            if (type == typeof(KeyCode)) fInfo.SetValue(instance, Parser.Convert<KeyCode>(value));
            if (type == typeof(Color)) fInfo.SetValue(instance, Parser.Convert<Color>(value));
        }

        /// <summary>
        /// Save Data which will be transferred to the next scene.
        /// </summary>
        public void SaveNextSceneData(string scene)
        {
            jsonManager.ClearArray();
            SaveSerializedData(false);
        }

        async void SerializeSaveData(bool betweenScenes)
        {
            GetComponent<HFPS_GameManager>().ShowSaveIcon();
            string path = jsonManager.GetCurrentPath();

            if (!betweenScenes)
            {
                if (Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    int count = di.GetFiles("*.sav").Length;
                    lastSave = "Save" + count + ".sav";

                    using (FileStream file = new FileStream(Path.Combine(path, lastSave), FileMode.OpenOrCreate))
                    {
                        await Task.Run(() => jsonManager.SerializeJsonDataAsync(file));
                    }
                }
                else
                {
                    Directory.CreateDirectory(jsonManager.GetCurrentPath());
                    lastSave = "Save0.sav";

                    using (FileStream file = new FileStream(Path.Combine(path, "Save0.sav"), FileMode.OpenOrCreate))
                    {
                        await Task.Run(() => jsonManager.SerializeJsonDataAsync(file));
                    }
                }

                Prefs.Game_SaveName(lastSave);
                _dataBetweenScenes = null;
            }
            else
            {
                _dataBetweenScenes = jsonManager.JsonFromDict();
            }

            OnGameSaved?.Invoke();
        }
    }
}