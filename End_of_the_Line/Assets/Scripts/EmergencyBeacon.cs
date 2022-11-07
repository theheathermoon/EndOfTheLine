using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmergencyBeacon : MonoBehaviour
{

    public bool isPowered = false;
    public bool isActive = false;

    public KeyCode makeActive;
    public KeyCode makeInactive;

    public void PowerOn()
    {
        if(isPowered == false)
        {
            isPowered = true;
        }
    }

    //make this emergency beacon the active beacon
    void Activate()
    {
        
    }

    void DeActivate()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        if(other.name == "Player")
        {
            if (Input.GetKey(makeActive))
            {
                if(isPowered == true)
                {
                    if(isActive == false)
                    {
                        isActive = true;
                        Debug.Log(gameObject.name + "is now active");
                    }
                    if(isActive == true)
                    {
                        Debug.Log(gameObject.name + "is already active");
                    }
                }
                if(isPowered == false)
                {
                    Debug.Log("Emergency Beacon is not powered");
                }
            }

            if (Input.GetKey(makeInactive))
            {
                if(isActive == true)
                {
                    isActive = false;
                    Debug.Log(gameObject.name + "is no longer active");
                }
            }
        }
    }
}
