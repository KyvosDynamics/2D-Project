using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Room
{
    public GameObject MyGameObject;
    public GameObject[] MyEightPlatforms;
    public const float roomWidth = 50.25f; //because we are using 3 backgrounds each having a 1676 pixel width
    public const float platformWidth = 6.28125f; //(notice platformWidth * 8 = roomWidth)

    public Room(GameObject myGameObject, GameObject platformPrefab)
    {
        MyGameObject = myGameObject;



        MyEightPlatforms = new GameObject[8];

        for (int i = 0; i < 8; i++)
        {
            MyEightPlatforms[i] = Object.Instantiate(platformPrefab);
            MyEightPlatforms[i].transform.position = myGameObject.transform.position - new Vector3(roomWidth / 2 - platformWidth / 2 - platformWidth * i, 0);
        }
    }

}

public class GeneratorScript : MonoBehaviour
{
    public GameObject platformPrefab;
    public GameObject roomPrefab; //for this we drag the room PREFAB to the inspector
    public GameObject StartRoom; //for this we drag the existing room INSTANCE (in the hierarchy) to the inspector
    private List<Room> _existingRooms;
    private float _screenWidth;



    void Start()
    {
        _existingRooms = new List<Room> { new Room(StartRoom, platformPrefab) };

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
        }

        foreach (var room in roomsToRemove)
        {
            _existingRooms.Remove(room);
            Destroy(room.MyGameObject);
        }



        if (addRoom)
        {
            GameObject roomGameObject = Instantiate(roomPrefab);

            float roomCenter = farthestRoomEndX + Room.roomWidth * 0.5f;

            roomGameObject.transform.position = new Vector3(roomCenter, 0, 0);

            Room room = new Room(roomGameObject, platformPrefab);

            _existingRooms.Add(room);
        }
    }




}