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
    public class GuardianBehavior : MonoBehaviour
    {
        bool Frozen;
        EnemyAi enemy;
        // Start is called before the first frame update
        void Start()
        {
            enemy = gameObject.GetComponent(typeof(EnemyAi)) as EnemyAi;
        }

        // Update is called once per frame
        void Update()
        {
            if (Frozen)
            {
                //Debug.Log("Frozen");
                enemy.agent.isStopped = true;
;
            }
            else
            {
                enemy.agent.isStopped = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger && other.name == "FlashlightCone")
            {

                //Debug.Log("Freeze");
                Frozen = true;
            }
            

        }

        private void OnTriggerExit(Collider other)
        {
            if (other.isTrigger && other.name == "FlashlightCone")
            {
                //Debug.Log("Backtopatrolling");

                Frozen = false;
            }
        }
    }
}