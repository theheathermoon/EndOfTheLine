using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenerator
{
    public class ApplyMinimapTexture : MonoBehaviour
    {
        private MeshRenderer meshren;

        void Start()
        {
            meshren = gameObject.GetComponent<MeshRenderer>();
            meshren.material.mainTexture = CreateMinimap.player_map_texture;
        }
    }
}
