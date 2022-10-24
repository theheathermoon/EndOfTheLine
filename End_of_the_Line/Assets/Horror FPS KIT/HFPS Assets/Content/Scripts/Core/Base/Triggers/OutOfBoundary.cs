using UnityEngine;
using HFPS.Player;

namespace HFPS.Systems
{
    public class OutOfBoundary : MonoBehaviour
    {
        public Vector3 objTeleportOffset;

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.GetComponent<PlayerController>())
            {
                other.transform.root.GetComponent<HealthManager>().InstantDeath();
            }

            if (other.transform.GetComponent<InteractiveItem>())
            {
                Vector3 pos = other.transform.GetComponent<InteractiveItem>().lastFloorPosition;
                pos += objTeleportOffset;

                other.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                other.transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

                other.transform.position = pos;
            }
        }
    }
}