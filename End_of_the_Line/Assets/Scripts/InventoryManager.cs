using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    float maxBattery = 100f;
    int maxLighters = 2;
    int maxMatchBooks = 5;
    int maxMatches = 12;


    public float curBattery;
    public Image flashlightBar;

    public int curLighter;
    public float lighterFuel;
    public TextMeshProUGUI lighterCount;

    public int curMatchbooks;
    public float matches;
    public TextMeshProUGUI matchCount;

    public GameObject equippedLight;
    public GameObject flashlight;
    public GameObject lighter;
    public GameObject match;

    public GameObject flashlightUI;
    public GameObject lighterUI;
    public GameObject matchUI;
    public GameObject curUI;

    bool lightOn = false;

    private void Awake()
    {
        EquipFlashlight();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            equippedLight.SetActive(false);
            lightOn = false;
            EquipFlashlight();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            equippedLight.SetActive(false);
            lightOn = false;
            EquipLighter();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            equippedLight.SetActive(false);
            lightOn = false;
            EquipMatches();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            if(lightOn == false)
            {
                TurnOnLight();
            }
            else
            {
                TurnOffLight();
            }
        }
    }

    private void TurnOnLight()
    {
        if(equippedLight == flashlight && curBattery > 0)
        {
            lightOn = true;
            equippedLight.SetActive(true);
        }
        if(equippedLight == lighter && lighterFuel > 0)
        {
            lightOn = true;
            equippedLight.SetActive(true);
        }
        if(equippedLight == match && matches > 0)
        {
            lightOn = true;
            equippedLight.SetActive(true);
        }
    }

    private void TurnOffLight()
    {
        lightOn = false;
        equippedLight.SetActive(false);
    }

    private void EquipFlashlight()
    {
        equippedLight = flashlight;
        curUI.SetActive(false);
        curUI = flashlightUI;
        curUI.SetActive(true);
    }

    private void EquipLighter()
    {
        equippedLight = lighter;
        curUI.SetActive(false);
        curUI = lighterUI;
        curUI.SetActive(true);
    }

    private void EquipMatches()
    {
        equippedLight = match;
        curUI.SetActive(false);
        curUI = matchUI;
        curUI.SetActive(true);
    }


}
