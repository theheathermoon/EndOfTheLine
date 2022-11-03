using FlashlightSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSystem : MonoBehaviour
{
    bool atControlPanel = false;
   // bool powerOn = false;

    public KeyCode PowerOn;

    public static PowerSystem instance;
    public GameObject trainLineLights;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        PowerLine();
    }


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

    ///
    void PowerLine () {

        if (atControlPanel == true || Input.GetKey(PowerOn))
        {
            trainLineLights.SetActive(true);
        }

    }

}
