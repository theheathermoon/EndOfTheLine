using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Room : MonoBehaviour
{
    //Use this vectors to make sure there's none connected
    private Vector2Int TLCorner;
    private Vector2Int BLCorner;
    private Vector2Int TRCorner;
    private Vector2Int BRCorner;

    private List<Room> connectedRooms = new List<Room>();
    private List<Room> savedConnectedRooms = new List<Room>();
    private List<GameObject> corners = new List<GameObject>();
    private List<GameObject> midPoints = new List<GameObject>();
    private List<GameObject> closestRooms = new List<GameObject>();
    private Dictionary<int, float> connectedRoomsAngles = new Dictionary<int, float>();

    private bool isPlayerInside;

    private int maxAmountOfConnections;

    public int MaxAmountOfConnections { get => maxAmountOfConnections;}
    public List<Room> ConnectedRooms { get => connectedRooms; }
    public List<GameObject> ClosestRooms { get => closestRooms; }
    public Dictionary<int, float> ConnectedRoomsAngles { get => connectedRoomsAngles; }
    public List<GameObject> MidPoints { get => midPoints; }
    public List<Room> SavedConnectedRooms { get => savedConnectedRooms; set => savedConnectedRooms = value; }
    public List<GameObject> Corners { get => corners; set => corners = value; }
    public bool IsPlayerInside { get => isPlayerInside; set => isPlayerInside = value; }

    public void CalculateConnections()
    {
        int amountRooms = GameObject.FindObjectOfType<Generator>().AmountOfAdjacentRoomsToConnect;

        int tempRoom = Random.Range(1, amountRooms + 1);

        maxAmountOfConnections = tempRoom;
    }

    public void AddConnectedRooms(Room room)
    {
        bool isAdded = false;

        //TODO: CHANGE THIS INTO .FIND
        foreach(Room r in ConnectedRooms)
        {
            if(r == room)
            {
                isAdded = true;
            }
        }

        if(isAdded == false)
        {
            ConnectedRooms.Add(room);

            //Add Angles to room
            ConnectedRoomsAngles.Add(connectedRooms.Count - 1, Vector3.Angle(this.gameObject.transform.position, room.gameObject.transform.position));

            Vector3 tempDirection = this.gameObject.transform.position - room.gameObject.transform.position;

            Vector3 vec1 = this.gameObject.transform.position;
            Vector3 vec2 = room.gameObject.transform.position;
            //Get the dot product

            float dot = Vector3.Dot(vec1, vec2);
            // Divide the dot by the product of the magnitudes of the vectors
            dot = dot / (vec1.magnitude * vec2.magnitude);
            //Get the arc cosin of the angle, you now have your angle in radians 
            var acos = Mathf.Acos(dot);
            //Multiply by 180/Mathf.PI to convert to degrees
            float angle = acos * 180 / Mathf.PI;
            //Congrats, you made it really hard on yourself.

            //WHEN YOU USE VECTOR3 SIGNED ANGLE THE FLOAT IS ONLY NEGATIVE WHEN BELOW IT
            float test = Vector3.SignedAngle(vec1, vec2, new Vector3(1,0,0));     

            //MAYBE THERE IS ANOTHER WAY TO FIX THIS PROBLEM, MAYBE CHECK THE DIFFERENCE BETWEEN POSITIONS IN VEC3
            //Log angles
            //Debug.Log(this.gameObject.name + " / " + room.gameObject.name + ": " + test);
        }
    }

    public void AddRoom(GameObject room)
    {
        bool isRoomAlreadyAdded = false;

        if(ClosestRooms.Count > 0)
        {
            foreach(GameObject a in ClosestRooms)
            {
                if(a == room)
                {
                    isRoomAlreadyAdded = true;
                    return;
                }
            }
        }

        if(isRoomAlreadyAdded == false)
        {
            ClosestRooms.Add(room);
        }
    }

    public void AddMidPoint()
    {
        //midPoint = transform.GetChild()
        //if total number of children is odd just add 0.5f to the total children/2
        if ((transform.childCount % 2) == 0)
        {

            List<float> childDistances = new List<float>();
            Dictionary<float, int> childAndDistances = new Dictionary<float, int>();

            for (int i = 0; i < transform.childCount; i++)
            {
                float tempDistance = Vector3.Distance(transform.position, transform.GetChild(i).position);
                //Add the distance between all the children tiles

                if (childAndDistances.ContainsKey(tempDistance))
                {
                    tempDistance += Random.Range(-0.0001f, 0.0001f);

                    if (childAndDistances.ContainsKey(tempDistance))
                    {
                        i--;
                    }
                    else
                    {
                        childDistances.Add(tempDistance);
                        childAndDistances.Add(tempDistance, i);
                    }
                }
                else
                {
                    childDistances.Add(tempDistance);
                    childAndDistances.Add(tempDistance, i);
                }
            }

            childDistances.Sort();

            //The two represents the middle points of each room
            //TODO: IN THE FUTURE THIS SHOULD ADAPT BETWEEN 2 AND FOUR
            for (int i = 0; i < 2; i++)
            {
                //Closes children
                int closeChildren = childAndDistances[childDistances[i]];

                //Add to MidPoints array
                midPoints.Add(transform.GetChild(closeChildren).gameObject);
            }

            int randomPoint = Random.Range(0, midPoints.Count);

        }
        else
        {
            int childNumber = Mathf.RoundToInt((transform.childCount / 2) + 1);

            MidPoints.Add(transform.GetChild(childNumber).gameObject);
        }
    }
}
