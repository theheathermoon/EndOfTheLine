///////////////////////////////////
/// Create and edit by QerO
/// 09.2018
/// lidan-357@mail.ru
///////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenerator
{
    public class BasickDoor : MonoBehaviour
    {

        private int mode; //
        private Animator anim;

        void Start()
        {
            mode = 0;
            anim = gameObject.GetComponent<Animator>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                mode++;
                anim.SetBool("Open", true);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                mode--;
                if (mode == 0)
                {
                    anim.SetBool("Open", false);
                }
            }
        }
    }
}
