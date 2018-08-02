using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Room
{
    public float LastPlatformY;
    public float StartX;
    public float EndX;
    private GameObject _gameObject;
    private GameObject[] _myEightPlatforms;
    private const float _width = 50.25f; //because we are using 3 backgrounds each having a 1676 pixel width
    private const float _platformWidth = 6.28125f; //(notice platformWidth * 8 = roomWidth)

    public float CenterX
    { get { return _gameObject.transform.position.x; } }


    public Room(float roomStartX, float yOfPreviousRoomsLastPlatform, bool isStartRoom = false)
    {
        StartX = roomStartX;
        EndX = StartX + _width;
        float centerX = StartX + _width * 0.5f;


        RoomGenerator.NumOfRoomsCreated++;



        _gameObject = Object.Instantiate(RoomGenerator.StaticRoomPrefab);
        _gameObject.transform.position = new Vector3(centerX, 0, 0);



        _myEightPlatforms = new GameObject[8];

        for (int i = 0; i < 8; i++)
        {


            if (
                RoomGenerator.NumOfRoomsCreated > 0 //don't add any dangers to the first room so the player adjusts to the gameplay mechanics
                &&
                Random.Range(0, 10) == 0 //let's say for now a 10% probability of spikeplatform
               )
                _myEightPlatforms[i] = Object.Instantiate(RoomGenerator.StaticSpikePlatformPrefab); //danger Will Robinson! :P
            else
                _myEightPlatforms[i] = Object.Instantiate(RoomGenerator.StaticPlatformPrefab); //simple platform


            _myEightPlatforms[i].transform.parent = _gameObject.transform;



            Vector3 deterministicPosition = _gameObject.transform.position - new Vector3(_width / 2 - _platformWidth / 2 - _platformWidth * i, 0);
            _myEightPlatforms[i].transform.position = deterministicPosition;

            if (isStartRoom && i == 0)
            {//we want the very first platform of our game to be at a fixed position
            }
            else
            {//for every other platform we want a y that is the y of the previous platform plus/minus a small random value


                float randomDifference = Random.Range(-1f, 1f);

                float newY = yOfPreviousRoomsLastPlatform + randomDifference;

                if (newY > 3.4f || newY < -3.4f)
                {//out of allowed game bounds, go the other way

                    newY -= 2 * randomDifference;
                }

                _myEightPlatforms[i].transform.position = new Vector3(deterministicPosition.x, newY);

                //new Vector3(roomWidth / 2 - platformWidth / 2 - platformWidth * i, newY);

                if (i == 7)
                    LastPlatformY = newY;
            }

        }
    }

    public void Dispose()
    {
        Object.Destroy(_gameObject);
    }

}

public class RoomGenerator : MonoBehaviour
{
    public GameObject platformPrefab;
    public GameObject spikePlatformPrefab;
    public GameObject roomPrefab;
    public static GameObject StaticPlatformPrefab;
    public static GameObject StaticRoomPrefab;
    public static GameObject StaticSpikePlatformPrefab;
    public static int NumOfRoomsCreated = 0; //this is not used for now but could be used in the future to adjust game difficulty. The larger the number of rooms created the more we have progressed in the game.

    private List<Room> _existingRooms;
    private float _screenWidth;



    void Start()
    {
        StaticPlatformPrefab = platformPrefab;
        StaticRoomPrefab = roomPrefab;
        StaticSpikePlatformPrefab = spikePlatformPrefab;

        //find the scene room and destroy it. It is only there for visual reference for us developers. 
        var debugroom = GameObject.Find("DummyRoom");
        Destroy(debugroom);




        _existingRooms = new List<Room> { new Room(-25.14f, -1, true) }; //-25.14 because -16.76 + -16.76/2  ,  the -1 is ignored here





        float screenHeight = 2.0f * Camera.main.orthographicSize;
        _screenWidth = screenHeight * Camera.main.aspect;
        //print("screen width= " + screenWidth);


        StartCoroutine(GeneratorCheck());
    }




    private IEnumerator GeneratorCheck()
    {
        while (true)
        {

            Room firstRoom = _existingRooms[0];
            float leftCameraBound = Camera.main.transform.position.x - _screenWidth / 2;

            if (firstRoom.EndX < leftCameraBound)
            {//remove the oldest room
                firstRoom.Dispose();
                _existingRooms.RemoveAt(0);
            }


            Room lastRoom = _existingRooms[_existingRooms.Count - 1];
            float rightCameraBound = leftCameraBound + _screenWidth;// Camera.main.transform.position.x + _screenWidth / 2;

            if (lastRoom.CenterX < rightCameraBound)// <= transform.position.x + _screenWidth)
            {//add a new room
                _existingRooms.Add(new Room(lastRoom.EndX, lastRoom.LastPlatformY)); //we want the new room to start at the end of the last room
            }




            yield return new WaitForSeconds(0.25f);
        }
    }






}