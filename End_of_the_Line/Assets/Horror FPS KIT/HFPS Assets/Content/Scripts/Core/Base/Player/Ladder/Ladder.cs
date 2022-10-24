using UnityEngine;
using HFPS.Player;

namespace HFPS.Systems
{
    public class Ladder : MonoBehaviour
    {
        private PlayerController player;
        private MouseLook mouseLook;

        [Header("Ladder Look Rotation")]
        public Vector2 LadderLook;

        [Header("Ladder")]
        public Vector3 ladderCenter;
        public Vector3 LadderCenter => transform.TransformPoint(ladderCenter);

        public Vector3 ladderUp;
        public Vector3 LadderUp => transform.TransformPoint(ladderCenter + ladderUp);

        public Vector3 ladderExit;
        public Vector3 LadderExit => transform.TransformPoint(ladderCenter + ladderUp + ladderExit);

        public Collider Collider => GetComponent<Collider>();

        void Awake()
        {
            player = PlayerController.Instance;
            mouseLook = ScriptManager.Instance.GetComponent<MouseLook>();
        }

        public void UseObject()
        {
            if (!player) return;

            Vector2 rotation = LadderLook;
            rotation.x -= mouseLook.playerOriginalRotation.eulerAngles.y;

            if(LadderDotUp() > 0)
            {
                Vector3 ladderEnter = LadderCenter;
                ladderEnter.y = player.transform.position.y + Physics.defaultContactOffset;

                if (!CheckForObstacles(ladderEnter))
                {
                    player.UseLadder(this, rotation, true);
                    Collider.enabled = false;
                }
            }
            else
            {
                Vector3 ladderEnter = LadderUp;
                ladderEnter.y -= 0.5f;

                if (!CheckForObstacles(ladderEnter))
                {
                    player.UseLadder(this, rotation, false);
                    Collider.enabled = false;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(LadderCenter, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(LadderUp, 0.05f);

            if (PlayerController.HasReference)
            {
                PlayerController playerController = PlayerController.Instance;

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(LadderExit, 0.05f);

                Vector3 exit = LadderExit;
                exit.y += playerController.CharacterControl.skinWidth +
                    (playerController.controllerAdjustments.normalHeight / 2);

                Gizmos.DrawSphere(exit, 0.05f);
                Gizmos.DrawLine(LadderExit, exit);

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, 0.2f);

                Vector2 rotation = LadderLook;
                rotation.x -= playerController.transform.eulerAngles.y;

                Vector3 gizmoRotation = Quaternion.Euler(new Vector3(0, rotation.x, rotation.y)) * playerController.transform.forward * 1f;

                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, gizmoRotation);
            }
        }

        float LadderDotUp()
        {
            Vector3 ladderPos = LadderExit;
            Vector3 playerPos = player.transform.position;
            Vector3 p1 = new Vector3(0, playerPos.y, 0);
            Vector3 p2 = new Vector3(0, ladderPos.y, 0);
            Vector3 dir = p2 - p1;
            return Vector3.Dot(dir.normalized, Vector3.up);
        }

        bool CheckForObstacles(Vector3 position)
        {
            float height = player.CharacterControl.height;
            float radius = player.CharacterControl.radius;
            Ray ray = new Ray(position, Vector3.up);

            if(Physics.SphereCast(ray, radius, out RaycastHit hit, height, player.surfaceCheckMask))
            {
                Debug.Log(hit.collider.name);
                return true;
            }

            return false;
        }
    }
}