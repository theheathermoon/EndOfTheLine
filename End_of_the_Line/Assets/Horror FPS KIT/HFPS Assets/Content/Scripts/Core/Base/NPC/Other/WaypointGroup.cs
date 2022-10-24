using System.Collections.Generic;
using UnityEngine;

namespace HFPS.Systems
{
    [ExecuteInEditMode]
    public class WaypointGroup : MonoBehaviour
    {
        public List<Waypoint> Waypoints = new List<Waypoint>();
        public Color WaypointsColor = Color.yellow;

        void Update()
        {
            if (transform.childCount < Waypoints.Count)
            {
                Waypoints.Clear();
            }

            if (transform.childCount > Waypoints.Count)
            {
                foreach (Transform t in transform)
                {
                    if (!t.gameObject.GetComponent<Waypoint>())
                    {
                        Waypoints.Add(t.gameObject.AddComponent<Waypoint>());
                    }
                    else
                    {
                        Waypoints.Add(t.gameObject.GetComponent<Waypoint>());
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            if (Waypoints.Count > 0)
            {
                Gizmos.color = WaypointsColor;

                foreach (Waypoint point in Waypoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawSphere(point.transform.position, 0.5f);
                    }
                }
            }
        }
    }
}