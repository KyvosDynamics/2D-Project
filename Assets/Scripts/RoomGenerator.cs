using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ObjectDefinition
{
    public enum Type { Spike, Saw, Ponger }
    public Type MyType { get; private set; }

    public ObjectDefinition(Type type)
    {
        MyType = type;
    }
}

public class Platform
{
    public static GameObject StaticPlatformPrefab;
    public ObjectDefinition AttachedObjectDefinition = null;
    private GameObject _unityObject = null;


    public Platform(Transform parent)
    {
        _unityObject = Object.Instantiate(StaticPlatformPrefab, parent);
    }

    public Vector3 Position
    {
        set { _unityObject.transform.position = value; }
        get { return _unityObject.transform.position; }
    }
}

public class Room 
{
    public static GameObject StaticRoomPrefab;
    public static GameObject StaticSpikePrefab;
    public static GameObject StaticSawPrefab;
    public static GameObject StaticPongerPrefab;
    public float EndX;
    public int Index;
    private GameObject _unityObject;
    private Platform[] _myEightPlatforms = new Platform[8];



    public Room(Room previousRoom)
    {
       RoomGenerator.StaticRoomIndex++;
        Index = RoomGenerator.StaticRoomIndex;

        const float width = 50.28f; //because we are using 3 backgrounds each having a 1676 pixel width
        const float platformWidth = 6.285f; //(notice platformWidth * 8 = roomWidth)  , also 1356 * scale= 628.5, scale= 0.4635
        const float platformHeight = 0.616455f;


        Platform previousPlatform = null;
        float startX;

        if (previousRoom == null)
        {//this is the very first room of the game
            startX = -25.14f; //-25.14 because -16.76 + -16.76/2
        }
        else
        {
            startX = previousRoom.EndX; //we want the new room to start at the end of the previous room
            previousPlatform = previousRoom.LastPlatform;  //the previous platform is the last platform of the previous room
        }

        EndX = startX + width;
        float centerX = startX + width * 0.5f;


        _unityObject = Object.Instantiate(StaticRoomPrefab, new Vector3(centerX, 0, 0), Quaternion.identity);



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
                //we want the very first platform of our game to be at at y minus one

                platformY = -1;

            }
            else
            {//for every other platform we want a y that is the y of the previous platform plus/minus a small random value


                //the only exception is when the previous platform had a ponger on it. We want the new platform to be placed much higher than usual
                if (previousPlatform.AttachedObjectDefinition != null && previousPlatform.AttachedObjectDefinition.MyType == ObjectDefinition.Type.Ponger)
                {

                    platformY = previousPlatform.Position.y + 4 * platformHeight;

                }
                else
                {




                    float randomDifference = 0;
                    int upSameOrDown = Random.Range(0, 3);
                    switch (upSameOrDown)
                    {
                        case 0: //up
                            randomDifference = platformHeight;
                            break;
                        case 1: //same level
                            randomDifference = 0;
                            break;
                        case 2: //down
                            randomDifference = -platformHeight;
                            break;
                    }

                    platformY = previousPlatform.Position.y + randomDifference;

                    //check for out-of-game-bounds
                    if (platformY > 2f)
                        platformY = previousPlatform.Position.y - platformHeight;
                    else if (platformY < -4f)
                        platformY = previousPlatform.Position.y + platformHeight;





                }

            }



            //now we know y. For x it's easy as we know the position of the room and the relative position of the platform inside the room
            float platformX = roomTransform.position.x - (width / 2 - platformWidth / 2 - platformWidth * i);

            //therefore:
            p.Position = new Vector3(platformX, platformY);








            //check if we should attach an object to the top of the platform




            if (
                Index > 0 //don't add any objects to the first room so the player adjusts to the gameplay mechanics
                &&
                previousPlatform.AttachedObjectDefinition == null //don't add objects to two platforms in a row
                &&
                Random.Range(0, 4) == 0 //25% probability of object
               )
            {//add object






                Vector3 offsetRelativeToPlatform = new Vector3();
                GameObject prefabToUse = null;




                int type = Random.Range(0, 3);

                if (type == 2)
                {//we want to add a ponger. But we should be careful: when we add a ponger the next platform is placed higher than usual.
                    //this means in order to add ponger we should ensure that there is sufficient space above

                    if (p.Position.y > 1)
                    {//nope, we are way too high for ponger, instantiate another type

                        type = Random.Range(0, 2);
                    }
                }


                switch (type)
                {
                    case 0: //spike
                        prefabToUse = StaticSpikePrefab;
                        p.AttachedObjectDefinition = new ObjectDefinition(ObjectDefinition.Type.Spike );

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
                        prefabToUse = StaticSawPrefab;
                        p.AttachedObjectDefinition = new ObjectDefinition( ObjectDefinition.Type.Saw );
                        offsetRelativeToPlatform = new Vector3(0, 1.32f, 0);
                        break;

                    case 2: //ponger
                        prefabToUse = StaticPongerPrefab;
                        p.AttachedObjectDefinition = new ObjectDefinition(ObjectDefinition.Type.Ponger );
                        offsetRelativeToPlatform = new Vector3(0, 1.04f, 0);
                        break;
                }

                Object.Instantiate(prefabToUse, p.Position + offsetRelativeToPlatform, Quaternion.identity, roomTransform);



            }



        }//for platform
    }






    public float CenterX
    { get { return _unityObject.transform.position.x; } }


    public Platform LastPlatform
    { get { return _myEightPlatforms[7]; } }


    public void Dispose()
    { Object.Destroy(_unityObject); }

}

public class RoomGenerator : MonoBehaviour
{
    public static int PlayerIsInRoomIndex = -1; //this could also be useful for increasing game difficulty based on progress
    public static int StaticRoomIndex = -1; //it is important for this to be initialized minus one so that the first room is at index 0

    public GameObject RoomPrefab;
    public GameObject PlatformPrefab;
    public GameObject SpikePrefab;
    public GameObject SawPrefab;
    public GameObject PongerPrefab;
    private List<Room> _rooms;
    private float _screenWidth;
    public Text CurrentScoreText;
    public Text HighScoreText;


    private void Awake()
    {
        //it is absolutely vital that we first reset the static variables because they are not automatically reset when we start a new game (e.g. after we are killed)
        PlayerIsInRoomIndex = -1;
        StaticRoomIndex = -1;

    }

    void Start()
    {



        Platform.StaticPlatformPrefab = PlatformPrefab;
        Room.StaticRoomPrefab = RoomPrefab;
        Room.StaticSpikePrefab = SpikePrefab;
        Room.StaticSawPrefab = SawPrefab;
        Room.StaticPongerPrefab = PongerPrefab;


        //find the scene room and destroy it. It is only there for visual reference for us developers 
        Destroy(GameObject.Find("DummyRoom"));


        //let's add the first real room (we pass null because there is no previous room)
        _rooms = new List<Room> { new Room(null) };


        float screenHeight = 2.0f * Camera.main.orthographicSize;
        _screenWidth = screenHeight * Camera.main.aspect;


        CurrentScoreText.text = "0";
        HighScoreText.text = PlayerPrefs.GetInt("HighScore", 0).ToString();



        StartCoroutine(GeneratorCheck());
    }




    private IEnumerator GeneratorCheck()
    {
        while (PlayerController.IsAlive)
        {
            bool roomAddedOrRemoved = false;

            Room oldestRoom = _rooms[0];
            float leftCameraBound = Camera.main.transform.position.x - _screenWidth / 2;

            if (oldestRoom.EndX < leftCameraBound)
            {//remove the oldest room
                oldestRoom.Dispose();
                _rooms.RemoveAt(0);
                roomAddedOrRemoved = true;
            }

            Room latestRoom = _rooms[_rooms.Count - 1];
            float rightCameraBound = leftCameraBound + _screenWidth;

            if (latestRoom.CenterX < rightCameraBound)
            {//add a new room
                _rooms.Add(new Room(latestRoom));
                roomAddedOrRemoved = true;
            }




            if (roomAddedOrRemoved)
            {//rooms changed so update the progress text

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


                CurrentScoreText.text = PlayerIsInRoomIndex.ToString();

                if (PlayerIsInRoomIndex > PlayerPrefs.GetInt("HighScore", 0))
                {
                    PlayerPrefs.SetInt("HighScore", PlayerIsInRoomIndex);
                    HighScoreText.text = PlayerIsInRoomIndex.ToString();
                }

            }


            yield return new WaitForSeconds(0.25f);
        }
    }





}