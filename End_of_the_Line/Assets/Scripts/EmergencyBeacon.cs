using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmergencyBeacon : MonoBehaviour
{

    public bool isPowered = false;
    public bool isActive = false;
    bool inTrigger = false;

    public KeyCode makeActive = KeyCode.E;
    public KeyCode makeInactive = KeyCode.R;
    public KeyCode fastTravel = KeyCode.T;

    public Transform spawnPoint;

    public void PowerOn()
    {
        if(isPowered == false)
        {
            isPowered = true;
        }
    }
    private void Update()
    {
        //activate the current beacon
        if (Input.GetKeyDown(makeActive))
        {
            if(inTrigger == true)
            {
                if (isPowered == true)
                {
                    //check to see if there is an active beacon
                    if (GameManager.Instance.activeBeacon != null)
                    {
                        Debug.Log("Another beacon is already active");
                    }
                    else
                    {
                        //set this beacon as the active beacon
                        if (isActive == false)
                        {
                            isActive = true;
                            GameManager.Instance.SetActiveBeacon(gameObject);
                            Debug.Log(gameObject.name + "is now active");
                        }
                    }

                }
                if (isPowered == false)
                {
                    Debug.Log("Emergency Beacon is not powered");
                }
            }

        }
        //deactivate the currently active beacon
        if (Input.GetKeyDown(makeInactive))
        {
            if(inTrigger == true)
            {
                if (isActive == true)
                {
                    isActive = false;
                    GameManager.Instance.DeactivateBeacon();
                    Debug.Log(gameObject.name + "is no longer active");
                }
            }

        }
        if (Input.GetKeyDown(fastTravel))
        {
            if(inTrigger == true)
            {
                if (isActive != true)
                {
                    GameManager.Instance.FastTravel();
                }
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "Player")
        {
            inTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.name == "Player")
        {
            inTrigger = false;
        }
    }
}
