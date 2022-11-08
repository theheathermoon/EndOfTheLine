using FlashlightSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSystem : MonoBehaviour
{
    bool atControlPanel = false;
   // bool powerOn = false;

    public KeyCode PowerOn;

    public bool powered = false;
    bool inTrigger = false;

    public static PowerSystem instance;
    public GameObject trainLineLights;

    //this could be an array to hold all beacons
    public GameObject[] emergencyBeacons;

    [Header("Power Activation Timers")]
    [SerializeField] private float activatePowerRadial = 1.0f;
    private float maxActivatePowerRadial = 1.0f;

    void ToggleRadialIndicator(bool on)
    {
        FLUIManager.instance.ToggleRadialIndicator(on);
    }

    void UpdateRadialIndicator(float amount)
    {
        FLUIManager.instance.UpdateRadialIndicatorUI(amount);
    }

    private void Update()
    {
        if (Input.GetKey(PowerOn))
        {
            if(inTrigger == true)
            {
                if (powered == false)
                {
                    powered = true;
                    PowerLights();
                    PowerBeacons();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            inTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Player")
        {
            inTrigger = false;
        }
    }

    /*
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
    */
    ///
    void PowerLights()
    {
        trainLineLights.SetActive(true);
    }

    void PowerBeacons()
    {
        Debug.Log("beacons are powered");
        //code here can send a call for each beacon in the array to set powered = true or something like that
        foreach (GameObject emergencyBeacon in emergencyBeacons)
        {
            Debug.Log(emergencyBeacon.name + "is powered on");
            emergencyBeacon.GetComponent<EmergencyBeacon>().PowerOn();
        }
    }
 }
