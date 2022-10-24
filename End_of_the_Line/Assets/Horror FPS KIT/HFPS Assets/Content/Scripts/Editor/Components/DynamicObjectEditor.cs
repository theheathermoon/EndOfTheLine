using UnityEngine;
using UnityEditor;
using HFPS.Systems;
using ThunderWire.Editors;

namespace HFPS.Editors
{
    [CustomEditor(typeof(DynamicObject)), CanEditMultipleObjects]
    public class DynamicObjectEditor : Editor
    {
        #region Enums
        public SerializedProperty sp_dynamicType;
        public SerializedProperty sp_useType;
        public SerializedProperty sp_interactType;
        public SerializedProperty sp_keyType;
        #endregion

        #region Generic
        public SerializedProperty sp_Animation;
        public SerializedProperty sp_IgnoreColliders;
        public SerializedProperty sp_InteractEvent;
        public SerializedProperty sp_DisabledEvent;
        public SerializedProperty sp_customText;
        public SerializedProperty sp_customTextKey;
        public SerializedProperty sp_keyID;
        public SerializedProperty sp_loadDisabled;
        public SerializedProperty sp_useAnim;
        public SerializedProperty sp_backUseAnim;
        public SerializedProperty sp_DebugAngle;
        #endregion

        #region Door
        public SerializedProperty sp_knobAnim;
        public SerializedProperty sp_useJammedPlanks;
        public SerializedProperty sp_Planks;
        public SerializedProperty sp_startRot;
        public SerializedProperty sp_useSR;
        public SerializedProperty sp_doorStopSpeed;
        #endregion

        #region Drawer
        public SerializedProperty sp_moveWithX;
        public SerializedProperty sp_minMaxMove;
        public SerializedProperty sp_reverseMove;
        public SerializedProperty sp_drawerDragSounds;
        public SerializedProperty sp_InteractPos;
        public SerializedProperty sp_drawerDragPlay;
        #endregion

        #region Lever
        public SerializedProperty sp_stopAngle;
        public SerializedProperty sp_lockOnUp;
        #endregion

        #region Valve
        public SerializedProperty sp_valveTurnSounds;
        public SerializedProperty sp_valveSoundAfter;
        public SerializedProperty sp_valveTurnSpeed;
        public SerializedProperty sp_valveTurnTime;
        public SerializedProperty sp_valveTurnAxis;
        #endregion

        #region Drawer
        public SerializedProperty sp_Volume;
        public SerializedProperty sp_Open;
        public SerializedProperty sp_Close;
        public SerializedProperty sp_LockedTry;
        public SerializedProperty sp_UnlockSound;
        public SerializedProperty sp_LeverUpSound;
        #endregion

        void OnEnable()
        {
            sp_dynamicType = serializedObject.FindProperty("dynamicType");
            sp_useType = serializedObject.FindProperty("useType");
            sp_interactType = serializedObject.FindProperty("interactType");
            sp_keyType = serializedObject.FindProperty("keyType");
            sp_Animation = serializedObject.FindProperty("m_Animation");
            sp_IgnoreColliders = serializedObject.FindProperty("IgnoreColliders");
            sp_InteractEvent = serializedObject.FindProperty("InteractEvent");
            sp_DisabledEvent = serializedObject.FindProperty("DisabledEvent");
            sp_customText = serializedObject.FindProperty("customText");
            sp_customTextKey = serializedObject.FindProperty("customTextKey");
            sp_keyID = serializedObject.FindProperty("keyID");
            sp_loadDisabled = serializedObject.FindProperty("loadDisabled");
            sp_useAnim = serializedObject.FindProperty("useAnim");
            sp_backUseAnim = serializedObject.FindProperty("backUseAnim");
            sp_knobAnim = serializedObject.FindProperty("knobAnim");
            sp_useJammedPlanks = serializedObject.FindProperty("useJammedPlanks");
            sp_Planks = serializedObject.FindProperty("Planks");
            sp_useSR = serializedObject.FindProperty("useSR");
            sp_doorStopSpeed = serializedObject.FindProperty("doorStopSpeed");
            sp_startRot = serializedObject.FindProperty("startingRotation");
            sp_moveWithX = serializedObject.FindProperty("moveWithX");
            sp_minMaxMove = serializedObject.FindProperty("minMaxMove");
            sp_reverseMove = serializedObject.FindProperty("reverseMove");
            sp_drawerDragSounds = serializedObject.FindProperty("drawerDragSounds");
            sp_drawerDragPlay = serializedObject.FindProperty("drawerDragPlay");
            sp_InteractPos = serializedObject.FindProperty("InteractPos");
            sp_stopAngle = serializedObject.FindProperty("stopAngle");
            sp_lockOnUp = serializedObject.FindProperty("lockOnUp");
            sp_valveTurnSounds = serializedObject.FindProperty("valveTurnSounds");
            sp_valveSoundAfter = serializedObject.FindProperty("valveSoundAfter");
            sp_valveTurnSpeed = serializedObject.FindProperty("valveTurnSpeed");
            sp_valveTurnTime = serializedObject.FindProperty("valveTurnTime");
            sp_valveTurnAxis = serializedObject.FindProperty("turnAxis");
            sp_Volume = serializedObject.FindProperty("m_Volume");
            sp_Open = serializedObject.FindProperty("Open");
            sp_Close = serializedObject.FindProperty("Close");
            sp_LockedTry = serializedObject.FindProperty("LockedTry");
            sp_UnlockSound = serializedObject.FindProperty("UnlockSound");
            sp_LeverUpSound = serializedObject.FindProperty("LeverUpSound");
            sp_DebugAngle = serializedObject.FindProperty("DebugAngle");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Type_Dynamic dynamicType = (Type_Dynamic)sp_dynamicType.enumValueIndex;
            Type_Use useType = (Type_Use)sp_useType.enumValueIndex;
            Type_Interact int_type = (Type_Interact)sp_interactType.enumValueIndex;
            Type_Key keyType = (Type_Key)sp_keyType.enumValueIndex;

            EditorGUILayout.PropertyField(sp_dynamicType);

            if (dynamicType == Type_Dynamic.Door ||
                dynamicType == Type_Dynamic.Drawer ||
                dynamicType == Type_Dynamic.Lever)
            {
                EditorGUILayout.PropertyField(sp_interactType, new GUIContent("Interact Type"));
            }

            if (dynamicType == Type_Dynamic.Door || dynamicType == Type_Dynamic.Drawer)
            {
                EditorGUILayout.PropertyField(sp_useType, new GUIContent("Use Type"));

                if (useType == Type_Use.Locked)
                {
                    EditorGUILayout.PropertyField(sp_keyType, new GUIContent("Key Type"));

                    if (keyType == Type_Key.Inventory)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(sp_keyID, new GUIContent("Unlock Item ID"));
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(sp_IgnoreColliders, new GUIContent("Ignore Colliders"), true);

            if(dynamicType == Type_Dynamic.Valve || dynamicType == Type_Dynamic.MovableInteract) 
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(sp_InteractEvent, new GUIContent("Interact Event"), true);
            }

            if (dynamicType == Type_Dynamic.Door || dynamicType == Type_Dynamic.Drawer)
            {
                if (useType == Type_Use.Locked)
                {
                    if (!HFPS_GameManager.LocalizationEnabled)
                    {
                        EditorGUILayout.PropertyField(sp_customText, new GUIContent("Locked Text"));
                    }
                    else
                    {
                        Rect rect = GUILayoutUtility.GetRect(1, 20);
                        EditorUtils.DrawLocaleSelector(rect, sp_customTextKey, new GUIContent("Locked Text Key"));
                    }
                }

                if (dynamicType == Type_Dynamic.Door && useType == Type_Use.Jammed)
                {
                    EditorGUILayout.Space();
                    DrawTitle("Jammed Door Planks");
                    EditorGUILayout.PropertyField(sp_useJammedPlanks, new GUIContent("Use Planks"));
                    EditorGUILayout.PropertyField(sp_Planks, new GUIContent("Planks"), true);
                }
            }

            if (dynamicType == Type_Dynamic.Door ||
                dynamicType == Type_Dynamic.Drawer && int_type == Type_Interact.Animation ||
                dynamicType == Type_Dynamic.Lever && int_type == Type_Interact.Animation)
            {
                EditorGUILayout.Space();
                DrawTitle("Animation");
                EditorGUILayout.PropertyField(sp_Animation, new GUIContent("Animation Object"));

                if (dynamicType == Type_Dynamic.Door || dynamicType == Type_Dynamic.Drawer)
                {
                    if (int_type == Type_Interact.Animation)
                    {
                        EditorGUILayout.PropertyField(sp_useAnim, new GUIContent("Open Anim"));
                        EditorGUILayout.PropertyField(sp_backUseAnim, new GUIContent("Close Anim"));
                    }

                    if (dynamicType != Type_Dynamic.Drawer)
                    {
                        EditorGUILayout.PropertyField(sp_knobAnim, new GUIContent("Knob Animation"));
                    }
                }
                else if (dynamicType == Type_Dynamic.Lever)
                {
                    EditorGUILayout.PropertyField(sp_useAnim, new GUIContent("SwitchUp Anim"));
                    EditorGUILayout.PropertyField(sp_backUseAnim, new GUIContent("SwitchDown Anim"));
                }
            }

            EditorGUILayout.Space();

            if (dynamicType == Type_Dynamic.Door)
            {
                DrawTitle("Door Properties");

                EditorGUILayout.PropertyField(sp_Open, new GUIContent("Open Sound"));
                EditorGUILayout.PropertyField(sp_Close, new GUIContent("Close Sound"));
                EditorGUILayout.PropertyField(sp_UnlockSound, new GUIContent("Unlock Sound"));
                EditorGUILayout.PropertyField(sp_LockedTry, new GUIContent("Locked Try Sound"));

                if (int_type == Type_Interact.Mouse)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(sp_doorStopSpeed, new GUIContent("Door Stop Speed"));
                    EditorGUILayout.PropertyField(sp_useSR, new GUIContent("Use Start Rotation"));
                    if (sp_useSR.boolValue)
                    {
                        EditorGUILayout.PropertyField(sp_startRot, new GUIContent("Starting Rotation"));
                    }

                    EditorGUILayout.PropertyField(sp_DebugAngle, new GUIContent("Debug Door Angle"));
                }
            }
            else if (dynamicType == Type_Dynamic.Drawer)
            {
                DrawTitle("Drwer Properties");

                if (int_type == Type_Interact.Mouse)
                {
                    EditorGUILayout.PropertyField(sp_minMaxMove, new GUIContent("Min Max Move"));
                    EditorGUILayout.PropertyField(sp_moveWithX, new GUIContent("Move With X"));
                    EditorGUILayout.PropertyField(sp_reverseMove, new GUIContent("Reverse Move"));
                    EditorGUILayout.PropertyField(sp_drawerDragSounds, new GUIContent("Drag Sounds"));

                    EditorGUILayout.Space();
                    DrawTitle("Drwer Sounds");

                    if (sp_drawerDragSounds.boolValue)
                    {
                        EditorGUILayout.PropertyField(sp_drawerDragPlay, new GUIContent("Drag Play Value"));
                        EditorGUILayout.PropertyField(sp_Open, new GUIContent("Open Sound"));
                        EditorGUILayout.PropertyField(sp_Close, new GUIContent("Close Sound"));
                    }

                    EditorGUILayout.PropertyField(sp_UnlockSound, new GUIContent("Unlock Sound"));
                    EditorGUILayout.PropertyField(sp_LockedTry, new GUIContent("Locked Try Sound"));
                }
                else
                {
                    EditorGUILayout.Space();
                    DrawTitle("Drwer Sounds");

                    EditorGUILayout.PropertyField(sp_Open, new GUIContent("Open Sound"));
                    EditorGUILayout.PropertyField(sp_Close, new GUIContent("Close Sound"));
                    EditorGUILayout.PropertyField(sp_UnlockSound, new GUIContent("Unlock Sound"));
                    EditorGUILayout.PropertyField(sp_LockedTry, new GUIContent("Locked Try Sound"));
                }

                EditorGUILayout.PropertyField(sp_Volume, new GUIContent("Sound Volume"));
            }
            else if (dynamicType == Type_Dynamic.Lever)
            {
                DrawTitle("Lever Events");
                EditorGUILayout.PropertyField(sp_InteractEvent, new GUIContent("Interact Event"));
                EditorGUILayout.PropertyField(sp_DisabledEvent, new GUIContent("Disabled Event"));
                EditorGUILayout.Space();

                DrawTitle("Lever Properties");
                EditorGUILayout.PropertyField(sp_lockOnUp, new GUIContent("Up Lock"));
                EditorGUILayout.PropertyField(sp_reverseMove, new GUIContent("Reverse Rotation"));
                if (int_type == Type_Interact.Mouse)
                {
                    EditorGUILayout.PropertyField(sp_stopAngle, new GUIContent("Lever Stop Angle"));
                    EditorGUILayout.PropertyField(sp_DebugAngle, new GUIContent("Debug Lever Angle"));
                }

                EditorGUILayout.Space();
                DrawTitle("Lever Sounds");

                EditorGUILayout.PropertyField(sp_LeverUpSound, new GUIContent("Lever Up Sound"));
                EditorGUILayout.PropertyField(sp_Volume, new GUIContent("Sound Volume"));
            }
            else if(dynamicType == Type_Dynamic.Valve)
            {
                DrawTitle("Valve Properties");

                EditorGUILayout.PropertyField(sp_valveTurnAxis, new GUIContent("Turn Axis"));
                EditorGUILayout.PropertyField(sp_valveTurnSpeed, new GUIContent("Turn Speed"));
                EditorGUILayout.PropertyField(sp_valveTurnTime, new GUIContent("Turn Time"));

                EditorGUILayout.Space();
                DrawTitle("Valve Sounds");

                EditorGUILayout.PropertyField(sp_valveTurnSounds, new GUIContent("Turn Sounds"), true);
                EditorGUILayout.PropertyField(sp_valveSoundAfter, new GUIContent("Turn Sound After"));
                EditorGUILayout.PropertyField(sp_Volume, new GUIContent("Turn Volume"));
            }
            else if (dynamicType == Type_Dynamic.MovableInteract)
            {
                DrawTitle("Movable Interact Properties");

                EditorGUILayout.PropertyField(sp_minMaxMove, new GUIContent("Min Max Move"));
                EditorGUILayout.PropertyField(sp_InteractPos, new GUIContent("Interact Position"));
                EditorGUILayout.PropertyField(sp_moveWithX, new GUIContent("Move With X"));
                EditorGUILayout.PropertyField(sp_reverseMove, new GUIContent("Reverse Move"));
            }

            /*
            switch (dynamic)
            {
                case Type_Dynamic.Door:
                    EditorGUILayout.PropertyField(sp_useType, new GUIContent("Use Type"));
                    EditorGUILayout.PropertyField(sp_interactType, new GUIContent("Interact Type"));

                    if (useType == Type_Use.Locked)
                    {
                        EditorGUILayout.PropertyField(sp_keyType, new GUIContent("Key Type"));

                        if (keyType == Type_Key.Inventory)
                        {
                            EditorGUILayout.PropertyField(sp_keyID, new GUIContent("KeyID"));
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(sp_IgnoreColliders, new GUIContent("Ignore Colliders"), true);

                    if (!HFPS_GameManager.LocalizationEnabled)
                    {
                        EditorGUILayout.PropertyField(sp_customText, new GUIContent("Locked Text"));
                    }
                    else
                    {
                        Rect rect = GUILayoutUtility.GetRect(1, 20);
                        EditorUtils.DrawLocaleSelector(rect, sp_customTextKey, new GUIContent("Locked Text Key"));
                    }

                    if (useType == Type_Use.Jammed)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Jammed Door Planks", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(sp_useJammedPlanks, new GUIContent("Use Planks"));
                        EditorGUILayout.PropertyField(sp_Planks, new GUIContent("Planks"), true);
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(sp_Animation, new GUIContent("Animation Object"));

                    if (int_type == Type_Interact.Animation)
                    {
                        EditorGUILayout.PropertyField(sp_useAnim, new GUIContent("Open Anim"));
                        EditorGUILayout.PropertyField(sp_backUseAnim, new GUIContent("Close Anim"));
                    }

                    EditorGUILayout.PropertyField(sp_knobAnim, new GUIContent("Knob Animation"));

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Door Properties", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(sp_Open, new GUIContent("Open Sound"));
                    EditorGUILayout.PropertyField(sp_Close, new GUIContent("Close Sound"));
                    EditorGUILayout.PropertyField(sp_UnlockSound, new GUIContent("Unlock Sound"));
                    EditorGUILayout.PropertyField(sp_LockedTry, new GUIContent("Locked Try Sound"));
                    EditorGUILayout.PropertyField(sp_Volume, new GUIContent("Sound Volume"));

                    if (int_type == Type_Interact.Mouse)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(sp_useSR, new GUIContent("Use Start Rotation"));
                        if (sp_useSR.boolValue)
                        {
                            EditorGUILayout.PropertyField(sp_startRot, new GUIContent("Starting Rotation"));
                        }

                        EditorGUILayout.PropertyField(sp_DebugAngle, new GUIContent("Debug Door Angle"));
                    }
                    break;
                case Type_Dynamic.Drawer:
                    EditorGUILayout.PropertyField(sp_useType, new GUIContent("Use Type"));
                    EditorGUILayout.PropertyField(sp_interactType, new GUIContent("Interact Type"));

                    if (useType == Type_Use.Locked)
                    {
                        EditorGUILayout.PropertyField(sp_keyType, new GUIContent("Key Type"));

                        if (keyType == Type_Key.Inventory)
                        {
                            EditorGUILayout.PropertyField(sp_keyID, new GUIContent("KeyID"));
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(sp_IgnoreColliders, new GUIContent("Ignore Colliders"), true);

                    if (!HFPS_GameManager.LocalizationEnabled)
                    {
                        EditorGUILayout.PropertyField(sp_customText, new GUIContent("Locked Text"));
                    }
                    else
                    {
                        Rect rect = GUILayoutUtility.GetRect(1, 20);
                        EditorUtils.DrawLocaleSelector(rect, sp_customTextKey, new GUIContent("Locked Text Key"));
                    }

                    if (int_type == Type_Interact.Animation)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(sp_Animation, new GUIContent("Animation Object"));
                        EditorGUILayout.PropertyField(sp_useAnim, new GUIContent("Open Anim"));
                        EditorGUILayout.PropertyField(sp_backUseAnim, new GUIContent("Close Anim"));
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Drawer Properties", EditorStyles.boldLabel);

                    if (int_type == Type_Interact.Mouse)
                    {
                        EditorGUILayout.PropertyField(sp_minMaxMove, new GUIContent("Min Max Move"));
                        EditorGUILayout.PropertyField(sp_moveWithX, new GUIContent("Move With X"));
                        EditorGUILayout.PropertyField(sp_reverseMove, new GUIContent("Reverse Move"));
                        EditorGUILayout.PropertyField(sp_drawerDragSounds, new GUIContent("Drag Sounds"));

                        if (sp_drawerDragSounds.boolValue)
                        {
                            EditorGUILayout.PropertyField(sp_drawerDragPlay, new GUIContent("Drag Play Value"));
                            EditorGUILayout.PropertyField(sp_Open, new GUIContent("Open Sound"));
                            EditorGUILayout.PropertyField(sp_Close, new GUIContent("Close Sound"));
                        }

                        EditorGUILayout.PropertyField(sp_UnlockSound, new GUIContent("Unlock Sound"));
                        EditorGUILayout.PropertyField(sp_LockedTry, new GUIContent("Locked Try Sound"));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(sp_Open, new GUIContent("Open Sound"));
                        EditorGUILayout.PropertyField(sp_Close, new GUIContent("Close Sound"));
                        EditorGUILayout.PropertyField(sp_UnlockSound, new GUIContent("Unlock Sound"));
                        EditorGUILayout.PropertyField(sp_LockedTry, new GUIContent("Locked Try Sound"));
                    }
                    EditorGUILayout.PropertyField(sp_Volume, new GUIContent("Sound Volume"));
                    break;
                case Type_Dynamic.Lever:
                    EditorGUILayout.PropertyField(sp_interactType, new GUIContent("Interact Type"));
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(sp_IgnoreColliders, new GUIContent("Ignore Colliders"), true);

                    if (int_type == Type_Interact.Animation)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(sp_Animation, new GUIContent("Animation Object"));
                        EditorGUILayout.PropertyField(sp_useAnim, new GUIContent("SwitchUp Anim"));
                        EditorGUILayout.PropertyField(sp_backUseAnim, new GUIContent("SwitchDown Anim"));
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Lever Properties", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(sp_InteractEvent, new GUIContent("Interact Event"));
                    EditorGUILayout.PropertyField(sp_DisabledEvent, new GUIContent("Disabled Event"));
                    EditorGUILayout.PropertyField(sp_LeverUpSound, new GUIContent("Lever Up Sound"));
                    EditorGUILayout.PropertyField(sp_lockOnUp, new GUIContent("Up Lock"));
                    EditorGUILayout.PropertyField(sp_reverseMove, new GUIContent("Reverse Rotation"));
                    if (int_type == Type_Interact.Mouse)
                    {
                        EditorGUILayout.PropertyField(sp_stopAngle, new GUIContent("Lever Stop Angle"));
                        EditorGUILayout.PropertyField(sp_DebugAngle, new GUIContent("Debug Lever Angle"));
                    }
                    EditorGUILayout.PropertyField(sp_Volume, new GUIContent("Sound Volume"));
                    break;
                case Type_Dynamic.Valve:
                    EditorGUILayout.PropertyField(sp_IgnoreColliders, new GUIContent("Ignore Colliders"), true);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(sp_InteractEvent, new GUIContent("Interact Event"));

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Valve Properties", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(sp_valveTurnAxis, new GUIContent("Turn Axis"));
                    EditorGUILayout.PropertyField(sp_valveTurnSounds, new GUIContent("Turn Sounds"), true);
                    EditorGUILayout.PropertyField(sp_Volume, new GUIContent("Turn Volume"));
                    EditorGUILayout.PropertyField(sp_valveSoundAfter, new GUIContent("Turn Sound After"));
                    EditorGUILayout.PropertyField(sp_valveTurnSpeed, new GUIContent("Turn Speed"));
                    EditorGUILayout.PropertyField(sp_valveTurnTime, new GUIContent("Turn Time"));
                    break;
                case Type_Dynamic.MovableInteract:
                    EditorGUILayout.PropertyField(sp_IgnoreColliders, new GUIContent("Ignore Colliders"), true);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(sp_InteractEvent, new GUIContent("Interact Event"));

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Movable Interact Properties", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(sp_minMaxMove, new GUIContent("Min Max Move"));
                    EditorGUILayout.PropertyField(sp_InteractPos, new GUIContent("Interact Position"));
                    EditorGUILayout.PropertyField(sp_moveWithX, new GUIContent("Move With X"));
                    EditorGUILayout.PropertyField(sp_reverseMove, new GUIContent("Reverse Move"));
                    break;
            }
            */

            serializedObject.ApplyModifiedProperties();
        }

        void DrawTitle(string title)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
        }
    }
}