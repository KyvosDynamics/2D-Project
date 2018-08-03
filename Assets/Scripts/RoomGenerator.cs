using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform
{
    public GameObject MyGameObject = null;
    public bool HasDangerousObject = false;

    public Platform(Transform parent)
    {
        MyGameObject = Object.Instantiate(RoomGenerator.StaticPlatformPrefab, parent);
    }

    public Vector3 Position
    {
        set
        {
            MyGameObject.transform.position = value;
        }
        get
        {
            return MyGameObject.transform.position;
        }
    }

}

public class Room
{
    private static float _newestPlatformY = 0;
    public float StartX;
    public float EndX;
    public int Index;
    public static int StaticIndex = -1;
    private GameObject _gameObject;
    private Platform[] _myEightPlatforms;
    private const float _width = 50.25f; //because we are using 3 backgrounds each having a 1676 pixel width
    private const float _platformWidth = 6.28125f; //(notice platformWidth * 8 = roomWidth)

    public float CenterX
    { get { return _gameObject.transform.position.x; } }


    public Platform LatestPlatform
    { get { return _myEightPlatforms[7]; } }



    public Room(Room previousRoom)// float roomStartX, GameObject previousPlatform)//  bool isStartRoom = false)
    {
        float roomStartX = 0.0f;
        Platform previousPlatform = null;

        if (previousRoom == null)
        {//this is the very first room of the game
            roomStartX = -25.14f; //-25.14 because -16.76 + -16.76/2
        }
        else
        {
            roomStartX = previousRoom.EndX; //we want the new room to start at the end of the last room
            previousPlatform = previousRoom.LatestPlatform;

        }


        StaticIndex++;
        Index = StaticIndex;


        StartX = roomStartX;
        EndX = StartX + _width;
        float centerX = StartX + _width * 0.5f;

        _gameObject = Object.Instantiate(RoomGenerator.StaticRoomPrefab);
        _gameObject.transform.position = new Vector3(centerX, 0, 0);



        _myEightPlatforms = new Platform[8];


        Transform myTransform = _gameObject.transform;


        //   Platform p = null;
        for (int i = 0; i < 8; i++)
        {
            if(i>0)
            {
                previousPlatform = _myEightPlatforms[i - 1];
            }

            _myEightPlatforms[i] = new Platform(myTransform);

            //_myEightPlatforms[i].transform.parent = _gameObject.transform;





            float previousPlatformY = _newestPlatformY;


            //float platformY = 0;

            //Vector3 deterministicPosition = _gameObject.transform.position - new Vector3(, platformY);
            float newestPlatformX = _gameObject.transform.position.x - (_width / 2 - _platformWidth / 2 - _platformWidth * i);




            if (
                previousPlatform == null //it is only null in the first room
                && i == 0
                )
            {//we want the very first platform of our game to be at a fixed position

                // _myEightPlatforms[i].transform.position = deterministicPosition;

            }
            else
            {//for every other platform we want a y that is the y of the previous platform plus/minus a small random value


                float randomDifference = Random.Range(-1f, 1f);

                _newestPlatformY = previousPlatformY + randomDifference;

                if (_newestPlatformY > 3.4f || _newestPlatformY < -3.4f)
                {//out of allowed game bounds, go the other way

                    _newestPlatformY -= 2 * randomDifference;
                }



                //if (i == 7)
                //  LatestPlatformY = newY;
            }


            _myEightPlatforms[i].Position = (new Vector3(newestPlatformX, _newestPlatformY));

            //_newestPlatformY = _newestPlatformY;







            //check if we should add a dangerous object on top of the platform
            if (

                  Index > 0 //don't add any dangers to the first room so the player adjusts to the gameplay mechanics
                            //             &&
                            //         Random.Range(0, 5) == 0 //let's say for now a 20% probability of dangerous platform


               && previousPlatform.HasDangerousObject == false //don't add dangerous objects to two platform in a row
               )
            {//add dangerous object
                _myEightPlatforms[i].HasDangerousObject = true;


                GameObject dangerousObject = null;


                if (Random.Range(0, 2) == 0) //50% spike, 50% saw
                {
                    //pt = PlatformType.Spike;//  isSpike = true;
                    dangerousObject = Object.Instantiate(RoomGenerator.StaticSpikePrefab, _gameObject.transform);
                    //   dangerousObject.transform.parent = ;
                    //

                    //when we go to a spikeplatform that is higher than the previous one it is difficult to avoid the spike, so we move the spike to the right
                    //when we go to a spikeplatform that is lower than the previous one it is difficult to avoid the spike, so we move the spike to the left

                    // if (isSpike)
                    // {
                    //      Vector3 shift = new Vector3(0, 0, 0);
                    if (_newestPlatformY > previousPlatformY)// newY > _latestPlatformY)
                    {//new platform is higher, so move the spike right


                        dangerousObject.transform.position = _myEightPlatforms[i].Position + new Vector3(2, 0.8f, 0); //shift = new Vector3(2, 0, 0);
                    }
                    else
                    {//move spike left
                        dangerousObject.transform.position = _myEightPlatforms[i].Position + new Vector3(-2, 0.8f, 0);// shift = new Vector3(-2, 0, 0);
                    }


                    //   dangerousObject.transform.position += shift;
                    // }

                }
                else
                {
                    //  pt= 
                    //   _myEightPlatforms[i] = Object.Instantiate(RoomGenerator.StaticPlatformPrefab);
                    dangerousObject = Object.Instantiate(RoomGenerator.StaticSawPrefab, _gameObject.transform);
                    //dangerousObject.transform.parent = _gameObject.transform;
                    //
                    // saw.transform.parent = _myEightPlatforms[i].transform;

                    dangerousObject.transform.position = _myEightPlatforms[i].Position + new Vector3(0, 1.32f, 0);

                }


            }



            //    _myEightPlatforms[i] = p;
        }//for platform
    }

    public void Dispose()
    {
        Object.Destroy(_gameObject);
    }

}

public class RoomGenerator : MonoBehaviour
{
    public GameObject RoomPrefab;
    public GameObject PlatformPrefab;
    public GameObject SpikePrefab;
    public GameObject SawPrefab;
    public static GameObject StaticRoomPrefab;
    public static GameObject StaticPlatformPrefab;
    public static GameObject StaticSpikePrefab;
    public static GameObject StaticSawPrefab;
    private List<Room> _rooms;
    private float _screenWidth;



    void Start()
    {
        StaticRoomPrefab = RoomPrefab;
        StaticPlatformPrefab = PlatformPrefab;
        StaticSpikePrefab = SpikePrefab;
        StaticSawPrefab = SawPrefab;

        //find the scene room and destroy it. It is only there for visual reference for us developers. 
        var debugroom = GameObject.Find("DummyRoom");
        Destroy(debugroom);




        //Room room = ;//, _latestPlatform);
        _rooms = new List<Room> { new Room(null) };



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

                //  Room room =;
                //_latestPlatform = room.LatestPlatform;
                _rooms.Add(new Room(latestRoom));
            }



            if (_rooms.Count == 1)
            {
                //                print("player is at room index " + _rooms[0].Index);
            }
            else
            {//two rooms
                if (_rooms[0].EndX >= transform.position.x)
                {
                    //                  print("player is at room index " + _rooms[0].Index);
                }
                else
                {
                    //                print("player is at room index " + _rooms[1].Index);
                }


            }


            yield return new WaitForSeconds(0.25f);
        }
    }






}