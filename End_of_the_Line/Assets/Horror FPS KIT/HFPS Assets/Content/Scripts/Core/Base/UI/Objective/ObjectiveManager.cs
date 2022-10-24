/*
 * ObjectiveManager.cs - by ThunderWire Studio
 * ver. 1.2
*/

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Utility;
using HFPS.UI;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
#endif

namespace HFPS.Systems
{
    /// <summary>
    /// Main Objectives Script
    /// </summary>
    public class ObjectiveManager : Singleton<ObjectiveManager>
    {
        public List<ObjectiveModel> activeObjectives = new List<ObjectiveModel>();
        public List<ObjectiveModel> objectives = new List<ObjectiveModel>();
        private ObjectiveEvent[] objectiveEventsCache;

        [Header("Main")]
        public ObjectivesScriptable SceneObjectives;

        [Header("UI")]
        public GameObject ObjectivesUI;
        public GameObject PushObjectivesUI;
        public GameObject ObjectivePrefab;
        public GameObject PushObjectivePrefab;

        [Header("Timing")]
        public float CompleteTime = 3f;

        [Header("Other")]
        public bool isUppercased;
        public bool allowPreCompleteText = true;

        [Header("Audio")]
        public AudioClip newObjective;
        public AudioClip completeObjective;
        [Range(0, 1f)] public float volume;

        private string multipleObjectivesText;
        private string preCompleteText;
        private string updateText;

        private AudioSource soundEffects;
        private bool objShown;

        void Awake()
        {
            TextsSource.Subscribe(OnInitTexts);

#if TW_LOCALIZATION_PRESENT
            if (HFPS_GameManager.LocalizationEnabled)
            {
                LocalizationSystem.OnChangeLanguage += _ => OnLanguageUpdate();
            }
#endif

            if (SceneObjectives)
            {
                foreach (var obj in SceneObjectives.Objectives)
                {
                    bool eventExist = false;

                    foreach (var evt in GetEvents())
                    {
                        if (evt.EventID.Equals(obj.eventID))
                        {
                            objectives.Add(new ObjectiveModel(obj.objectiveID, obj.completeCount, obj.objectiveText, evt));
                            eventExist = true;
                            break;
                        }
                    }

                    if (!eventExist)
                    {
                        objectives.Add(new ObjectiveModel(obj.objectiveID, obj.completeCount, obj.objectiveText, null));
                    }
                }
            }
            else
            {
                throw new System.NullReferenceException("Please assign the objectives asset first!");
            }

            soundEffects = ScriptManager.Instance.SoundEffects;
            objShown = true;
        }

#if TW_LOCALIZATION_PRESENT
        private void OnDestroy()
        {
            LocalizationSystem.OnChangeLanguage -= _ => OnLanguageUpdate();
        }

        void OnLanguageUpdate()
        {
            if (SceneObjectives.enableLocalization)
            {
                var translations = LocalizationSystem.GetCurrentTranslations();

                for (int i = 0; i < objectives.Count; i++)
                {
                    string translationKey = SceneObjectives.Objectives[i].localeKey;
                    string translation = objectives[i].objectiveText;

                    if (!string.IsNullOrEmpty(translationKey) && translations.ContainsKey(translationKey))
                    {
                        translation = translations[translationKey];
                        objectives[i].objectiveText = translation;
                    }

                    if (activeObjectives.Count > 0)
                    {
                        foreach (var obj in activeObjectives)
                        {
                            if (obj.identifier == objectives[i].identifier)
                            {
                                obj.objectiveText = translation;

                                if (obj.objective != null)
                                {
                                    obj.objective.transform.GetChild(0).GetComponent<Text>().text = translation;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"You are trying to use localization but it is disabled on \"{SceneObjectives.name}\"!");
            }
        }
#endif

        public void RefreshLocalization()
        {
#if TW_LOCALIZATION_PRESENT
            OnLanguageUpdate();
#endif
        }

        void OnInitTexts()
        {
            multipleObjectivesText = TextsSource.GetText("Objective.Multiple", "You have new objectives, press [Inventory] to check it.");
            preCompleteText = TextsSource.GetText("Objective.PreComplete", "Objective Pre-Completed");
            updateText = TextsSource.GetText("Objective.Updated", "Objective Updated");
        }

        public ObjectiveEvent[] GetEvents()
        {
            if (objectiveEventsCache != null && objectiveEventsCache.Length > 0)
            {
                return objectiveEventsCache;
            }

            return objectiveEventsCache = FindObjectsOfType<ObjectiveEvent>();
        }

        void Update()
        {
            if (objShown)
            {
                if (activeObjectives.Count > 0 && activeObjectives.Any(obj => obj.isCompleted == false))
                {
                    ObjectivesUI.SetActive(true);

                    foreach (var obj in activeObjectives)
                    {
                        if (obj.objective != null)
                        {
                            if (obj.objectiveText.Contains("{") && obj.objectiveText.Contains("}"))
                            {
                                obj.objective.GetComponentInChildren<Text>().text = string.Format(obj.objectiveText, obj.completion, obj.toComplete);
                            }
                        }
                    }
                }
                else
                {
                    ObjectivesUI.SetActive(false);
                }
            }
            else
            {
                ObjectivesUI.SetActive(false);
            }
        }

        void PlaySound(AudioClip audio)
        {
            if (audio != null)
            {
                soundEffects.clip = audio;
                soundEffects.volume = volume;
                soundEffects.Play();
            }
        }

        public void ShowObjectives(bool show)
        {
            objShown = show;
            ObjectivesUI.SetActive(show);
        }

        public void AddObjective(int objectiveID, float time, bool sound = true)
        {
            if (!CheckObjective(objectiveID))
            {
                ObjectiveModel objModel = objectives.FirstOrDefault(o => o.identifier == objectiveID);

                if (!objModel.isCompleted)
                {
                    GameObject obj = Instantiate(ObjectivePrefab, ObjectivesUI.transform);
                    obj.transform.GetChild(0).GetComponent<Text>().text = objModel.objectiveText;
                    objModel.objective = obj;

                    activeObjectives.Add(objModel);

                    string text = objModel.objectiveText;

                    if (text.Contains("{") && text.Contains("}"))
                    {
                        text = string.Format(text, objModel.completion, objModel.toComplete);
                    }

                    PushObjectiveText(text, time, isUppercased);

                    if (sound) { PlaySound(newObjective); }
                }
            }
        }

        public void AddObjectives(int[] objectivesID, float time, bool sound = true)
        {
            int newObjectives = 0;
            string singleObjective = "";

            foreach (var obj in objectivesID)
            {
                if (!CheckObjective(obj))
                {
                    var objModel = objectives[obj];

                    if (!objModel.isCompleted)
                    {
                        GameObject objObject = Instantiate(ObjectivePrefab, ObjectivesUI.transform);
                        objObject.transform.GetChild(0).GetComponent<Text>().text = objModel.objectiveText;
                        objModel.objective = objObject;
                        activeObjectives.Add(objModel);
                        singleObjective = objModel.objectiveText;
                        newObjectives++;
                    }
                }
            }

            if (newObjectives != 0)
            {
                if (newObjectives > 1)
                {
                    PushObjectiveText(multipleObjectivesText.GetStringWithInput('[', ']', '[', ']'), time, isUppercased);
                }
                else
                {
                    PushObjectiveText(singleObjective, time, isUppercased);
                }

                if (sound) { PlaySound(newObjective); }
            }
        }

        public void AddObjectiveModel(ObjectiveModel model)
        {
            ObjectiveModel objModel = CreateWithEvent(model);

            if (!objModel.isCompleted)
            {
                GameObject objObject = Instantiate(ObjectivePrefab, ObjectivesUI.transform);
                objObject.transform.GetChild(0).GetComponent<Text>().text = objModel.objectiveText;
                objModel.objective = objObject;
                activeObjectives.Add(objModel);
            }
        }

        ObjectiveModel CreateWithEvent(ObjectiveModel model)
        {
            foreach (var obj in SceneObjectives.Objectives)
            {
                if (obj.objectiveID == model.identifier)
                {
                    foreach (var evt in GetEvents())
                    {
                        if (evt.EventID == obj.eventID)
                        {
                            ObjectiveModel newEventModel = new ObjectiveModel(model.identifier, obj.completeCount, model.isCompleted, evt)
                            {
                                objectiveText = obj.objectiveText
                            };

                            newEventModel.completion = model.completion;
                            return newEventModel;
                        }
                    }

                    ObjectiveModel newModel = new ObjectiveModel(model.identifier, obj.completeCount, model.isCompleted, null)
                    {
                        objectiveText = obj.objectiveText
                    };

                    newModel.completion = model.completion;
                    return newModel;
                }
            }

            return default;
        }

        void PushObjectiveText(string text, float time, bool upper = false)
        {
            GameObject obj = Instantiate(PushObjectivePrefab, PushObjectivesUI.transform);
            obj.GetComponent<Notification>().SetMessage(text, time, upper: upper);
        }

        public void CompleteObjective(int ID, bool sound = true)
        {
            foreach (var obj in activeObjectives)
            {
                if (obj.identifier == ID)
                {
                    obj.completion++;

                    if (obj.completion >= obj.toComplete)
                    {
                        obj.isCompleted = true;

                        if (obj.objEvent != null)
                            obj.objEvent.ExecuteEvent();

                        Destroy(obj.objective);
                        PushObjectiveText(updateText, CompleteTime);
                        if (sound) { PlaySound(completeObjective); }
                    }
                }
            }
        }

        public void PreCompleteObjective(int ID)
        {
            foreach (var obj in objectives)
            {
                if (obj.identifier == ID)
                {
                    obj.completion++;
                    obj.isTouched = true;

                    if (obj.completion >= obj.toComplete)
                    {
                        obj.isCompleted = true;

                        if (obj.objEvent != null)
                            obj.objEvent.ExecuteEvent();

                        if (allowPreCompleteText)
                        {
                            PushObjectiveText(preCompleteText, CompleteTime);
                            PlaySound(completeObjective);
                        }
                    }
                }
            }
        }

        public bool CheckObjective(int ID)
        {
            foreach (var obj in activeObjectives)
            {
                if (obj.identifier == ID)
                {
                    if (obj.isCompleted)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ContainsObjective(int ID)
        {
            foreach (var obj in activeObjectives)
            {
                if (obj.identifier == ID)
                {
                    return true;
                }
            }

            return false;
        }

        public int[] ReturnNonExistObjectives(int[] Objectives)
        {
            int[] result = Objectives.Except(activeObjectives.Select(x => x.identifier).ToArray()).ToArray();
            return result;
        }
    }

    public sealed class ObjectiveModel
    {
        public string objectiveText;
        public int identifier;

        public int toComplete;
        public int completion;

        public GameObject objective;
        public bool isCompleted;
        public bool isTouched;

        public ObjectiveEvent objEvent;

        public ObjectiveModel(int id, int complete, string text, ObjectiveEvent evt)
        {
            identifier = id;
            toComplete = complete;
            objectiveText = text;
            objEvent = evt;
        }

        public ObjectiveModel(int id, int complete, bool completed, ObjectiveEvent evt)
        {
            identifier = id;
            toComplete = complete;
            isCompleted = completed;
            objectiveText = "";
            objEvent = evt;
        }

        public ObjectiveModel() { }
    }
}