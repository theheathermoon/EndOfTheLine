using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Respawn()
    {

    }

    public void FastTravel()
    {
        player.transform.SetPositionAndRotation(activeSpawn, Quaternion.Euler(0,0,0));
    }
}