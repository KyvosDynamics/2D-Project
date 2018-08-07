using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform
{
    public bool HasAttachedObject = false;
    private GameObject _unityObject = null;


    public Platform(Transform parent)
    {
        _unityObject = Object.Instantiate(RoomGenerator.StaticPlatformPrefab, parent);
    }

    public Vector3 Position
    {
        set { _unityObject.transform.position = value; }
        get { return _unityObject.transform.position; }
    }
}

public class Room
{

    public float StartX;
    public float EndX;
    public int Index;
    public static int StaticIndex = -1; //it is important for this to be initialized minus one so that the first room is at index 0
    private GameObject _unityObject;
    private Platform[] _myEightPlatforms = new Platform[8];
    private const float _width = 50.25f; //because we are using 3 backgrounds each having a 1676 pixel width
    private const float _platformWidth = 6.28125f; //(notice platformWidth * 8 = roomWidth)

    public float CenterX
    { get { return _unityObject.transform.position.x; } }


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


        _unityObject = Object.Instantiate(RoomGenerator.StaticRoomPrefab, new Vector3(centerX, 0, 0), Quaternion.identity);



        Transform roomTransform = _unityObject.transform;


        Platform p = null;
        for (int i = 0; i < 8; i++)
        {
            p = _myEightPlatforms[i] = new Platform(roomTransform);


            if (i > 0) //i.e. it is not the first platform of this room. When i is 0 the previous platform is the last platform of the previous room as set a few lines above
                previousPlatform = _myEightPlatforms[i - 1];



            float platformY;

            if (previousPlatform == null)
            {
                //the previous platform is null only in the very first room of the game and the very first platform of that first room
                //we want the very first platform of our game to be at at y zero

                platformY = 0;

            }
            else
            {//for every other platform we want a y that is the y of the previous platform plus/minus a small random value


                float randomDifference = Random.Range(-1f, 1f);

                platformY = previousPlatform.Position.y + randomDifference;

                if (platformY > 3.4f || platformY < -3.4f)
                {//out of allowed game bounds, go the other way
                    platformY -= 2 * randomDifference;
                }

            }


            //now we know y. For x it's easy as we know the position of the room and the relative position of the platform inside the room
            float platformX = roomTransform.position.x - (_width / 2 - _platformWidth / 2 - _platformWidth * i);

            //therefore:
            p.Position = new Vector3(platformX, platformY);






            //check if we should attach an object to the top of the platform
            if (
                Index > 0 //don't add any objects to the first room so the player adjusts to the gameplay mechanics
                &&
                previousPlatform.HasAttachedObject == false //don't add objects to two platforms in a row
                &&
                Random.Range(0, 4) == 0 //25% probability of object
               )
            {//add object

                p.HasAttachedObject = true;




                int type = Random.Range(0, 3);



                Vector3 offsetRelativeToPlatform = new Vector3();
                GameObject prefabToUse = null;

                switch (type)
                {
                    case 0: //spike
                        prefabToUse = RoomGenerator.StaticSpikePrefab;

                        //when we go to a spikeplatform that is higher than the previous one it is difficult to avoid the spike, so we move the spike to the right
                        //when we go to a spikeplatform that is lower than the previous one it is difficult to avoid the spike, so we move the spike to the left
                        if (platformY > previousPlatform.Position.y)
                        {//new platform is higher, so move the spike right
                            offsetRelativeToPlatform = new Vector3(2.5f, 0.8f, 0);
                        }
                        else
                        {//move spike left
                            offsetRelativeToPlatform = new Vector3(-2, 0.8f, 0);
                        }
                        break;

                    case 1: //saw
                        prefabToUse = RoomGenerator.StaticSawPrefab;
                        offsetRelativeToPlatform = new Vector3(0, 1.32f, 0);
                        break;

                    case 2: //ponger
                        prefabToUse = RoomGenerator.StaticPongerPrefab;
                        offsetRelativeToPlatform = new Vector3(0, 1.04f, 0);
                        break;
                }

                Object.Instantiate(prefabToUse, p.Position + offsetRelativeToPlatform, Quaternion.identity, roomTransform);



            }



        }//for platform
    }


    public void Dispose()
    {
        Object.Destroy(_unityObject);
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