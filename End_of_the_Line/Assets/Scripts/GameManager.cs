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

    public Vector3 activeSpawn;
    public Vector3 geometrySpawn;

    public bool isPaused = false;

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
            activeSpawn = activeBeacon.transform.position;
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

    public void FastTravel()
    {
        player.transform.SetPositionAndRotation(activeSpawn, Quaternion.Euler(0,0,0));
    }
    #endregion
    #region scene management
    public void MovetoScene(string nextScene)
    {
        SceneManager.LoadScene(nextScene);
    }
    #endregion
}