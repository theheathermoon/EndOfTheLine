using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Utility;

namespace HFPS.Systems
{
    public enum SurfaceID { Tag, Texture, TexOrTag }

    /// <summary>
    /// Contains all Required Surface Details.
    /// </summary>
    public class SurfaceDetailsScriptable : ScriptableObject
    {
        public List<SurfaceDetails> surfaceDetails = new List<SurfaceDetails>();

        /// <summary>
        /// Get Surface Details which contains a specified Tag.
        /// </summary>
        public SurfaceDetails GetSurfaceDetails(string Tag)
        {
            if (surfaceDetails.Any(x => !string.IsNullOrEmpty(x.SurfaceProperties.SurfaceTag) && x.SurfaceProperties.SurfaceTag.Equals(Tag)))
            {
                return surfaceDetails.SingleOrDefault(x => x.SurfaceProperties.SurfaceTag.Equals(Tag));
            }

            return default;
        }

        /// <summary>
        /// Get Surface Details which contains a specified Texture.
        /// </summary>
        public SurfaceDetails GetSurfaceDetails(Texture2D texture)
        {
            if (surfaceDetails.Any(x => x.SurfaceProperties.SurfaceTextures.Length > 0 && x.SurfaceProperties.SurfaceTextures.Any(y => y == texture)))
            {
                return surfaceDetails.SingleOrDefault(x => x.SurfaceProperties.SurfaceTextures.Any(y => y == texture));
            }

            return default;
        }

        /// <summary>
        /// Get Surface Details which contains any of a specified Textures.
        /// </summary>
        public SurfaceDetails GetSurfaceDetails(Texture2D[] textures)
        {
            if (surfaceDetails.Any(x => x.SurfaceProperties.SurfaceTextures.Length > 0 && x.SurfaceProperties.SurfaceTextures.Any(y => textures.Any(z => y == z))))
            {
                return surfaceDetails.SingleOrDefault(x => x.SurfaceProperties.SurfaceTextures.Any(y => textures.Any(z => y == z)));
            }

            return default;
        }

        /// <summary>
        /// Get Surface Details which contains Texture or Tag in specified GameObject.
        /// </summary>
        public SurfaceDetails GetSurfaceDetails(GameObject gameObject, SurfaceID surface)
        {
            MeshRenderer meshRenderer;
            SurfaceDetails details;

            if ((surface == SurfaceID.Texture || surface == SurfaceID.TexOrTag) && (meshRenderer = gameObject.GetComponent<MeshRenderer>()) != null)
            {
                Texture2D[] textures = meshRenderer.materials.Select(x => x.mainTexture).Cast<Texture2D>().ToArray();

                if ((details = GetSurfaceDetails(textures)) != null)
                {
                    return details;
                }
            }
            else if ((surface == SurfaceID.Tag || surface == SurfaceID.TexOrTag) && (details = GetSurfaceDetails(gameObject.tag)) != null)
            {
                return details;
            }

            return default;
        }

        /// <summary>
        /// Get Surface Details at Terrain Position.
        /// </summary>
        public SurfaceDetails GetTerrainSurfaceDetails(Terrain terrain, Vector3 worldPos)
        {
            SurfaceDetails details;
            Texture2D terrainLayer = Utilities.TerrainPosToTex(terrain, worldPos);

            if(terrainLayer == null) 
                throw new Exception("The terrain must have at least one terrain layer assigned to it.");

            if ((details = GetSurfaceDetails(terrainLayer)) != null)
                return details;

            return default;
        }

        /// <summary>
        /// Get Footstep Sounds Array of the specified Surface.
        /// </summary>
        /// <param name="type">Terrain or GameObject Type.</param>
        /// <param name="target">Position of the Player.</param>
        /// <param name="surfaceID">Surface Type ID.</param>
        /// <returns>Array of Surface Footstep Sounds.</returns>
        public AudioClip[] GetSurfaceFootsteps(object type, Transform player, SurfaceID surfaceID)
        {
            if (type == null || (type is Terrain is false && type is GameObject is false))
                throw new ArgumentException($"You have entered wrong type '{type.GetType().Name}'.");

            if (surfaceDetails.Count > 0)
            {
                SurfaceDetails surface = type.GetType() == typeof(Terrain) ?
                    GetTerrainSurfaceDetails((Terrain)type, player.position) :
                    GetSurfaceDetails((GameObject)type, surfaceID);

                if (surface != null)
                {
                    if (surface != null && surface.FootstepProperties.SurfaceFootsteps.Length > 0 && surface.SurfaceProperties.AllowFootsteps)
                    {
                        return surface.FootstepProperties.SurfaceFootsteps;
                    }
                }
            }

            return new AudioClip[0];
        }
    }

    [Serializable]
    public sealed class SurfaceDetails
    {
        public string SurfaceName;

        public Surface SurfaceProperties = new Surface();
        public Footsteps FootstepProperties = new Footsteps();
        public ImpactMark ImpactProperties = new ImpactMark();

        [Serializable]
        public sealed class Surface
        {
            public Texture2D[] SurfaceTextures;
            public string SurfaceTag;
            public bool AllowFootsteps = true;
            public bool AllowImpactMark = true;
        }

        [Serializable]
        public sealed class Footsteps
        {
            public AudioClip[] SurfaceFootsteps;
            public float FootstepsModifier = 1f;
        }

        [Serializable]
        public sealed class ImpactMark
        {
            public GameObject[] SurfaceBulletmarks;
            public AudioClip[] BulletmarkImpacts;

            public GameObject[] SurfaceMeleemarks;
            public AudioClip[] MeleemarkImpacts;

            public float ImpactVolume = 1f;
        }

        public bool HasFootsteps() => FootstepProperties.SurfaceFootsteps.Length > 0;
        public bool HasBulletmarks() => ImpactProperties.SurfaceBulletmarks.Length > 0;
        public bool HasBulletImpacts() => ImpactProperties.BulletmarkImpacts.Length > 0;
        public bool HasMeleemarks() => ImpactProperties.SurfaceMeleemarks.Length > 0;
        public bool HasMeleeImpacts() => ImpactProperties.MeleemarkImpacts.Length > 0;

        /// <summary>
        /// Pick Random Surface Bulletmark Object
        /// </summary>
        public GameObject Bulletmark() => ImpactProperties.SurfaceBulletmarks.Random();

        /// <summary>
        /// Pick Random Surface Bullet Impact Sound
        /// </summary>
        public AudioClip BulletImpact() => ImpactProperties.BulletmarkImpacts.Random();

        /// <summary>
        /// Pick Random Surface Meleemark Object
        /// </summary>
        public GameObject Meleemark() => ImpactProperties.SurfaceMeleemarks.Random();

        /// <summary>
        /// Pick Random Surface Melee Impact Sound
        /// </summary>
        public AudioClip MeleeImpact() => ImpactProperties.MeleemarkImpacts.Random();
    }
}