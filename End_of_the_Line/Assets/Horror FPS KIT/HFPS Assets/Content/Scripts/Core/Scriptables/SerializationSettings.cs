using System;
using UnityEngine;

namespace ThunderWire.Helpers
{
    [Serializable]
    public class SerializationSettings : ScriptableObject
    {
        public bool EncryptData;
        public SerializationPath SerializePath = SerializationPath.GameDataPath;
        public string EncryptionKey;

        /// <summary>
        /// Get Serialization Settings File Path
        /// </summary>
        public string GetSerializationPath()
        {
            return SerializationTool.GetSerializationPath(SerializePath);
        }
    }

    public class SerializationTool
    {
        /// <summary>
        /// Get Serialization File Path
        /// </summary>
        public static string GetSerializationPath(SerializationPath path)
        {
            switch (path)
            {
                case SerializationPath.GamePath:
                    return Application.dataPath + "/";
                case SerializationPath.GameDataPath:
                    return Application.dataPath + "/Data/";
                case SerializationPath.GameSavesPath:
                    return Application.dataPath + "/Data/SavedGame/";
                case SerializationPath.DocumentsGamePath:
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/";
                case SerializationPath.DocumentsDataPath:
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/Data/";
                case SerializationPath.DocumentsSavesPath:
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + Application.productName + "/Data/SavedGame/";
                default:
                    return default;
            }
        }
    }

    [Serializable]
    public enum SerializationPath
    {
        GamePath,
        GameDataPath,
        GameSavesPath,
        DocumentsGamePath,
        DocumentsDataPath,
        DocumentsSavesPath
    }
}