using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{

    public static InventoryManager Instance;

    [SerializeField]
    float maxBattery = 100f;
    float curBattery = 0f;
    [SerializeField]
    float batterDrain = 1f;
    public Image flashlightBar;

    [SerializeField]
    float maxLighterFuel = 100f;
    [SerializeField]
    float curLighterFuel = 0f;
    [SerializeField]
    float lighterDrain = 5f;
    public Image lighterBar;

    [SerializeField]
    float matchTime = 10f;
    float matchTimeSave;
    [SerializeField]
    float matchDrain = 1f;
    public int curMatches;
    int maxMatches = 12;
    public TextMeshProUGUI matchCount;

    public GameObject equippedLight;
    public GameObject flashlight;
    public GameObject lighter;
    public GameObject matchBook;

    public GameObject flashlightUI;
    public GameObject lighterUI;
    public GameObject matchUI;
    public GameObject curUI;

    bool lightOn = false;
    bool flashlightOn = false;
    bool lighterOn = false;
    bool matchLit = false;

    private void Awake()
    {
        Instance = this;
        EquipFlashlight();
        SetBatteryValue();
        SetLighterValue();
        SetMatchValue();
        matchTimeSave = matchTime;
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

        if(flashlightOn == true)
        {
            DrainFlashlight();
        }
        if(lighterOn == true)
        {
            DrainLighter();
        }
        if(matchLit == true)
        {
            DrainMatch();
        }
    }

    private void TurnOnLight()
    {
        if(equippedLight == flashlight && curBattery > 0)
        {
            lightOn = true;
            flashlightOn = true;
            equippedLight.SetActive(true);
        }

        else if(equippedLight == lighter && curLighterFuel > 0)
        {
            lightOn = true;
            lighterOn = true;
            equippedLight.SetActive(true);
        }

        else if(equippedLight == matchBook && curMatches > 0)
        {
            matchTime = matchTimeSave;
            curMatches = curMatches - 1;
            SetMatchValue();
            lightOn = true;
            matchLit = true;
            equippedLight.SetActive(true);
        }
    }

    private void TurnOffLight()
    {
        lightOn = false;
        flashlightOn = false;
        lighterOn = false;
        matchLit = false;
        equippedLight.SetActive(false);
    }
    #region equipping
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
        equippedLight = matchBook;
        curUI.SetActive(false);
        curUI = matchUI;
        curUI.SetActive(true);
    }
    #endregion
    #region pickups
    public void PickUpMatches()
    {
        if(curMatches < maxMatches)
        {
            curMatches = 12;
            SetMatchValue();
        }
    }
    public void PickUpLighter()
    {
        if(curLighterFuel < maxLighterFuel)
        {
            curLighterFuel = maxLighterFuel;
            SetLighterValue();
            
        }
    }
    public void PickUpBatteries()
    {
        if (curBattery < maxBattery)
        {
            curBattery = maxBattery;
            SetBatteryValue();
        }
    }
    #endregion
    #region set values in UI
    private void SetMatchValue()
    {
        matchCount.text = curMatches + "/12";
    }

    private void SetLighterValue()
    {
        lighterBar.fillAmount = curLighterFuel / 100;
    }

    private void SetBatteryValue()
    {
        flashlightBar.fillAmount = curBattery / 100;
    }
    #endregion
    #region drain lights
    private void DrainFlashlight()
    {
        curBattery -= Time.deltaTime * batterDrain;
        SetBatteryValue();
        if(curBattery <= 0)
        {
            TurnOffLight();
        }
    }
    private void DrainLighter()
    {
        curLighterFuel -= Time.deltaTime * lighterDrain;
        SetLighterValue();
        if(curLighterFuel <= 0)
        {
            TurnOffLight();
        }
    }
    private void DrainMatch()
    {
        matchTime -= Time.deltaTime * matchDrain;
        if(matchTime <= 0)
        {
            TurnOffLight();
            matchTime = matchTimeSave;
        }
    }
    #endregion
}
