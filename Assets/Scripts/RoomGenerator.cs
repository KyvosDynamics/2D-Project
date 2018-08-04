using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform
{
    public bool HasObject = false;
    private GameObject _gameObject = null;


    public Platform(Transform parent)
    {
        _gameObject = Object.Instantiate(RoomGenerator.StaticPlatformPrefab, parent);
    }

    public Vector3 Position
    {
        set { _gameObject.transform.position = value; }
        get { return _gameObject.transform.position; }
    }
}

public class Room
{
    private static float _newestPlatformY;
    public float StartX;
    public float EndX;
    public int Index;
    public static int StaticIndex = -1; //it is important for this to be initialized minus one so that the first room is at index 0
    private GameObject _gameObject;
    private Platform[] _myEightPlatforms = new Platform[8];
    private const float _width = 50.25f; //because we are using 3 backgrounds each having a 1676 pixel width
    private const float _platformWidth = 6.28125f; //(notice platformWidth * 8 = roomWidth)

    public float CenterX
    { get { return _gameObject.transform.position.x; } }


    public Platform LastPlatform
    { get { return _myEightPlatforms[7]; } }




    public Room(Room previousRoom)
    {
        StaticIndex++;
        Index = StaticIndex;


        Platform previousPlatform = null;

        if (previousRoom == null)
        {//this is the very first room of the game
            StartX = -25.14f; //-25.14 because -16.76 + -16.76/2
        }
        else
        {
            StartX = previousRoom.EndX; //we want the new room to start at the end of the previous room
            previousPlatform = previousRoom.LastPlatform;  //the previous platform is the last platform of the previous room
        }
        EndX = StartX + _width;
        float centerX = StartX + _width * 0.5f;


        _gameObject = Object.Instantiate(RoomGenerator.StaticRoomPrefab, new Vector3(centerX, 0, 0), Quaternion.identity);





        Transform roomTransform = _gameObject.transform;


        Platform p = null;
        for (int i = 0; i < 8; i++)
        {
            if (i > 0)
                previousPlatform = _myEightPlatforms[i - 1];


            p = _myEightPlatforms[i] = new Platform(roomTransform);


            float previousPlatformY = _newestPlatformY;

            if (
                previousPlatform == null //it is the very first room of the game (previousPlatform is only null in the first room)
                &&
                i == 0 //and it is the first platform of the first room
               )
            {//we want the very first platform of our game to be at at y zero

                _newestPlatformY = 0;

            }
            else
            {//for every other platform we want a y that is the y of the previous platform plus/minus a small random value


                float randomDifference = Random.Range(-1f, 1f);

                _newestPlatformY = previousPlatformY + randomDifference;

                if (_newestPlatformY > 3.4f || _newestPlatformY < -3.4f)
                {//out of allowed game bounds, go the other way
                    _newestPlatformY -= 2 * randomDifference;
                }

            }


            //now we know y. For x it's easy as we know the position of the room and the relative position of the platform inside the room
            float newestPlatformX = roomTransform.position.x - (_width / 2 - _platformWidth / 2 - _platformWidth * i);

            //therefore:
            p.Position = new Vector3(newestPlatformX, _newestPlatformY);








            //check if we should add a dangerous object on top of the platform
            if (
                Index > 0 //don't add any objects to the first room so the player adjusts to the gameplay mechanics
                && previousPlatform.HasObject == false //don't add objects to two platforms in a row
                && Random.Range(0, 4) == 0 //25% probability of object
               )
            {//add object
                _myEightPlatforms[i].HasObject = true;


                GameObject obj = null;



                int type = Random.Range(0, 3);


                switch (type)
                {
                    case 0: //spike

                        obj = Object.Instantiate(RoomGenerator.StaticSpikePrefab, roomTransform);

                        //when we go to a spikeplatform that is higher than the previous one it is difficult to avoid the spike, so we move the spike to the right
                        //when we go to a spikeplatform that is lower than the previous one it is difficult to avoid the spike, so we move the spike to the left

                        if (_newestPlatformY > previousPlatformY)// newY > _latestPlatformY)
                        {//new platform is higher, so move the spike right


                            obj.transform.position = p.Position + new Vector3(2, 0.8f, 0); //shift = new Vector3(2, 0, 0);
                        }
                        else
                        {//move spike left
                            obj.transform.position = p.Position + new Vector3(-2, 0.8f, 0);// shift = new Vector3(-2, 0, 0);
                        }



                        break;

                    case 1: //saw

                        obj = Object.Instantiate(RoomGenerator.StaticSawPrefab, roomTransform);

                        obj.transform.position = p.Position + new Vector3(0, 1.32f, 0);

                        break;

                    case 2: //ponger

                        obj = Object.Instantiate(RoomGenerator.StaticPongerPrefab, roomTransform);
                        obj.transform.position = p.Position + new Vector3(0, 1.04f, 0);

                        break;
                }



            }



        }//for platform
    }

    public void Dispose()
    {
        Object.Destroy(_gameObject);
    }

}

public class RoomGenerator : MonoBehaviour
{
    public static int PlayerIsInRoomIndex = -1; //this is not used yet but could be helpful if for example we want to increase game difficulty based on progress
    public GameObject RoomPrefab;
    public GameObject PlatformPrefab;
    public GameObject SpikePrefab;
    public GameObject SawPrefab;
    public GameObject PongerPrefab;
    public static GameObject StaticRoomPrefab;
    public static GameObject StaticPlatformPrefab;
    public static GameObject StaticSpikePrefab;
    public static GameObject StaticSawPrefab;
    public static GameObject StaticPongerPrefab;
    private List<Room> _rooms;
    private float _screenWidth;



    void Start()
    {
        StaticRoomPrefab = RoomPrefab;
        StaticPlatformPrefab = PlatformPrefab;
        StaticSpikePrefab = SpikePrefab;
        StaticSawPrefab = SawPrefab;
        StaticPongerPrefab = PongerPrefab;


        //find the scene room and destroy it. It is only there for visual reference for us developers. 
        Destroy(GameObject.Find("DummyRoom"));


        //let's add the first real room (we pass null because there is no previous room)
        _rooms = new List<Room> { new Room(null) };


        float screenHeight = 2.0f * Camera.main.orthographicSize;
        _screenWidth = screenHeight * Camera.main.aspect;


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
                _rooms.Add(new Room(latestRoom));
            }


            if (_rooms.Count == 1)
            {
                PlayerIsInRoomIndex = _rooms[0].Index;
            }
            else
            {//two rooms

                if (_rooms[0].EndX >= transform.position.x)
                    PlayerIsInRoomIndex = _rooms[0].Index;
                else
                    PlayerIsInRoomIndex = _rooms[1].Index;
            }


            yield return new WaitForSeconds(0.25f);
        }
    }





}