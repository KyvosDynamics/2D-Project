using System;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance = null;

    [HideInInspector]
    public Dictionary<PowerUpType, PowerUp> CollectedPowerUps = new Dictionary<PowerUpType, PowerUp>();

  //  [HideInInspector]
    //public bool IsCyan;

    [HideInInspector]
    public SpriteRenderer SpriteRenderer;

    [HideInInspector]
    public bool JumpFromGround = false;



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

    public StateGroupManager StateGroupManager = new StateGroupManager();


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

        transform.position = new Vector3(6.28125f / 2, 1);

      MyState.iscyan = true; //start cyan
        ApplyColorAccordingToFlag(true);


        //CurrentStateGroup = new StateGroup(transform, this, transform.position, IsCyan, true);
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
            Debug.Log("trying to kill player because y<-5, isrewinding=" + StateGroupManager.IsRewinding + "   stategroupid=" + StateGroupManager.CurrentStateGroup.myID);
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


        _rigidbody.velocity = new Vector2(Speed, yVel);



        if (_switchcolor)
        {
            _switchcolor = false;


        MyState.iscyan= !MyState.iscyan;
            ApplyColorAccordingToFlag(true);

            if (CollectedPowerUps.ContainsKey(PowerUpType.Ghost))// .Contains("Ghost"))// hasGhost)
                CollectedPowerUps[PowerUpType.Ghost].Activate();// _spriteRenderer.color = new Color(255, 255, 255, 0);

        }





        MyState.position = transform.position;
        StateGroupManager.CurrentStateGroup.AddState(MyState);// new State() { position = transform.position, iscyan = IsCyan, InForeground = inforeground });// new StateValues(transform.position, IsCyan));//.AddPoint();
        //_currentTronTrail.AddPoint(transform.position);
    }

//    bool inforeground = true;

    internal void SetPosition(Vector3 position)
    {
        MyState.position = transform.position = position;        
    }

    public void ApplyColorAccordingToFlag(bool initiateSimilarlyColoredTrail)
    {
        SpriteRenderer.color = MyState.iscyan ? Color.cyan : Color.green;
        if (initiateSimilarlyColoredTrail)
            StartForegroundTronTrail();
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

        Debug.Log("trying to kill player");

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


    public State MyState = new State();


    public void StartForegroundTronTrail()
    {
        //todo: are the following two lines really necessary?
        MyState.position = transform.position;
        StateGroupManager.CloseCurrentStateGroup(MyState);// new State() { position = transform.position, iscyan = IsCyan, InForeground = inforeground });

       MyState.InForeground = true;
        StateGroupManager.StartNewStateGroup(MyState);// new State() { position = transform.position, iscyan = IsCyan, InForeground = inforeground });// IsCyan);

    }
    public void StartBackgroundTronTrail()
    {//eg when the player is passing through a portal. We don't want the trail to pass on top of the portal effect
     //todo: are the following two lines really necessary?
        MyState.position = transform.position;
        StateGroupManager.CloseCurrentStateGroup(MyState);// new State() { position = transform.position, iscyan = IsCyan, InForeground = inforeground });

       MyState.InForeground = false;
        StateGroupManager.StartNewStateGroup(MyState);// new State() { position = transform.position, iscyan = IsCyan, InForeground = inforeground });// IsCyan);
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (StateGroupManager.IsRewinding) //do not bother with collisions when we we are rewinding
            return;


        switch (collision.gameObject.tag)
        {
            case "CyanSaw":
                if (!MyState.iscyan)
                    TryToKillPlayer();
                break;

            case "GreenSaw":
                if (MyState.iscyan)
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
                    if (CollectedPowerUps.ContainsKey(poweruptype)) //we already have this powerup
                        return;

                    var powerupHandle = Activator.CreateInstance(null, secondpart + "PowerUp"); //(by convention we name the class as the powerup tag +"PowerUp", eg GhostPowerUp)
                    PowerUp powerup = (PowerUp)powerupHandle.Unwrap();

                    CollectedPowerUps.Add(poweruptype, powerup);

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
        PlayerController.Instance.ApplyColorAccordingToFlag(false); //restore color according to the IsCyan flag, we set false because the trail was active anyway
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
        PlayerController.Instance.CollectedPowerUps.Remove(mytype);
        GameObject uiImage = GameObject.Find("UIcanvas").transform.Find(mytype.ToString() + "Image").gameObject; //(by convention we name the image as the powerup tag +"Image", eg GhostImage)
        uiImage.SetActive(false);
    }

}