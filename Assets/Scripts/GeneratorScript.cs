using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorScript : MonoBehaviour
{
    public GameObject roomPrefab;
    public List<GameObject> currentRooms;
    private float screenWidthInPoints;
    private const float roomWidth= 50.25f; //because we are using 3 backgrounds each having a 1676 pixel width


    void Start()
    {
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
        List<GameObject> roomsToRemove = new List<GameObject>();
        float playerX = transform.position.x;
        float removeRoomX = playerX - screenWidthInPoints;
        float addRoomX = playerX + screenWidthInPoints;
        float farthestRoomEndX = 0;

        bool addRoom = true;
        foreach (var room in currentRooms)
        {
        //    float roomWidth = room.transform.Find("floor").localScale.x;
            float roomStartX = room.transform.position.x - (roomWidth * 0.5f);
            float roomEndX = roomStartX + roomWidth;
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
            Destroy(room);
        }

        if (addRoom)
            AddRoom(farthestRoomEndX);
    }


    void AddRoom(float farthestRoomEndX)
    {

        //2
        GameObject room = (GameObject)Instantiate(roomPrefab);
        //3
       // float roomWidth = room.transform.Find("floor").localScale.x;
        //4
        float roomCenter = farthestRoomEndX + roomWidth * 0.5f;
        //5
        room.transform.position = new Vector3(roomCenter, 0, 0);
        //6
        currentRooms.Add(room);
    }


}