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
    [SerializeField]
    GameObject pauseUI;
    [SerializeField]
    GameObject settingsUI;
    [SerializeField]
    GameObject audioUI;
    [SerializeField]
    GameObject brightnessUI;
    [SerializeField]
    GameObject winUI;
    [SerializeField]
    GameObject curUI;
    [SerializeField]
    GameObject placeholderUI;
    [SerializeField]
    GameObject lastUI;

    //Darkness setup
    [SerializeField]
    Image darknessBar;
    [SerializeField]
    GameObject lightIndicator;
    [SerializeField]
    float maxDarkness = 1000f;
    [SerializeField]
    float curDarkness = 100f;
    //the amount to decrease light by when in the dark
    [SerializeField]
    float inDarknessValue = 0.1f;
    //the amount to increase light by when in the light
    [SerializeField]
    float inLightValue = 1f;
    public bool lightLit;
    public bool inLight;

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
        inLight = false;
        curDarkness = maxDarkness;
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

        if (inLight)
        {
            lightIndicator.SetActive(true);
            IncreaseDarkness(inLightValue);
        }
        if (!inLight)
        {
            lightIndicator.SetActive(false);
            DecreaseDarkness(inDarknessValue);
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
    #region darkness
    public void IncreaseDarkness(float increaseAmount)
    {
        if(curDarkness < maxDarkness)
        {
            curDarkness = curDarkness + (increaseAmount * Time.deltaTime);
            SetDarknessBar();
        }
    }
    public void DecreaseDarkness(float decreaseAmount)
    {
        if(curDarkness > 0)
        {
            curDarkness = curDarkness - (decreaseAmount * Time.deltaTime);
            SetDarknessBar();
        }
        if(curDarkness <= 0)
        {
            PlayerDeath();
        }
    }
    public void SetDarknessBar()
    {
        float barFill = curDarkness / maxDarkness;
        darknessBar.fillAmount = barFill;
    }
    public void SetInLight()
    {
        inLight = true;
    }
    public void SetInDark()
    {
        inLight = false;
    }
    #endregion
    public void WonGame()
    {
        winUI.SetActive(true);
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void RestartGame()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        SceneManager.LoadScene("MainMenu");
    }
}