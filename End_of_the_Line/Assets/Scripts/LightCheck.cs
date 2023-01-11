using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCheck : MonoBehaviour
{


    private void OnTriggerStay(Collider other)
    {
        if(other.name == "Player")
        {
            GameManager.Instance.SetInLight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.name == "Player")
        {
            GameManager.Instance.SetInDark();
        }
    }
}
