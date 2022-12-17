using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EnemySystem
{
    public class CreeperBehavior : MonoBehaviour
    {
        EnemyAi enemy;
        bool Fleeing;

        private Vector3 DirToPlayer;
        // Start is called before the first frame update
        void Start()
        {
            enemy = gameObject.GetComponent(typeof(EnemyAi)) as EnemyAi;
            Fleeing = false;

        }

        // Update is called once per frame
        void Update()
        {
            if (enemy.InFlashLight == true && Fleeing == false)
            {
                BeginCircling();
            }
            if(enemy.InFlashLight == false && Fleeing == true)
            {
                enemy.agent.isStopped = false;
                Fleeing = false;
            }
        }

        public void BeginCircling()
        {
            DirToPlayer = (enemy.player.position - transform.position);

            var angle:float = Vector3.Angle(DirToPlayer, enemy.player.forward);

            var cross:Vector3 = Vector3.Cross(DirToPlayer, enemy.player.forward);
            
            if(cross.y < 0)
            {
                angle = -angle;
                enemy.agent.isStopped = true;
                Debug.Log("Right");
                Fleeing = true;
            }
            else
            {
                enemy.agent.isStopped = true;
                Debug.Log("Left");
                Fleeing = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            //Draw a red sphere to show the attack radius
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, enemy.player.position);
        }

    }
}

