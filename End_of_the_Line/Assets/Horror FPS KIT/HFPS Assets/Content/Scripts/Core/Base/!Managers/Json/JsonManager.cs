using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThunderWire.Helpers;

namespace ThunderWire.Json
{
    /// <summary>
    /// Provides methods for writing and reading JSON files.
    /// </summary>
    public class JsonManager
    {
        protected Dictionary<string, object> JObjectArray = new Dictionary<string, object>();

        private string folderPath;
        private string fullPath;

        private readonly bool enableDebug;
        private readonly bool enableEncryption;
        private readonly string cipherKey;
        private string jsonString;

        public event Action OnJsonChanged;

        public JsonManager(SerializationSettings settings, bool debug = false)
        {
            cipherKey = settings.EncryptionKey;
            enableEncryption = settings.EncryptData;
            folderPath = settings.GetSerializationPath();
            enableDebug = debug;
        }

        public JsonManager(SerializationSettings settings, string serializationPath, bool debug = false)
        {
            cipherKey = settings.EncryptionKey;
            enableEncryption = settings.EncryptData;
            folderPath = serializationPath;
            enableDebug = debug;
        }

        public JsonManager(SerializationPath path, bool debug = false)
        {
            folderPath = SerializationTool.GetSerializationPath(path);
            enableDebug = debug;
        }

        public bool IsDeserialized()
        {
            return !string.IsNullOrEmpty(jsonString);
        }

        public string GetCurrentPath()
        {
            return folderPath;
        }

        public void ClearArray()
        {
            JObjectArray.Clear();
            jsonString = string.Empty;
        }

        public void Add(string Parent, object Values)
        {
            JObjectArray.Add(Parent, Values);
        }

        public string JsonOut()
        {
            return jsonString;
        }

        public string SerializedJsonOut()
        {
            string jstring = JsonConvert.SerializeObject(JObjectArray, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return jstring;
        }

        public string JString(object obj)
        {
            string jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return jsonString;
        }

        public string JString(string path)
        {
            JObject rss = JObject.Parse(jsonString);
            return rss.SelectToken(path).ToString();
        }

        public JObject Json()
        {
            JObject rss = JObject.Parse(jsonString);
            return rss;
        }

        public JObject JsonFromDict()
        {
            JObject rss = JObject.FromObject(JObjectArray);
            return rss;
        }

        public JObject Json(string Json)
        {
            JObject rss = JObject.Parse(Json);
            return rss;
        }

        public T Json<T>()
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public T Json<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public bool HasKey(string key)
        {
            return JObject.Parse(jsonString).TryGetValue(key, out _);
        }

        /// <summary>
        /// Function to Serialize Json Data from Stream Asynchronously.
        /// </summary>
        public async Task SerializeJsonDataAsync(FileStream stream, bool isHidden = false)
        {
            jsonString = JsonConvert.SerializeObject(JObjectArray, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            OnJsonChanged?.Invoke();

            if (enableEncryption)
            {
                stream.Seek(0, SeekOrigin.End);
                byte[] es = EncryptData(jsonString);
                await stream.WriteAsync(es, 0, es.Length);
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    await sw.WriteAsync(jsonString);
                }
            }

            if (isHidden && File.Exists(fullPath))
            {
                new FileInfo(fullPath).Attributes = FileAttributes.Hidden;
            }

            stream.Dispose();

            if (enableDebug) { Debug.Log("<color=green>[JsonManager] Json Serialized:</color> " + stream.Name); }
        }

        /// <summary>
        /// Function to Serialize Json Data Asynchronously.
        /// </summary>
        public async Task SerializeJsonDataAsync(string filename, bool isHidden = false)
        {
            jsonString = JsonConvert.SerializeObject(JObjectArray, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            OnJsonChanged?.Invoke();

            FileStream stream = new FileStream(filename, FileMode.OpenOrCreate);

            if (enableEncryption)
            {
                stream.Seek(0, SeekOrigin.End);
                byte[] es = EncryptData(jsonString);
                await stream.WriteAsync(es, 0, es.Length);
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    await sw.WriteAsync(jsonString);
                }
            }

            if (isHidden && File.Exists(fullPath))
            {
                new FileInfo(fullPath).Attributes = FileAttributes.Hidden;
            }

            stream.Dispose();

            if (enableDebug) { Debug.Log("<color=green>[JsonManager] Json Serialized:</color> " + (stream).Name); }
        }

        /// <summary>
        /// Function to Serialize Json Data
        /// </summary>
        public void SerializeJsonData(string filename, bool isHidden = false)
        {
            jsonString = JsonConvert.SerializeObject(JObjectArray, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            OnJsonChanged?.Invoke();

            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = SerializationTool.GetSerializationPath(SerializationPath.GameDataPath);
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!filename.Contains('.'))
                filename = Path.ChangeExtension(filename, "json");

            fullPath = Path.Combine(folderPath, filename);

            if (enableEncryption)
            {
                byte[] es = EncryptData(jsonString);
                File.WriteAllBytes(fullPath, es);
            }
            else
            {
                File.WriteAllText(fullPath, jsonString);
            }

            if (isHidden && File.Exists(fullPath))
            {
                new FileInfo(fullPath).Attributes = FileAttributes.Hidden;
            }

            if (enableDebug) { Debug.Log("<color=green>[JsonManager] Json Serialized:</color> " + fullPath); }
        }

        /// <summary>
        /// Function to Serialize Json Data to FilePath
        /// </summary>
        public void SerializeJsonData(SerializationPath path, string filename, bool isHidden = false)
        {
            jsonString = JsonConvert.SerializeObject(JObjectArray, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            OnJsonChanged?.Invoke();

            folderPath = SerializationTool.GetSerializationPath(path);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!filename.Contains('.'))
                filename = Path.ChangeExtension(filename, "json");

            fullPath = Path.Combine(folderPath, filename);

            if (enableEncryption)
            {
                byte[] es = EncryptData(jsonString);
                File.WriteAllBytes(fullPath, es);
            }
            else
            {
                File.WriteAllText(fullPath, jsonString);
            }

            if (isHidden && File.Exists(fullPath))
            {
                new FileInfo(fullPath).Attributes = FileAttributes.Hidden;
            }

            if (enableDebug) { Debug.Log("<color=green>Game Saved: </color> " + fullPath); }
        }

        /// <summary>
        /// Function to Deserialize Json String Asynchronously.
        /// </summary>
        public async Task DeserializeDataAsync(string filename)
        {
            if (!filename.Contains('.'))
                filename = Path.ChangeExtension(filename, "json");

            fullPath = Path.Combine(folderPath, filename);

            if (!File.Exists(fullPath))
            {
                Debug.LogError("File (" + fullPath + ") does not exist!");
                return;
            }

            jsonString = await DeserializeJsonDataAsync();
            OnJsonChanged?.Invoke();
        }

        /// <summary>
        /// Function to Deserialize Json String Asynchronously.
        /// </summary>
        public async Task DeserializeDataAsync(string path, string filename)
        {
            if (!filename.Contains('.'))
                filename = Path.ChangeExtension(filename, "json");

            fullPath = Path.Combine(path, filename);

            if (!File.Exists(fullPath))
            {
                Debug.LogError("File (" + fullPath + ") does not exist!");
                return;
            }

            jsonString = await DeserializeJsonDataAsync();
            OnJsonChanged?.Invoke();
        }

        /// <summary>
        /// Function to Deserialize Json String
        /// </summary>
        public void DeserializeData(string filename)
        {
            if (filename.Contains('.'))
            {
                fullPath = folderPath + filename;
            }
            else
            {
                fullPath = folderPath + filename + ".json";
            }

            if (!File.Exists(fullPath))
            {
                Debug.LogError("File (" + fullPath + ") does not exist!");
                return;
            }

            jsonString = DeserializeJsonData();
            OnJsonChanged?.Invoke();
        }

        /// <summary>
        /// Function to Deserialize Json String from FilePath
        /// </summary>
        public void DeserializeData(SerializationPath path, string filename)
        {
            if (filename.Contains('.'))
            {
                fullPath = SerializationTool.GetSerializationPath(path) + filename;
            }
            else
            {
                fullPath = SerializationTool.GetSerializationPath(path) + filename + ".json";
            }

            if (!File.Exists(fullPath))
            {
                Debug.LogError("File (" + fullPath + ") does not exist!");
                return;
            }

            jsonString = DeserializeJsonData();
            OnJsonChanged?.Invoke();
        }

        /// <summary>
        /// Function to Deserialize Json String by given Stream.
        /// </summary>
        public void DeserializeData(Stream stream)
        {
            fullPath = ((FileStream)stream).Name;

            if (!File.Exists(fullPath))
            {
                Debug.LogError("File (" + fullPath + ") does not exist!");
                return;
            }

            jsonString = DeserializeJsonData();
            OnJsonChanged?.Invoke();
        }

        private string DeserializeJsonData()
        {
            if (File.Exists(fullPath))
            {
                if (enableEncryption)
                {
                    byte[] read = File.ReadAllBytes(fullPath);
                    return DecryptData(read);
                }
                else
                {
                    string jsonRead = File.ReadAllText(fullPath);
                    return jsonRead;
                }
            }
            else
            {
                if (enableDebug) { Debug.Log("<color=red>File does not exist: </color> " + fullPath); }
            }

            return default;
        }

        private async Task<string> DeserializeJsonDataAsync()
        {
            if (File.Exists(fullPath))
            {
                if (enableEncryption)
                {
                    byte[] result;

                    using (FileStream fileStream = File.Open(fullPath, FileMode.Open))
                    {
                        result = new byte[fileStream.Length];
                        await fileStream.ReadAsync(result, 0, (int)fileStream.Length);
                    }

                    return DecryptData(result);
                }
                else
                {
                    StreamReader reader = new StreamReader(fullPath);
                    string jsonRead = await reader.ReadToEndAsync();
                    reader.Dispose();
                    return jsonRead;
                }
            }
            else
            {
                if (enableDebug) { Debug.Log("<color=red>File does not exist: </color> " + fullPath); }
            }

            return default;
        }

        private byte[] EncryptData(string raw)
        {
            byte[] result;
            byte[] IV;
            byte[] AESkey = Encoding.UTF8.GetBytes(cipherKey);

            using (Aes aes = Aes.Create())
            {
                aes.Key = AESkey;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;

                IV = aes.IV;
                byte[] DataToEncrypt = Encoding.UTF8.GetBytes(raw);

                try
                {
                    ICryptoTransform Encryptor = aes.CreateEncryptor();
                    result = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
                }
                finally
                {
                    aes.Clear();
                }
            }

            byte[] cmbIV = new byte[IV.Length + result.Length];
            Array.Copy(IV, 0, cmbIV, 0, IV.Length);
            Array.Copy(result, 0, cmbIV, IV.Length, result.Length);
            return cmbIV;
        }

        private string DecryptData(byte[] bytes)
        {
            byte[] result;
            byte[] DataToDecrypt = bytes;
            byte[] AESkey = Encoding.UTF8.GetBytes(cipherKey);

            using (Aes aes = Aes.Create())
            {
                aes.Key = AESkey;

                byte[] IV = new byte[aes.BlockSize / 8];
                byte[] cipherText = new byte[DataToDecrypt.Length - IV.Length];
                Array.Copy(DataToDecrypt, IV, IV.Length);
                Array.Copy(DataToDecrypt, IV.Length, cipherText, 0, cipherText.Length);

                aes.IV = IV;
                aes.Mode = CipherMode.CBC;

                try
                {
                    ICryptoTransform Encryptor = aes.CreateDecryptor();
                    result = Encryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                }
                finally
                {
                    aes.Clear();
                }
            }

            return Encoding.UTF8.GetString(result);
        }
    }
}