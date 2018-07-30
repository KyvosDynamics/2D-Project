using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Room
{
    public GameObject MyGameObject;
    public GameObject[] MyEightPlatforms;
    public const float roomWidth = 50.25f; //because we are using 3 backgrounds each having a 1676 pixel width
    public const float platformWidth = 6.28125f; //(notice platformWidth * 8 = roomWidth)
    public float lastPlatformY;

    public Room(GameObject platformPrefab, GameObject roomPrefab, float roomStartX, float yOfPreviousRoomsLastPlatform, bool isStartRoom=false)
    {

        GameObject roomGameObject = Object.Instantiate(roomPrefab);

        float roomCenter = roomStartX + roomWidth * 0.5f;

        roomGameObject.transform.position = new Vector3(roomCenter, 0, 0);



        MyGameObject = roomGameObject;



        MyEightPlatforms = new GameObject[8];



        for (int i = 0; i < 8; i++)
        {
            MyEightPlatforms[i] = Object.Instantiate(platformPrefab);



            Vector3 determinsticPosition= roomGameObject.transform.position - new Vector3(roomWidth / 2 - platformWidth / 2 - platformWidth * i, 0);
            MyEightPlatforms[i].transform.position = determinsticPosition;

            if (isStartRoom && i==0)
            {//we want the very first platform of our game to be at a fixed position
            }
            else
            {//for every other platform we want a y that is the y of the previous platform plus/minus a small random value


                float randomDifference = Random.Range(-1f,1f);

                float newY = yOfPreviousRoomsLastPlatform + randomDifference;

                if(newY>3.4f || newY<-3.4f)
                {//out of allowed game bounds, go the other way

                    newY -= 2 * randomDifference;
                }

                MyEightPlatforms[i].transform.position = new Vector3(determinsticPosition.x, newY);
                    
                    //new Vector3(roomWidth / 2 - platformWidth / 2 - platformWidth * i, newY);

                if (i == 7)
                    lastPlatformY = newY;
            }
            
        }
    }

    public void Dispose()
    {
        Object.Destroy(MyGameObject);
        foreach (GameObject platform in MyEightPlatforms)
            Object.Destroy(platform);
    }

}

public class RoomGenerator : MonoBehaviour
{
    public GameObject platformPrefab;
    public GameObject roomPrefab; //for this we drag the room PREFAB to the inspector
    public GameObject StartRoom; //for this we drag the existing room INSTANCE (in the hierarchy) to the inspector
    private List<Room> _existingRooms;
    private float _screenWidth;



    void Start()
    {
        _existingRooms = new List<Room> { new Room(platformPrefab, roomPrefab, -25.14f, -1, true) }; //-25.14 because -16.76 + -16.76/2  ,  the -1 is ignored here

        float screenHeight = 2.0f * Camera.main.orthographicSize;
        _screenWidth = screenHeight * Camera.main.aspect;
        //print("screen width= " + screenWidth);


        StartCoroutine(GeneratorCheck());
    }

    private IEnumerator GeneratorCheck()
    {
        while (true)
        {
            GenerateRoomIfRequired();
            yield return new WaitForSeconds(0.25f);
        }
    }


    private void GenerateRoomIfRequired()
    {
        List<Room> roomsToRemove = new List<Room>(); //(we remove a room from the game when it gets well behind the visible area)
        float playerX = transform.position.x;
        float removeRoomThresholdX = playerX - _screenWidth;
        float addRoomThresholdX = playerX + _screenWidth;
        float farthestRoomEndX = 0;
        float yOfPreviousRoomsLastPlatform = -1;
        bool addRoom = true;
        foreach (var room in _existingRooms)
        {
            float roomStartX = room.MyGameObject.transform.position.x - (Room.roomWidth * 0.5f); //for now we assume that all rooms have the same width. If we change that in the future we could do something like float roomWidth = room.transform.Find("floor").localScale.x;
            float roomEndX = roomStartX + Room.roomWidth;

            if (roomStartX > addRoomThresholdX)
                addRoom = false;

            if (roomEndX < removeRoomThresholdX)
                roomsToRemove.Add(room);

            farthestRoomEndX = Mathf.Max(farthestRoomEndX, roomEndX);

            yOfPreviousRoomsLastPlatform = room.lastPlatformY;
        }

        foreach (var room in roomsToRemove)
        {
            _existingRooms.Remove(room);
            room.Dispose();
        }



        if (addRoom)
        {
            Room room = new Room(platformPrefab, roomPrefab, farthestRoomEndX, yOfPreviousRoomsLastPlatform);

            _existingRooms.Add(room);
        }
    }




}