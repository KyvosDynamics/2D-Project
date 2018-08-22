using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class Platform
{
    public ItemThatSitsOnPlatform AttachedObject = null;
    public Vector3 Position { get; private set; }


    public Platform(Transform parent, Vector3 position)
    {
        Position = position;
        Object.Instantiate(RoomGenerator.StaticPlatformPrefab, position, Quaternion.identity, parent);
    }

}


public class ProceduralRoom : Room
{
    public Platform LastPlatform { get; private set; }


    public ProceduralRoom(ProceduralRoom previousRoom) : base(previousRoom)
    {
        Platform previousPlatform = previousRoom == null ? null : previousRoom.LastPlatform;  //the previous platform is the last platform of the previous room


        const float width = 50.28f; //because we are using 3 backgrounds each having a 1676 pixel width
        const float platformWidth = 6.285f; //(notice platformWidth * 8 = roomWidth)  , also 1356 * scale= 628.5, scale= 0.4635
        const float platformHeight = 0.616455f;

        int randomIndex = Random.Range(0, RoomGenerator.StaticRoomDefinitions.Length);
        Vector3 position = new Vector3(CenterX, 0, 0);
        _unityObject = Object.Instantiate(RoomGenerator.StaticRoomDefinitions[randomIndex].Prefab,position, Quaternion.identity);
        Transform roomTransform = _unityObject.transform;

        Platform platform = null;


        for (int i = 0; i < 8; i++)
        {
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
                if (previousPlatform.AttachedObject != null && previousPlatform.AttachedObject.RequiresSufficientSpaceAbove)
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
            float platformX = position.x - (width / 2 - platformWidth / 2 - platformWidth * i);

            //therefore:
            platform = new Platform(roomTransform, new Vector3(platformX, platformY));






            //check if we should attach an object to the top of the platform

            if (
                Index > 0 //don't add any objects to the first room so the player adjusts to the gameplay mechanics
                &&
                previousPlatform.AttachedObject == null //don't add objects to two platforms in a row
                &&
                Random.Range(0, 4) == 0 //25% probability of object
               )
            {//add object


                int typeIndex = Random.Range(0, RoomGenerator.StaticItemsThatSitOnPlatforms.Length);
                var item = RoomGenerator.StaticItemsThatSitOnPlatforms[typeIndex];

                if (item.RequiresSufficientSpaceAbove)
                {//we want to add a ponger. But we should be careful: when we add a ponger the next platform is placed higher than usual.
                    //this means in order to add ponger we should ensure that there is sufficient space above

                    if (platform.Position.y > 1)
                    {//nope, we are way too high for ponger, instantiate another type

                        int anotherTypeIndex;
                        do
                        {
                            anotherTypeIndex = Random.Range(0, RoomGenerator.StaticItemsThatSitOnPlatforms.Length);

                        } while (anotherTypeIndex == typeIndex);

                        typeIndex = anotherTypeIndex;
                        item = RoomGenerator.StaticItemsThatSitOnPlatforms[typeIndex];
                    }
                }



                //now we know what item to add
                platform.AttachedObject = item;

                GameObject go = Object.Instantiate(item.Prefab, roomTransform);


                //let's determine its position

                Vector3 itemPosition = platform.Position;


                //to have the object perfectly sit on top of the platform we should move it up a bit.
                //By how much? ...   By half its height + half the platform's height (constant 0.308227f)
                //(Oh and it is important that we use the instantiated object for that, not the prefab. The prefab has no bounds)
                //so:
                float verticalOffset = go.GetComponent<Collider2D>().bounds.extents.y + 0.308227f;


                float horizontalOffset = 0.0f;
                if (item.AutoDetermineHorizontalOffset)
                {
                    //when we go to a spikeplatform that is higher than the previous one it is difficult to avoid the spike, so we move the spike to the right
                    //when we go to a spikeplatform that is lower than the previous one it is difficult to avoid the spike, so we move the spike to the left
                    if (platformY > previousPlatform.Position.y)
                    {//new platform is higher, so move the spike right
                        horizontalOffset = 2.5f;
                    }
                    else
                    {//move spike left
                        horizontalOffset = -2f;
                    }
                }


                itemPosition += new Vector3(horizontalOffset, verticalOffset);

                go.transform.position = itemPosition;



            }//add object



            //make the latestplatform previousplatform for the next room
            previousPlatform = platform;
        }//for platform

        LastPlatform = platform;

    }




}


public class CampaignRoom : Room
{

    public CampaignRoom(CampaignRoom previousRoom) : base(previousRoom)
    {
        _unityObject = Object.Instantiate(RoomGenerator.StaticRoomDefinitions[Index].Prefab, new Vector3(CenterX, 0, 0), Quaternion.identity);
    }
}



public class Room
{
    public float CenterX;
    public float EndX;
    public int Index;
    protected GameObject _unityObject;


    public Room(Room previousRoom)
    {
        RoomGenerator.StaticRoomIndex++;
        Index = RoomGenerator.StaticRoomIndex;

        const float width = 50.28f; //because we are using 3 backgrounds each having a 1676 pixel width



        float startX;

        if (previousRoom == null)
        {//this is the very first room of the game
            startX = -25.14f; //-25.14 because -16.76 + -16.76/2
        }
        else
        {
            startX = previousRoom.EndX; //we want the new room to start at the end of the previous room
        }

        EndX = startX + width;
        CenterX = startX + width * 0.5f;

    }




    public void Dispose()
    { Object.Destroy(_unityObject); }

}


[System.Serializable]
public class ItemThatSitsOnPlatform
{
    public bool RequiresSufficientSpaceAbove; //e.g. ponger
    public bool AutoDetermineHorizontalOffset; //e.g. spike
    public GameObject Prefab;
}

public enum Theme { Fire, Ice };


[System.Serializable]
public class RoomDefinition
{
    public GameObject Prefab;
    public bool IsProcedural; //procedural rooms have no platforms or objects. They procedurally generated
    public int IndexForNonProcedural; //non-procedural i.e. custom rooms have index from 0 to infinity. The smaller the index the sooner the room will be encountered in the game
    public Theme MyTheme;
}


public class RoomGenerator : MonoBehaviour
{
    public static RoomDefinition[] StaticRoomDefinitions;
    public static GameObject StaticPlatformPrefab;
    public static bool StaticProcedural;
    public static ItemThatSitsOnPlatform[] StaticItemsThatSitOnPlatforms;

    public Theme ThemeToUse = Theme.Fire;
    public bool Procedural = true;

    public ItemThatSitsOnPlatform[] ItemsThatSitOnPlatforms;
    public RoomDefinition[] RoomDefinitions;

    public static int PlayerIsInRoomIndex = -1; //this could also be useful for increasing game difficulty based on progress
    public static int StaticRoomIndex = -1; //it is important for this to be initialized minus one so that the first room is at index 0

    public GameObject FirePlatformPrefab;
    public GameObject IcePlatformPrefab;

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
        //find the scene room and destroy it. It is only there for visual reference for us developers 
        Destroy(GameObject.Find("DummyRoom"));



        StaticProcedural = Procedural;

        StaticItemsThatSitOnPlatforms = ItemsThatSitOnPlatforms;

        StaticPlatformPrefab = ThemeToUse == Theme.Fire ? FirePlatformPrefab : IcePlatformPrefab;


        if (Procedural)
        {
            StaticRoomDefinitions = RoomDefinitions.Where(r => r.IsProcedural == true && r.MyTheme == ThemeToUse).ToArray();

            _rooms = new List<Room> { new ProceduralRoom(null) }; //let's add the first real room (we pass null because there is no previous room)

        }
        else
        {
            StaticRoomDefinitions = RoomDefinitions.Where(r => r.IsProcedural == false && r.MyTheme == ThemeToUse).OrderBy(r => r.IndexForNonProcedural).ToArray();

            _rooms = new List<Room> { new CampaignRoom(null) }; //let's add the first real room (we pass null because there is no previous room)

        }








        float screenHeight = 2.0f * Camera.main.orthographicSize;
        _screenWidth = screenHeight * Camera.main.aspect;


        CurrentScoreText.text = "0";
        HighScoreText.text = PlayerPrefs.GetInt("HighScore", 0).ToString();



        StartCoroutine(GeneratorCheck());
    }




    private IEnumerator GeneratorCheck()
    {
        while (PlayerController.IsActive)
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
            {
                //should add a new room

                //but we should be careful in the non-procedural case if we have run out of prefabs (the procedural is infinite and can reuse the same prefab multiple times)
                if (Procedural == false && latestRoom.Index == StaticRoomDefinitions.Length - 1)
                {//no more custom rooms available. We are approaching victory (when the last room ends)


                    if (latestRoom.EndX < rightCameraBound)
                    {
                        GameObject.Find("Runner").GetComponent<PlayerController>().PlayerWon();

                    }


                }
                else
                {

                    if (Procedural)
                        _rooms.Add(new ProceduralRoom((ProceduralRoom)latestRoom));
                    else
                        _rooms.Add(new CampaignRoom((CampaignRoom)latestRoom));

                    roomAddedOrRemoved = true;
                }
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