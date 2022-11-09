using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicketCounter : MonoBehaviour
{
    public string nextScene;
    public KeyCode insertTicket = KeyCode.E;

    bool isTriggered = false;

    [SerializeField]
    private bool quitCounter;
    [SerializeField]
    private bool playCounter;
    [SerializeField]
    private bool settingsCounter;

    private void Update()
    {
       if(isTriggered == true)
        {
            if (Input.GetKeyDown(insertTicket))
            {
                if(quitCounter == true)
                {
                    Application.Quit();
                    Debug.Log("you've quit the game");
                }
                if(playCounter == true)
                {
                    GameManager.Instance.MovetoScene("PowerPrototype");
                }
                if(settingsCounter == true)
                {
                    Debug.Log("this will open the settings");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "Player")
        {
            isTriggered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.name == "Player")
        {
            isTriggered = false;
        }
    }
}
