using UnityEngine;
using HFPS.Player;

namespace HFPS.Systems
{
    public class WaterPlayerExit : MonoBehaviour
    {
        public enum LerpWith { PlayerX, PlayerZ }
        private PlayerController player;

        public LerpWith lerpWith = LerpWith.PlayerX;
        public Vector3 LerpOffset;

        void Awake()
        {
            player = PlayerController.Instance;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && player && player.isInWater)
            {
                Vector3 newPosition = transform.position + LerpOffset;

                if (lerpWith == LerpWith.PlayerX)
                {
                    newPosition.x = player.transform.position.x;
                }
                else if (lerpWith == LerpWith.PlayerZ)
                {
                    newPosition.z = player.transform.position.z;
                }

                player.LerpPlayer(newPosition, Vector2.zero, false);
                player.isInWater = false;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position + LerpOffset, 0.1f);
        }
    }
}