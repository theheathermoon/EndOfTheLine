using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public bool isPaused = false;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {

        }
    }

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

    }

    #endregion
    #region scene management
    public void MovetoScene(string nextScene)
    {
        SceneManager.LoadScene(nextScene);
    }
    #endregion
}