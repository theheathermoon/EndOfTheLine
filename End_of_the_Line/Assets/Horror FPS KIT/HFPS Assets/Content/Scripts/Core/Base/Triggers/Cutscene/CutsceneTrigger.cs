using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HFPS.Systems
{
    /// <summary>
    /// Provides Cutscene Trigger Methods
    /// </summary>
    public class CutsceneTrigger : MonoBehaviour, ISaveable
    {
        private CutsceneManager manager;

        public string[] cutscenesQueue;

        [HideInInspector, SaveableField]
        public bool isTriggered;

        void Awake()
        {
            manager = CutsceneManager.Instance;
        }

        public void SkipCutscene()
        {
            manager.SkipCutscene();
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isTriggered)
            {
                if (cutscenesQueue.Length > 0)
                {
                    foreach (var item in cutscenesQueue)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            manager.PlayOrAddCutscene(item);
                        }
                    }
                }
                else
                {
                    Debug.LogError("[Cutscene Trigger] Cutscenes Queue could not be empty!");
                }

                isTriggered = true;
            }
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>()
            {
                { "isTriggered", isTriggered }
            };
        }

        public void OnLoad(JToken token)
        {
            isTriggered = (bool)token["isTriggered"];
        }
    }
}
