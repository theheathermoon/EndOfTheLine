using UnityEngine;
using UnityEngine.UI;

namespace HFPS.Systems
{
    public class SavedGame : MonoBehaviour
    {
        public Text sceneName;
        public Text dateTime;

        [HideInInspector] public string save;
        [HideInInspector] public string scene;

        public void SetSavedGame(string SaveName, string SceneName, string LevelName, string DateTime)
        {
            sceneName.text = LevelName;
            dateTime.text = DateTime;
            scene = SceneName;
            save = SaveName;
        }
    }
}