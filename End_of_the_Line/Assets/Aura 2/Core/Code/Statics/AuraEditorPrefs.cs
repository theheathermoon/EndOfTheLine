
/***************************************************************************
*                                                                          *
*  Copyright (c) Raphaël Ernaelsten (@RaphErnaelsten)                      *
*  All Rights Reserved.                                                    *
*                                                                          *
*  NOTICE: Aura 2 is a commercial project.                                 * 
*  All information contained herein is, and remains the property of        *
*  Raphaël Ernaelsten.                                                     *
*  The intellectual and technical concepts contained herein are            *
*  proprietary to Raphaël Ernaelsten and are protected by copyright laws.  *
*  Dissemination of this information or reproduction of this material      *
*  is strictly forbidden.                                                  *
*                                                                          *
***************************************************************************/

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Aura2API
{
    /// <summary>
    /// Collection of accessors/functions related to Aura editor preferences
    /// </summary>
    /// 
    [InitializeOnLoad]
    public class AuraEditorPrefs
    {
        #region Private Members
        private static Dictionary<string, bool> _boolDictionary;
        private const string _displayMainIntroductionScreenString = "AURA2_DisplayMainIntroductionScreen";
        private const string _displayCameraIntroductionScreenString = "AURA2_DisplayCameraIntroductionScreen";
        private const string _displayLightIntroductionScreenString = "AURA2_DisplayLightIntroductionScreen";
        private const string _displayVolumeIntroductionScreenString = "AURA2_DisplayVolumeIntroductionScreen";
        private const string _displayToolboxString = "AURA2_DisplayToolbox";
        private const string _expandToolboxString = "AURA2_ExpandToolbox";
        private const string _showToolboxNotificationsString = "AURA2_ShowToolboxNotifications";
        private const string _enableToolboxAnimationsString = "AURA2_EnableToolboxAnimations";
        private const string _displayCameraSlicesInEditionString = "AURA2_DisplayCameraSlicesInEdition";
        private const string _displayDebugPanelInToolboxString = "AURA2_DisplayDebugPanelInToolbox";
        private const string _enableAuraInSceneViewString = "AURA2_EnableAuraInSceneView";
        private const string _displayAuraGuiInParentComponentsString = "AURA2_DisplayAuraGuiInParentComponents";
        private const string _displayGizmosWhenSelectedString = "AURA2_DisplayGizmosWhenSelected";
        private const string _displayGizmosWhenUnselectedString = "AURA2_DisplayGizmosWhenUnselected";
        private const string _displayGizmosOnCamerasString = "AURA2_DisplayGizmosOnCameras";
        private const string _displayGizmosOnLightsString = "AURA2_DisplayGizmosOnLights";
        private const string _displayGizmosOnVolumesString = "AURA2_DisplayGizmosOnVolumes";
        private const string _displayButtonsInHierarchyString = "AURA2_DisplayButtonsInHierarchy";
        private const string _displayPreviewButtonInSceneViewString = "AURA2_DisplayPreviewButtonInSceneView";

        private static Dictionary<string, int> _intDictionary;
        private const string _toolboxPositionString = "AURA2_ToolboxPosition";
        private const string _toolboxPresetsPreviewsPerRowString = "AURA2_ToolboxPresetsPreviewsPerRow";
        #endregion

        static AuraEditorPrefs()
        {
            _boolDictionary = new Dictionary<string, bool>();
            _boolDictionary.Add(    _displayMainIntroductionScreenString,       EditorPrefs.GetBool(    _displayMainIntroductionScreenString,       true));
            _boolDictionary.Add(    _displayCameraIntroductionScreenString,     EditorPrefs.GetBool(    _displayCameraIntroductionScreenString,     true));
            _boolDictionary.Add(    _displayLightIntroductionScreenString,      EditorPrefs.GetBool(    _displayLightIntroductionScreenString,      true));
            _boolDictionary.Add(    _displayVolumeIntroductionScreenString,     EditorPrefs.GetBool(    _displayVolumeIntroductionScreenString,     true));
            _boolDictionary.Add(    _displayToolboxString,                      EditorPrefs.GetBool(    _displayToolboxString,                      true));
            _boolDictionary.Add(    _expandToolboxString,                       EditorPrefs.GetBool(    _expandToolboxString,                       true));
            _boolDictionary.Add(    _showToolboxNotificationsString,            EditorPrefs.GetBool(    _showToolboxNotificationsString,            true));
            _boolDictionary.Add(    _enableToolboxAnimationsString,             EditorPrefs.GetBool(    _enableToolboxAnimationsString,             true));
            _boolDictionary.Add(    _displayCameraSlicesInEditionString,        EditorPrefs.GetBool(    _displayCameraSlicesInEditionString,        false));
            _boolDictionary.Add(    _displayDebugPanelInToolboxString,          EditorPrefs.GetBool(    _displayDebugPanelInToolboxString,          false));
            _boolDictionary.Add(    _enableAuraInSceneViewString,               EditorPrefs.GetBool(    _enableAuraInSceneViewString,               true));
            _boolDictionary.Add(    _displayAuraGuiInParentComponentsString,    EditorPrefs.GetBool(    _displayAuraGuiInParentComponentsString,    false));
            _boolDictionary.Add(    _displayGizmosWhenSelectedString,           EditorPrefs.GetBool(    _displayGizmosWhenSelectedString,           true));
            _boolDictionary.Add(    _displayGizmosWhenUnselectedString,         EditorPrefs.GetBool(    _displayGizmosWhenUnselectedString,         false));
            _boolDictionary.Add(    _displayGizmosOnCamerasString,              EditorPrefs.GetBool(    _displayGizmosOnCamerasString,              true));
            _boolDictionary.Add(    _displayGizmosOnLightsString,               EditorPrefs.GetBool(    _displayGizmosOnLightsString,               true));
            _boolDictionary.Add(    _displayGizmosOnVolumesString,              EditorPrefs.GetBool(    _displayGizmosOnVolumesString,              true));
            _boolDictionary.Add(    _displayButtonsInHierarchyString,           EditorPrefs.GetBool(    _displayButtonsInHierarchyString,           true));
            _boolDictionary.Add(    _displayPreviewButtonInSceneViewString,     EditorPrefs.GetBool(    _displayPreviewButtonInSceneViewString,     true));

            _intDictionary = new Dictionary<string, int>();
            _intDictionary.Add(     _toolboxPositionString,                     EditorPrefs.GetInt(     _toolboxPositionString,                 0));
            _intDictionary.Add(     _toolboxPresetsPreviewsPerRowString,        EditorPrefs.GetInt(     _toolboxPresetsPreviewsPerRowString,    2));
        }

        #region Properties
        #region Bools
        public static bool DisplayMainIntroductionScreen
        {
            get
            {
                return _boolDictionary[_displayMainIntroductionScreenString];
            }
            set
            {
                _boolDictionary[_displayMainIntroductionScreenString] = value;
                EditorPrefs.SetBool(_displayMainIntroductionScreenString, value);
            }
        }

        public static bool DisplayCameraIntroductionScreen
        {
            get
            {
                return _boolDictionary[_displayCameraIntroductionScreenString];
            }
            set
            {
                _boolDictionary[_displayCameraIntroductionScreenString] = value;
                EditorPrefs.SetBool(_displayCameraIntroductionScreenString, value);
            }
        }

        public static bool DisplayLightIntroductionScreen
        {
            get
            {
                return _boolDictionary[_displayLightIntroductionScreenString];
            }
            set
            {
                _boolDictionary[_displayLightIntroductionScreenString] = value;
                EditorPrefs.SetBool(_displayLightIntroductionScreenString, value);
            }
        }

        public static bool DisplayVolumeIntroductionScreen
        {
            get
            {
                return _boolDictionary[_displayVolumeIntroductionScreenString];
            }
            set
            {
                _boolDictionary[_displayVolumeIntroductionScreenString] = value;
                EditorPrefs.SetBool(_displayVolumeIntroductionScreenString, value);
            }
        }

        public static bool DisplayToolbox
        {
            get
            {
                return _boolDictionary[_displayToolboxString];
            }
            set
            {
                _boolDictionary[_displayToolboxString] = value;
                EditorPrefs.SetBool(_displayToolboxString, value);
            }
        }

        public static bool ExpandToolbox
        {
            get
            {
                return _boolDictionary[_expandToolboxString];
            }
            set
            {
                _boolDictionary[_expandToolboxString] = value;
                EditorPrefs.SetBool(_expandToolboxString, value);
            }
        }

        public static bool ShowToolboxNotifications
        {
            get
            {
                return _boolDictionary[_showToolboxNotificationsString];
            }
            set
            {
                _boolDictionary[_showToolboxNotificationsString] = value;
                EditorPrefs.SetBool(_showToolboxNotificationsString, value);
            }
        }

        public static bool EnableToolboxAnimations
        {
            get
            {
                return _boolDictionary[_enableToolboxAnimationsString];
            }
            set
            {
                _boolDictionary[_enableToolboxAnimationsString] = value;
                EditorPrefs.SetBool(_enableToolboxAnimationsString, value);
            }
        }

        public static bool DisplayCameraSlicesInEdition
        {
            get
            {
                return _boolDictionary[_displayCameraSlicesInEditionString];
            }
            set
            {
                _boolDictionary[_displayCameraSlicesInEditionString] = value;
                EditorPrefs.SetBool(_displayCameraSlicesInEditionString, value);
            }
        }

        public static bool DisplayDebugPanelInToolbox
        {
            get
            {
                return _boolDictionary[_displayDebugPanelInToolboxString];
            }
            set
            {
                _boolDictionary[_displayDebugPanelInToolboxString] = value;
                EditorPrefs.SetBool(_displayDebugPanelInToolboxString, value);
            }
        }

        public static bool EnableAuraInSceneView
        {
            get
            {
                return _boolDictionary[_enableAuraInSceneViewString];
            }
            set
            {
                _boolDictionary[_enableAuraInSceneViewString] = value;
                EditorPrefs.SetBool(_enableAuraInSceneViewString, value);
            }
        }

        public static bool DisplayAuraGuiInParentComponents
        {
            get
            {
                return _boolDictionary[_displayAuraGuiInParentComponentsString];
            }
            set
            {
                _boolDictionary[_displayAuraGuiInParentComponentsString] = value;
                EditorPrefs.SetBool(_displayAuraGuiInParentComponentsString, value);
            }
        }

        public static bool DisplayGizmosWhenSelected
        {
            get
            {
                return _boolDictionary[_displayGizmosWhenSelectedString];
            }
            set
            {
                _boolDictionary[_displayGizmosWhenSelectedString] = value;
                EditorPrefs.SetBool(_displayGizmosWhenSelectedString, value);
            }
        }

        public static bool DisplayGizmosWhenUnselected
        {
            get
            {
                return _boolDictionary[_displayGizmosWhenUnselectedString];
            }
            set
            {
                _boolDictionary[_displayGizmosWhenUnselectedString] = value;
                EditorPrefs.SetBool(_displayGizmosWhenUnselectedString, value);
            }
        }

        public static bool DisplayGizmosOnCameras
        {
            get
            {
                return _boolDictionary[_displayGizmosOnCamerasString];
            }
            set
            {
                _boolDictionary[_displayGizmosOnCamerasString] = value;
                EditorPrefs.SetBool(_displayGizmosOnCamerasString, value);
            }
        }

        public static bool DisplayGizmosOnLights
        {
            get
            {
                return _boolDictionary[_displayGizmosOnLightsString];
            }
            set
            {
                _boolDictionary[_displayGizmosOnLightsString] = value;
                EditorPrefs.SetBool(_displayGizmosOnLightsString, value);
            }
        }

        public static bool DisplayGizmosOnVolumes
        {
            get
            {
                return _boolDictionary[_displayGizmosOnVolumesString];
            }
            set
            {
                _boolDictionary[_displayGizmosOnVolumesString] = value;
                EditorPrefs.SetBool(_displayGizmosOnVolumesString, value);
            }
        }

        public static bool DisplayButtonsInHierarchy
        {
            get
            {
                return _boolDictionary[_displayButtonsInHierarchyString];
            }
            set
            {
                _boolDictionary[_displayButtonsInHierarchyString] = value;
                EditorPrefs.SetBool(_displayButtonsInHierarchyString, value);
            }
        }

        public static bool DisplayPreviewButtonInSceneView
        {
            get
            {
                return _boolDictionary[_displayPreviewButtonInSceneViewString];
            }
            set
            {
                _boolDictionary[_displayPreviewButtonInSceneViewString] = value;
                EditorPrefs.SetBool(_displayPreviewButtonInSceneViewString, value);
            }
        } 
        #endregion

        #region Ints
        public static int ToolboxPosition
        {
            get
            {
                return _intDictionary[_toolboxPositionString];
            }
            set
            {
                _intDictionary[_toolboxPositionString] = value;
                EditorPrefs.SetInt(_toolboxPositionString, value);
            }
        }

        public static int ToolboxPresetsPreviewsPerRow
        {
            get
            {
                return _intDictionary[_toolboxPresetsPreviewsPerRowString];
            }
            set
            {
                _intDictionary[_toolboxPresetsPreviewsPerRowString] = value;
                EditorPrefs.SetInt(_toolboxPresetsPreviewsPerRowString, value);
            }
        } 
        #endregion
        #endregion
    }
}
#endif
