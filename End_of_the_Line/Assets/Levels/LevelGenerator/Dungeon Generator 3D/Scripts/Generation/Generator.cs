using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.AI;

public class Generator : MonoBehaviour
{
    //[SerializeField] private GameObject m_wall;

    [SerializeField] private GenerationSettings generationSettings;

    private bool deleteInnerCorners;

    private int rooms;
    private int minRooms;
    private int maxRooms;
    private int areaWidth;
    private int areaHeight;
    private int minRoomWidth;
    private int maxRoomWidth;
    private int minRoomHeight;
    private int maxRoomHeight;
    private int corridorNumber;
    private int destroyablesRate;
    private int minBossRoomWidth;
    private int maxBossRoomWidth;
    private int minBossRoomHeight;
    private int maxBossRoomHeight;
    private int cornerObjectsRate;
    private int middleObjectsRate;
    private int minEnemiesPerRoom;
    private int maxEnemiesPerRoom;
    private int minDistanceBetweenRooms;
    private int wallObjConsequitiveDistance;
    private int amountOfAdjacentRoomsToConnect;

    private List<ObjectList> walls;
    private List<ObjectList> doors;
    private List<ObjectList> bosses;
    private List<ObjectList> enemies;
    private List<ObjectList> uCorners;
    private List<ObjectList> flooring;
    private List<ObjectList> breakables;
    private List<ObjectList> wallObjects;
    private List<ObjectList> outerCorners;
    private List<ObjectList> innerCorners;
    private List<ObjectList> squareCorners;
    private List<ObjectList> staticObjects;
    private List<ObjectList> middleObjects;

    private List<Vector3> wallObjDisplacements;
    private List<Vector3> doorsDisplacements;

    private GameObject boss;
    private GameObject player;
    private GameObject[,] area;
    private GameObject[] parents;
    private GameObject emptyObject;
    private GameObject emptyParentRoom;

    private Vector2Int[] roomPos;
    private Vector2Int[] roomArea;

    private Room[] parentRooms;
    private Room bossSpawnRoom;
    private Room playerSpawnRoom;

    private List<Vector2Int> tempArea = new List<Vector2Int>();

    private List<GameObject> corridors = new List<GameObject>();
    private List<GameObject> doorGameObjects = new List<GameObject>();

    private static int numberOfRooms;

    private string[] floorRemovables = { "Floor", "(", ")", "", " ", "," };

    public GameObject Player { get => player; }
    public Room PlayerSpawnRoom { get => playerSpawnRoom; }
    public List<GameObject> DoorGameObjects { get => doorGameObjects; }
    public List<ObjectList> Enemies { get => enemies; }
    public int AmountOfAdjacentRoomsToConnect { get => amountOfAdjacentRoomsToConnect; }
    public GenerationSettings GenerationSettings { get => generationSettings; }

    private void Awake()
    {
        RunProgram();
    }

    public void RunProgram()
    {
        if(generationSettings != null)
        {
            ClearChildren();

            GetGenerationSettings();

            Initialise();

            InitialiseEmptyObjects();

            Generate();
        }
        else
        {
            Debug.Log("Need Generation settings to generate Dungeon.");
        }
    }

    private void ClearChildren()
    {
        if(transform.childCount > 0)
        {
            while(transform.childCount!= 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }

    private void GetGenerationSettings()
    {
        Data settings;
        settings = GenerationSettings.generation;

        minRooms = settings.minRooms;
        maxRooms = settings.maxRooms;
        if (minRooms == 0)
            Debug.LogError("Minimum amount of rooms can not equal zero for generation to work.");
        if (maxRooms == 0)
            Debug.LogError("Maximum amount of rooms can not equal zero for generation to work.");

        amountOfAdjacentRoomsToConnect = settings.amountOfAdjacentRoomsToConnect;

        if (AmountOfAdjacentRoomsToConnect == 0)
            Debug.LogError("Amount of AdjacentRoomsToConnect can not equal zero for generation to work.");

        if (AmountOfAdjacentRoomsToConnect >= minRooms)
            Debug.LogError("Amount of Adjacent Rooms to connect needs to be smaller than minimum of rooms");

        minRoomWidth = settings.minRoomWidth;
        maxRoomWidth = settings.maxRoomWidth;
        if (minRoomWidth == 0)
            Debug.LogError("Minimum Room Width can not equal zero for generation to work.");
        if (maxRoomWidth == 0)
            Debug.LogError("Maximum Room Width can not equal zero for generation to work.");

        minRoomHeight = settings.minRoomWidth;
        maxRoomHeight = settings.maxRoomHeight;
        if (minRoomHeight == 0)
            Debug.LogError("Minimum Room Height can not equal zero for generation to work.");
        if (maxRoomHeight == 0)
            Debug.LogError("Maximum Room Height can not equal zero for generation to work.");

        minBossRoomWidth = settings.minBossRoomWidth;
        maxBossRoomWidth = settings.maxBossRoomWidth;
        if (minBossRoomWidth == 0)
            Debug.LogError("Minimum Boss Room Width can not equal zero for generation to work.");
        if (maxBossRoomWidth == 0)
            Debug.LogError("Maximum Boss Room Width can not equal zero for generation to work.");

        minBossRoomHeight = settings.minBossRoomHeight;
        maxBossRoomHeight = settings.maxBossRoomHeight;
        if (minBossRoomHeight == 0)
            Debug.LogError("Minimum Boss Room Height can not equal zero for generation to work.");
        if (maxBossRoomHeight == 0)
            Debug.LogError("Maximum Boss Room Height can not equal zero for generation to work.");

        areaWidth = settings.areaWidth;
        areaHeight = settings.areaHeight;
        if (areaWidth == 0)
            Debug.LogError("Area Width can not equal zero for generation to work.");
        if (areaHeight == 0)
            Debug.LogError("Area Height can not equal zero for generation to work.");

        minDistanceBetweenRooms = settings.minDistanceBetweenRooms;
        if(minDistanceBetweenRooms == 0)
            Debug.Log("Minimum distance between rooms advised to be at least one.");

        walls = settings.walls;
        if (!CheckListObject(walls))
            Debug.LogError("Walls can't be null or nonexistent for generation to work.");

        outerCorners = settings.outerCorners;
        if (!CheckListObject(outerCorners))
            Debug.LogError("Outer Corners can't be null or nonexistent for generation to work.");

        innerCorners = settings.innerCorners;
        if (!CheckListObject(innerCorners))
            Debug.Log("Inner Corners are null, they are not however not required.");

        deleteInnerCorners = settings.deleteInnerCorners;

        uCorners = settings.uCorners;
        if (!CheckListObject(uCorners))
            Debug.Log("U Corners can't be null or nonexistent for generation.");

        squareCorners = settings.squareCorners;
        if (!CheckListObject(squareCorners))
            Debug.LogError("Square Corners can't be null or nonexistent for generation.");

        doors = settings.doors;
        if (!CheckListObject(doors))
            Debug.Log("Doors are null or nonexistent.");
        
        doorsDisplacements = settings.doorsDisplacements;

        flooring = settings.flooring;
        if (!CheckListObject(flooring))
            Debug.LogError("Flooring can't be null or nonexistent for generation.");

        minEnemiesPerRoom = settings.minEnemiesPerRoom;
        if (minEnemiesPerRoom == 0)
            Debug.Log("Minimum enemies per room are currently equal to 0.");

        maxEnemiesPerRoom = settings.maxEnemiesPerRoom;
        if (maxEnemiesPerRoom == 0)
            Debug.Log("Maximum enemies per room are currently equal to 0");

        player = settings.player;
        if(player == null)
            Debug.Log("Player is null or nonexistent.");

        enemies = settings.enemies;
        if (!CheckListObject(Enemies))
            Debug.Log("Enemies are null or nonexistent.");

        bosses = settings.bosses;
        if (!CheckListObject(bosses))
            Debug.Log("Bosses are null or nonexistent.");

        breakables = settings.breakables;
        if (!CheckListObject(breakables))
            Debug.Log("Breakables are null or nonexistent.");

        staticObjects = settings.staticObjects;
        if (!CheckListObject(staticObjects))
            Debug.Log("Static Objects are null or nonexistent.");

        wallObjects = settings.wallObjects;
        if (!CheckListObject(wallObjects))
            Debug.Log("Wall Objects are null or nonexistent.");

        wallObjConsequitiveDistance = settings.wallObjConsequitiveDistance;
        if (wallObjConsequitiveDistance == 0)
            Debug.Log("Consequitive distance between Wall objects can not equal 0 for generation to work. Advised to be at least 2 for non cluster.");

        wallObjDisplacements = settings.wallObjDisplacements;

        while (wallObjDisplacements.Count != wallObjects.Count)
        {
            if(wallObjDisplacements.Count < wallObjects.Count)
            {
                wallObjDisplacements.Add(new Vector3());
            }
            else
            {
                wallObjDisplacements.RemoveAt(wallObjDisplacements.Count - 1);
            }
        } 

        Debug.Log("Wall OBj displacement will vary accordingly to listed item.");

        cornerObjectsRate = settings.cornerObjectsRate;
        if (cornerObjectsRate == 0 && staticObjects.Count > 0 && breakables.Count > 0)
            Debug.Log("Rate of corner objects can not equal 0 for corner objects to be generated.");

        destroyablesRate = settings.destroyablesRate;
        if (destroyablesRate == 0 && breakables.Count > 0)
            Debug.Log("Rate of Destroyables can not equal 0 for corner objects to be generated.");

        middleObjects = settings.middleObjects;
        if (!CheckListObject(middleObjects))
            Debug.Log("Middle Objects are null or nonexistent.");

        middleObjectsRate = settings.middleObjectsRate;
        if(middleObjectsRate == 0 && middleObjects.Count > 0)
            Debug.Log("Rate of MiddleObjects can not equal 0 for MiddleObjects objects to be generated.");
    }

    private bool CheckListObject(List<ObjectList> objectList)
    {
        int listCount = 0;

        for (int i = 0; i < objectList.Count; i++)
        {
            if (objectList[i].gameObject != null)
                listCount += 1;
        }


        if (listCount == objectList.Count && objectList.Count != 0)
        {
            return true;
        }

        return false;
    }

    private void Initialise()
    {

        area = new GameObject[areaWidth, areaHeight];

        rooms = Random.Range(minRooms, maxRooms);
        
        parents = new GameObject[rooms];
        parentRooms = new Room[rooms];

        roomArea = new Vector2Int[rooms];
        roomPos = new Vector2Int[rooms];
    }

    private void InitialiseEmptyObjects()
    {
        emptyObject = new GameObject();
        emptyObject.hideFlags = HideFlags.HideInHierarchy;

        emptyParentRoom = new GameObject();
        emptyParentRoom.hideFlags = HideFlags.HideInHierarchy;
        emptyParentRoom.AddComponent(typeof(Room));
        emptyParentRoom.tag = "Room";
    }

    //TODO: Add comments to this function
    private void Generate()
    {
        int randomBossRoom = Random.Range(0, rooms);
        bool isBossRoom = false;

        for (int i = 0; i < rooms; i++)
        {
            //Set temp area to area of room
            roomPos[i].x = Random.Range(0, areaWidth);
            roomPos[i].y = Random.Range(0, areaHeight);

            if (i == randomBossRoom)
            {
                isBossRoom = true;
                roomArea[i].x = Random.Range(minBossRoomWidth, maxBossRoomWidth);
                roomArea[i].y = Random.Range(minBossRoomHeight, maxBossRoomHeight);
            }
            else
            {
                isBossRoom = false;
                roomArea[i].x = Random.Range(minRoomWidth, maxRoomWidth);
                roomArea[i].y = Random.Range(minRoomHeight, maxRoomHeight);
            }

            if (CreateRooms(i, ref isBossRoom) == false)
            {
                i--;
            }
            else
            {
                parentRooms[i] = parents[i].GetComponent<Room>();

                parentRooms[i].AddMidPoint();

                if (isBossRoom)
                    bossSpawnRoom = parentRooms[i];
            }
        }

        int randomRoom = Random.Range(0, parentRooms.Length);

        CalculateCorridors();

        List<Room> tempList = new List<Room>();

        Room tempRoom = parentRooms[0];

        AddConnectedRoom(tempRoom, ref tempList, true);

        CheckConnectedRoom(tempRoom, tempList);

        GenerateWalls();

        DoubleCheckCorners();

        GenerateDoors();
        
        GenerateContent();

        DoubleCheckRoomCorners();

        SpawnEntities();
    }

    //TODO: Add comments to this function   
    private bool CreateRooms(int i, ref bool isBossRoom)
    {
        for (int a = 0; a < roomArea[i].x; a++)
        {
            for (int b = 0; b < roomArea[i].y; b++)
            {
                //Check bounds
                if (roomPos[i].x + roomArea[i].x > areaWidth - (roomArea[i].x / 2) - 2|| roomPos[i].y + roomArea[i].y > areaHeight - (roomArea[i].y / 2) - 2 
                    || roomPos[i].x - roomArea[i].x < 2 || roomPos[i].y - roomArea[i].y < 2)
                {
                    return false;
                }
                else
                {
                    if(tempArea.Count < 1)
                    {
                        for (int d = - minDistanceBetweenRooms; d < roomArea[i].x + (minDistanceBetweenRooms); d++)
                        {
                            for (int e = -minDistanceBetweenRooms; e < roomArea[i].y + (minDistanceBetweenRooms); e++)
                            {
                                if(roomPos[i].x + d < areaWidth || roomPos[i].x + d > 0 || roomPos[i].y + e < areaHeight || roomPos[i].y + e > 0)
                                {
                                    if (area[roomPos[i].x + d, roomPos[i].y + e] != null)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }

                    //Check if there is a room at the position
                    if (area[roomPos[i].x + a, roomPos[i].y + b] == null)
                    {
                        //Do only once per room
                        if(tempArea.Count == 0)
                        {
                            numberOfRooms++;
                            
                            Vector2 tempArea = CalculateCenterPoint(roomArea[i], roomPos[i]);
                            
                            parents[i] = Instantiate(emptyParentRoom, new Vector3(tempArea.x, 0, tempArea.y), Quaternion.identity, gameObject.transform);
                            parents[i].name = "Room" + numberOfRooms + "(" + roomPos[i].x + ", " + roomPos[i].y + ")";

                            BoxCollider collider = parents[i].AddComponent<BoxCollider>();
                            collider.size = new Vector3(roomArea[i].x - 1 , 3, roomArea[i].y - 1);
                            collider.isTrigger = true;
                        }

                        int randomFLooring = Random.Range(0, flooring.Count);
                        //Instantiate Floor
                        area[roomPos[i].x + a, roomPos[i].y + b] = Instantiate(flooring[randomFLooring].gameObject, new Vector3(roomPos[i].x + a, 0, roomPos[i].y + b), Quaternion.identity, parents[i].transform);

                        int tempX = roomPos[i].x + a;
                        int tempY = roomPos[i].y + b;

                        area[roomPos[i].x + a, roomPos[i].y + b].name = "Floor" + " (" + tempX + ", " + tempY + ")";

                        //Add cords to list
                        tempArea.Add(new Vector2Int(roomPos[i].x + a, roomPos[i].y + b));
                    }
                    else
                    {
                        //If it is not null, delete the whole room that was just instatiated with i and the room area
                        if (tempArea.Count > 0)
                        {
                            for (int e = 0; e < tempArea.Count; e++)
                            {
                                DestroyImmediate(area[tempArea[e].x, tempArea[e].y]);
                                area[tempArea[e].x, tempArea[e].y] = null;
                            }

                            //Delete parent
                            DestroyImmediate(parents[i]);
                            DestroyImmediate(parentRooms[i]);
                            parentRooms[i] = null;
                            parents[i] = null;
                            numberOfRooms--;

                            tempArea.Clear();
                        }

                        //Restart iteneration so this is done again
                        return false;
                    }

                    //If a and b both Max Area reset counter for room[]
                    if (a >= roomArea[i].x - 1  && b >= roomArea[i].y - 1)
                    { 
                        tempArea.Clear();
                    }
                }
            }
        }

        return true;
    }

    public void CalculateCorridors()
    {
        for (int i = 0; i < parentRooms.Length; i++)
        {
            parentRooms[i].CalculateConnections();
        }

        CalculateClosestRooms();

        AddRoomConnections();

        CreateCorridors();
    }

    private void AddRoomConnections()
    {
        List<int> tempNumbers = new List<int>();

        for (int a = 0; a < parentRooms.Length; a++)
        {
            //Chose random rooms between the closest
            for (int b = 0; b < parentRooms[a].MaxAmountOfConnections; b++)
            {

                //Add all the possible rooms that can be chosen
                for (int c = 0; c < parentRooms[a].MaxAmountOfConnections; c++)
                {
                    tempNumbers.Add(c);
                }

                int tempRandomInt = tempNumbers[Random.Range(0, tempNumbers.Count)];

                parentRooms[a].AddConnectedRooms(parentRooms[a].ClosestRooms[tempRandomInt].GetComponent<Room>());
                parentRooms[a].SavedConnectedRooms = parentRooms[a].ConnectedRooms;

                //Delete number from list so it doesn't appear again
                tempNumbers.Remove(tempRandomInt);

            }

            //Clear list for the next room
            tempNumbers.Clear();
        }
    }

    private void CreateCorridors()
    {
        for (int a = 0; a < parentRooms.Length; a++)
        {
            for (int b = 0; b < parentRooms[a].ConnectedRooms.Count; b++)
            {
                int randomPoint;

                if (parentRooms[a].ConnectedRooms[b].MidPoints.Count > 1)
                {
                    randomPoint = Random.Range(0, parentRooms[a].ConnectedRooms[b].MidPoints.Count - 1);
                }
                else
                {
                    randomPoint = 0;
                }

                bool isXEqual = false;
                bool isYEqual = false;


                GameObject targetPoint = parentRooms[a].ConnectedRooms[b].MidPoints[randomPoint];

                randomPoint = Random.Range(0, parentRooms[a].MidPoints.Count);

                Transform startPoint = parentRooms[a].MidPoints[randomPoint].transform;

                CompareRoomXOrYPos(startPoint, targetPoint.transform, ref isXEqual, ref isYEqual);
                ChooseDirection(isXEqual, isYEqual, targetPoint.transform, startPoint);
            }
        }
    }     

    private void CompareRoomXOrYPos(Transform self, Transform target, ref bool boolx, ref bool booly)
    {
        if (target.position.x == self.position.x)
        {
            boolx = true;
        }

        if (target.position.z == self.position.z)
        {
            booly = true;
        }
    }

    private void AddConnectedRoom(Room room, ref List<Room> roomList, bool firstCheck)
    {
        for (int i = 0; i < room.ConnectedRooms.Count; i++)
        {
            if(!roomList.Contains(room))
            {
                roomList.Add(room);
            }

            //If connected rooms are more than 0 of connected room
            //And if the list does not already contain that room
            //Call this function again with the specified connected room
            if(room.ConnectedRooms[i].ConnectedRooms.Count != 0 && !roomList.Contains(room.ConnectedRooms[i]))
            {
                AddConnectedRoom(room.ConnectedRooms[i], ref roomList, false);
            }

            if (room.ConnectedRooms[i].ConnectedRooms.Count == 0 && !roomList.Contains(room.ConnectedRooms[i]))
            {
                roomList.Add(room.ConnectedRooms[i]);
            }
        }

        //If the first check is true
        //Which should only happen at the end of the first iteration
        //Check if count of rooms is equal to count of list
        if (firstCheck == true && roomList.Count != rooms)
        {
            //Check if all rooms are actually connected, and add the missing ones
            CheckForConnections(ref roomList);
        }
    }

    private void CheckForConnections(ref List<Room> roomList)
    {
        //Loop through all the parents rooms
        for (int i = 0; i < parentRooms.Length; i++)
        {
            //Loop through all the roomlist rooms
            for (int z = 0; z < roomList.Count; z++)
            {
                //Check if the room is already added to the roomList
                if (!roomList.Contains(parentRooms[i]))
                {
                    Room tempRoom;

                    //Create local list to work around the lamda function which doesn't take ref Lists
                    List<Room> tempLambdaWorkAround = roomList;

                    //Make local room equal to the parents room iterated connected room if it finds the iterated roomList z room, but only if it contains it.
                    tempRoom = parentRooms[i].ConnectedRooms.Find(x => x.ConnectedRooms.Contains(tempLambdaWorkAround[z]));

                    if (tempRoom != null)
                    {
                        //If the room is found, and doesn't equal to null call the add connected room function
                        //So it can look for it
                        AddConnectedRoom(parentRooms[i], ref roomList, true);
                    }

                    //This double checks it, and makes the bug of where one room is missing not happen anymore.
                    for (int j = 0; j < parentRooms[i].ConnectedRooms.Count; j++)
                    {
                        if (parentRooms[i].ConnectedRooms[j] == roomList[z])
                        {
                            AddConnectedRoom(parentRooms[i], ref roomList, true);
                        }
                    }
                }
            }
        }
    }

    private void CheckConnectedRoom(Room room, List<Room> roomList)
    {
        //Check if all the paths are already connected or not
        if(roomList.Count != rooms)
        { 
            do
            {
                Room targetRoom = null;
                List<Room> targetList = new List<Room>();

                //Compare this list and check which rooms are closer between both lists connect those
                for (int a = 0; a < parentRooms.Length; a++)
                {
                    for (int b = 0; b < roomList.Count; b++)
                    {
                        //Whatever is first on the list is the closest to the room
                        for (int c = 0; c < parentRooms[a].ClosestRooms.Count; c++)
                        {
                            //Make sure room is not in the connected ones list
                            if (!roomList.Contains(parentRooms[a]))
                            {
                                targetRoom = parentRooms[a];
                                break;
                            }
                        }

                        break;
                    }

                    if (targetRoom != null)
                    {
                        Vector3Int vector3Int;
                        bool isXEqual = false;
                        bool isYEqual = false;


                        //Get the closes rooms to connect them
                        //x = distance, y = first list item wanted, z = second list item wanted
                        vector3Int = CheckClosestRooms(ref roomList, ref targetList);

                        //Get the start Mid Point
                        Transform startPoint = roomList[vector3Int.y].MidPoints[0].transform;

                        //Get the targetPoint
                        GameObject targetPoint = targetList[vector3Int.z].MidPoints[0];

                        //Compare x and y to see if they are both different
                        CompareRoomXOrYPos(startPoint, targetPoint.transform, ref isXEqual, ref isYEqual);

                        //Chose random direction and create corridor
                        ChooseDirection(isXEqual, isYEqual, startPoint, targetPoint.transform);

                        //Add all the members from target list to roomList
                        for (int i = 0; i < targetList.Count; i++)
                        {
                            roomList.Add(targetList[i]);
                        }

                        break;
                    }
                }

            } while (roomList.Count != rooms);
        }
    }

    //Keep calling this function until the roomListNumber is equal to the rooms
    //Makes sure all rooms are connected
    private Vector3Int CheckClosestRooms(ref List<Room> roomList, ref List<Room> targetList)
    {
        Vector3Int distanceIterations = new Vector3Int();

        //Compare between the roomlist and parents list
        for (int a = 0; a < roomList.Count; a++)
        {
            for (int b = 0; b < parentRooms.Length; b++)
            {
                //Create a list with the first room which is not on the list
                if(!roomList.Contains(parentRooms[b]))
                {
                    //Add to that list all of it's connected rooms in total
                    AddConnectedRoom(parentRooms[b], ref targetList, true);

                    //Calculate between which of the rooms is closest to any of the roomList
                    distanceIterations = CalculateDistanceBetweenTwoLists(ref roomList, ref targetList);

                    return distanceIterations;
                }
            }
        }

        return distanceIterations;
    }

    //First value of vector3 int would be Distance, second the startPoint in number of iteration, third the targetPoint in number of iteration
    //This functiton will retrieve a vector 3 int of the lowest distance
    Vector3Int CalculateDistanceBetweenTwoLists(ref List<Room> listOne, ref List<Room> listTwo)
    {
        List<Vector3Int> tempVector3Int = new List<Vector3Int>();

        for (int a = 0; a < listOne.Count; a++)
        {
            for (int b = 0; b < listTwo.Count; b++)
            {
                float distance = Vector3.Distance(listOne[a].transform.position, listTwo[b].transform.position);

                tempVector3Int.Add(new Vector3Int((int)distance, a, b));
            }
        }

        tempVector3Int.Sort((a, b) => a.x.CompareTo(b.x));

        //Store the vector with the least distance
        //And then return it.
        Vector3Int tempVec3Int = tempVector3Int[0];

        return tempVec3Int;
    }
  
    private void ChooseDirection(bool equalx, bool equalY, Transform target, Transform startPoint)
    {
        int dir;
        bool calculateNext = false;
        bool isFirstTime = true;
        if (equalx == true && equalY == false)
        {
            dir = 0;
        }
        else if(equalY == true && equalx == false)
        {
            dir = 1;
        }
        else
        {
            dir = Random.Range(0, 2);
            calculateNext = true;
        }

        CreateCorridor(dir, calculateNext, isFirstTime, target, startPoint);
    }

    private void CreateCorridor(int corridor, bool calculateNext, bool isFirstTime, Transform target, Transform startPoint)
    {
        GameObject corridorFloor;

        //Only does this the first time
        if (isFirstTime)
        {
            Room tempTarget = target.transform.parent.gameObject.GetComponent<Room>();
            Room tempStart = startPoint.transform.parent.gameObject.GetComponent<Room>();

            //If the targeted room contains start room, remove it from the connected rooms of target
            if (tempTarget.ConnectedRooms.Contains(tempStart))
            {
                tempTarget.SavedConnectedRooms = tempTarget.ConnectedRooms;
                tempTarget.ConnectedRooms.Remove(tempStart);
            }

            corridors.Add(Instantiate(emptyObject));
            corridors[corridorNumber].transform.SetParent(startPoint.parent);
            corridors[corridorNumber].name = "Corridor " + corridorNumber;
        }

        //Do X Corridor
        if (corridor == 0)
        {
            int additive = 0;

            do
            {
                if(startPoint.position.z < target.position.z)
                {
                    additive++;
                }
                else
                {
                    additive--;
                }

                //Get string from the self
                string tempString = startPoint.gameObject.name;
                string[] tempStringArray = tempString.Split(floorRemovables, System.StringSplitOptions.RemoveEmptyEntries);

                //Make a parent object for corridor which is child of room
                int tempIntY = int.Parse(tempStringArray[1]) + additive;
                int tempIntX = int.Parse(tempStringArray[0]);

                int randomFloor = Random.Range(0, flooring.Count);
                corridorFloor = Instantiate(flooring[randomFloor].gameObject, new Vector3(startPoint.position.x, 0, startPoint.position.z + additive), Quaternion.identity); 
                //Name corridor floor
                corridorFloor.name = "Floor (" + tempIntX + ", " + tempIntY + ")";

                //Set parent to be corridor parent object
                corridorFloor.transform.SetParent(corridors[corridorNumber].transform);

                if (area[tempIntX, tempIntY] != null)
                {
                    DestroyImmediate(corridorFloor);
                    corridorFloor = area[tempIntX, tempIntY];
                }
                else
                {
                    area[tempIntX, tempIntY] = corridorFloor;
                }

            } while (corridorFloor.transform.position.z != target.position.z);


            if (calculateNext)
            {
                CreateCorridor(1, false, false, target, corridorFloor.transform);
            }
        }
        else if(corridor == 1) // Do Y corridor
        {
            int additive = 0;

            do
            {
                if (startPoint.position.x < target.position.x)
                {
                    additive++;
                }
                else
                {
                    additive--;
                }


                string tempString = startPoint.gameObject.name;
                string[] tempStringArray = tempString.Split(floorRemovables, System.StringSplitOptions.RemoveEmptyEntries);
                tempStringArray[0].Remove(0);


                //Make a parent object for corridor which is child of room
                int tempIntY = int.Parse(tempStringArray[1]);
                int tempIntX = int.Parse(tempStringArray[0]) + additive;

                int randomFloor = Random.Range(0, flooring.Count);
                corridorFloor = Instantiate(flooring[randomFloor].gameObject, new Vector3(startPoint.position.x + additive, 0, startPoint.position.z), Quaternion.identity);

                //Name corridor floor
                corridorFloor.name = "Floor" + " (" + tempIntX + ", " + tempStringArray[1] + ")";

                //Set parent to be corridor parent object
                corridorFloor.transform.SetParent(corridors[corridorNumber].transform);

                if (area[tempIntX, tempIntY] != null)
                {
                    DestroyImmediate(corridorFloor);
                    corridorFloor = area[tempIntX, tempIntY];
                }
                else
                {
                    area[tempIntX, tempIntY] = corridorFloor;
                }


            } while (corridorFloor.transform.position.x != target.position.x);

            if (calculateNext)
            {
                CreateCorridor(0, false, false, target, corridorFloor.transform);
            }
        }
        else
        {
            Debug.LogError("Create Corridor Function's int passed non available to create corridor in specific axis");
        }

        if (isFirstTime)
        {
            //Increase corridor number
            corridorNumber++;
        }
    }

    private void CalculateClosestRooms()
    {
        int maxDistance = 0;

        List<float> distances;

        Dictionary<float, GameObject> tempDictionary;

        for (int a = 0; a < parents.Length; a++)
        {
            distances = new List<float>();
            tempDictionary = new Dictionary<float, GameObject>();

            for (int b = 0; b < parents.Length; b++)
            {
                //Make sure they are not equal cus distance otherwise will be 0
                if(a != b)
                {
                    if(!tempDictionary.ContainsKey(Vector3.Distance(parents[a].transform.position, parents[b].transform.position)))
                    {
                        //Calculate distances and add them to containers
                        distances.Add(Vector3.Distance(parents[a].transform.position, parents[b].transform.position));

                        tempDictionary.Add(Vector3.Distance(parents[a].transform.position, parents[b].transform.position), parents[b]);
                    }               
                }
            }

            if(distances.Count > 0)
            {
                distances.Sort();
            }

            //Add closest adjacent rooms
            for (int i = 0; i < AmountOfAdjacentRoomsToConnect; i++)
            {
                parentRooms[a].AddRoom(tempDictionary[distances[i]]);
            }

            for (int i = tempDictionary.Count - 1; i >= 0; i--)
            {
                if (bossSpawnRoom == parents[a].GetComponent<Room>())
                {
                    if (distances[i] > maxDistance)
                    {
                        maxDistance = (int)distances[i];
                        playerSpawnRoom = tempDictionary[distances[i]].GetComponent<Room>();
                    }
                }
            }
        }
    }

    private Vector2 CalculateCenterPoint(Vector2Int area, Vector2Int pos)
    {
        Vector2 tempRoomPos = pos;

        if (area.x == 2 || area.x == 4)
        {
            tempRoomPos.x -= 0.5f;
        }

        if (area.y == 2 || area.y == 4)
        {
            tempRoomPos.y -= 0.5f;
        }

        Vector2 tempPos = new Vector2(tempRoomPos.x + (area.x / 2), tempRoomPos.y + (area.y / 2));

        return tempPos;
    }

    private void GenerateWalls()
    {
        //Loop through the whole area
        //check each object, and then check all 8 blocks around if there is something
        for (int a = 0; a < areaWidth; a++)
        {
            for (int b = 0; b < areaHeight; b++)
            {
                if(CheckNullAndTag("Floor", true, area[a, b]))
                {
                    CreateWalls(a, b);

                    CreateOuterCorners(a, b);

                    CreateInnerCorners(a, b);
                }
            }
        }
    }

    //TODO: Make functions to compact code
    private void DoubleCheckCorners()
    {
        foreach (GameObject obj in area)
        {
            if (obj != null)
            {
                if (obj.tag == "OuterCorner")
                {
                    int a = (int)obj.transform.position.x;
                    int b = (int)obj.transform.position.z;

                    int amountOfThrough = 0;

                    //Check up and down
                    if(CheckNullAndTag("Floor", true, area[a, b + 1], area[a, b - 1]))
                    {
                        //Right side from up to down
                        if (CheckNullAndTag("Floor", true, area[a + 1, b + 1], area[a + 1, b - 1], area[a + 1, b]))
                        {
                            amountOfThrough += 1;
                            DestroyImmediate(area[a, b]);

                            int randomUcorner = Random.Range(0, uCorners.Count);
                            area[a, b] = Instantiate(uCorners[randomUcorner].gameObject, new Vector3(a, uCorners[randomUcorner].gameObject.transform.position.y, b), Quaternion.Euler(0, -90, 0), area[a + 1, b + 1].transform.parent);
                            area[a, b].name = uCorners[randomUcorner].name + ": 1";
                        }

                        //Left side from up to down
                        if (CheckNullAndTag("Floor", true, area[a - 1, b + 1], area[a - 1, b], area[a - 1, b - 1]))
                        {
                            amountOfThrough += 1;
                            DestroyImmediate(area[a, b]);
                            int randomUcorner = Random.Range(0, uCorners.Count);
                            area[a, b] = Instantiate(uCorners[randomUcorner].gameObject, new Vector3(a, uCorners[randomUcorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 90, 0), area[a - 1, b + 1].transform.parent);
                            area[a, b].name = uCorners[randomUcorner].name + ": 2";
                        }   
                    }

                    if (CheckNullAndTag("Floor", true, area[a + 1, b], area[a - 1, b]))
                    {
                        if (CheckNullAndTag("Floor", true, area[a, b + 1], area[a + 1, b + 1], area[a - 1, b + 1]))
                        {
                            amountOfThrough += 1;
                            DestroyImmediate(area[a, b]);
                            int randomUcorner = Random.Range(0, uCorners.Count);
                            area[a, b] = Instantiate(uCorners[randomUcorner].gameObject, new Vector3(a, uCorners[randomUcorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 180, 0), area[a, b + 1].transform.parent);
                            area[a, b].name = uCorners[randomUcorner].name + ": 3";
                        }

                        if (CheckNullAndTag("Floor", true, area[a, b - 1], area[a + 1, b - 1], area[a - 1, b - 1]))
                        {
                            amountOfThrough += 1;
                            DestroyImmediate(area[a, b]);
                            int randomUcorner = Random.Range(0, uCorners.Count);
                            area[a, b] = Instantiate(uCorners[randomUcorner].gameObject, new Vector3(a, uCorners[randomUcorner].gameObject.transform.position.y, b), Quaternion.identity, area[a, b - 1].transform.parent);
                            area[a, b].name = uCorners[randomUcorner].name + ": 4";
                        } 
                    }

                    if (amountOfThrough == 4)
                    {
                        //THIS ISN?T WORKING
                        DestroyImmediate(area[a, b]);
                        int randomUcorner = Random.Range(0, uCorners.Count);
                        area[a, b] = Instantiate(squareCorners[randomUcorner].gameObject, new Vector3(a, uCorners[randomUcorner].gameObject.transform.position.y, b), Quaternion.identity, area[a, b - 1].transform.parent);
                        area[a, b].name = uCorners[randomUcorner].name + ": SQUARE";
                    }
                }
                else if (obj.tag == "Wall")
                {
                    float localX = obj.transform.position.x;
                    float localY = obj.transform.position.z;

                    int a = 0;
                    int b = 0;

                    if (localX % 1 > 0.5f)
                    {
                        a = (int)obj.transform.position.x + 1;

                    }
                    else if (localX % 1 < 0.5f)
                    {
                        a = (int)obj.transform.position.x;
                    }

                    if (localY % 1 > 0.5f)
                    {
                        b = (int)obj.transform.position.z + 1;
                    }
                    else if (localY % 1 < 0.5f)
                    {
                        b = (int)obj.transform.position.z;
                    }


                    if (CheckNullAndTag("Floor", true, area[a + 1, b], area[a + 1, b - 1], area[a, b - 1]))
                    {
                        if (CheckNullAndTag("Floor", false, area[a - 1, b], area[a, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.identity, area[a + 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 1a";
                        }

                        if (CheckNullAndTag("Floor", false, area[a - 1, b], area[a + 1, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.identity, area[a + 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 1b";
                        }

                        if (CheckNullAndTag("Floor", false, area[a - 1, b - 1], area[a, b]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.identity, area[a + 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 1c";
                        }

                        if (CheckNullAndTag("Floor", false, area[a - 1, b - 1], area[a + 1, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.identity, area[a + 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 1d";
                        }

                        if (CheckNullAndTag("Floor", true, area[a - 1, b - 1], area[a - 1, b]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomUcorner = Random.Range(0, uCorners.Count);
                            area[a, b] = Instantiate(uCorners[randomUcorner].gameObject, new Vector3(a, uCorners[randomUcorner].gameObject.transform.position.y, b), Quaternion.identity, area[a + 1, b].transform.parent);
                            area[a, b].name = uCorners[randomUcorner].name + ": 1e";
                        }
                    }

                    if (CheckNullAndTag("Floor", true, area[a - 1, b], area[a - 1, b - 1], area[a, b - 1]))
                    {
                        if (CheckNullAndTag("Floor", false, area[a + 1, b], area[a, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 90, 0), area[a - 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 2a";
                        }

                        if (CheckNullAndTag("Floor", false, area[a + 1, b - 1], area[a, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 90, 0), area[a - 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 2b";
                        }

                        if (CheckNullAndTag("Floor", false, area[a + 1, b], area[a - 1, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 90, 0), area[a - 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 2c";
                        }

                        if (CheckNullAndTag("Floor", false, area[a + 1, b - 1], area[a - 1, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 90, 0), area[a - 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 2d";
                        }

                        if (CheckNullAndTag("Floor", true, area[a - 1, b + 1], area[a, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomUcorner = Random.Range(0, uCorners.Count);
                            area[a, b] = Instantiate(uCorners[randomUcorner].gameObject, new Vector3(a, uCorners[randomUcorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 90, 0), area[a - 1, b].transform.parent);
                            area[a, b].name = uCorners[randomUcorner].name + ": 2e";
                        }
                    }

                    if (CheckNullAndTag("Floor", true, area[a - 1, b], area[a - 1, b + 1], area[a, b + 1]))
                    {
                        if (CheckNullAndTag("Floor", false, area[a, b - 1], area[a + 1, b]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 180, 0), area[a - 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 3a";
                        }

                        if (CheckNullAndTag("Floor", false, area[a, b - 1], area[a + 1, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 180, 0), area[a - 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 3b";
                        }

                        if (CheckNullAndTag("Floor", false, area[a - 1, b - 1], area[a + 1, b]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 180, 0), area[a - 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 3cunts";
                        }

                        if (CheckNullAndTag("Floor", false, area[a - 1, b - 1], area[a + 1, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 180, 0), area[a - 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 3d";
                        }

                        if (CheckNullAndTag("Floor", true, area[a + 1, b + 1], area[a + 1, b ]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomUcorner = Random.Range(0, uCorners.Count);
                            area[a, b] = Instantiate(uCorners[randomUcorner].gameObject, new Vector3(a, uCorners[randomUcorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 180, 0), area[a - 1, b].transform.parent);
                            area[a, b].name = uCorners[randomUcorner].name + ": 3e";
                        }
                    }

                    if (CheckNullAndTag("Floor", true, area[a + 1, b], area[a + 1, b + 1], area[a, b + 1]))
                    {
                        if (CheckNullAndTag("Floor", false, area[a, b - 1], area[a - 1, b]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, -90, 0), area[a + 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 4a";
                        }

                        if (CheckNullAndTag("Floor", false, area[a, b - 1], area[a - 1, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, -90, 0), area[a + 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 4b";
                        }

                        if (CheckNullAndTag("Floor", false, area[a + 1, b - 1], area[a - 1, b]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, -90, 0), area[a + 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 4c";
                        }

                        if (CheckNullAndTag("Floor", false, area[a + 1, b - 1], area[a - 1, b + 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomOutercorner = Random.Range(0, outerCorners.Count);
                            area[a, b] = Instantiate(outerCorners[randomOutercorner].gameObject, new Vector3(a, outerCorners[randomOutercorner].gameObject.transform.position.y, b), Quaternion.Euler(0, -90, 0), area[a + 1, b].transform.parent);
                            area[a, b].name = outerCorners[randomOutercorner].name + ": 4d";
                        }

                        if (CheckNullAndTag("Floor", true, area[a, b - 1], area[a + 1, b - 1]))
                        {
                            DestroyImmediate(area[a, b]);
                            int randomUcorner = Random.Range(0, uCorners.Count);
                            area[a, b] = Instantiate(uCorners[randomUcorner].gameObject, new Vector3(a, uCorners[randomUcorner].gameObject.transform.position.y, b), Quaternion.Euler(0, -90, 0), area[a + 1, b].transform.parent);
                            area[a, b].name = uCorners[randomUcorner].name + ": 4e";
                        }
                    }
                }
            }
        }
    }

    //TODO: Make functions to compact code
    private void CreateWalls(int a, int b)
    {
        //CREATE WALLS MAKE THIS FUNCTION
        //========================================================
        //Check all sides, counting from up, down, left and rights
        //Check each side and see which one needs a wall
        bool isXBigger = false;

        //Check if Left side of floor needs wall
        if (CheckIfNull(area[a - 1, b]))
        {
            int angle = 0;
            int randomWall = Random.Range(0, walls.Count);

            float displacement = CheckWallSize(walls[randomWall].gameObject, ref isXBigger);
            displacement = displacement / 2;

            if (isXBigger) angle = 90;

            area[a - 1, b] = Instantiate(walls[randomWall].gameObject, new Vector3(a - 0.5f - displacement, walls[randomWall].gameObject.transform.position.y, b), Quaternion.Euler(new Vector3(0, angle, 0)));
            area[a - 1, b].name = walls[randomWall].name + " 1 A";
            //Check if walls are needed on the other side in the same block

            if (CheckNullAndTag("Floor", true, area[a - 2, b]))
            {
                angle = 0;
                randomWall = Random.Range(0, walls.Count);

                displacement = CheckWallSize(walls[randomWall].gameObject, ref isXBigger);
                displacement = displacement / 2;

                if (isXBigger) angle = 90;

                //Create a corridor on the other side, but the same block itself
                GameObject localObject = Instantiate(walls[randomWall].gameObject, new Vector3(a - 1.5f + displacement, walls[randomWall].gameObject.transform.position.y, b), Quaternion.Euler(new Vector3(0, angle, 0)));
                localObject.name = walls[randomWall].name + " 1 B";
                //Set this object's parent to be the object created previously, so when it needs to get destroyed it will get destroyed
                //Since it can't be accessed through the array.
                localObject.transform.parent = area[a - 1, b].transform;
            }

            area[a - 1, b].transform.SetParent(area[a, b].transform.parent);
        }

        //Check if Down side of floor needs wall
        if (CheckIfNull(area[a, b - 1]))
        {
            int angle = -90;
            int randomWall = Random.Range(0, walls.Count);

            float displacement = CheckWallSize(walls[randomWall].gameObject, ref isXBigger);
            displacement = displacement / 2;

            if (isXBigger) angle = 0;

            area[a, b - 1] = Instantiate(walls[randomWall].gameObject, new Vector3(a, walls[randomWall].gameObject.transform.position.y, b - 0.5f - displacement), Quaternion.Euler(new Vector3(0, angle, 0)));
            area[a, b - 1].name = walls[randomWall].name + " 2 A";

            //Check if walls are needed on the other side in the same block
            if (CheckNullAndTag("Floor", true, area[a, b - 2]))
            {
                angle = -90;
                randomWall = Random.Range(0, walls.Count);
                displacement = CheckWallSize(walls[randomWall].gameObject, ref isXBigger);
                displacement = displacement / 2;

                if (isXBigger) angle = 0;

                //Create a corridor on the other side, but the same block itself
                GameObject localObject = Instantiate(walls[randomWall].gameObject, new Vector3(a, walls[randomWall].gameObject.transform.position.y, b - 1.5f + displacement), Quaternion.Euler(new Vector3(0, angle, 0)));
                localObject.name = walls[randomWall].name + " 2 B";
                //Set this object's parent to be the object created previously, so when it needs to get destroyed it will get destroyed
                //Since it can't be accessed through the array.
                localObject.transform.parent = area[a, b - 1].transform;
            }

            area[a, b - 1].transform.SetParent(area[a, b].transform.parent);
        }

        //Check if Up side of floor needs wall
        if (CheckIfNull(area[a, b + 1]))
        {
            int angle = 90;
            int randomWall = Random.Range(0, walls.Count);

            float displacement = CheckWallSize(walls[randomWall].gameObject, ref isXBigger);
            displacement = displacement / 2;

            if (isXBigger) angle = 180;

            area[a, b + 1] = Instantiate(walls[randomWall].gameObject, new Vector3(a, walls[randomWall].gameObject.transform.position.y, b + 0.5f + displacement), Quaternion.Euler(new Vector3(0, angle, 0)));
            area[a, b + 1].name = walls[randomWall].name + " 3 A";
            //Check if walls are needed on the other side in the same block
            if (CheckNullAndTag("Floor", true, area[a, b + 2]))
            {
                angle = 90;
                randomWall = Random.Range(0, walls.Count);
                displacement = CheckWallSize(walls[randomWall].gameObject, ref isXBigger);
                displacement = displacement / 2;

                if (isXBigger) angle = 180;

                //Create a corridor on the other side, but the same block itself
                GameObject localObject = Instantiate(walls[randomWall].gameObject, new Vector3(a, walls[randomWall].gameObject.transform.position.y, b + 1.5f - displacement), Quaternion.Euler(new Vector3(0, angle, 0)));
                localObject.name = walls[randomWall].name + "3 B";
                //Set this object's parent to be the object created previously, so when it needs to get destroyed it will get destroyed
                //Since it can't be accessed through the array.
                localObject.transform.parent = area[a, b + 1].transform;
            }

            area[a, b + 1].transform.SetParent(area[a, b].transform.parent);
        }

        //Check if Right side of floor needs wall
        if (CheckIfNull(area[a + 1, b]))
        {
            int angle = 180;
            int randomWall = Random.Range(0, walls.Count);

            float displacement = CheckWallSize(walls[randomWall].gameObject, ref isXBigger);
            displacement = displacement / 2;

            if(isXBigger) angle = 90;

            area[a + 1, b] = Instantiate(walls[randomWall].gameObject, new Vector3(a + 0.5f + displacement, walls[randomWall].gameObject.transform.position.y, b), Quaternion.Euler(new Vector3(0, angle, 0)));
            area[a + 1, b].name = walls[randomWall].name + " 4 A";

            //Check if walls are needed on the other side in the same block
            if (CheckNullAndTag("Floor", true, area[a + 2, b]))
            {
                angle = 180;
                randomWall = Random.Range(0, walls.Count);
                //Create a corridor on the other side, but the same block itself
                displacement = CheckWallSize(walls[randomWall].gameObject, ref isXBigger);
                displacement = displacement / 2;

                if (isXBigger) angle = 90;

                GameObject localObject = Instantiate(walls[randomWall].gameObject, new Vector3(a + 1.5f - displacement, walls[randomWall].gameObject.transform.position.y, area[a + 1, b].transform.position.z), Quaternion.Euler(new Vector3(0, angle, 0)));
                localObject.name = walls[randomWall].name + "4 B";
                //Set this object's parent to be the object created previously, so when it needs to get destroyed it will get destroyed
                //Since it can't be accessed through the array.
                localObject.transform.parent = area[a + 1, b].transform;
            }

            area[a + 1, b].transform.SetParent(area[a, b].transform.parent);
        }
    }

    private void CreateOuterCorners(int a, int b)
    {
        //Check if Right and Left side of floor have walls
        int number;
        int extraRotation = 0;

        //Always check if it is null first before trying to get the tag of the object.
        if (CheckNullAndTag("Floor", false, area[a + 1, b], area[a - 1, b]))
        { 
            //Check if object down is a floor
            if (CheckNullAndTag("Floor", true, area[a, b - 1]))
            {
                //Check if Object down and to the right is a floor

                if (CheckNullAndTag("Floor", true, area[a + 1, b - 1]))
                {
                    number = 1;
                    int randomOuterCorner = Random.Range(0, outerCorners.Count);

                    //Destroy previous object in the list and create new one
                    DestroyImmediate(area[a + 1, b]);
                    area[a + 1, b] = Instantiate(outerCorners[randomOuterCorner].gameObject, new Vector3(a + 1, outerCorners[randomOuterCorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 90, 0), area[a + 1, b - 1].transform.parent);
                    area[a + 1, b].name = outerCorners[randomOuterCorner].name + number;
                }

                if (CheckNullAndTag("Floor", true, (area[a - 1, b - 1])))
                {
                    number = 2;
                    int randomOuterCorner = Random.Range(0, outerCorners.Count);

                    //Destroy previous object in the list and create new one
                    DestroyImmediate(area[a - 1, b]);
                    area[a - 1, b] = Instantiate(outerCorners[randomOuterCorner].gameObject, new Vector3(a - 1, outerCorners[randomOuterCorner].gameObject.transform.position.y, b), Quaternion.identity, (area[a - 1, b - 1]).transform.parent);
                    area[a - 1, b].name = outerCorners[randomOuterCorner].name + number;
                }
            }

            //Check if object Up is a floor
            if (CheckNullAndTag("Floor", true, area[a, b + 1]))
            {
                //Check if Object up and to the left is a floor
                if (CheckNullAndTag("Floor", true, area[a - 1, b + 1]))
                {
                    number = 3;
                    int randomOuterCorner = Random.Range(0, outerCorners.Count);
                    //Destroy previous object in the list and create new one
                    DestroyImmediate(area[a - 1, b]);
                    area[a - 1, b] = Instantiate(outerCorners[randomOuterCorner].gameObject, new Vector3(a - 1, outerCorners[randomOuterCorner].gameObject.transform.position.y, b), Quaternion.Euler(0, -90, 0), area[a - 1, b + 1].transform.parent);
                    area[a - 1, b].name = outerCorners[randomOuterCorner].name + number;
                }

                //Check if Object up and to the right is a floor
                if (CheckNullAndTag("Floor", true, area[a + 1, b + 1]))
                {
                    number = 4;
                    int randomOuterCorner = Random.Range(0, outerCorners.Count);
                    //Destroy previous object in the list and create new one
                    DestroyImmediate(area[a + 1, b]);
                    area[a + 1, b] = Instantiate(outerCorners[randomOuterCorner].gameObject, new Vector3(a + 1, outerCorners[randomOuterCorner].gameObject.transform.position.y, b), Quaternion.Euler(0, 180, 0), area[a + 1, b + 1].transform.parent);
                    area[a + 1, b].name = outerCorners[randomOuterCorner].name + number;
                }
            }
        }

        //Check if Up and Down side of floor have walls

        //Always check if it is null first before trying to get the tag of the object.
        if (CheckNullAndTag("Floor", false, area[a, b + 1], area[a, b - 1]))
        {

            //Check if object to the right is a floor
            if (CheckNullAndTag("Floor", true, area[a + 1, b]))
            {
                // Check if Object down and to the right is a floor

                if (CheckNullAndTag("Floor", true, area[a + 1, b - 1]))
                {
                    number = 5;
                    int randomOuterCorner = Random.Range(0, outerCorners.Count);
                    //Destroy previous object in the list and create new one
                    DestroyImmediate(area[a, b - 1]);
                    area[a, b - 1] = Instantiate(outerCorners[randomOuterCorner].gameObject, new Vector3(a, outerCorners[randomOuterCorner].gameObject.transform.position.y, b - 1), Quaternion.Euler(0, -90, 0), area[a + 1, b - 1].transform.parent);
                    area[a, b - 1].name = outerCorners[randomOuterCorner].name + number;
                }

                // Check if Object up and to the right is a floor
                if (CheckNullAndTag("Floor", true, area[a + 1, b + 1])) 
                {
                    number = 6;
                    int randomOuterCorner = Random.Range(0, outerCorners.Count);
                    //Destroy previous object in the list and create new one
                    DestroyImmediate(area[a, b + 1]);
                    area[a, b + 1] = Instantiate(outerCorners[randomOuterCorner].gameObject, new Vector3(a, outerCorners[randomOuterCorner].gameObject.transform.position.y, b + 1), Quaternion.identity, area[a + 1, b + 1].transform.parent);
                    area[a, b + 1].name = outerCorners[randomOuterCorner].name + number;
                }
            }

            //Check if object to the left is a floor
            if(CheckNullAndTag("Floor", true, area[a - 1, b]))
            {
                // Check if Object down and to the left is a floor
                if(CheckNullAndTag("Floor", true, area[a - 1, b - 1]))
                {
                    number = 7;
                    int randomOuterCorner = Random.Range(0, outerCorners.Count);
                    //Destroy previous object in the list and create new one
                    DestroyImmediate(area[a, b - 1]);
                    area[a, b - 1] = Instantiate(outerCorners[randomOuterCorner].gameObject, new Vector3(a, outerCorners[randomOuterCorner].gameObject.transform.position.y, b - 1), Quaternion.Euler(0, 180, 0), area[a - 1, b - 1].transform.parent);
                    area[a, b - 1].name = outerCorners[randomOuterCorner].name + number;
                }

                // Check if Object up and to the left is a floor

                if (CheckNullAndTag("Floor", true, area[a - 1, b + 1]))
                {               
                    number = 8;
                    int randomOuterCorner = Random.Range(0, outerCorners.Count);
                    //Destroy previous object in the list and create new one
                    DestroyImmediate(area[a, b + 1]);
                    area[a, b + 1] = Instantiate(outerCorners[randomOuterCorner].gameObject, new Vector3(a, outerCorners[randomOuterCorner].gameObject.transform.position.y, b + 1), Quaternion.Euler(0, 90, 0), area[a - 1, b + 1].transform.parent);
                    area[a, b + 1].name = outerCorners[randomOuterCorner].name + number;
                }
            }
        }
    }

    //TODO: Make functions to compact code
    private void CreateInnerCorners(int a, int b)
    {
        if(innerCorners.Count > 0)
        {
            int randomInnerCorner = Random.Range(0, this.innerCorners.Count );
            GameObject innerCorner = null;
            Vector2Int innerCornersCoords = new Vector2Int();
    
            if (CheckNullAndTag("Wall", "OuterCorner", true, area[a - 1, b], area[a, b - 1]))
            {
                innerCornersCoords.x = -1;
                innerCornersCoords.y = -1;
    
                innerCorner = Instantiate(this.innerCorners[randomInnerCorner].gameObject, new Vector3(a - this.innerCorners[randomInnerCorner].gameObject.transform.position.x, this.innerCorners[randomInnerCorner].gameObject.transform.position.y, b - this.innerCorners[randomInnerCorner].gameObject.transform.position.z), Quaternion.identity, area[a - 1, b].transform.parent);
            }
            else if (CheckNullAndTag("Wall", "OuterCorner", true, area[a - 1, b], area[a, b + 1]))
            {
                innerCornersCoords.x = -1;
                innerCornersCoords.y = 1;
    
                innerCorner = Instantiate(this.innerCorners[randomInnerCorner].gameObject, new Vector3(a - this.innerCorners[randomInnerCorner].gameObject.transform.position.x, this.innerCorners[randomInnerCorner].gameObject.transform.position.y, b + this.innerCorners[randomInnerCorner].gameObject.transform.position.z), Quaternion.Euler(new Vector3(0, 90, 0)), area[a - 1, b].transform.parent);
            }
            else if (CheckNullAndTag("Wall", "OuterCorner", true, area[a + 1, b], area[a, b - 1]))
            {
                innerCornersCoords.x = 1;
                innerCornersCoords.y = -1;
    
                innerCorner = Instantiate(this.innerCorners[randomInnerCorner].gameObject, new Vector3(a + this.innerCorners[randomInnerCorner].gameObject.transform.position.x, this.innerCorners[randomInnerCorner].gameObject.transform.position.y, b - this.innerCorners[randomInnerCorner].gameObject.transform.position.z), Quaternion.Euler(new Vector3(0, -90, 0)), area[a + 1, b].transform.parent);
            }
            else if (CheckNullAndTag("Wall", "OuterCorner", true, area[a + 1, b], area[a, b + 1]))
            {
                innerCornersCoords.x = 1;
                innerCornersCoords.y = 1;
    
                innerCorner = Instantiate(this.innerCorners[randomInnerCorner].gameObject, new Vector3(a + this.innerCorners[randomInnerCorner].gameObject.transform.position.x, this.innerCorners[randomInnerCorner].gameObject.transform.position.y, b + this.innerCorners[randomInnerCorner].gameObject.transform.position.z), Quaternion.Euler(new Vector3(0, 180, 0)), area[a + 1, b].transform.parent);
            }
    
            if(deleteInnerCorners && innerCornersCoords.x != 0 && innerCornersCoords.y != 0)
            {
                DestroyImmediate(area[a + innerCornersCoords.x, b]);
                DestroyImmediate(area[a, b + innerCornersCoords.y]);
            }
    
            if(innerCorner != null)
            innerCorner.name = this.innerCorners[randomInnerCorner].name;
        }
    }

    private bool CheckNullAndTag(string tag, bool isTagEqual, params GameObject[] obj)
    {
        List<int> checkList = new List<int>();

        for (int i = 0; i < obj.Length; i++)
        {
            if (obj[i] != null)
            {
                if (isTagEqual)
                {
                    if (obj[i].tag == tag)
                    {
                        checkList.Add(i);
                    }
                }
                else
                {
                    if (obj[i].tag != tag)
                    {
                        checkList.Add(i);
                    }
                }
            }
        }
        
        if(checkList.Count == obj.Length)
        {
            return true;
        }
        
        return false;
    }

    private bool CheckNullAndTag(string tag1, string tag2, bool isTagEqual, params GameObject[] obj)
    {
        List<int> checkList = new List<int>();

        for (int i = 0; i < obj.Length; i++)
        {
            if (obj[i] != null)
            {
                if (isTagEqual)
                {
                    if (obj[i].tag == tag1 || obj[i].tag == tag2)
                    {
                        checkList.Add(i);
                    }
                }
                else
                {
                    if (obj[i].tag != tag1 && obj[i].tag != tag2)
                    {
                        checkList.Add(i);
                    }
                }
            }
        }

        if (checkList.Count == obj.Length)
        {
            return true;
        }

        return false;
    }

    private bool CheckIfNull(params GameObject[] obj)
    {
        List<int> checkList = new List<int>();

        for (int i = 0; i < obj.Length; i++)
        {
            if (obj[i] == null)
            {
                checkList.Add(i);
            }
        }

        if (checkList.Count == obj.Length)
        {
            return true;
        }

        return false;
    }

    private float CheckWallSize(GameObject obj, ref bool isXBigger)
    {
        Vector3 size = obj.GetComponent<MeshRenderer>().bounds.size;
        float displacement;
        isXBigger = false;

        if (size.x > size.z)
        {
            isXBigger = true;
            displacement = size.z;
        }
        else if (size.z > size.x)
        {
            displacement = size.x;
        }
        else
        {
            displacement = 0;
        }

        return displacement;
    }

    //TODO: Make functions to compact code
    private void DoubleCheckRoomCorners()
    {
        for (int i = 0; i < parentRooms.Length; i++)
        {
            for (int j = 0; j < parentRooms[i].Corners.Count; j++)
            {
                string tempString = parentRooms[i].Corners[j].name;
                string[] tempStringArray = tempString.Split(floorRemovables, System.StringSplitOptions.RemoveEmptyEntries);

                int tempIntY = int.Parse(tempStringArray[1]);
                int tempIntX = int.Parse(tempStringArray[0]);

                int randomWall = 0;

                if (area[tempIntX + 1, tempIntY].CompareTag("OuterCorner"))
                {
                    if(area[tempIntX, tempIntY + 1].CompareTag("OuterCorner"))
                    {
                        randomWall = Random.Range(0, walls.Count);
                        GameObject tempWall = Instantiate(walls[randomWall].gameObject, area[tempIntX + 1, tempIntY].transform.position, Quaternion.Euler(0, 0, 0), parentRooms[i].transform);
                        tempWall.name = walls[randomWall].name;

                        randomWall = Random.Range(0, walls.Count);
                        tempWall = Instantiate(walls[randomWall].gameObject, area[tempIntX, tempIntY + 1].transform.position, Quaternion.Euler(0, 0, 0), parentRooms[i].transform);
                        tempWall.name = walls[randomWall].name;
                    }

                    if(area[tempIntX, tempIntY - 1].CompareTag("OuterCorner"))
                    {
                        randomWall = Random.Range(0, walls.Count);
                        GameObject tempWall = Instantiate(walls[randomWall].gameObject, area[tempIntX + 1, tempIntY].transform.position, Quaternion.Euler(0, 0, 0), parentRooms[i].transform);
                        tempWall.name = walls[randomWall].name;

                        randomWall = Random.Range(0, walls.Count);
                        tempWall = Instantiate(walls[randomWall].gameObject, area[tempIntX, tempIntY - 1].transform.position, Quaternion.Euler(0,0,0), parentRooms[i].transform);
                        tempWall.name = walls[randomWall].name;
                    }
                }

                if (area[tempIntX - 1, tempIntY].CompareTag("OuterCorner"))
                {
                    if (area[tempIntX, tempIntY + 1].CompareTag("OuterCorner"))
                    {
                        randomWall = Random.Range(0, walls.Count);
                        GameObject tempWall = Instantiate(walls[randomWall].gameObject, area[tempIntX - 1, tempIntY].transform.position, Quaternion.Euler(0, 0, 0), parentRooms[i].transform);
                        tempWall.name = walls[randomWall].name;

                        randomWall = Random.Range(0, walls.Count);
                        tempWall = Instantiate(walls[randomWall].gameObject, area[tempIntX, tempIntY + 1].transform.position, Quaternion.Euler(0, 0, 0), parentRooms[i].transform);
                        tempWall.name = walls[randomWall].name;
                    }

                    if (area[tempIntX, tempIntY - 1].CompareTag("OuterCorner"))
                    {
                        randomWall = Random.Range(0, walls.Count);
                        GameObject tempWall = Instantiate(walls[randomWall].gameObject, area[tempIntX - 1, tempIntY].transform.position, Quaternion.Euler(0, 0, 0), parentRooms[i].transform);
                        tempWall.name = walls[randomWall].name;

                        randomWall = Random.Range(0, walls.Count);
                        tempWall = Instantiate(walls[randomWall].gameObject, area[tempIntX, tempIntY - 1].transform.position, Quaternion.Euler(0, 0, 0), parentRooms[i].transform);
                        tempWall.name = walls[randomWall].name;
                    }
                }
            }
        }
    }

    private void GenerateDoors()
    {
        if(doorGameObjects.Count > 0)
        {
            for (int a = 0; a < areaWidth; a++)
            {
                for (int b = 0; b < areaHeight; b++)
                {
                    if(CheckNullAndTag("Floor", true, area[a, b]))
                    {
                        if(CheckNullAndTag("Floor", false, area[a + 1, b], area[a - 1, b]))
                        {
                            if(CheckNullAndTag("Floor", true, area[a, b + 1]))
                            {
                                if(CheckNullAndTag("Floor", true, area[a + 1, b + 1]) || CheckNullAndTag("Floor", true, area[a - 1, b + 1]))
                                {
                                    //Instantiate doors
                                    if (CheckNullAndTag("Floor", true, area[a - 1, b + 2]) || CheckNullAndTag("Floor", true, area[a + 1, b - 2]))
                                    {
                                        int randomDoor = Random.Range(0, doors.Count);

                                        Vector3 pos = new Vector3(area[a, b].transform.position.x, area[a, b].transform.position.y, area[a, b].transform.position.z + doorsDisplacements[randomDoor].z);
                                        doorGameObjects.Add(Instantiate(doors[randomDoor].gameObject, pos, Quaternion.identity, area[a, b].transform.parent)); 
                                    }
                                }
                            }

                            if (CheckNullAndTag("Floor", true, area[a, b - 1]))
                            {
                                if (CheckNullAndTag("Floor", true, area[a + 1, b - 1]) || CheckNullAndTag("Floor", true, area[a - 1, b - 1]))
                                {
                                    if (CheckNullAndTag("Floor", true, area[a - 1, b - 2]) || CheckNullAndTag("Floor", true, area[a + 1, b - 2]))
                                    {
                                        int randomDoor = Random.Range(0, doors.Count);

                                        Vector3 pos = new Vector3(area[a, b].transform.position.x, area[a, b].transform.position.y, area[a, b].transform.position.z - doorsDisplacements[randomDoor].z);
                                        doorGameObjects.Add(Instantiate(doors[randomDoor].gameObject, pos, Quaternion.identity, area[a, b].transform.parent));
                                    }
                                }
                            }
                        }
                        else if (CheckNullAndTag("Floor", false, area[a, b - 1], area[a, b + 1]))
                        {
                            if (CheckNullAndTag("Floor", true, area[a + 1, b]))
                            {
                                if (CheckNullAndTag("Floor", true, area[a + 1, b - 1]) || CheckNullAndTag("Floor", true, area[a + 1, b + 1]))
                                {
                                    if (CheckNullAndTag("Floor", true, area[a + 2, b - 1]) || CheckNullAndTag("Floor", true, area[a + 2, b + 1]))
                                    {
                                        int randomDoor = Random.Range(0, doors.Count);

                                        Vector3 pos = new Vector3(area[a, b].transform.position.x + doorsDisplacements[randomDoor].z, area[a, b].transform.position.y, area[a, b].transform.position.z);
                                        doorGameObjects.Add(Instantiate(doors[randomDoor].gameObject, pos, Quaternion.Euler(0, 90, 0), area[a, b].transform.parent));
                                    }
                                }
                            }

                            if (CheckNullAndTag("Floor", true, area[a - 1, b]))
                            {
                                if (CheckNullAndTag("Floor", true, area[a - 1, b - 1]) || CheckNullAndTag("Floor", true, area[a - 1, b + 1]))
                                {
                                    if(CheckNullAndTag("Floor", true, area[a - 2, b - 1]) || CheckNullAndTag("Floor", true, area[a - 2, b + 1]))
                                    {
                                        int randomDoor = Random.Range(0, doors.Count);

                                        Vector3 pos = new Vector3(area[a, b].transform.position.x - doorsDisplacements[randomDoor].z, area[a, b].transform.position.y, area[a, b].transform.position.z);
                                        doorGameObjects.Add(Instantiate(doors[randomDoor].gameObject, pos, Quaternion.Euler(0, -90, 0), area[a, b].transform.parent));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void SpawnEntities()
    {
        DestroyImmediate(PlayerSpawnRoom.gameObject.GetComponent<BoxCollider>());

        if(player != null)
        { 
            player = Instantiate(Player, PlayerSpawnRoom.transform.position, Quaternion.identity, gameObject.transform);
        }

        if(bosses.Count > 0)
        {
            int randomBoss = Random.Range(0, bosses.Count);
            boss = Instantiate(bosses[randomBoss].gameObject, bossSpawnRoom.transform.position, Quaternion.identity, bossSpawnRoom.transform);
            bossSpawnRoom.name = "Boss Room";
            boss.name = bosses[randomBoss].name;
        }

        if(enemies.Count > 0)
        {
            SpawnEnemies();
        }
    }

    private void GenerateContent()
    {
        for (int i = 0; i < parentRooms.Length; i++)
        {
            GenerateRoomWallObjs(i);
        }

        GenerateCorridorWallObjs();

        GenerateObjects();
        
        GenerateMiddleObjects();
    }

    private void GenerateRoomWallObjs(int i)
    {
        if(wallObjects.Count > 0)
        {
            //Generate lighting in each 4 corners of the rooms
            parentRooms[i].Corners.Add(parentRooms[i].transform.GetChild(0).gameObject);
            //parentRooms[i].transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);

            int childCornerNumber = roomArea[i].y - 1;
            parentRooms[i].Corners.Add(parentRooms[i].transform.GetChild(childCornerNumber).gameObject);
            //parentRooms[i].transform.GetChild(childCornerNumber).gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);

            childCornerNumber = (roomArea[i].x * roomArea[i].y) - roomArea[i].y;
            parentRooms[i].Corners.Add(parentRooms[i].transform.GetChild(childCornerNumber).gameObject);
            //parentRooms[i].transform.GetChild(childCornerNumber).gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);

            childCornerNumber = (roomArea[i].x * roomArea[i].y) - 1;
            parentRooms[i].Corners.Add(parentRooms[i].transform.GetChild(childCornerNumber).gameObject);
            //parentRooms[i].transform.GetChild(childCornerNumber).gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);

            List<int> tempCorners = new List<int>();
            for (int j = 0; j < parentRooms[i].Corners.Count; j++)
            {
                Vector3Int tempVec3 = CheckSidesCorridor(parentRooms[i].Corners[j], 2);

                if (tempVec3.x == 1)
                {
                    int randomWallObject = Random.Range(0, wallObjects.Count);
                    Vector3 tempTorchDisplacement = new Vector3(-wallObjDisplacements[randomWallObject].x, wallObjDisplacements[randomWallObject].y, -wallObjDisplacements[randomWallObject].z);
                    float extraYAngle = 0;

                    if(j == 1 || j == 3)
                    {
                        tempTorchDisplacement.x = -tempTorchDisplacement.x;
                        tempTorchDisplacement.z = -tempTorchDisplacement.z;
                        extraYAngle = 180;   
                    }

                    GameObject tempWallObject = Instantiate(wallObjects[randomWallObject].gameObject, new Vector3(area[tempVec3.y, tempVec3.z].transform.position.x + tempTorchDisplacement.x, tempTorchDisplacement.y, area[tempVec3.y, tempVec3.z].transform.position.z + tempTorchDisplacement.z), Quaternion.Euler(0, extraYAngle, 0), parentRooms[i].transform);
                    tempWallObject.name = wallObjects[randomWallObject].name;
                }
                else
                {
                    //Remove from list
                    tempCorners.Add(j);
                }

                if(j == 3)
                {
                    int tempInt = 0;
                    for (int a = 0; a < tempCorners.Count; a++)
                    {
                        parentRooms[i].Corners.Remove(parentRooms[i].Corners[tempCorners[a] - tempInt]);

                        tempInt += 1;
                    }
                }
            }
        }
    }

    private void GenerateCorridorWallObjs()
    {
        if(wallObjects.Count > 0)
        {
            for (int i = 0; i < corridors.Count; i++)
            {
                if( corridors[i].transform.childCount > wallObjConsequitiveDistance)
                {
                    int checkIteration = 0;

                    for (int j = 0; j < corridors[i].transform.childCount; j++)
                    {
                        if(j > 0 && corridors[i].transform.GetChild(j).CompareTag("Floor"))
                        {
                            Vector3 position = corridors[i].transform.GetChild(j).transform.position;

                            if(area[(int)position.x + 1, (int)position.z].CompareTag("Wall") && area[(int)position.x - 1, (int)position.z].CompareTag("Wall") && checkIteration == wallObjConsequitiveDistance)
                            {
                                int randomWallObject = Random.Range(0, wallObjects.Count);
                                GameObject tempWallObject = Instantiate(wallObjects[randomWallObject].gameObject, new Vector3(position.x + wallObjDisplacements[randomWallObject].z, wallObjDisplacements[randomWallObject].y, position.z + wallObjDisplacements[randomWallObject].x), Quaternion.Euler(0, -90, 0), corridors[i].transform);
                                tempWallObject.name = wallObjects[randomWallObject].name;
                                checkIteration = 0;
                            }
                            else if(area[(int)position.x, (int)position.z + 1].CompareTag("Wall") && area[(int)position.x, (int)position.z - 1].CompareTag("Wall") && checkIteration == wallObjConsequitiveDistance)
                            {
                                int randomWallObject = Random.Range(0, wallObjects.Count);
                                GameObject tempWallObject = Instantiate(wallObjects[randomWallObject].gameObject, new Vector3(position.x + wallObjDisplacements[randomWallObject].x, wallObjDisplacements[randomWallObject].y, position.z + wallObjDisplacements[randomWallObject].z), Quaternion.Euler(0, 180, 0), corridors[i].transform);
                                tempWallObject.name = wallObjects[randomWallObject].name;
                                checkIteration = 0;  
                            }

                            checkIteration += 1;
                        }
                    }
                }
            }
        }
    }

    private void SpawnEnemies()
    {
        if(enemies.Count > 0)
        {
            for (int i = 0; i < parentRooms.Length; i++)
            {

                if (PlayerSpawnRoom != parentRooms[i] && bossSpawnRoom != parentRooms[i])
                {
                    int area = 0;

                    for (int j = 0; j < parentRooms[i].transform.childCount; j++)
                    {
                        if(parentRooms[i].transform.GetChild(j).CompareTag("Floor"))
                        {
                            area += 1;
                        }
                    }

                    int enem = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom);

                    for (int a = 0; a < enem; a++)
                    {
                        bool isInstantiated = false;

                        do
                        {
                            int randomChild = Random.Range(0, area);

                            GameObject tempChild = parentRooms[i].transform.GetChild(randomChild).gameObject;

                            if(tempChild.CompareTag("Floor") && !parentRooms[i].MidPoints.Contains(tempChild) && !parentRooms[i].Corners.Contains(tempChild))
                            {
                                Vector3 pos = tempChild.transform.position;

                                int randomEnemy = Random.Range(0, Enemies.Count);
                                int randomRotation = Random.Range(-360, 361);
                                GameObject tempEnemy = Instantiate(Enemies[randomEnemy].gameObject, new Vector3(pos.x, pos.y + Enemies[randomEnemy].gameObject.transform.position.y, pos.z), Quaternion.Euler(0, randomRotation, 0), parentRooms[i].transform);
                                tempEnemy.name = Enemies[randomEnemy].name;
                                

                                isInstantiated = true;
                            }

                        } while (isInstantiated == false);
                    }
                }
            }
        }
    }

    private Vector3Int CheckSidesCorridor(GameObject obj, int maxSurroundings)
    {
        int additive = 0;

        string tempString = obj.name;
        string[] tempStringArray = tempString.Split(floorRemovables, System.StringSplitOptions.RemoveEmptyEntries);

        int tempIntY = int.Parse(tempStringArray[1]);
        int tempIntX = int.Parse(tempStringArray[0]);

        if(area[tempIntX, tempIntY + 1].CompareTag("Floor"))
        {
            additive += 1;    
        }

        if (area[tempIntX, tempIntY - 1].CompareTag("Floor"))
        {
            additive += 1;
        }

        if (area[tempIntX + 1, tempIntY].CompareTag("Floor"))
        {
            additive += 1;
        }

        if (area[tempIntX - 1, tempIntY].CompareTag("Floor"))
        {
            additive += 1;
        }

        if(additive > maxSurroundings)
        {
            return new Vector3Int(0, 0, 0);
        }

        return new Vector3Int(1, tempIntX, tempIntY);
    }

    private Vector3Int CheckSidesCorridor(GameObject obj, int maxSurroundings, Vector2Int coords, params string[] tags)
    {
        int additive = 0;

        for (int i = 0; i < tags.Length; i++)
        {
            if(CheckNullAndTag(tags[i], true, area[coords.x, coords.y + 1]))
            {
                additive += 1;
            }

            if (CheckNullAndTag(tags[i], true, area[coords.x, coords.y - 1]))
            {
                additive += 1;
            }

            if (CheckNullAndTag(tags[i], true, area[coords.x + 1, coords.y]))
            {
                additive += 1;
            }

            if (CheckNullAndTag(tags[i], true, area[coords.x - 1, coords.y]))
            {
                additive += 1;
            }
        }

        if (additive > maxSurroundings)
        {
            return new Vector3Int(0, 0, 0);
        }

        return new Vector3Int(1, coords.x, coords.y);
    }

    //TODO: Make functions to compact code
    private void GenerateObjects()
    {
        if (staticObjects.Count > 0 && breakables.Count > 0)
        {
            int randomNumber = 0;

            int onlyXObjectMin = 0;
            int onlyXObjectMax = 2;

            if (staticObjects.Count == 0)
            {
                onlyXObjectMin = 1;
                onlyXObjectMax = 2;
            }
            else if (breakables.Count == 0)
            {
                onlyXObjectMin = 0;
                onlyXObjectMax = 1;
            }

            int staticOrBreakable = Random.Range(onlyXObjectMin, onlyXObjectMax);

            GameObject tempContent = null;

            if (staticOrBreakable == 0)
            {
                int randomStaticObject = Random.Range(0, staticObjects.Count);
                tempContent = staticObjects[randomStaticObject].gameObject;
                tempContent.name = staticObjects[randomStaticObject].name;
            }
            else
            {
                int randomBreakableObject = Random.Range(0, breakables.Count);
                tempContent = breakables[randomBreakableObject].gameObject;
                tempContent.name = breakables[randomBreakableObject].name;
            }

            for (int i = 0; i < parentRooms.Length; i++)
            {
                for (int j = 0; j < parentRooms[i].Corners.Count; j++)
                {
                    Vector2Int coords = new Vector2Int((int)parentRooms[i].Corners[j].transform.position.x, (int)parentRooms[i].Corners[j].transform.position.z);

                    Vector2Int additive = new Vector2Int();


                    if (area[coords.x + 1, coords.y].CompareTag("Floor"))
                    {
                        if (area[coords.x, coords.y + 1].CompareTag("Floor"))
                        {
                            additive.x = 1;
                            additive.y = 0;

                            if (CheckSidesCorridor(area[coords.x + additive.x, coords.y + additive.y], 3, new Vector2Int(coords.x + additive.x, coords.y + additive.y), "Floor", "Destroyables", "Static").x != 0)
                            {
                                RandomiseContent(ref randomNumber, ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);

                                CreateDestroyableRows(ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);
                            }

                            additive.x = 0;
                            additive.y = 1;

                            if (CheckSidesCorridor(area[coords.x + additive.x, coords.y + additive.y], 3, new Vector2Int(coords.x + additive.x, coords.y + additive.y), "Floor", "Destroyables", "Static").x != 0)
                            {
                                RandomiseContent(ref randomNumber, ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);

                                CreateDestroyableRows(ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);
                            }
                        }

                        if (area[coords.x, coords.y - 1].CompareTag("Floor"))
                        {
                            additive.x = 1;
                            additive.y = 0;

                            if (CheckSidesCorridor(area[coords.x + additive.x, coords.y + additive.y], 3, new Vector2Int(coords.x + additive.x, coords.y + additive.y), "Floor", "Destroyables", "Static").x != 0)
                            {
                                RandomiseContent(ref randomNumber, ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);

                                CreateDestroyableRows(ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);
                            }

                            additive.x = 0;
                            additive.y = -1;

                            if (CheckSidesCorridor(area[coords.x + additive.x, coords.y + additive.y], 3, new Vector2Int(coords.x + additive.x, coords.y + additive.y), "Floor", "Destroyables", "Static").x != 0)
                            {
                                RandomiseContent(ref randomNumber, ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);

                                CreateDestroyableRows(ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);
                            }
                        }
                    }

                    if (area[coords.x - 1, coords.y].CompareTag("Floor"))
                    {
                        if (area[coords.x, coords.y + 1].CompareTag("Floor"))
                        {
                            additive.x = -1;
                            additive.y = 0;

                            if (CheckSidesCorridor(area[coords.x + additive.x, coords.y + additive.y], 3, new Vector2Int(coords.x + additive.x, coords.y + additive.y), "Floor", "Destroyables", "Static").x != 0)
                            {
                                RandomiseContent(ref randomNumber, ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);

                                CreateDestroyableRows(ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);
                            }

                            additive.x = 0;
                            additive.y = 1;

                            if (CheckSidesCorridor(area[coords.x + additive.x, coords.y + additive.y], 3, new Vector2Int(coords.x + additive.x, coords.y + additive.y), "Floor", "Destroyables", "Static").x != 0)
                            {
                                RandomiseContent(ref randomNumber, ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);

                                CreateDestroyableRows(ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);
                            }
                        }

                        if (area[coords.x, coords.y - 1].CompareTag("Floor"))
                        {
                            additive.x = -1;
                            additive.y = 0;

                            if (CheckSidesCorridor(area[coords.x + additive.x, coords.y + additive.y], 3, new Vector2Int(coords.x + additive.x, coords.y + additive.y), "Floor", "Destroyables", "Static").x != 0)
                            {
                                RandomiseContent(ref randomNumber, ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);

                                CreateDestroyableRows(ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);
                            }

                            additive.x = 0;
                            additive.y = -1;

                            if (CheckSidesCorridor(area[coords.x + additive.x, coords.y + additive.y], 3, new Vector2Int(coords.x + additive.x, coords.y + additive.y), "Floor", "Destroyables", "Static").x != 0)
                            {
                                RandomiseContent(ref randomNumber, ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);

                                CreateDestroyableRows(ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);
                            }
                        }
                    }
                }
            }
        }
    }

    private void GenerateMiddleObjects()
    {
        if(middleObjects.Count > 0)
        {
            for (int a = 0; a < parentRooms.Length; a++)
            {
                for (int b = 0; b < parentRooms[a].MidPoints.Count; b++)
                {
                    int randomChange = Random.Range(0, 101);
                    
                    if(randomChange > 0 && randomChange < middleObjectsRate)
                    {
                        int randomMiddleObject = Random.Range(0, middleObjects.Count);
                        int randomRotation = Random.Range(-360, 361);

                        if(parentRooms[a] != playerSpawnRoom && parentRooms[a] != bossSpawnRoom)
                        {
                            GameObject tempMiddleObject = Instantiate(middleObjects[randomMiddleObject].gameObject, parentRooms[a].MidPoints[b].transform.position, Quaternion.Euler(0, randomRotation, 0), parentRooms[a].transform);
                            tempMiddleObject.name = middleObjects[randomMiddleObject].name;
                        }
                    }
                }
            }
        }
    }

    //TODO: Make functions to compact code
    private void RandomiseContent(ref int randomNumber, ref int staticOrBreakable, ref GameObject tempContent, ref Vector2Int coords, ref int i, ref int j, ref Vector2Int additive)
    {
        randomNumber = Random.Range(0, 101);

        if (staticOrBreakable == 0)
        {
            int randomStaticObject = Random.Range(0, staticObjects.Count);
            tempContent = staticObjects[randomStaticObject].gameObject;
            tempContent.name = staticObjects[randomStaticObject].name;
        }
        else
        {
            int randomBreakableObject = Random.Range(0, breakables.Count);
            tempContent = breakables[randomBreakableObject].gameObject;
            tempContent.name = breakables[randomBreakableObject].name;
        }

        //If not floor, delete it, most likely it is a destroyable object
        if(!area[coords.x + additive.x, coords.y + additive.y].CompareTag("Floor") && !area[coords.x + additive.x, coords.y + additive.y].CompareTag("Wall") && !area[coords.x + additive.x, coords.y + additive.y].CompareTag("OuterCorner"))
        {
            DestroyImmediate(area[coords.x + additive.x, coords.y + additive.y]);
        }

        if(!CheckIfNull(area[coords.x + additive.x, coords.y + additive.y], parentRooms[i].Corners[j]) && area[coords.x + additive.x, coords.y + additive.y].transform.parent != null)
        {
            if (randomNumber > 0 && randomNumber <= cornerObjectsRate)
            {
                if(area[coords.x + additive.x, coords.y + additive.y].transform.parent.CompareTag("Room") && area[coords.x + additive.x, coords.y + additive.y].transform.position != parentRooms[i].Corners[j].transform.position)
                {
                    area[coords.x + additive.x, coords.y + additive.y] = Instantiate(tempContent, area[coords.x + additive.x, coords.y + additive.y].transform.position, Quaternion.identity, parentRooms[i].Corners[j].transform.parent.transform);

                    area[coords.x + additive.x, coords.y + additive.y].transform.position = new Vector3(area[coords.x + additive.x, coords.y + additive.y].transform.position.x, tempContent.transform.position.y, area[coords.x + additive.x, coords.y + additive.y].transform.position.z);
                }
            }
        }
    }

    //TODO: Make functions to compact code
    private void CreateDestroyableRows(ref int staticOrBreakable, ref GameObject tempContent, ref Vector2Int coords, ref int i, ref int j, ref Vector2Int additive)
    {
        int randomChance = Random.Range(0, 101);

        if(randomChance > 0 && randomChance <= destroyablesRate)
        {
            if (staticOrBreakable == 1)
            {
                if (CheckNullAndTag("Floor", true, area[coords.x + additive.x, coords.y + additive.y]) || CheckNullAndTag("Breakable", true, area[coords.x + additive.x, coords.y + additive.y]) || CheckNullAndTag("Static", true, area[coords.x + additive.x, coords.y + additive.y]))
                {
                    if (CheckSidesCorridor(area[coords.x + additive.x, coords.y + additive.x], 3, new Vector2Int(coords.x + additive.x, coords.y + additive.y), "Floor", "Breakable", "Static").x != 0)
                    {
                        if (!area[coords.x + additive.x, coords.y + additive.y].CompareTag("Floor"))
                        {
                            DestroyImmediate(area[coords.x + additive.x, coords.y + additive.y]);
                        }

                        //Check if it is in the room
                        if (area[coords.x + additive.x, coords.y + additive.y].transform.parent.CompareTag("Room") && area[coords.x + additive.x, coords.y + additive.y].transform.position != parentRooms[i].Corners[j].transform.position)
                        {
                            area[coords.x + additive.x, coords.y + additive.y] = Instantiate(tempContent, area[coords.x + additive.x, coords.y + additive.y].transform.position, Quaternion.identity, parentRooms[i].Corners[j].transform.parent.transform);

                            area[coords.x + additive.x, coords.y + additive.y].transform.position = new Vector3(area[coords.x + additive.x, coords.y + additive.y].transform.position.x, tempContent.transform.position.y, area[coords.x + additive.x, coords.y + additive.y].transform.position.z);
                        }

                        coords.x = coords.x + additive.x;
                        coords.y = coords.y + additive.y;

                        CreateDestroyableRows(ref staticOrBreakable, ref tempContent, ref coords, ref i, ref j, ref additive);
                    }
                }
            }
        }
    }
}