using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region singleton setup
    public static GameManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public GameObject player;

    public GameObject activeBeacon;

    public Transform activeSpawn;
    public Transform geometrySpawn;

    public bool gameOver = false;

    //UI setup
    public static bool isPaused = false;
    public GameObject pauseUI;
    public GameObject settingsUI;
    public GameObject audioUI;
    public GameObject brightnessUI;
    public GameObject curUI;
    public GameObject placeholderUI;
    public GameObject lastUI;

    //DeathUI
    public GameObject deathUI;
    public Image deathBackground;


    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pauseUI.SetActive(false);
        settingsUI.SetActive(false);
        audioUI.SetActive(false);
        brightnessUI.SetActive(false);
        deathUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused == false)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }

        if (gameOver && Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            PlayerDeath();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    #region player death
    private void PlayerDeath()
    {
        gameOver = true;
        deathBackground.canvasRenderer.SetAlpha(0);
        deathUI.SetActive(true);
        deathBackground.CrossFadeAlpha(0.8f, 3f, false);
        //StartCoroutine(DeathScreen());
    }

    //IEnumerator DeathScreen()
    //{

    //}

    #endregion
    #region pause/resume
    public void PauseGame()
    {
        isPaused = true;
        curUI = pauseUI;
        pauseUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void ReturnToPause()
    {
        curUI.SetActive(false);
        lastUI = curUI;
        curUI = pauseUI;
        curUI.SetActive(true);
    }

    public void OpenSettings()
    {
        curUI.SetActive(false);
        lastUI = curUI;
        curUI = settingsUI;
        curUI.SetActive(true);
    }

    public void OpenAudio()
    {
        curUI.SetActive(false);
        lastUI = curUI;
        curUI = audioUI;
        curUI.SetActive(true);
    }

    public void OpenBrightness()
    {
        curUI.SetActive(false);
        lastUI = curUI;
        curUI = brightnessUI;
        curUI.SetActive(true);
    }

    #endregion
    #region beacons
    public void SetActiveBeacon(GameObject newBeacon)
    {
        if(activeBeacon == null)
        {
            activeBeacon = newBeacon;
            activeSpawn = activeBeacon.GetComponent<EmergencyBeacon>().spawnPoint.transform;
        }
    }
    public void DeactivateBeacon()
    {
        activeBeacon = null;
        activeSpawn = geometrySpawn;
    }
    #endregion
    #region respawn and fast travel
    public void Respawn()
    {
        deathBackground.canvasRenderer.SetAlpha(0);
        deathUI.SetActive(true);
        player.transform.position = activeSpawn.position;
    }

    public void FastTravel()
    {
        player.transform.position = activeSpawn.position;
    }
    #endregion
    #region scene management
    public void MovetoScene(string nextScene)
    {
        SceneManager.LoadScene(nextScene);
    }
    #endregion
}