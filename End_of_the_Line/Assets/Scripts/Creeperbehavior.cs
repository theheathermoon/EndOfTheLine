using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This handles all behaviors unique to the Gaurdian class
/// By using the EnemyAi attahce to the game component, it can detect when basic enemy behaviors should take place
/// and change them according to the gaurdians necessary behaviors
/// </summary>
namespace EnemySystem
{
    public class CreeperBehavior : MonoBehaviour
    {

        EnemyAi enemy;
        
        bool TeleStart = false;
        public int TeleTimer;
        public float TeleRange;
        public Vector3 TelePoint;
        //public Transform WarpPoint;
        public LayerMask whatIsground, whatIsplayer;

        // Start is called before the first frame update
        void Start()
        {
            enemy = gameObject.GetComponent(typeof(EnemyAi)) as EnemyAi;
            //WarpPoint = transform.Find("CreeperWarpPoint");
        }

        // Update is called once per frame
        void Update()
        {
            if (enemy.InFlashLight == true && TeleStart == false)
            {
                Debug.Log("Frozen");
                //enemy.agent.isStopped = true;
                TeleStart = true;
                CreateTeleport();
                
            }
            else if (enemy.InFlashLight == false && TeleStart == true)
            {
                Debug.Log("Unfrozen");
                TeleStart = false;
                //enemy.agent.isStopped = false;
            }
        }

        public void CreateTeleport()
        {
            Debug.Log("SearchingForWalkPoint");
            //Calculate random point in range
            float randomZ = Random.Range(-TeleRange, TeleRange);
            float randomX = Random.Range(-TeleRange, TeleRange);

            TelePoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(TelePoint, -transform.up, 2f, whatIsground))
            {
                //WarpPoint.position = WarpPoint.position + TelePoint;
                Debug.Log("WalkpointFound");
                //CheckTeleport();
                enemy.agent.Warp(TelePoint);


            }
        }

        public void Teleport()
        {

        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(TelePoint, new Vector3(1, 1, 1));
        }
    }

}
