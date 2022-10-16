using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemySystem
{
    public class GuardianBehavior : MonoBehaviour
    {
        bool Frozen;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Frozen)
            {
                Debug.Log("Frozen");

                EnemyAi.instance.walkPoint = transform.position;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger && other.name == "FlashlightCone")
            {
                EnemyAi.instance.InFlashLight = true;
                Debug.Log("InFlashlight");
                Frozen = true;
            }
            

        }

        private void OnTriggerExit(Collider other)
        {
            if (other.isTrigger && other.name == "FlashlightCone")
            {
                Debug.Log("Backtopatrolling");
                EnemyAi.instance.Patrolling();
                Frozen = false;
            }
        }
    }
}