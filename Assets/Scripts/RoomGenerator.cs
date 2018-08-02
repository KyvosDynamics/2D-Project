using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Room
{
    private static float _latestPlatformY = 0;
    public float StartX;
    public float EndX;
    public int Index;
    public static int StaticIndex = -1;
    private GameObject _gameObject;
    private GameObject[] _myEightPlatforms;
    private const float _width = 50.25f; //because we are using 3 backgrounds each having a 1676 pixel width
    private const float _platformWidth = 6.28125f; //(notice platformWidth * 8 = roomWidth)

    public float CenterX
    { get { return _gameObject.transform.position.x; } }


    public Room(float roomStartX,  bool isStartRoom = false)
    {
        StartX = roomStartX;
        EndX = StartX + _width;
        float centerX = StartX + _width * 0.5f;



        StaticIndex++;
        Index = StaticIndex;

        _gameObject = Object.Instantiate(RoomGenerator.StaticRoomPrefab);
        _gameObject.transform.position = new Vector3(centerX, 0, 0);



        _myEightPlatforms = new GameObject[8];

        for (int i = 0; i < 8; i++)
        {

            bool spike = false;

            if (
                Index>0 //don't add any dangers to the first room so the player adjusts to the gameplay mechanics
                &&
                Random.Range(0, 10) == 0 //let's say for now a 10% probability of spikeplatform
               )
            {
                spike = true;
                _myEightPlatforms[i] = Object.Instantiate(RoomGenerator.StaticSpikePlatformPrefab); //danger Will Robinson! :P
            }
            else
            {
                spike = false;
                _myEightPlatforms[i] = Object.Instantiate(RoomGenerator.StaticPlatformPrefab); //simple platform
            }


            _myEightPlatforms[i].transform.parent = _gameObject.transform;



            Vector3 deterministicPosition = _gameObject.transform.position - new Vector3(_width / 2 - _platformWidth / 2 - _platformWidth * i, 0);
            _myEightPlatforms[i].transform.position = deterministicPosition;

            if (isStartRoom && i == 0)
            {//we want the very first platform of our game to be at a fixed position
            }
            else
            {//for every other platform we want a y that is the y of the previous platform plus/minus a small random value


                float randomDifference = Random.Range(-1f, 1f);

                float newY = _latestPlatformY + randomDifference;

                if (newY > 3.4f || newY < -3.4f)
                {//out of allowed game bounds, go the other way

                    newY -= 2 * randomDifference;
                }

               


                //when we go to a spikeplatform that is higher than the previous one it is difficult to avoid the spike, so we move the spike to the right
                //when we go to a spikeplatform that is lower than the previous one it is difficult to avoid the spike, so we move the spike to the left

                if(spike)
                {
                    Vector3 shift = new Vector3(0, 0, 0);
                    if(newY> _latestPlatformY)
                    {//new platform is higher, so move the spike right


                        shift = new Vector3(2, 0, 0);
                    }
                    else
                    {//move spike left
                        shift = new Vector3(-2, 0, 0);
                    }


                    _myEightPlatforms[i].transform.GetChild(0).transform.position = _myEightPlatforms[i].transform.GetChild(0).transform.position + shift;
                }

                _myEightPlatforms[i].transform.position = new Vector3(deterministicPosition.x, newY);
                _latestPlatformY = newY;


                //if (i == 7)
                  //  LatestPlatformY = newY;
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

    private List<Room> _rooms;
    private float _screenWidth;



    void Start()
    {
        StaticPlatformPrefab = platformPrefab;
        StaticRoomPrefab = roomPrefab;
        StaticSpikePlatformPrefab = spikePlatformPrefab;

        //find the scene room and destroy it. It is only there for visual reference for us developers. 
        var debugroom = GameObject.Find("DummyRoom");
        Destroy(debugroom);




        _rooms = new List<Room> { new Room(-25.14f, true) }; //-25.14 because -16.76 + -16.76/2





        float screenHeight = 2.0f * Camera.main.orthographicSize;
        _screenWidth = screenHeight * Camera.main.aspect;
        //print("screen width= " + screenWidth);


        StartCoroutine(GeneratorCheck());
    }




    private IEnumerator GeneratorCheck()
    {
        while (true)
        {

            Room oldestRoom = _rooms[0];
            float leftCameraBound = Camera.main.transform.position.x - _screenWidth / 2;

            if (oldestRoom.EndX < leftCameraBound)
            {//remove the oldest room
                oldestRoom.Dispose();
                _rooms.RemoveAt(0);
            }

            Room latestRoom = _rooms[_rooms.Count - 1];
            float rightCameraBound = leftCameraBound + _screenWidth;

            if (latestRoom.CenterX < rightCameraBound)
            {//add a new room
                _rooms.Add(new Room(latestRoom.EndX)); //we want the new room to start at the end of the last room
            }



            if(_rooms.Count==1)
            {
                print("player is at room index " + _rooms[0].Index);
            }
            else
            {//two rooms
                if (_rooms[0].EndX >= transform.position.x)
                {
                    print("player is at room index " + _rooms[0].Index);
                }
                else
                {
                    print("player is at room index " + _rooms[1].Index);
                }


            }


            yield return new WaitForSeconds(0.25f);
        }
    }






}