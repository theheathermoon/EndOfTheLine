using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuTriggers : MonoBehaviour
{
    public string sceneName;
    //public GameObject player;
    public GameObject quitUI;
    [SerializeField]
    bool quitTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "Player")
        {
            if(quitTrigger == true)
            {
                OpenQuitMenu();
            }
            else
            {
                SceneManager.LoadScene(sceneName);
                Debug.Log("this should have loaded a scene");
            }
        }
    }

    private void OpenQuitMenu()
    {
        Time.timeScale = 0;
        quitUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void CloseQuitMenu()
    {
        Time.timeScale = 1;
        quitUI.SetActive(false);
        Cursor.visible = false;
    }


    public void QuitGame()
    {
        Application.Quit();
    }


}
