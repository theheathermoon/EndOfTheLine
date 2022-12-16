
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aura2API
{
    /// <summary>
    /// Collection of functions related to the Aura system
    /// </summary>
    public static class Aura
    {
        #region Private members
        /// <summary>
        /// Collection of all referenced resources required to make aura2 work
        /// </summary>
        private static AuraResourcesCollection _auraResourcesCollection;
        /// <summary>
        /// Tells if the compatibility check has already been done
        /// </summary>
        private static bool _hasCheckedCompatibility;
        /// <summary>
        /// Checks if the environment is able to run Aura 2
        /// </summary>
        private static bool _isCompatible;
        #endregion

        #region Properties
        /// <summary>
        /// Checks if the environment is able to run Aura 2
        /// </summary>
        /// <returns>True if the environment is able to run Aura 2, false otherwise</returns>
        public static bool IsCompatible
        {
            get
            {
                if (!_hasCheckedCompatibility)
                {
                    _isCompatible = true;
                    List<string> errorReasons = new List<string>(); // should be replaced with a bitfield but it's overkill now

#if UNITY_2017_2_OR_NEWER
                    if (!SystemInfo.supports2DArrayTextures)
                    {
                        _isCompatible = false;
                        errorReasons.Add("2D Texture Arrays not supported");
                    }

                    if (!SystemInfo.supports3DTextures)
                    {
                        _isCompatible = false;
                        errorReasons.Add("3D Textures not supported");
                    }

                    if (!SystemInfo.supports3DRenderTextures)
                    {
                        _isCompatible = false;
                        errorReasons.Add("3D Render Textures not supported");
                    }

                    if (!SystemInfo.supportsComputeShaders)
                    {
                        _isCompatible = false;
                        errorReasons.Add("Compute Shaders not supported");
                    }

                    if (!SystemInfo.supportsRawShadowDepthSampling)
                    {
                        _isCompatible = false;
                        errorReasons.Add("Raw Shadow Maps not supported");
                    }
#else
                    _isCompatible = false;
                    errorReasons.Add("Unity version is lower than 2017.2");
#endif

                    if (!_isCompatible)
                    {
                        string errorMessage = "Aura 2 is not supported for this target";
#if UNITY_EDITOR
                        errorMessage += " (" + (BuildPipeline.isBuildingPlayer ? ("building for " + EditorUserBuildSettings.activeBuildTarget.ToString()) : Application.platform.ToString()) + ")";
#else
                        errorMessage += " (" + Application.platform.ToString() + ")";
#endif
                        errorMessage += " and will be disabled.";
                        errorMessage += "\nReason" + (errorReasons.Count > 1 ? "s" : "") + " : ";
                        for (int i = 0; i < errorReasons.Count; ++i)
                        {
                            errorMessage += errorReasons[i];
                            if(i != (errorReasons.Count - 1))
                            {
                                errorMessage += ", ";
                            }
                        }

                        Debug.LogWarning(errorMessage);
                    }

                    _hasCheckedCompatibility = true;
                }
                
                return _isCompatible;
            }
        }

        /// <summary>
        /// Collection of all referenced resources required to make aura work
        /// </summary>
        public static AuraResourcesCollection ResourcesCollection
        {
            get
            {
                if (_auraResourcesCollection == null)
                {
                    _auraResourcesCollection = Resources.Load<AuraResourcesCollection>("AuraResourcesCollection");
                }

                return _auraResourcesCollection;
            }
        }
#endregion

#region Functions
        /// <summary>
        /// Returns an array with all the aura cameras
        /// </summary>
        /// <param name="minAmount">Creates default Aura cameras to reach the minimum amount</param>
        /// <returns>An array with the aura cameras</returns>
        public static AuraCamera[] GetAuraCameras(int minAmount = 0)
        {
            AuraCamera[] auraCameras = AuraCamera.FindObjectsOfType<AuraCamera>();
            if (minAmount > auraCameras.Length)
            {
                AuraCamera[] newCameras = new AuraCamera[minAmount - auraCameras.Length];
                for(int i = 0; i < newCameras.Length; ++i)
                {
                    newCameras[i] = AuraCamera.CreateGameObject("Aura Camera").GetComponent<AuraCamera>();
                }

                auraCameras = auraCameras.Append(newCameras);
            }

            return auraCameras;
        }

        /// <summary>
        /// Returns an array with all the aura volumes
        /// </summary>
        /// <returns>An array with the aura volumes</returns>
        public static AuraVolume[] GetAuraVolumes()
        {
            return AuraVolume.FindObjectsOfType<AuraVolume>();
        }

        /// <summary>
        /// Returns an array with all the aura volumes matching the type parameter
        /// </summary>
        /// <param name="type">The wanted type of volume</param>
        /// <returns>An array with the aura volumes</returns>
        public static AuraVolume[] SortOutByType(this AuraVolume[] volumes, VolumeType type)
        {
            return volumes.Where(x => x.volumeShape.shape == type).ToArray();
        }

        /// <summary>
        /// Returns an array with all the aura lights
        /// </summary>
        /// <returns>An array with the aura lights</returns>
        public static AuraLight[] GetAuraLights()
        {
            return AuraLight.FindObjectsOfType<AuraLight>();
        }

        /// <summary>
        /// Returns an array with all the aura light matching the type parameter
        /// </summary>
        /// <param name="type">The wanted type of light</param>
        /// <returns>An array with the aura lights</returns>
        public static AuraLight[] SortOutByType(this AuraLight[] auraLights, LightType type)
        {
            return auraLights.Where(x => x.Type == type).ToArray();
        }

        /// <summary>
        /// Returns an array with all the aura light matching the type parameter
        /// </summary>
        /// <param name="type">The wanted type of light</param>
        /// <param name="minAmount">Creates default Aura cameras to reach the minimum amount</param>
        /// <returns>An array with the aura lights</returns>
        public static AuraLight[] GetAuraLights(LightType type, int minAmount = 0)
        {
            AuraLight[] auraLights = GetAuraLights();
            AuraLight[] sortedLights = auraLights.SortOutByType(type);
            if (minAmount > sortedLights.Length)
            {
                AuraLight[] newLights = new AuraLight[minAmount - sortedLights.Length];
                for (int i = 0; i < newLights.Length; ++i)
                {
                    newLights[i] = AuraLight.CreateGameObject("Aura " + type + " Light", type).GetComponent<AuraLight>();
                }

                sortedLights = sortedLights.Append(newLights);
            }

            return sortedLights;
        }

        /// <summary>
        /// Applies a preset to the Aura system
        /// </summary>
        /// <param name="preset">The desired preset</param>
        public static void ApplyPreset(Presets preset)
        {
            AuraPreset.ApplyPreset(preset);
        }

#region Add Aura to GameObjects
        /// <summary>
        /// Adds the Aura component to all the Cameras
        /// </summary>
        /// <param name="amountToCreateIfNone">Creates an amount of default Aura cameras if there is none found</param>
        /// <returns>An array with the aura cameras</returns>
        public static AuraCamera[] AddAuraToCameras(int amountToCreateIfNone = 0)
        {
            Camera[] camerasArray = GameObject.FindObjectsOfType<Camera>();
            for (int i = 0; i < camerasArray.Length; ++i)
            {
                if (camerasArray[i].GetComponent<AuraCamera>() == null)
                {
                    camerasArray[i].gameObject.AddComponent<AuraCamera>();
                }
            }

            return GetAuraCameras(amountToCreateIfNone);
        }

        /// <summary>
        /// Adds the Aura component to all the Directional Lights
        /// </summary>
        /// <param name="amountToCreateIfNone">Creates an amount of default Aura Directional Light if there is none found</param>
        /// <returns>An array with the aura directional lights</returns>
        public static AuraLight[] AddAuraToDirectionalLights(int amountToCreateIfNone = 0)
        {
            Light[] lightsArray = GameObject.FindObjectsOfType<Light>();
            for (int i = 0; i < lightsArray.Length; ++i)
            {
                if (lightsArray[i].GetComponent<AuraLight>() == null)
                {
                    if (lightsArray[i].type == LightType.Directional)
                    {
                        lightsArray[i].gameObject.AddComponent<AuraLight>();
                    }
                }
            }

            return GetAuraLights(LightType.Directional, amountToCreateIfNone);
        }

        /// <summary>
        /// Adds the Aura component to all the Spot Lights
        /// </summary>
        /// <param name="amountToCreateIfNone">Creates an amount of default Aura Spot Light if there is none found</param>
        /// <returns>An array with the aura spot lights</returns>
        public static AuraLight[] AddAuraToSpotLights(int amountToCreateIfNone = 0)
        {
            Light[] lightsArray = GameObject.FindObjectsOfType<Light>();
            for (int i = 0; i < lightsArray.Length; ++i)
            {
                if (lightsArray[i].GetComponent<AuraLight>() == null)
                {
                    if (lightsArray[i].type == LightType.Spot)
                    {
                        lightsArray[i].gameObject.AddComponent<AuraLight>();
                    }
                }
            }

            return GetAuraLights(LightType.Spot, amountToCreateIfNone);
        }

        /// <summary>
        /// Adds the Aura component to all the Point Lights
        /// </summary>
        /// <param name="amountToCreateIfNone">Creates an amount of default Aura Point Light if there is none found</param>
        /// <returns>An array with the aura point lights</returns>
        public static AuraLight[] AddAuraToPointLights(int amountToCreateIfNone = 0)
        {
            Light[] lightsArray = GameObject.FindObjectsOfType<Light>();
            for (int i = 0; i < lightsArray.Length; ++i)
            {
                if (lightsArray[i].GetComponent<AuraLight>() == null)
                {
                    if (lightsArray[i].type == LightType.Point)
                    {
                        lightsArray[i].gameObject.AddComponent<AuraLight>();
                    }
                }
            }

            return GetAuraLights(LightType.Point, amountToCreateIfNone);
        }
#endregion
#endregion
    }
}