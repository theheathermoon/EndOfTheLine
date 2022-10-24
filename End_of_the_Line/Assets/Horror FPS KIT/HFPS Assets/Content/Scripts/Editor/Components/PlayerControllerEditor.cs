using UnityEngine;
using UnityEditor;
using HFPS.Player;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomEditor(typeof(PlayerController)), CanEditMultipleObjects]
    public class PlayerControllerEditor : Editor
    {
        private const string VERSION = "3.2";

        // player states
        private SerializedProperty characterState;
        private SerializedProperty movementState;

        // references
        private SerializedProperty baseKickback;
        private SerializedProperty weaponKickback;
        private SerializedProperty mouseLook;
        private SerializedProperty waterParticles;

        // layers
        private SerializedProperty surfaceCheckMask;
        private SerializedProperty slidingMask;

        // ground settings
        private SerializedProperty groundCheckOffset;
        private SerializedProperty groundCheckRadius;

        // controller main
        private SerializedProperty basicSettings;
        private SerializedProperty controllerFeatures;
        private SerializedProperty controllerSettings;
        private SerializedProperty slidingSettings;
        private SerializedProperty staminaSettings;
        private SerializedProperty autoMoveSettings;
        private SerializedProperty controllerAdjustments;

        // head bob
        private SerializedProperty cameraHeadBob;
        private SerializedProperty armsHeadBob;

        // sounds
        private SerializedProperty jumpSounds;
        private SerializedProperty jumpVolume;

        private void OnEnable()
        {
            characterState = serializedObject.FindProperty("characterState");
            movementState = serializedObject.FindProperty("movementState");

            baseKickback = serializedObject.FindProperty("baseKickback");
            weaponKickback = serializedObject.FindProperty("weaponKickback");
            mouseLook = serializedObject.FindProperty("mouseLook");
            waterParticles = serializedObject.FindProperty("waterParticles");

            surfaceCheckMask = serializedObject.FindProperty("surfaceCheckMask");
            slidingMask = serializedObject.FindProperty("slidingMask");

            groundCheckOffset = serializedObject.FindProperty("groundCheckOffset");
            groundCheckRadius = serializedObject.FindProperty("groundCheckRadius");

            basicSettings = serializedObject.FindProperty("basicSettings");
            controllerFeatures = serializedObject.FindProperty("controllerFeatures");
            controllerSettings = serializedObject.FindProperty("controllerSettings");
            slidingSettings = serializedObject.FindProperty("slidingSettings");
            staminaSettings = serializedObject.FindProperty("staminaSettings");
            autoMoveSettings = serializedObject.FindProperty("autoMoveSettings");
            controllerAdjustments = serializedObject.FindProperty("controllerAdjustments");

            cameraHeadBob = serializedObject.FindProperty("cameraHeadBob");
            armsHeadBob = serializedObject.FindProperty("armsHeadBob");

            jumpSounds = serializedObject.FindProperty("jumpSounds");
            jumpVolume = serializedObject.FindProperty("jumpVolume");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PlayerController.CharacterState chState = (PlayerController.CharacterState)characterState.intValue;
            PlayerController.MovementState movState = (PlayerController.MovementState)movementState.intValue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Character State", EditorStyles.label, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(chState.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Movement State", EditorStyles.label, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(movState.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtils.TrIconText("References", "FixedJoint Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(baseKickback);
                EditorGUILayout.PropertyField(weaponKickback);
                EditorGUILayout.PropertyField(mouseLook);
                EditorGUILayout.PropertyField(waterParticles);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtils.TrIconText("Layers", "GUILayer Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(surfaceCheckMask);
                EditorGUILayout.PropertyField(slidingMask);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtils.TrIconText("Ground Settings", "TerrainCollider Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(groundCheckOffset);
                EditorGUILayout.PropertyField(groundCheckRadius);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Controller Main", EditorStyles.boldLabel);
            EditorUtils.DrawHeaderProperty("Basic Settings", "CharacterController Icon", basicSettings);
            EditorUtils.DrawHeaderProperty("Controller Features", "ConfigurableJoint Icon", controllerFeatures);
            EditorUtils.DrawHeaderProperty("Controller Settings", "Grid.MoveTool", controllerSettings);
            EditorUtils.DrawHeaderProperty("Sliding Settings", "AnalyticsTracker Icon", slidingSettings);
            EditorUtils.DrawHeaderProperty("Stamina Settings", "OffMeshLink Icon", staminaSettings);
            EditorUtils.DrawHeaderProperty("Auto-Move Settings", "CompositeCollider2D Icon", autoMoveSettings);
            EditorUtils.DrawHeaderProperty("Controller Adjustments", "Avatar Icon", controllerAdjustments);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("HeadBob Settings", EditorStyles.boldLabel);
            EditorUtils.DrawHeaderProperty("Camera HeadBob", "Camera Icon", cameraHeadBob);
            EditorUtils.DrawHeaderProperty("Arms Camera HeadBob", "SceneViewCamera", armsHeadBob);

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorUtils.TrIconText("Sounds", "AudioSource Icon", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(jumpSounds);
                EditorGUILayout.PropertyField(jumpVolume);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Version: " + VERSION, EditorStyles.miniLabel);

            Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}