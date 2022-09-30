using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSystem : MonoBehaviour
{
    bool atControlPanel = false;
    bool powerOn = false;




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
        if (other.name == "Player")
        {
            atControlPanel = true;
            Debug.Log("player is in the trigger area");




        }
    }



    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Player")
        {
            atControlPanel = false;
            Debug.Log("player is NOT in the trigger area");
        }
    }



}
