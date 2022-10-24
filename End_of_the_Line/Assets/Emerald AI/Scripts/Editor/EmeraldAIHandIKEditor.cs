using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldAIHandIK))]
    [System.Serializable]
    public class EmeraldAIHandIKEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EmeraldAIHandIK self = (EmeraldAIHandIK)target;

            if (self.HandIKProfileData == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Note: Users should have their AI's weapon parented to their AI's right hand. The Hand IK System only supports Ranged Weapons (with AI using the Emerald IK Type) " +
                    "and should only be used for weapons like guns that require two hands. You can only modify hand point movement during runtime.", MessageType.Info);
                EditorGUILayout.Space();

                SeeTutorialButton();
                EditorGUILayout.Space();

                EditorGUILayout.HelpBox("Please create a Hand IK Profile. This will be used to save your AI's hand points' position and rotation.", MessageType.Info);
                EditorGUILayout.Space();

                if (GUILayout.Button("Create Hand IK Profile"))
                {
                    self.FilePath = EditorUtility.SaveFilePanelInProject("Save as HandIKProfile", "New HandIKProfile", "asset", "Please enter a file name to save the file to");

                    if (self.FilePath != string.Empty)
                    {
                        HandIKProfile HandIKProfileAsset = CreateInstance<HandIKProfile>();
                        AssetDatabase.CreateAsset(HandIKProfileAsset, self.FilePath);
                        self.HandIKProfileData = HandIKProfileAsset;
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Import the specified Hand IK Profile. Note: If changes are made to a Hand IK Profile, all AI sharing said profile will be affected by the changes.", MessageType.Info);
                self.ImportedHandIKProfileData = (HandIKProfile)EditorGUILayout.ObjectField("Hand IK Profile", self.ImportedHandIKProfileData, typeof(HandIKProfile), false);
                if (GUILayout.Button("Import Hand IK Profile"))
                {
                    string FilePath = AssetDatabase.GetAssetPath(self.ImportedHandIKProfileData);
                    self.FilePath = FilePath;
                    self.HandIKProfileData = self.ImportedHandIKProfileData;
                }
            }
            else 
            {
                if (Application.isPlaying)
                {
                    EditorGUILayout.Space();
                    if (self.HandIKProfileData.ValuesModified)
                    {
                        GUI.backgroundColor = new Color(1f, 0.0f, 0.0f, 0.25f);
                        EditorGUILayout.HelpBox("Changes have been made to the Hand IK Data. Please ensure you save these after you exit runtime.", MessageType.Info);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.Space();
                    }

                    GUI.backgroundColor = new Color(0.9f, 0.9f, 0, 0.5f);
                    EditorGUILayout.HelpBox("You can now make changes to the Hand IK Points' position and rotation. When you are done, press the 'Update Hand IK Profile' button before exiting runtime.", MessageType.Info);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Update Hand IK Profile"))
                    {
                        self.HandIKProfileData.RightHandPosition = self.RightHandPoint.localPosition;
                        self.HandIKProfileData.RightHandRotation = self.RightHandPoint.localEulerAngles;
                        self.HandIKProfileData.LeftHandPosition = self.LeftHandPoint.localPosition;
                        self.HandIKProfileData.LeftHandRotation = self.LeftHandPoint.localEulerAngles;
                        self.HandIKProfileData.ValuesModified = true;
                    }

                    EditorGUILayout.Space();
                    GUI.backgroundColor = new Color(0.9f, 0.9f, 0, 0.5f);
                    EditorGUILayout.HelpBox("You can press the button below to make the Left Hand Point the active gameobject so it can be adjusted.", MessageType.Info);
                    GUI.backgroundColor = Color.white;

                    if (GUILayout.Button("Select Left Hand Point"))
                    {
                        Selection.activeObject = self.LeftHandPoint;
                        MakeHierarchySelection();
                    }

                    EditorGUILayout.Space();
                    GUI.backgroundColor = new Color(0.9f, 0.9f, 0, 0.5f);
                    EditorGUILayout.HelpBox("You can press the button below to make the Right Hand Point the active gameobject so it can be adjusted.", MessageType.Info);
                    GUI.backgroundColor = new Color(1f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.HelpBox("Note: It is recommended the Right Hand Point's rotation is not adjusted as it shouldn't require modification. " +
                        "The Right Hand's position also shouldn't require modification, if the AI's weapon is positioned correctly to their hand transform.", MessageType.Info);
                    GUI.backgroundColor = Color.white;

                    if (GUILayout.Button("Select Right Hand Point"))
                    {
                        Selection.activeObject = self.RightHandPoint;
                        MakeHierarchySelection();
                    }
                }
                else
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Note: Users should have their AI's weapon parented to their AI's right hand. The Hand IK System only supports Ranged Weapons " +
                        "(with AI using the Emerald IK Type) and should only be used for weapons like guns that require two hands. You can only modify hand point movement during runtime.", MessageType.Info);
                    EditorGUILayout.Space();

                    SeeTutorialButton();

                    if (self.HandIKProfileData.ValuesModified)
                    {
                        GUI.backgroundColor = new Color(1f, 0.0f, 0.0f, 0.25f);
                        EditorGUILayout.HelpBox("Changes have been made to the Hand IK Data, please press the 'Save Hand IK Profile Changes' button to save the changes made during runtime.", MessageType.Info);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.Space();

                        if (GUILayout.Button("Save Hand IK Profile Changes"))
                        {
                            Vector3 RightHandPos = self.HandIKProfileData.RightHandPosition;
                            Vector3 RightHandRot = self.HandIKProfileData.RightHandRotation;
                            Vector3 LeftHandPos = self.HandIKProfileData.LeftHandPosition;
                            Vector3 LeftHandRot = self.HandIKProfileData.LeftHandRotation;

                            if (AssetDatabase.Contains(self.HandIKProfileData))
                                AssetDatabase.DeleteAsset(self.FilePath);

                            HandIKProfile HandIKProfileAsset = CreateInstance<HandIKProfile>();
                            HandIKProfileAsset.RightHandPosition = RightHandPos;
                            HandIKProfileAsset.RightHandRotation = RightHandRot;
                            HandIKProfileAsset.LeftHandPosition = LeftHandPos;
                            HandIKProfileAsset.LeftHandRotation = LeftHandRot;

                            AssetDatabase.CreateAsset(HandIKProfileAsset, self.FilePath);

                            self.HandIKProfileData = HandIKProfileAsset;
                            AssetDatabase.Refresh();
                            self.HandIKProfileData.ValuesModified = false;
                        }
                    }
                    else
                    {
                        EditorGUILayout.Space();
                        GUI.backgroundColor = new Color(0f, 1.0f, 0.0f, 0.25f);
                        EditorGUILayout.HelpBox("Changes to the Hand IK Data are up to date.", MessageType.Info);
                        GUI.backgroundColor = Color.white;
                        if (GUILayout.Button("See Hand IK Profile"))
                        {
                            Selection.activeObject = self.HandIKProfileData;
                        }
                        EditorGUILayout.Space();

                        GUI.backgroundColor = new Color(0.9f, 0.9f, 0, 0.5f);
                        EditorGUILayout.HelpBox("Release the current Hand IK Profile so a new on can be created or imported.", MessageType.Info);
                        GUI.backgroundColor = Color.white;
                        if (GUILayout.Button("Release Hand IK Profile"))
                        {
                            self.HandIKProfileData = null;
                        }

                        EditorGUILayout.Space();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void MakeHierarchySelection()
        {
            var SceneHierarchyWindow = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var window = EditorWindow.GetWindow(SceneHierarchyWindow);
            var recursive = SceneHierarchyWindow.GetMethod("SetExpandedRecursive");
            recursive.Invoke(window, new object[] { Selection.activeGameObject.GetInstanceID(), true });
        }

        void SeeTutorialButton ()
        {
            var HelpButtonStyle = new GUIStyle(GUI.skin.button);
            HelpButtonStyle.normal.textColor = Color.white;
            HelpButtonStyle.fontStyle = FontStyle.Bold;

            GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
            EditorGUILayout.LabelField("For a detailed tutorial on using the Hand IK System, please see the tutorial below.", EditorStyles.helpBox);
            GUI.backgroundColor = new Color(0, 0.65f, 0, 0.8f);
            if (GUILayout.Button("See Tutorial", HelpButtonStyle, GUILayout.Height(20)))
            {
                Application.OpenURL("https://github.com/Black-Horizon-Studios/Emerald-AI/wiki/Setting-up-Ranged-Weapon-Hand-IK#setting-up-ranged-weapon-hand-ik");
            }
            GUI.backgroundColor = Color.white;
        }
    }
}