using System;
using System.Collections.Generic;
using UnityEngine;


public enum ChangedFieldName { Position, Velocity, PlayerColor, IsTrailCyan, IsTrailInForeground };// TrailCyan, TrailInForeground, X, Y, PlayerColor, SpeedX, SpeedY };//,PowerUps};
public struct ChangedField
{
    public ChangedFieldName ChangedFieldName;
    public object OldValue;
}

public class ChangedFieldsCollection
{

    public void AddChangedField(ChangedField fieldDelta)
    {
        ChangedFields.Add(fieldDelta);
    }

    public List<ChangedField> ChangedFields = new List<ChangedField>();
}

public enum PlayerColor { Transparent, Cyan, Green }

public class PlayerState
{
    public Vector3 Position;
    public Vector3 Velocity;
    public PlayerColor PlayerColor;
    public bool IsTrailCyan;
    public bool IsTrailInForeground;
    public Dictionary<PowerUpType, PowerUp> CollectedPowerUps = new Dictionary<PowerUpType, PowerUp>();


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
        CollectedPowerUps = playerState.CollectedPowerUps;
    }


    internal PlayerState RetrievePreviousState(ChangedFieldsCollection cfc)
    {//it doesn't affect the original object, it creates a clone

        PlayerState clone = new PlayerState(this);

        foreach (ChangedField cf in cfc.ChangedFields)
        {
            switch (cf.ChangedFieldName)
            {
                case ChangedFieldName.IsTrailCyan:// .TrailCyan:
                    clone.IsTrailCyan = (bool)cf.OldValue;// = !clone.IsTrailCyan;
                    break;
                case ChangedFieldName.PlayerColor:
                    clone.PlayerColor = (PlayerColor)cf.OldValue;// -= (int)fd.OldValue;
                    break;
                case ChangedFieldName.IsTrailInForeground:// .TrailInForeground:
                    clone.IsTrailInForeground = (bool)cf.OldValue;// !clone.IsTrailInForeground;
                    break;
                case ChangedFieldName.Position:// .X:
                    clone.Position = (Vector3)cf.OldValue;
                    break;
                //case ChangedFieldName.Y:
                //  clone.Position.y = (float)cf.OldValue;
                // break;
                case ChangedFieldName.Velocity:// .SpeedX:
                    clone.Velocity = (Vector3)cf.OldValue;
                    break;
                    //case ChangedFieldName.SpeedY:
                    //   clone.Velocity.y = (float)cf.OldValue;
                    // break;
            }

        }

        return clone;
    }

    internal ChangedFieldsCollection FindDeltasToState(PlayerState state)
    {
        ChangedFieldsCollection result = new ChangedFieldsCollection();

        if (IsTrailCyan != state.IsTrailCyan)
        {
            ChangedField fd = new ChangedField();
            fd.ChangedFieldName = ChangedFieldName.IsTrailCyan;// .TrailCyan;
            //no need to specify deltavalue for boolean
            fd.OldValue = IsTrailCyan;
            result.AddChangedField(fd);
        }
        if (PlayerColor != state.PlayerColor)// IsCyan != state.IsCyan)
        {
            ChangedField fd = new ChangedField();
            fd.ChangedFieldName = ChangedFieldName.PlayerColor;// = FieldNames.Cyan;
            fd.OldValue = PlayerColor;
            result.AddChangedField(fd);
        }
        if (IsTrailInForeground != state.IsTrailInForeground)
        {
            ChangedField fd = new ChangedField();
            fd.ChangedFieldName = ChangedFieldName.IsTrailInForeground;// .TrailInForeground;
            //no need to specify deltavalue for boolean
            fd.OldValue = IsTrailInForeground;
            result.AddChangedField(fd);
        }
        if (Position != state.Position)// != state.Position.x)
        {
            ChangedField fd = new ChangedField();
            fd.ChangedFieldName = ChangedFieldName.Position;// ChangedFieldName.X;
            fd.OldValue = Position;// state.Position.x - Position.x;
            result.AddChangedField(fd);
        }
        //    if (Position.y != state.Position.y)
        //  {
        //    ChangedField fd = new ChangedField();
        //   fd.ChangedFieldName = ChangedFieldName.Y;
        //  fd.OldValue =  Position.y;
        // result.AddChangedField(fd);
        // }
        if (Velocity != state.Velocity)// .x != state.Velocity.x)
        {
            ChangedField fd = new ChangedField();
            fd.ChangedFieldName = ChangedFieldName.Velocity;//  ChangedFieldName.SpeedX;
            fd.OldValue = Velocity;
            result.AddChangedField(fd);
        }
        //        if (Velocity.y != state.Velocity.y)
        //      {
        //        ChangedField fd = new ChangedField();
        //      fd.ChangedFieldName = ChangedFieldName.SpeedY;
        //    fd.OldValue =Velocity.y;
        //  result.AddChangedField(fd);
        //}
        //    if(CollectedPowerUps.GetHashCode() != state.CollectedPowerUps.GetHashCode() )
        //   {
        //     FieldDelta fd = new FieldDelta();
        //   fd.DeltaName = DeltaName.PowerUps;
        //   //for the powerups we don't really use a delta but the actual values
        // //fd.DeltaValue=
        //}

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
    public StateDeltasGroupManager StateGroupManager = new StateDeltasGroupManager();

    [HideInInspector]
    public PlayerState CurrentState = new PlayerState();

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


        PutPlayerInState(state, true);
    }





    void FixedUpdate()
    {
        if (StateGroupManager.IsRewinding)
        {
            Debug.Log("command to initiate rewinding");
            StateGroupManager.OneRewind();
            return; //do not bother with physics when we are rewinding
        }

        if (transform.position.y < -5)
        {//player fell to the void

            //Debug.Log("trying to kill player because y<-5, isrewinding=" + StateGroupManager.IsRewinding + "   stategroupid=" + StateGroupManager.CurrentStateGroup.myID);


            //  CurrentState.position = transform.position;
            TryToKillPlayer();// PlayerWasKilled();
            return;
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










        PlayerState newState = new PlayerState(CurrentState);
        newState.Position = transform.position;
        newState.Velocity = new Vector2(Speed, yVel);


        if (_switchcolor)
        {//should create new stategroup
            _switchcolor = false;

            if (newState.PlayerColor == PlayerColor.Cyan)
                newState.PlayerColor = PlayerColor.Green;
            else if (newState.PlayerColor == PlayerColor.Green)
                newState.PlayerColor = PlayerColor.Cyan;
            //else transparent!

            newState.IsTrailCyan = !newState.IsTrailCyan;

        }




        PutPlayerInState(newState);
    }




    internal void PutPlayerInState(PlayerState newState, bool force = false) //when force is true it sets everything no matter what changed (useful for initialization)
    {
        //find what changed
        bool playerPositionChanged = false;
        bool playerVelocityChanged = false;
        bool playerColorChanged = false;
        bool trailChanged = false;

        if (CurrentState.Position != newState.Position)
            playerPositionChanged = true;

        if (CurrentState.Velocity != newState.Velocity)
            playerVelocityChanged = true;

        if (CurrentState.PlayerColor != newState.PlayerColor)
            playerColorChanged = true;

        if (
            CurrentState.IsTrailCyan != newState.IsTrailCyan
            ||
            CurrentState.IsTrailInForeground != newState.IsTrailInForeground
           )
            trailChanged = true;




        if (playerPositionChanged || force)
            RefreshPlayerPosition(newState);

        if (playerVelocityChanged || force)
            RefreshPlayerVelocity(newState);


        if (playerColorChanged || force)
            RefreshPlayerOnlyColor(newState);





        if (StateGroupManager.IsRewinding == false) //(don't add state while we are rewinding)
        {
            StateGroupManager.AddStateToCurrentGroup(newState); //add the new state
            //(it is important that we first add the state to the current group and then start a new group is necessary


            if (trailChanged || force)
            { //there's no such thing as refresh the trail, we must start a new one:
                StateGroupManager.StartNewStateGroup(newState);
                StateGroupManager.AddStateToCurrentGroup(newState); //add the new state
            }
        }




        CurrentState = new PlayerState(newState);
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
                if (CurrentState.CollectedPowerUps.ContainsKey(PowerUpType.DoubleJump))
                    CurrentState.CollectedPowerUps[PowerUpType.DoubleJump].Activate();
            }
        }
    }


    private void TryToKillPlayer()
    {//TODO: make it check the reason of death. If we fell into the abyss there is no need to check for the ghost powerup!

        Debug.Log("trying to kill player");

        if (CurrentState.CollectedPowerUps.ContainsKey(PowerUpType.Ghost))
        {//we are a ghost, we cannot be killed! But we lose that ability now
            CurrentState.CollectedPowerUps[PowerUpType.Ghost].Deactivate();
        }
        else if (CurrentState.CollectedPowerUps.ContainsKey(PowerUpType.RewindTime))
        {//we are not a ghost and we can be killed BUT we can rewind time ;)
            CurrentState.CollectedPowerUps[PowerUpType.RewindTime].Activate();
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
                if (CurrentState.PlayerColor == PlayerColor.Green)//  .IsCyan)
                    TryToKillPlayer();
                break;

            case "GreenSaw":
                if (CurrentState.PlayerColor == PlayerColor.Cyan)// .IsCyan)
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

                    Destroy(collision.gameObject);

                    string secondpart = collision.gameObject.tag.Substring(8); //(to remove the PowerUp_ string from the tag)

                    PowerUpType poweruptype = (PowerUpType)Enum.Parse(typeof(PowerUpType), secondpart);
                    if (CurrentState.CollectedPowerUps.ContainsKey(poweruptype)) //we already have this powerup
                        return;

                    var powerupHandle = Activator.CreateInstance(null, secondpart + "PowerUp"); //(by convention we name the class as the powerup tag +"PowerUp", eg GhostPowerUp)
                    PowerUp powerup = (PowerUp)powerupHandle.Unwrap();

                    CurrentState.CollectedPowerUps.Add(poweruptype, powerup);

                    GameObject uiImage = GameObject.Find("UIcanvas").transform.Find(secondpart + "Image").gameObject; //(by convention we name the image as the powerup tag +"Image", eg GhostImage)
                    uiImage.SetActive(true);
                    uiImage.GetComponent<AudioSource>().Play();

                    if (powerup.IsActivatedImmediately) //eg Ghost
                        powerup.Activate();
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
        PlayerController.Instance.StateGroupManager.InitiateRewinding();
        base.Activate();
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
        PlayerController.Instance.CurrentState.CollectedPowerUps.Remove(mytype);
        GameObject uiImage = GameObject.Find("UIcanvas").transform.Find(mytype.ToString() + "Image").gameObject; //(by convention we name the image as the powerup tag +"Image", eg GhostImage)
        uiImage.SetActive(false);
    }

}