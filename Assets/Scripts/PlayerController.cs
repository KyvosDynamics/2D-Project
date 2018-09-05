using System;
using System.Collections.Generic;
using UnityEngine;

public class TronTrail
{
    private List<Vector3> _points = new List<Vector3>();
    private LineRenderer _lineRenderer = null;
    private const float _minSampleDistance = 0.1f;


    public TronTrail(Color color, Vector3 startPosition, bool inForeground)
    {
        GameObject gameObject = new GameObject("TronTrailHolder");
        _lineRenderer = gameObject.AddComponent<LineRenderer>();

        _lineRenderer.sortingLayerName = inForeground ? "Player" : "RoomObjectsBehindPlayer";
        _lineRenderer.sortingOrder = -10; //so that for example it is behind a portal effect

        _lineRenderer.startColor = _lineRenderer.endColor = color;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));//or Shader.Find("Particles/Additive")); ?
        //_lineRenderer.SetWidth(1, 1);


        AddPoint(startPosition);
    }

    public void AddPoint(Vector3 position)
    {
        _points.Add(position);
        _lineRenderer.positionCount = _points.Count;
        _lineRenderer.SetPositions(_points.ToArray());
    }

    public void CheckIfShouldAddPointAndIfYesAddIt(Vector3 newPosition)
    {
        //if the latest position is significantly far from the previous point we add a new point

        if ((newPosition - _points[_points.Count - 1]).sqrMagnitude > _minSampleDistance * _minSampleDistance)        
            AddPoint(newPosition);       
    }


}

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<PowerUpTypes, PowerUp> _collectedPowerUps = new Dictionary<PowerUpTypes, PowerUp>();






    public bool IsCyan { get; private set; }

    public float Speed;
    public LayerMask GroundLayer;
    public GameObject PlayerWasKilledUI;
    public GameObject PlayerWonUI;

    //by convention private fields start with the underscore '_' character followed by a lower-case letter
    private Rigidbody2D _rigidbody;
    [HideInInspector]
    public SpriteRenderer _spriteRenderer;
    private bool _isTouchingGround;
    private bool _isTouchingWall;
    private int _numOfHorRaycasts;
    private float _rayYIncr;
    private bool _switchcolor;
    [HideInInspector]
    public bool _jumpFromGround = false;
    private bool _jumpFromWall = false;
    private bool _jumpFromPonger = false;
    private const float _raycastingDistance = 0.1f;
    private const float _autoClimbSpeed = 2f;
    private const float _jumpSpeed = 8;
    private Vector3 _downVectorWithMagnitude;
    private Vector3 _rightVectorWithMagnitude;


    public static PlayerController Instance = null;


    [HideInInspector]
    public RewindTimeComponent _rewindTimeComponent;




    void Start()
    {
        Instance = this;
        _rewindTimeComponent = GetComponent<RewindTimeComponent>();
        //      _trailRenderer = GetComponent<TrailRenderer>();

        //line.mat


        //_trailRenderer.SetPositions(new Vector3[] { new Vector3(0, 0), new Vector3(1, 0) });

        //    _trailRendererGradient = new Gradient();
        //  _trailRendererGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.cyan, 1.0f) },
        //                  new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        //  line.colorGradient = _trailRendererGradient;  //_trailRenderer.colorGradient = _trailRendererGradient;



        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();


        Vector3 size = GetComponent<Collider2D>().bounds.size;

        //to see if the player has hit a wall let's create 4 horizontal raycasts (if we change either the player's or the platform's size 4 may be too few or too many)
        _numOfHorRaycasts = 4;
        _rayYIncr = size.y / (_numOfHorRaycasts - 1);//minus one because for example 4 points form 3 intervals

        _downVectorWithMagnitude = Vector3.down * (size.y / 2 + 0.01f); //don't mind the 0.01f
        _rightVectorWithMagnitude = Vector3.right * size.x / 2;


        transform.position = new Vector3(6.28125f / 2, 1);


        //the following two so that the player starts cyan
        //_switchcolor = true;
        //IsCyan = false;



        SetColor(true);



        //    }
        //  private void Start()
        //{


        //  if (started)
        // {
        //        head = tail = 0;
        //    points[0] = transform.position;
        //   times[0] = Time.time;
        // hasChanged = true;
        //started = false;
        //}
        //        Vector3[] currentPoints = new Vector3[head - tail + 1];
        //      System.Array.Copy(points, tail, currentPoints, 0, head - tail + 1);
        //        line.positionCount = 1;// head - tail + 1;
        //      line.SetPositions(new Vector3[] { points[0] });
        //   firstime = Time.time;

        //       currentSnake = new TronTrail(Color.white, transform.position, true);
    }

    TronTrail currentSnake = null;

    void FixedUpdate()
    {
        if (_rewindTimeComponent.isRewinding) //do not bother with physics when we are rewinding
            return;

        if (transform.position.y < -5)
        {//player fell to the void
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
        else if (_jumpFromGround)
        {
            _jumpFromGround = false;
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



            IsCyan = !IsCyan;


            SetColor(IsCyan);

            if (_collectedPowerUps.ContainsKey(PowerUpTypes.Ghost))// .Contains("Ghost"))// hasGhost)
                _collectedPowerUps[PowerUpTypes.Ghost].Activate();// _spriteRenderer.color = new Color(255, 255, 255, 0);

        }










        currentSnake.CheckIfShouldAddPointAndIfYesAddIt(transform.position);
    }





    public void SetColor(bool iscyan)
    {            // _trailRenderer.startColor = _trailRenderer.endColor = 
                 //        _spriteRenderer.color = IsCyan ? Color.cyan : Color.green;


        IsCyan = iscyan;

        Color color = IsCyan ? Color.cyan : Color.green;

        // setgradientcolor(color);
        _spriteRenderer.color = color;

        StartForegroundTronTrail();
    }




    void Update()
    {//keyboard handling

        if (_rewindTimeComponent.isRewinding) //do not respond to input when we are rewinding time
            return;



        if (Input.GetKeyDown(KeyCode.Q))
            _switchcolor = true;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_isTouchingGround)
                _jumpFromGround = true;
            else if (_isTouchingWall)
                _jumpFromWall = true;
            else
            {//it may still be possible to jump if we have the doublejump powerup
             //  private void Jump(bool isGrounded)
                if (_collectedPowerUps.ContainsKey(PowerUpTypes.DoubleJump))// "DoubleJump"))// hasDoubleJump)
                {
                    _collectedPowerUps[PowerUpTypes.DoubleJump].Activate();
                    //activateDoubleJumpPowerUp();
                }

            }
        }
    }




    private void TryToKillPlayer()
    {//TODO: make it check the reason of death. If we fell into the abyss there is no need to check for the ghost powerup!

        if (_collectedPowerUps.ContainsKey(PowerUpTypes.Ghost))// "Ghost"))// hasGhost)
        {
            _collectedPowerUps[PowerUpTypes.Ghost].Deactivate();
            //_collectedPowerUps[PowerUpTypes.Ghost].Remove();// GhostNoMore();
        }
        else if (_collectedPowerUps.ContainsKey(PowerUpTypes.RewindTime))// "RewindTime"))// hasRewindTime)
        {
            _collectedPowerUps[PowerUpTypes.RewindTime].Activate();
            //_collectedPowerUps[PowerUpTypes.RewindTime].Remove();// GhostNoMore();

            //   ewindTime();
        }
        else
            PlayerWasKilled();
    }



    public void StartBackgroundTronTrail()
    {
        currentSnake.AddPoint(transform.position);
        currentSnake = new TronTrail(IsCyan ? Color.cyan : Color.green, transform.position, false);
    }
    public void StartForegroundTronTrail()
    {
        if (currentSnake != null)//it can be null the first time we call this method
            currentSnake.AddPoint(transform.position);

        currentSnake = new TronTrail(IsCyan ? Color.cyan : Color.green, transform.position, true);
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (_rewindTimeComponent.isRewinding) //do not bother with collisions when we we are rewinding
            return;


        switch (collision.gameObject.tag)
        {
            case "CyanSaw":
                if (!IsCyan)
                {
                    TryToKillPlayer();
                }
                break;
            case "GreenSaw":
                if (IsCyan)
                {
                    TryToKillPlayer();
                }
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

                    string secondpart = collision.gameObject.tag.Substring(8);
                    CollectedPowerUp(secondpart);

                }
                break;
        }
    }

    bool masktrick = false;




    private void CollectedPowerUp(string secondpart)
    {
        var powerupHandle = Activator.CreateInstance(null, secondpart + "PowerUp"); //(by convention we name the class as the powerup tag +"PowerUp")
        var powerup = (PowerUp)powerupHandle.Unwrap();

        PowerUpTypes poweruptype = (PowerUpTypes)Enum.Parse(typeof(PowerUpTypes), secondpart);
        if (_collectedPowerUps.ContainsKey(poweruptype))// Enum.Parse(PowerUpTypes, secondpart))//  powerup)) //we already have this powerup
            return;
        _collectedPowerUps.Add(poweruptype, powerup);

        GameObject uiImage = GameObject.Find("UIcanvas").transform.Find(secondpart + "Image").gameObject; //(by convention we name the image as the powerup tag +"Image")
        uiImage.SetActive(true);
        uiImage.GetComponent<AudioSource>().Play();



        if (powerup.isActivatedImmediately)
            powerup.Activate();

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
    public DoubleJumpPowerUp()
    {
        mytype = PowerUpTypes.DoubleJump;
    }
    public override void Activate()
    {
        PlayerController.Instance._jumpFromGround = true;
        base.Activate();
    }

}
public class GhostPowerUp : PowerUp
{
    public GhostPowerUp()
    {
        mytype = PowerUpTypes.Ghost;
        isActivatedImmediately = true;
    }

    public override void Activate()
    {
        PlayerController.Instance._spriteRenderer.color = new Color(255, 255, 255, 0);

        base.Activate();
    }
    public override void Deactivate()
    {
        //PlayerController.Instance._trailRenderer.startColor = PlayerController.Instance._trailRenderer.endColor =
        PlayerController.Instance._spriteRenderer.color
        = PlayerController.Instance.IsCyan ? Color.cyan : Color.green;

        base.Deactivate();
    }
}
public class RewindTimePowerUp : PowerUp
{
    public RewindTimePowerUp()
    {
        mytype = PowerUpTypes.RewindTime;
    }

    public override void Activate()
    {
        PlayerController.Instance._rewindTimeComponent.StartRewind();
        base.Activate();
    }
}
public enum PowerUpTypes { DoubleJump, Ghost, RewindTime };

public class PowerUp
{
    //public string UIimagename;
    public PowerUpTypes mytype; //public string tag;
    public bool isActivatedImmediately;

    //    public delegate void ActivationMethod();
    //  public ActivationMethod MyActivationMethod;


    public virtual void Activate()
    {
        if (!isActivatedImmediately)
            Remove();
    }
    public virtual void Deactivate()
    {
        if (isActivatedImmediately)
            Remove();
    }




    private void Remove()
    {

        PlayerController.Instance._collectedPowerUps.Remove(mytype);//  .Remove(tag);
        GameObject uiImage = GameObject.Find("UIcanvas").transform.Find(mytype.ToString() + "Image").gameObject; //(by convention we name the image as the powerup tag +"Image")
        uiImage.SetActive(false);

        // public override void HideUiIcon()
        //  {
        /*
        private void activateDoubleJumpPowerUp()
        {

         //   hasDoubleJump = false;
            var powerUpImage = GameObject.Find("UIcanvas").transform.Find("DoubleJumpImage").gameObject;
            powerUpImage.SetActive(false);



        }



        private void RewindTime()
        {
            _collectedPowerUps.Remove("RewindTime");//    hasRewindTime = false;
            var rewindTimeImage = GameObject.Find("UIcanvas").transform.Find("RewindTimeImage").gameObject;
            rewindTimeImage.SetActive(false);

        }

        private void GhostNoMore()
        {
            _collectedPowerUps.Remove("Ghost");//   hasGhost = false;
            var ghostImage = GameObject.Find("UIcanvas").transform.Find("GhostImage").gameObject;
            ghostImage.SetActive(false);

        }*/
        // }


    }
}