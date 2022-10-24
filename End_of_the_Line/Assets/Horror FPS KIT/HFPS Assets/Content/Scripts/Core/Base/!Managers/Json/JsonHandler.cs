/*
 * JsonHandler.cs - by ThunderWire Games
 * ver. 1.0
*/

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Linq;
using ThunderWire.Helpers;
using HFPS.Systems;

namespace ThunderWire.Json
{
    /// <summary>
    /// Main Json file Handler
    /// </summary>
    public class JsonHandler : MonoBehaviour
    {
        private JsonManager jsonManager;

        [Header("Main")]
        public string JsonFilename = "file.json";
        public SerializationPath FilePath = SerializationPath.GameDataPath;

        [Header("Other")]
        public bool debugMode = false;
        public bool enableSaveSpinner = true;

        public event Action<JObject> OnJsonStringChanged;

        void Awake()
        {
            jsonManager = new JsonManager(FilePath, debugMode);

            jsonManager.OnJsonChanged += delegate
            {
                JObject root = Json();
                OnJsonStringChanged?.Invoke(root);
            };
        }

        public bool FileExist()
        {
            return File.Exists(GetCurrentPath() + JsonFilename);
        }

        public void DeleteFile()
        {
            File.Delete(GetCurrentPath() + JsonFilename);
        }

        public string GetCurrentPath()
        {
            return jsonManager.GetCurrentPath();
        }

        public void ClearArray()
        {
            jsonManager.ClearArray();
        }

        public void Add(string Parent, object Values)
        {
            jsonManager.Add(Parent, Values);
        }

        public string JsonOut()
        {
            return jsonManager.JsonOut();
        }

        public string SerializedJsonOut()
        {
            return jsonManager.SerializedJsonOut();
        }

        public string JString(string path)
        {
            return jsonManager.JString(path);
        }

        public JObject Json()
        {
            return jsonManager.Json();
        }

        public JObject Json(string Json)
        {
            return jsonManager.Json(Json);
        }

        public T Json<T>()
        {
            return jsonManager.Json<T>();
        }

        public T Json<T>(string json)
        {
            return jsonManager.Json<T>(json);
        }

        public bool HasKey(string key)
        {
            return jsonManager.HasKey(key);
        }

        /// <summary>
        /// Function to Serialize Json Data Asynchronously.
        /// </summary>
        public async Task SerializeJsonDataAsync(bool isHidden = false)
        {
            if (enableSaveSpinner && GetComponent<HFPS_GameManager>())
            {
                GetComponent<HFPS_GameManager>().ShowSaveIcon();
            }

            string file = SerializationTool.GetSerializationPath(FilePath) + JsonFilename;
            await Task.Run(() => jsonManager.SerializeJsonDataAsync(file, isHidden));
        }

        /// <summary>
        /// Function to Serialize Json Data to FilePath
        /// </summary>
        public void SerializeJsonData(bool isHidden = false)
        {
            if (enableSaveSpinner && GetComponent<HFPS_GameManager>())
            {
                GetComponent<HFPS_GameManager>().ShowSaveIcon();
            }

            jsonManager.SerializeJsonData(JsonFilename, isHidden);
        }

        /// <summary>
        /// Function to Deserialize Json File
        /// </summary>
        public void DeserializeData()
        {
            jsonManager.DeserializeData(JsonFilename);
        }

        /// <summary>
        /// Function to Deserialize Json File Asynchronously.
        /// </summary>
        public async Task DeserializeDataAsync()
        {
            await jsonManager.DeserializeDataAsync(JsonFilename);
        }

        /// <summary>
        /// Function to Deserialize Json String from Stream.
        /// </summary>
        public void DeserializeData(Stream stream)
        {
            jsonManager.DeserializeData(stream);
        }
    }
}