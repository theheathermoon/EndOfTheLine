using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour {

    public GameObject[] prefabsAsh;
    public GameObject[] prefabsDust;
    public Text[] buttonText;
    public GameObject objWindzone;
    Color defaultColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);

    public void ShowEffect1()
    {
        HideVFX();
        ResetTextColor();

        if (prefabsAsh[0]!=null) prefabsAsh[0].SetActive(true);
        if (prefabsDust[0] != null) prefabsDust[0].SetActive(true);

        buttonText[0].color = Color.red;
    }


    public void ShowEffect2()
    {
        HideVFX();
        ResetTextColor();

        if (prefabsAsh[1] != null) prefabsAsh[1].SetActive(true);
        if (prefabsDust[1] != null) prefabsDust[1].SetActive(true);

        buttonText[1].color = Color.red;
    }

    public void ShowEffect3()
    {
        HideVFX();
        ResetTextColor();

        if (prefabsAsh[2] != null) prefabsAsh[2].SetActive(true);
        if (prefabsDust[2] != null) prefabsDust[2].SetActive(true);

        buttonText[2].color = Color.red;
    }

    void HideVFX()
    {
        for (int i = 0; i < prefabsAsh.Length; i++)
        {
            if (prefabsAsh[i] != null) prefabsAsh[i].SetActive(false);
        }

        for (int i = 0; i < prefabsDust.Length; i++)
        {
            if (prefabsDust[i] != null) prefabsDust[i].SetActive(false);
        }

    }

    void ResetTextColor()
    {
        for (int i = 0; i < buttonText.Length; i++)
        {
            buttonText[i].color = defaultColor;
        }
    }

    public void SwitchWindzone(bool Selected) {
        objWindzone.SetActive(Selected);
    }

}
