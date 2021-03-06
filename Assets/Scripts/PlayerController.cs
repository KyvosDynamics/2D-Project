﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public struct ChangedField
{
    public object OldValue;
    public string Name;
    public Type Type;

    public ChangedField(object oldValue, string name, Type type)
    {
        OldValue = oldValue;
        Name = name;
        Type = type;
    }
}

public class ChangedFieldsCollection
{
    public List<ChangedField> ChangedFields { get; private set; }

    public ChangedFieldsCollection()
    {
        ChangedFields = new List<ChangedField>();
    }

    public void AddChangedField(ChangedField fieldDelta)
    {
        ChangedFields.Add(fieldDelta);
    }

}

public enum PlayerColor { Transparent, Cyan, Green }


public class WorldState
{
    public PlayerState PlayerState = new PlayerState();


}

public interface IState
{
    ChangedFieldsCollection FindChangedFieldsComparedTo(IState state);

    IState RetrievePreviousState(ChangedFieldsCollection cfc);
}

public class PlayerState:  IState
{
    public Vector3 Position;
    public Vector3 Velocity;
    public PlayerColor PlayerColor;
    public bool IsTrailCyan;
    public bool IsTrailInForeground;
    public List<PowerUpType> CollectedPowerUpTypes = new List<PowerUpType>();


    public PlayerState()
    {
    }

    public PlayerState(PlayerState playerState)
    {//for cloning purposes

        Position = playerState.Position;
        Velocity = playerState.Velocity;
        PlayerColor = playerState.PlayerColor;
        IsTrailCyan = playerState.IsTrailCyan;
        IsTrailInForeground = playerState.IsTrailInForeground;
        CollectedPowerUpTypes = HelperClass.CloneTrick(playerState.CollectedPowerUpTypes);
    }


    public IState RetrievePreviousState(ChangedFieldsCollection cfc)
    {//it doesn't affect the original object, it creates a clone

        PlayerState clone = new PlayerState(this);

        foreach (ChangedField cf in cfc.ChangedFields)
        {
            typeof(PlayerState).GetField(cf.Name).SetValue(clone, cf.OldValue);

            /*
            switch (cf.ChangedFieldName)
            {
                case ChangedFieldName.Velocity:
                    clone.Velocity = (Vector3)cf.OldValue;
                    break;
                case ChangedFieldName.PowerUpTypes:
                    clone.CollectedPowerUpTypes = HelperClass.CloneTrick((List<PowerUpType>)cf.OldValue);
                    break;
            }*/
        }

        return clone;
    }


    public ChangedFieldsCollection FindChangedFieldsComparedTo(IState statef)// PlayerState state)
    {
        ChangedFieldsCollection result = new ChangedFieldsCollection();


        PlayerState state = (PlayerState)statef;

        //todo: dynamically find fields
        //var fieldNames = typeof(PlayerState).GetFields()
          //                  .Select(field => field.Name)
            //                .ToList();



        if (IsTrailCyan != state.IsTrailCyan)
            result.AddChangedField(new ChangedField(IsTrailCyan, "IsTrailCyan", typeof(bool)));

        if (PlayerColor != state.PlayerColor)
            result.AddChangedField(new ChangedField(PlayerColor, "PlayerColor", typeof(PlayerColor)));

        if (IsTrailInForeground != state.IsTrailInForeground)
            result.AddChangedField(new ChangedField(IsTrailInForeground, "IsTrailInForeground", typeof(bool)));

        if (Position != state.Position)
            result.AddChangedField(new ChangedField(Position, "Position", typeof(Vector3)));

        if (Velocity != state.Velocity)
            result.AddChangedField(new ChangedField(Velocity, "Velocity", typeof(Vector3)));

        if (HelperClass.AreTheyDifferent(CollectedPowerUpTypes, state.CollectedPowerUpTypes))
            result.AddChangedField(new ChangedField(HelperClass.CloneTrick(CollectedPowerUpTypes), "CollectedPowerUpTypes", typeof(List<PowerUpType>)));

        return result;
    }
}

public static class HelperClass
{
    public static class MemberInfoGetting
    {
        public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
        {
            MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
            return expressionBody.Member.Name;
        }
        /*
         * To get name of a variable:

string testVariable = "value";
string nameOfTestVariable = MemberInfoGetting.GetMemberName(() => testVariable);
To get name of a parameter:

public class TestClass
{
    public void TestMethod(string param1, string param2)
    {
        string nameOfParam1 = MemberInfoGetting.GetMemberName(() => param1);
    }
}*/
    }

    public static bool AreTheyDifferent(List<PowerUpType> trick1, List<PowerUpType> trick2)
    {//true if different
        if (trick1.Count != trick2.Count)
            return true;

        for (int i = 0; i < trick1.Count; i++)
        {
            if (trick1[i] != trick2[i])
                return true;
        }

        return false;
    }

    public static List<PowerUpType> CloneTrick(List<PowerUpType> strings)
    {
        List<PowerUpType> result = new List<PowerUpType>();
        foreach (PowerUpType s in strings)
        {
            result.Add(s);
        }
        return result;
    }

}

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance = null;

    [HideInInspector]
    public SpriteRenderer SpriteRenderer;

    [HideInInspector]
    public bool JumpFromGround = false;

    [HideInInspector]
    public static StateDeltasGroupManager StateGroupManager = new StateDeltasGroupManager();



    [HideInInspector]
    public Dictionary<PowerUpType, PowerUp> CollectedPowerUps = new Dictionary<PowerUpType, PowerUp>();

    [HideInInspector]
    public PowerUpType? PowerUpTypeToAddOrRemove = null;
    [HideInInspector]
    public bool AddPowerUp = false;


    public float Speed;
    public LayerMask GroundLayer;
    public GameObject PlayerWasKilledUI;
    public GameObject PlayerWonUI;

    //by convention private fields start with the underscore '_' character followed by a lower-case letter
    private Rigidbody2D _rigidbody;
    private bool _isTouchingGround;
    private bool _isTouchingWall;
    private int _numOfHorRaycasts;
    private float _rayYIncr;
    private bool _switchcolor;
    private bool _jumpFromWall = false;
    private bool _jumpFromPonger = false;
    private const float _raycastingDistance = 0.1f;
    private const float _autoClimbSpeed = 2f;
    private const float _jumpSpeed = 8;
    private Vector3 _downVectorWithMagnitude;
    private Vector3 _rightVectorWithMagnitude;


    public GameObject RewindTimePrefab;
    public GameObject DoubleJumpPrefab;
    public GameObject GhostPrefab;


    void Start()
    {
        Instance = this;

        //RewindTimeComponent = GetComponent<RewindTimeComponent>();

        //RewindTimeComponent = new StateGroup(this.gameObject);
        _rigidbody = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();

        Vector3 size = GetComponent<Collider2D>().bounds.size;

        //to see if the player has hit a wall let's create 4 horizontal raycasts (if we change either the player's or the platform's size 4 may be too few or too many)
        _numOfHorRaycasts = 4;
        _rayYIncr = size.y / (_numOfHorRaycasts - 1);//minus one because for example 4 points form 3 intervals

        _downVectorWithMagnitude = Vector3.down * (size.y / 2 + 0.01f); //don't mind the 0.01f
        _rightVectorWithMagnitude = Vector3.right * size.x / 2;




        PlayerState state = new PlayerState();
        state.Position = new Vector3(6.28125f / 2, 1);
        state.PlayerColor = PlayerColor.Cyan;
        state.IsTrailCyan = true;
        state.IsTrailInForeground = true;
        //state.NumOfPowerUps = 0;

        PutPlayerInState(state, true);
    }





    void FixedUpdate()
    {
        if (StateGroupManager.IsRewinding)
        {
            // Debug.Log("command to initiate rewinding");
            StateGroupManager.OneRewind();
            return; //do not bother with physics when we are rewinding
        }



        Vector3 bottommostPoint = transform.position + _downVectorWithMagnitude;
        Vector3 bottommostRightmostPoint = bottommostPoint + _rightVectorWithMagnitude;


        _isTouchingGround = Physics2D.Raycast(bottommostPoint, Vector2.down, _raycastingDistance, GroundLayer);
        //enable this for debugging: Debug.DrawLine(bottommostPoint, bottommostPoint + Vector3.down * _raycastingDistance, _isTouchingGround ? Color.red : Color.green);


        _isTouchingWall = false;
        //lets do them from the bottom to the top
        for (int raycastIndex = 0; raycastIndex < _numOfHorRaycasts && _isTouchingWall == false; raycastIndex++)
        {
            Vector3 point = bottommostRightmostPoint + Vector3.up * raycastIndex * _rayYIncr;
            _isTouchingWall = Physics2D.Raycast(point, Vector2.right, _raycastingDistance, GroundLayer);
            //enable this for debugging: 
            Debug.DrawLine(point, point + Vector3.right * _raycastingDistance, _isTouchingWall ? Color.red : Color.green);

            if (_isTouchingWall)
            {
                int ksjdf = 34;
            }
        }


        float yVel = _rigidbody.velocity.y;

        if (_jumpFromPonger)
        {
            _jumpFromPonger = false;
            yVel = _jumpSpeed * 1.6f;
        }
        else if (JumpFromGround)
        {
            JumpFromGround = false;
            yVel = _jumpSpeed;
        }
        else if (_jumpFromWall)
        {
            _jumpFromWall = false;
            yVel = _jumpSpeed - _autoClimbSpeed;

        }
        else if (_isTouchingWall)
        {//auto climb

            yVel = _autoClimbSpeed;
        }










        PlayerState newState = new PlayerState(StateManager.CurrentState.PlayerState);
        newState.Position = transform.position;
        newState.Velocity = new Vector2(Speed, yVel);



        if (PowerUpTypeToAddOrRemove != null)
        {
            if (AddPowerUp)
                newState.CollectedPowerUpTypes.Add(PowerUpTypeToAddOrRemove.Value);// AddOrRemovePowerUp(newState, PowerUpTypeToAddOrRemove.Value, true);
            else
                newState.CollectedPowerUpTypes.Remove(PowerUpTypeToAddOrRemove.Value);

            PowerUpTypeToAddOrRemove = null;
        }


        if (_switchcolor)
        {//should create new stategroup
            _switchcolor = false;

            if (StateManager.CurrentState.PlayerState.PlayerColor == PlayerColor.Cyan)
                newState.PlayerColor = PlayerColor.Green;
            else if (StateManager.CurrentState.PlayerState.PlayerColor == PlayerColor.Green)
                newState.PlayerColor = PlayerColor.Cyan;
            //else transparent!

            newState.IsTrailCyan = !StateManager.CurrentState.PlayerState.IsTrailCyan;
        }



        PutPlayerInState(newState);



        if (transform.position.y < -5)
        {//player fell to the void

            //Debug.Log("trying to kill player because y<-5, isrewinding=" + StateGroupManager.IsRewinding + "   stategroupid=" + StateGroupManager.CurrentStateGroup.myID);

            //  CurrentState.position = transform.position;
            TryToKillPlayer();// PlayerWasKilled();
            return;
        }
    }









    internal void PutPlayerInState(PlayerState newState, bool force = false) //when force is true it sets everything no matter what changed (useful for initialization)
    {
        //find what changed
        bool playerPositionChanged = false;
        bool playerVelocityChanged = false;
        bool playerColorChanged = false;
        bool trailChanged = false;
        bool powerUpsChanged = false;


        if (StateManager.CurrentState.PlayerState.Position != newState.Position)
            playerPositionChanged = true;

        if (StateManager.CurrentState.PlayerState.Velocity != newState.Velocity)
            playerVelocityChanged = true;

        if (StateManager.CurrentState.PlayerState.PlayerColor != newState.PlayerColor)
            playerColorChanged = true;

        if (
            StateManager.CurrentState.PlayerState.IsTrailCyan != newState.IsTrailCyan
            ||
            StateManager.CurrentState.PlayerState.IsTrailInForeground != newState.IsTrailInForeground
           )
            trailChanged = true;


        if (HelperClass.AreTheyDifferent(StateManager.CurrentState.PlayerState.CollectedPowerUpTypes, newState.CollectedPowerUpTypes))
            powerUpsChanged = true;





        if (playerPositionChanged || force)
            RefreshPlayerPosition(newState);

        if (playerVelocityChanged || force)
            RefreshPlayerVelocity(newState);


        if (playerColorChanged || force)
            RefreshPlayerOnlyColor(newState);

        if (powerUpsChanged || force)
            RefreshPowerUps(newState);




        if (StateGroupManager.IsRewinding == false) //(don't add state while we are rewinding)
        {
            StateGroupManager.AddStateToCurrentGroup(newState); //add the new state
            //(it is important that we first add the state to the current group and then start a new group if necessary)

            if (trailChanged || force)
            { //there's no such thing as refresh the trail, we must start a new one:
                StateGroupManager.StartNewStateGroup(newState);
                StateGroupManager.AddStateToCurrentGroup(newState); //this is necessary because the above line merely initiated a new empty stategroup
            }
        }

        StateManager.CurrentState.PlayerState = newState; //no need to clone it because newstate was new and not a clone
    }



    private void RefreshPowerUps(PlayerState newState)
    {
        //1,2,3
        //1,2
        //->3
        var hadButNoMore = new List<PowerUpType>(CollectedPowerUps.Keys.Except(newState.CollectedPowerUpTypes)); //clone otherwise it will result in out-of-sync

        //1,2
        //1,2,3
        //->3
        var hasButDidnt = new List<PowerUpType>(newState.CollectedPowerUpTypes.Except(CollectedPowerUps.Keys)); //clone otherwise it will result in out-of-sync



        foreach (PowerUpType type in hadButNoMore)
        {
            GameObject uiImage = GameObject.Find("UIcanvas").transform.Find(type.ToString() + "Image").gameObject; //(by convention we name the image as the powerup tag +"Image", eg GhostImage)
            uiImage.SetActive(false);
            CollectedPowerUps.Remove(type);



            /*
            if (StateGroupManager.IsRewinding)
            {//time is going backwards and we just lost a powerup. This means that we haven't collected it yet!. So place it

                GameObject go = null;
                switch (type)
                {
                    case PowerUpType.DoubleJump:
                       go= UnityEngine.Object.Instantiate(DoubleJumpPrefab, transform.position  , Quaternion.identity);

                     
                        break;
                    case PowerUpType.Ghost:
                        go = UnityEngine.Object.Instantiate(GhostPrefab, transform.position , Quaternion.identity);
                        break;
                    case PowerUpType.RewindTime:
                        go = UnityEngine.Object.Instantiate(RewindTimePrefab, transform.position , Quaternion.identity);
                        break;
                }

                go.transform.position = go.transform.position + new Vector3(go.GetComponent<Collider2D>().bounds.extents.x, 0, 0) + new Vector3(this.GetComponent<Collider2D>().bounds.extents.x, 0, 0);
            }
            */

    }

        foreach (PowerUpType type in hasButDidnt)
        {
            GameObject uiImage = GameObject.Find("UIcanvas").transform.Find(type.ToString() + "Image").gameObject; //(by convention we name the image as the powerup tag +"Image", eg GhostImage)
            uiImage.SetActive(true);

            PowerUp powerup = (PowerUp)Activator.CreateInstance(null, type.ToString() + "PowerUp").Unwrap(); //(by convention we name the class as the powerup tag +"PowerUp", eg GhostPowerUp)
            CollectedPowerUps.Add(type, powerup);

            if (StateGroupManager.IsRewinding == false)
            {//we don't want to activate powerups while rewinding
                uiImage.GetComponent<AudioSource>().Play();
                if (powerup.IsActivatedImmediately) //eg Ghost
                    powerup.Activate();
            }
        }


    }




    private void RefreshPlayerPosition(PlayerState newState)
    {
        transform.position = newState.Position;
    }
    private void RefreshPlayerVelocity(PlayerState newState)
    {
        _rigidbody.velocity = newState.Velocity;
    }

    private void RefreshPlayerOnlyColor(PlayerState newState)
    {//this does not instatiate a new trail, it doesn't affect the trail

        Color color;
        switch (newState.PlayerColor)
        {
            case PlayerColor.Cyan:
                color = Color.cyan;
                break;
            case PlayerColor.Green:
                color = Color.green;
                break;
            default://            case PlayerColor.Transparent:
                color = new Color(0, 0, 0, 0); //(transparent)
                break;
        }

        SpriteRenderer.color = color;//  MyState.IsCyan ? Color.cyan : Color.green;
    }






    void Update()
    {//keyboard handling

        if (StateGroupManager.IsRewinding) //do not respond to input when we are rewinding time
            return;


        if (Input.GetKeyDown(KeyCode.Q))
            _switchcolor = true;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_isTouchingGround)
                JumpFromGround = true;
            else if (_isTouchingWall)
                _jumpFromWall = true;
            else
            {//it may still be possible to jump if we have the doublejump powerup
                if (CollectedPowerUps.ContainsKey(PowerUpType.DoubleJump))
                    CollectedPowerUps[PowerUpType.DoubleJump].Activate();
            }
        }
    }


    private void TryToKillPlayer()
    {//TODO: make it check the reason of death. If we fell into the abyss there is no need to check for the ghost powerup!

        //   Debug.Log("trying to kill player");

        if (CollectedPowerUps.ContainsKey(PowerUpType.Ghost))
        {//we are a ghost, we cannot be killed! But we lose that ability now
            CollectedPowerUps[PowerUpType.Ghost].Deactivate();
        }
        else if (CollectedPowerUps.ContainsKey(PowerUpType.RewindTime))
        {//we are not a ghost and we can be killed BUT we can rewind time ;)
            CollectedPowerUps[PowerUpType.RewindTime].Activate();
        }
        else //no powerups are protecting us from death
            PlayerWasKilled();
    }





    void OnTriggerEnter2D(Collider2D collision)
    {
        if (StateGroupManager.IsRewinding) //do not bother with collisions when we we are rewinding
            return;


        switch (collision.gameObject.tag)
        {
            case "CyanSaw":
                if (StateManager.CurrentState.PlayerState.PlayerColor == PlayerColor.Green)//  .IsCyan)
                    TryToKillPlayer();
                break;

            case "GreenSaw":
                if (StateManager.CurrentState.PlayerState.PlayerColor == PlayerColor.Cyan)// .IsCyan)
                    TryToKillPlayer();
                break;

            case "Killer":
                TryToKillPlayer();
                break;

            case "Ponger":
                _jumpFromPonger = true;
                break;

            case "EndPoint":
                PlayerWon();
                break;

            default:
                if (collision.gameObject.tag.StartsWith("PowerUp_"))
                {//we've triggered a powerup


                    collision.gameObject.GetComponent<StateMonitor>().CollectedChanged = true;// .Collected = true;

                    //Destroy(collision.gameObject);

                    string secondpart = collision.gameObject.tag.Substring(8); //(to remove the "PowerUp_" string from the tag string)

                    // poweruptoadd = secondpart;
                    PowerUpType poweruptype = (PowerUpType)Enum.Parse(typeof(PowerUpType), secondpart);

                    if (CollectedPowerUps.ContainsKey(poweruptype) == false) //we don't have this powerup
                    {
                        PowerUpTypeToAddOrRemove = poweruptype;
                        AddPowerUp = true;

                    }

                }
                break;
        }
    }




    private void PlayerWasKilled()
    {
        Time.timeScale = 0f;
        PlayerWasKilledUI.SetActive(true);
    }
    private void PlayerWon()
    {
        Time.timeScale = 0f;
        PlayerWonUI.SetActive(true);
    }


}


public class DoubleJumpPowerUp : PowerUp
{
    public DoubleJumpPowerUp() : base(PowerUpType.DoubleJump) { }

    public override void Activate()
    {
        PlayerController.Instance.JumpFromGround = true;
        base.Activate();
    }

}
public class GhostPowerUp : PowerUp
{
    public GhostPowerUp() : base(PowerUpType.Ghost, true) { }

    public override void Activate()
    {
        PlayerController.Instance.SpriteRenderer.color = new Color(255, 255, 255, 0); //trick to make player invisible (we want the trontrail to continue to be visible)
        base.Activate();
    }
    public override void Deactivate()
    {
        //temporarily disabled        PlayerController.Instance.RefreshPlayerOnlyColor();
        base.Deactivate();
    }
}
public class RewindTimePowerUp : PowerUp
{
    public RewindTimePowerUp() : base(PowerUpType.RewindTime) { }

    public override void Activate()
    {
        base.Activate();
        PlayerController.StateGroupManager.InitiateRewinding();
    }
}

public enum PowerUpType { DoubleJump, Ghost, RewindTime };

public class PowerUp
{
    public PowerUpType mytype;
    public bool IsActivatedImmediately;

    public PowerUp(PowerUpType type, bool isActivatedImmediately = false)
    {
        mytype = type;
    }

    public virtual void Activate()
    {
        if (!IsActivatedImmediately)
            Remove();
    }
    public virtual void Deactivate()
    {
        if (IsActivatedImmediately)
            Remove();
    }

    private void Remove()
    {
        PlayerController.Instance.PowerUpTypeToAddOrRemove = mytype;
        PlayerController.Instance.AddPowerUp = false;
    }

}