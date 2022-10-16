using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemySystem
{
    public class NewBehaviourScript : MonoBehaviour
    {
        bool Frozen;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger && other.name == "FlashlightCone")
            {
                Debug.Log("InFlashlight");
                
            }
            

        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log("Backtopatrolling");
         
        }
    }
}