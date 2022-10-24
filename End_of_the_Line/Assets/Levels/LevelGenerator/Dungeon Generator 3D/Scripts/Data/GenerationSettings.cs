using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Generation Settings")]
public class GenerationSettings : ScriptableObject
{
    [SerializeField] public Data generation;
}

[Serializable]
public class Data
{
    [SerializeField] public int minRooms;
    [SerializeField] public int maxRooms;

    [SerializeField] public int amountOfAdjacentRoomsToConnect;

    //Make Minimum of 2
    [SerializeField] public int minRoomWidth;
    [SerializeField] public int maxRoomWidth;

    [SerializeField] public int minRoomHeight;
    [SerializeField] public int maxRoomHeight;

    [SerializeField] public int minBossRoomWidth;
    [SerializeField] public int maxBossRoomWidth;

    [SerializeField] public int minBossRoomHeight;
    [SerializeField] public int maxBossRoomHeight;

    [SerializeField] public int areaWidth;
    [SerializeField] public int areaHeight;
    
    [SerializeField] public int minDistanceBetweenRooms;

    [SerializeField] public List<ObjectList> walls = new List<ObjectList>();
    [SerializeField] public List<ObjectList> outerCorners = new List<ObjectList>();
    [SerializeField] public List<ObjectList> innerCorners = new List<ObjectList>();
    [SerializeField] public bool deleteInnerCorners;
    [SerializeField] public List<ObjectList> uCorners = new List<ObjectList>();
    [SerializeField] public List<ObjectList> squareCorners = new List<ObjectList>();
    [SerializeField] public List<ObjectList> doors = new List<ObjectList>();
    [SerializeField] public List<Vector3> doorsDisplacements;
    [SerializeField] public List<ObjectList> flooring = new List<ObjectList>();

    [SerializeField] public GameObject player;

    [SerializeField] public int minEnemiesPerRoom;
    [SerializeField] public int maxEnemiesPerRoom;

    [SerializeField] public List<ObjectList> enemies = new List<ObjectList>();
    [SerializeField] public List<ObjectList> bosses = new List<ObjectList>();
    [SerializeField] public List<ObjectList> breakables = new List<ObjectList>();
    [SerializeField] public List<ObjectList> staticObjects = new List<ObjectList>();
    [SerializeField] public List<ObjectList> wallObjects = new List<ObjectList>();

    [SerializeField] public int wallObjConsequitiveDistance;
    [SerializeField] public List<Vector3> wallObjDisplacements;

    [Range(0, 100)]
    [SerializeField] public int cornerObjectsRate;
    [Range(0, 100)]
    [SerializeField] public int destroyablesRate;

    [SerializeField] public List<ObjectList> middleObjects = new List<ObjectList>();
    [Range(0, 100)]
    [SerializeField] public int middleObjectsRate;
}

[System.Serializable]
public class ObjectList
{
    public GameObject gameObject;

    public string name;
    //Can add other variables if wanted
}