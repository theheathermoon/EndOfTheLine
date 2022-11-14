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
    public class MapDraw : MonoBehaviour
    {

        public GameObject marker;
        public GameObject player_icon;
        public bool enable_map_floor_color;

        private readonly int draw_ray_density = 12; // This parameter change field of view`s resolution

        private Ray see = new Ray();
        private RaycastHit see_hit;
        private int wall_mask;
        public Color map_print_color;
        private float draw_ray_diffusion;

        private Transform parent_for_markers;

        public GameObject[] markers;

        private Vector3[] hits_vectors;
        private int[] mesh_triangles;


        private MeshFilter mf;
        private MeshRenderer mr;
        private Mesh floor;

        private int ray_num = 0;

        void Start()
        {

            wall_mask = LayerMask.GetMask("Wall");

            parent_for_markers = new GameObject("Minimap Draw Parent").transform;

            draw_ray_diffusion = Mathf.Sqrt(draw_ray_density) * 2;

            GameObject icon = Instantiate(player_icon, gameObject.transform);
            icon.transform.SetParent(gameObject.transform);

            markers = new GameObject[(draw_ray_density * 2) + 1];

            for (int c = -draw_ray_density; c < draw_ray_density + 1; c++)
            {
                GameObject created_marker = Instantiate(marker, (new Vector3(0, 0, 0)), new Quaternion(0, 0, 0, 0));
                created_marker.transform.SetParent(parent_for_markers);
                markers[c + draw_ray_density] = created_marker;
            }

            floor = new Mesh();
            mf = gameObject.GetComponent<MeshFilter>();
            mf.mesh = floor;
            mr = gameObject.GetComponent<MeshRenderer>();

            hits_vectors = new Vector3[(draw_ray_density * 2) + 2];

            mesh_triangles = new int[draw_ray_density * 6];
            SetTriangles();
        }

        void SetTriangles()
        {
            int center_num = hits_vectors.Length - 1;
            int vertices_num = 0;

            for (int t = 3; t < mesh_triangles.Length; t += 3)
            {
                mesh_triangles[t - 3] = vertices_num;
                vertices_num++;
                mesh_triangles[t - 2] = vertices_num;
                mesh_triangles[t - 1] = center_num;
            }
        }

        private void Update()
        {

            foreach (Transform child in parent_for_markers.transform)
            {
                child.gameObject.SetActive(false);
            }


            if (enable_map_floor_color)
                hits_vectors[(draw_ray_density * 2) + 1] = transform.InverseTransformPoint( new Vector3(gameObject.transform.position.x, -995, gameObject.transform.position.z));

            see.origin = gameObject.transform.position;

            for (int n = -draw_ray_density; n < draw_ray_density + 1; n++)
            {

                ray_num = n + draw_ray_density;

                Vector3 direct = new Vector3();
                direct = (gameObject.transform.forward + gameObject.transform.right * n / draw_ray_diffusion);

                see.direction = direct;

                if (Physics.Raycast(see, out see_hit, 15, wall_mask))
                {
                    markers[ray_num].transform.position = new Vector3(see_hit.point.x, -990, see_hit.point.z);

                    if (see_hit.collider.gameObject.GetComponentInParent<DoorColor_onMap>() != null)
                    {
                        markers[ray_num].GetComponent<MeshRenderer>().material.color = see_hit.collider.gameObject.GetComponentInParent<DoorColor_onMap>().door_color_on_map;
                    }
                    else
                    {
                        markers[ray_num].GetComponent<MeshRenderer>().material.color = map_print_color;
                    }

                    markers[ray_num].gameObject.SetActive(true);

                    if (enable_map_floor_color)
                        hits_vectors[ray_num] = transform.InverseTransformPoint(markers[ray_num].transform.position + new Vector3 (0, -5, 0));
                }
                else
                {
                    if (enable_map_floor_color)
                    {
                        if (n > 0)
                        {
                            hits_vectors[ray_num] = hits_vectors[ray_num - 1];
                        }
                        else
                        {
                            hits_vectors[ray_num] = hits_vectors[ray_num + 1];
                        }
                    }
                }

                if (enable_map_floor_color)
                {
                    floor.vertices = hits_vectors;
                    floor.triangles = mesh_triangles;
                }
                else
                {
                    floor.vertices = null;
                    floor.triangles = null;
                }

            }

        }
    }
}
