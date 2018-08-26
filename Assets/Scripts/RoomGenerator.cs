using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class Platform
{
    public ItemThatSitsOnPlatform AttachedItem = null;
    public Vector3 Position { get; private set; }


    public Platform(float x, float y, Transform roomTransform)
    {
        Position = new Vector3(x, y);
        Object.Instantiate(RoomGenerator.StaticPlatformPrefab, Position, Quaternion.identity, roomTransform);
    }

}


public class CampaignRoom : Room
{

    public CampaignRoom(CampaignRoom previousRoom, bool isGoalRoom = false) : base(previousRoom)
    {

        if (isGoalRoom == false)
            _unityObject = Object.Instantiate(RoomGenerator.StaticRoomDefinitions[Index].Prefab, new Vector3(CenterX, 0, 0), Quaternion.identity);
        else
            _unityObject = Object.Instantiate(RoomGenerator.StaticGoalRoomPrefab, new Vector3(CenterX, 0, 0), Quaternion.identity);
    }

}



public class ProceduralRoom : Room
{
    public Platform LastPlatform { get; private set; }


    public ProceduralRoom(ProceduralRoom previousRoom) : base(previousRoom)
    {
        Platform previousPlatform = previousRoom == null ? null : previousRoom.LastPlatform;  //the previous platform is the last platform of the previous room

        const float width = 50.28f; //because we are using a background with 5028 pixels width
        const float platformWidth = 6.285f; //(notice platformWidth * 8 = roomWidth)  , also 1356 * scale= 628.5, scale= 0.4635
        const float platformHeight = 0.616455f;

        int randomIndex = Random.Range(0, RoomGenerator.StaticRoomDefinitions.Length);
        Vector3 position = new Vector3(CenterX, 0, 0);
        _unityObject = Object.Instantiate(RoomGenerator.StaticRoomDefinitions[randomIndex].Prefab, position, Quaternion.identity);
        Transform roomTransform = _unityObject.transform;

        Platform platform = null;

        for (int i = 0; i < 8; i++)
        {
            float platformY = -1; //it is important that we initialize this to minus one. This is the desired y value for the very first platform of our game (for this platform previousplatform=null)

            if (previousPlatform != null)
            {//for every other platform we want a y that is the y of the previous platform plus/minus a small random value

                //the only exception is when the previous platform had a ponger on it. We want the new platform to be placed much higher than usual
                if (previousPlatform.AttachedItem != null && previousPlatform.AttachedItem.RequiresSufficientSpaceAbove)
                {
                    platformY = previousPlatform.Position.y + 4 * platformHeight;
                }
                else
                {
                    int upSameOrDown = Random.Range(0, 3);
                    switch (upSameOrDown)
                    {
                        case 0: //up
                            platformY = previousPlatform.Position.y + platformHeight;

                            if (platformY > 2f)//out of bounds, go down instead
                                platformY = previousPlatform.Position.y - platformHeight;
                            break;

                        case 1: //same level
                            platformY = previousPlatform.Position.y;
                            break;

                        case 2: //down
                            platformY = previousPlatform.Position.y - platformHeight;

                            if (platformY < -4f)//out of bounds, go up instead
                                platformY = previousPlatform.Position.y + platformHeight;
                            break;
                    }

                }
            }


            //now we know y. For x it's easy as we know the position of the room and the relative position of the platform inside the room
            float platformX = position.x - (width / 2 - platformWidth / 2 - platformWidth * i);

            //therefore:
            platform = new Platform(platformX, platformY, roomTransform);





            //check if we should attach an object to the top of the platform

            if (
                Index > 0 //don't add any objects to the first room so the player adjusts to the gameplay mechanics
                &&
                previousPlatform.AttachedItem == null //don't add objects to two platforms in a row
                &&
                Random.Range(0, 4) == 0 //25% probability of object
               )
            {//add object

                int typeIndex = Random.Range(0, RoomGenerator.StaticItemsThatSitOnPlatforms.Length);
                ItemThatSitsOnPlatform item = RoomGenerator.StaticItemsThatSitOnPlatforms[typeIndex];

                if (item.RequiresSufficientSpaceAbove)
                {//we want to add a ponger. But we should be careful: when we add a ponger the next platform is placed higher than usual.
                    //this means that in order to add a ponger we should first make sure that there is indeed sufficient space above

                    if (platform.Position.y > 1)
                    {//nope, we are way too high for a ponger, instantiate another type

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
                platform.AttachedItem = item;

                //instantiate it
                GameObject go = Object.Instantiate(item.Prefab, roomTransform);


                //let's determine its position
                //starting with its platform's position
                Vector3 itemPosition = platform.Position;

                //to have the object perfectly sit on top of the platform we should move it up a bit.
                //By how much? ...   By half its height + half the platform's height (constant 0.308227f)
                //(Oh and it is important that we use the instantiated object for that, not the prefab. The prefab has no bounds)
                //so:
                float verticalOffset = go.GetComponent<Collider2D>().bounds.extents.y + 0.308227f;

                float horizontalOffset = 0.0f;
                if (item.AutoDetermineHorizontalOffset)
                {//e.g. spike
                    //when we go to a spike platform that is higher than the previous one it is difficult to avoid the spike, so we move the spike to the right
                    //when we go to a spike platform that is lower than the previous one it is difficult to avoid the spike, so we move the spike to the left
                    if (platformY > previousPlatform.Position.y)
                    {//new platform is higher, so move the spike right
                        horizontalOffset = 2.5f;
                    }
                    else if (platformY < previousPlatform.Position.y)
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




public class Room
{
    public int Index;
    public float CenterX;
    public float EndX;
    protected GameObject _unityObject;


    public Room(Room previousRoom)
    {
        RoomGenerator.StaticRoomIndex++;
        Index = RoomGenerator.StaticRoomIndex;

        float width = 50.28f; //because we are using a background with width 5028 pixels

        float startX;

        if (previousRoom == null)
        {//this is the very first room of the game
            startX = -width / 2;
        }
        else
        {//we want the new room to start at the end of the previous room
            startX = previousRoom.EndX;
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
    public Theme MyTheme;
}

public enum Theme { All, Fire, Ice };


[System.Serializable]
public class CampaignRoomDefinition
{
    public GameObject Prefab;
//    public bool IsProcedural; //procedural rooms have no platforms or objects. They procedurally generated
    public int IndexForNonProcedural; //non-procedural i.e. custom rooms have index from 0 to infinity. The smaller the index the sooner the room will be encountered in the game
    public Theme MyTheme;
}


public class RoomGenerator : MonoBehaviour
{
    public GameObject FireProceduralRoom;
    public GameObject IceProceduralRoom;
    public GameObject FireCampaignGoalRoomPrefab;
    public GameObject IceCampaignGoalRoomPrefab;

    public static ItemThatSitsOnPlatform[] StaticItemsThatSitOnPlatforms;
    public static CampaignRoomDefinition[] StaticRoomDefinitions;
    public static GameObject StaticPlatformPrefab;
    public static GameObject StaticGoalRoomPrefab;

    public static int MaxRoomIndex = -1; //this could also be useful for increasing game difficulty based on progress
    public static int StaticRoomIndex = -1; //it is important for this to be initialized minus one so that the first room is at index 0
    [SerializeField]
    private Theme _themeNeverSetThisToAll = Theme.Fire;
    [SerializeField]
    private bool _procedural = true;
    public ItemThatSitsOnPlatform[] ItemsThatSitOnPlatforms;
    public CampaignRoomDefinition[] CampaignRoomDefinitions;
    public GameObject FirePlatformPrefab;
    public GameObject IcePlatformPrefab;

    public Text CurrentScoreText;
    public Text HighScoreText;

    private List<Room> _rooms;
    private float _screenWidth;

    public static Theme? StaticThemeToUse; //this is set in the menu script. We've made it nullable so that we can override it from the inspector
    public static bool? StaticProcedural; //this is set in the menu script. We've made it nullable so that we can override it from the inspector


    private void Awake()
    {
        //it is absolutely vital that we first reset the static variables because they are not automatically reset when we start a new game (e.g. after we are killed)
        MaxRoomIndex = -1;
        StaticRoomIndex = -1;


        if (StaticThemeToUse.HasValue)
            _themeNeverSetThisToAll = StaticThemeToUse.Value;
        if (StaticProcedural.HasValue)
            _procedural = StaticProcedural.Value;
    }

    void Start()
    {
        //find the scene room and destroy it. It is only there for visual reference for us developers 
        Destroy(GameObject.Find("DummyRoom"));



        StaticItemsThatSitOnPlatforms = ItemsThatSitOnPlatforms.Where(i => i.MyTheme == _themeNeverSetThisToAll || i.MyTheme == Theme.All).ToArray();


        if (_themeNeverSetThisToAll == Theme.Fire)
        {
            StaticPlatformPrefab = FirePlatformPrefab;
            StaticGoalRoomPrefab = FireCampaignGoalRoomPrefab;

            StaticRoomDefinitions = new CampaignRoomDefinition[] { new CampaignRoomDefinition() { Prefab = FireProceduralRoom } };
            
        }
        else
        {
            StaticPlatformPrefab = IcePlatformPrefab;
            StaticGoalRoomPrefab = IceCampaignGoalRoomPrefab;

            StaticRoomDefinitions = new CampaignRoomDefinition[] { new CampaignRoomDefinition() { Prefab = IceProceduralRoom } };
        }



        if (_procedural)
        {
            //StaticRoomDefinitions = RoomDefinitions.Where(r => r.IsProcedural == true && r.MyTheme == _themeNeverSetThisToAll).ToArray();
            //let's add the first real room (we pass null because there is no previous room)
            _rooms = new List<Room> { new ProceduralRoom(null) };
        }
        else
        {
            StaticRoomDefinitions = CampaignRoomDefinitions.Where(r=>r.MyTheme == _themeNeverSetThisToAll).OrderBy(r => r.IndexForNonProcedural).ToArray();
            //let's add the first real room (we pass null because there is no previous room)
            _rooms = new List<Room> { new CampaignRoom(null) };
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



                if (_procedural)
                {//nothing to consider, add a room
                    _rooms.Add(new ProceduralRoom((ProceduralRoom)latestRoom));

                    roomAddedOrRemoved = true;

                }
                else
                {//more tricky case...
                 //in campaign mode we eventually run out of prefabs to use (finite list).
                 //when that happens we should add a special "goal" room instead.


                    if (latestRoom.Index < StaticRoomDefinitions.Length - 1)
                    {//we have prefab

                        _rooms.Add(new CampaignRoom((CampaignRoom)latestRoom));
                        roomAddedOrRemoved = true;
                    }
                    else if (latestRoom.Index == StaticRoomDefinitions.Length - 1) //(strictly equal, NOT <= )
                    {//we've run out of prefabs, add the special "goal" room

                        _rooms.Add(new CampaignRoom((CampaignRoom)latestRoom, true));
                        roomAddedOrRemoved = true; //(we still set this flag because we want the goal room to increase the score too)
                    }









                }




            }




            if (roomAddedOrRemoved)
            {//rooms changed so update the progress text

                if (_rooms.Count == 1)
                {
                    MaxRoomIndex = _rooms[0].Index;
                }
                else
                {//two rooms
                    MaxRoomIndex = _rooms[1].Index;
                }


                CurrentScoreText.text = MaxRoomIndex.ToString();

                if (MaxRoomIndex > PlayerPrefs.GetInt("HighScore", 0))
                {
                    PlayerPrefs.SetInt("HighScore", MaxRoomIndex);
                    HighScoreText.text = MaxRoomIndex.ToString();
                }

            }



            yield return new WaitForSeconds(0.25f);
        }
    }





}