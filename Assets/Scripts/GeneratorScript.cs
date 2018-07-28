using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Room
{
    public GameObject MyGameObject;
    public GameObject[] MyEightPlatforms;
    public  static float roomWidth = 50.25f; //because we are using 3 backgrounds each having a 1676 pixel width

    public Room(GameObject myGameObject, GameObject platformPrefab)
    {
        MyGameObject = myGameObject;

        MyEightPlatforms = new GameObject[8];



        float roomWidthDividedBy8 = roomWidth / 8;

        for (int i = 0; i < 8; i++)
        {
            MyEightPlatforms[i] =
      (GameObject)MonoBehaviour.Instantiate(platformPrefab);

            MyEightPlatforms[i].transform.position = myGameObject.transform.position - new Vector3(roomWidth / 2 - roomWidthDividedBy8 / 2 - roomWidthDividedBy8 * i, 0);//
        }
    }

}
public class GeneratorScript : MonoBehaviour
{
    public GameObject platformPrefab;
    public GameObject roomPrefab;
    public GameObject StartRoom;
    private List<Room> currentRooms;
    private float screenWidthInPoints;
   


    void Start()
    {
        currentRooms = new List<Room> { new Room(StartRoom,platformPrefab) };

        float height = 2.0f * Camera.main.orthographicSize;
        screenWidthInPoints = height * Camera.main.aspect;
        print("screen width= " + screenWidthInPoints);



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
        List<Room> roomsToRemove = new List<Room>();
        float playerX = transform.position.x;
        float removeRoomX = playerX - screenWidthInPoints;
        float addRoomX = playerX + screenWidthInPoints;
        float farthestRoomEndX = 0;

        bool addRoom = true;
        foreach (var room in currentRooms)
        {
        //    float roomWidth = room.transform.Find("floor").localScale.x;
            float roomStartX = room.MyGameObject.transform.position.x - (Room.roomWidth * 0.5f);
            float roomEndX = roomStartX + Room.roomWidth;
            //8
            if (roomStartX > addRoomX)
            {
                addRoom = false;
            }
            //9
            if (roomEndX < removeRoomX)
            {
                roomsToRemove.Add(room);
            }
            //10
            farthestRoomEndX = Mathf.Max(farthestRoomEndX, roomEndX);
        }

        //11
        foreach (var room in roomsToRemove)
        {
            currentRooms.Remove(room);
            Destroy(room.MyGameObject);
        }

        if (addRoom)
            AddRoom(farthestRoomEndX);
    }


    void AddRoom(float farthestRoomEndX)
    {

        //2
        GameObject roomGO = (GameObject)Instantiate(roomPrefab);
        //3
       // float roomWidth = room.transform.Find("floor").localScale.x;
        //4
        float roomCenter = farthestRoomEndX + Room.roomWidth * 0.5f;
        //5
        roomGO.transform.position = new Vector3(roomCenter, 0, 0);
        Room room = new Room(roomGO,platformPrefab);

        //6
        currentRooms.Add(room);
    }


}