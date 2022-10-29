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
                Debug.Log("TeleStart");
                //enemy.agent.isStopped = true;
                TeleStart = true;
                StartCoroutine(Fade());
                //CreateTeleport();

            }
            else if (enemy.InFlashLight == false && TeleStart == true)
            {
                Debug.Log("Unfrozen");
                TeleStart = false;
                //enemy.agent.isStopped = false;
            }
        }
        IEnumerator Delay()
        {
            Color c = GetComponent<MeshRenderer>().material.color;
            for (float alpha = 1f; alpha >= 0; alpha -= 0.1f)
            {
                c.a = alpha;
                GetComponent<MeshRenderer>().material.color = c;
                yield return new WaitForSeconds(TeleTimer);

            }

        }

        IEnumerator Fade()
        {
            Color c = this.GetComponent<MeshRenderer>().material.color;
            for (float alpha = 1f; alpha >= 0; alpha -= 0.1f)
            {
                c.a = alpha;
                this.GetComponent<MeshRenderer>().material.color = c;
                yield return new WaitForSeconds(TeleTimer);

            }
        }

        public void CreateTeleport()
        {
            Debug.Log("Teleporting");
            //Calculate random point in range
            float randomZ = Random.Range(-TeleRange, TeleRange);
            float randomX = Random.Range(-TeleRange, TeleRange);

            TelePoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(TelePoint, -transform.up, 2f, whatIsground))
            {
                //WarpPoint.position = WarpPoint.position + TelePoint;
                Debug.Log("WalkpointFound");
                //CheckTeleport();
                TeleStart = false;
                enemy.agent.Warp(TelePoint);
            }
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(TelePoint, new Vector3(1, 1, 1));
        }
    }

}
