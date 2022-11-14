///////////////////////////////////
/// Create and edit by QerO
/// 05.2019
/// lidan-357@mail.ru
///////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenerator
{
    public class CreateMinimap : MonoBehaviour
    {

        public Camera map_draw_camera;
        public Camera map_player_camera;
        public int map_texture_size;
        public MeshRenderer game_map;
        private RenderTexture game_map_texture;
        // private RenderTexture player_map_texture;
        public static RenderTexture player_map_texture;


        void Awake()
        {
            game_map.enabled = true;

            game_map_texture = new RenderTexture(map_texture_size, map_texture_size, 24, RenderTextureFormat.ARGB32);
            player_map_texture = new RenderTexture(map_texture_size, map_texture_size, 24, RenderTextureFormat.ARGB32);
            map_draw_camera.targetTexture = game_map_texture;
            map_player_camera.targetTexture = player_map_texture;
            game_map.material.mainTexture = game_map_texture;
        }
    }
}
