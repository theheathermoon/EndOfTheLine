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
        // Start is called before the first frame update
        void Start()
        {
            enemy = gameObject.GetComponent(typeof(EnemyAi)) as EnemyAi;
        }

        // Update is called once per frame
        void Update()
        {
            if (enemy.InFlashLight = true)
            {
                //Debug.Log("Frozen");
                enemy.agent.isStopped = true;
                Teleport();
                
            }
            else
            {
                enemy.agent.isStopped = false;
            }
        }

        public void Teleport()
        {

        }

    }

}
