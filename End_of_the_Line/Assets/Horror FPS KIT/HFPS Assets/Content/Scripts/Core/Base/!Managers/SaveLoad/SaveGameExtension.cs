/*
 * SaveGameExtension.cs - by ThunderWire Studio
 * Version 1.1
*/

using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThunderWire.Json;
using ThunderWire.Helpers;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    /// <summary>
    /// Provides additional SaveGame Handler System functions.
    /// </summary>
    public static class SaveGameExtension
    {
        private static string SerializationPath
        {
            get
            {
                string folderPath = SerializationHelper.Settings.GetSerializationPath();
                return Path.Combine(folderPath, "SavedGame");
            }
        }

        /// <summary>
        /// Get Saved Games Asynchronously.
        /// </summary>
        public static async Task<List<SavedData>> RetrieveSavedGames()
        {
            JsonManager jsonManager = new JsonManager(SerializationHelper.Settings, SerializationPath);
            List<SavedData> result = new List<SavedData>();

            if (Directory.Exists(SerializationPath))
            {
                DirectoryInfo dinfo = new DirectoryInfo(SerializationPath);
                FileInfo[] finfo = dinfo.GetFiles("*.sav");

                if (finfo.Length > 0)
                {
                    foreach (var file in finfo)
                    {
                        await Task.Run(() => jsonManager.DeserializeDataAsync(file.Name));

                        string sceneName = (string)jsonManager.Json()["scene"];
                        string levelName = (string)jsonManager.Json()["levelName"];
                        string saveTime = (string)jsonManager.Json()["dateTime"];

#if TW_LOCALIZATION_PRESENT
                        if (LocalizationSystem.HasReference)
                        {
                            string localeKey = (string)jsonManager.Json()["levelNameKey"];
                            string lvlName = LocalizationSystem.GetTranslation(localeKey);

                            if (!string.IsNullOrEmpty(lvlName))
                                levelName = lvlName;
                        }
#endif

                        result.Add(new SavedData()
                        {
                            SaveName = file.Name,
                            LevelName = levelName,
                            Scene = sceneName,
                            SaveTime = saveTime
                        });
                    }
                }
            }

            return result.OrderBy(x => x.SaveTime).ToList();
        }
    }
}