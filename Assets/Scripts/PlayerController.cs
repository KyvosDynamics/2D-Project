using System;
using System.Collections.Generic;
using UnityEngine;

public class TronTrail
{
    public int HeadIndex;
    public int TailIndex;
    public Color MyColor;
    private Vector3[] points;
    private LineRenderer linerenderer;
    public static int sortingLayerID;

    public TronTrail(Color color, Vector3 startPosition, bool inForeground)
    {
        MyColor = color;
        const int MAXPOINTS = 348394;
        points = new Vector3[MAXPOINTS];

        points[0] = startPosition;// transform.position;
        //   times[0] = Time.time;
        // hasChanged = true;
        //started = false;
        //}
        //        Vector3[] currentPoints = new Vector3[head - tail + 1];
        //      System.Array.Copy(points, tail, currentPoints, 0, head - tail + 1);

        GameObject gObject = new GameObject("MyGameObject");
        linerenderer = gObject.AddComponent<LineRenderer>();

        if (inForeground)
        {
            linerenderer.sortingLayerName = "Player";// .sortingLayerID = sortingLayerID;


        }
        else
        {
            linerenderer.sortingLayerName = "RoomObjectsBehindPlayer";// .sortingLayerID = sortingLayerID;

        }
        linerenderer.sortingOrder = -10;


        linerenderer.SetColors(color, color);// Color.red, Color.blue);
                                             //  linerenderer.material = new Material(Shader.Find("Particles/Additive")); //or ?    
        linerenderer.material = new Material(Shader.Find("Sprites/Default"));
        linerenderer.SetWidth(1, 1);

        //     linerenderer.SetPosition(0, Vector3.zero);
        //   linerenderer.SetPosition(1, Vector3.one);

        linerenderer.positionCount = 1;// head - tail + 1;
        linerenderer.SetPositions(new Vector3[] { points[0] });
    }
    [SerializeField] float minSampleDistance = 0.1f;


    public void AddFinalPoint(Vector3 newPosition)
    {
        //traillength += sq;
        HeadIndex++;
        points[HeadIndex] = newPosition;// transform.position;
                                        //     times[head] = Time.time;
                                        //   hasChanged = true;
        Vector3[] currentPoints = new Vector3[HeadIndex - TailIndex + 1];
        System.Array.Copy(points, TailIndex, currentPoints, 0, HeadIndex - TailIndex + 1);
        linerenderer.positionCount = HeadIndex - TailIndex + 1;
        linerenderer.SetPositions(currentPoints);

    }


    public void CheckIfShouldAddPointAndIfYesAddIt(Vector3 newPosition)
    {


        float sq = (newPosition - points[HeadIndex]).sqrMagnitude;

        //add point if head far enough
        if (//head < MAXPOINTS - 1 &&
            sq > minSampleDistance * minSampleDistance)
        {
            AddFinalPoint(newPosition);
        }

        //     // remove old tail
        //   if (Time.time - times[tail] > lifetime)
        // {
        //    tail++;
        //   hasChanged = true;
        //}

        //   if (hasChanged)
        // {
        //  hasChanged = false;
        //
        //}


        // setgradientcolor(Color.blue);

    }
}

public class PlayerController : MonoBehaviour
{
    //LineRenderer linetouse = null;
    //
    //  [SerializeField] LineRenderer line;
    //
    //[SerializeField] float lifetime = 10003f;

    //  const int MAXPOINTS = 348394;
    //   Vector3[] points = new Vector3[MAXPOINTS];
    //float[] times = new float[MAXPOINTS];
    //int headIndex = 0;
    //int tailIndex = 0;

    /*  bool started;
      void OnEnable()
      {
          started = true;
      }

      void OnDisable()
      {
          line.positionCount = 0;
      }
      */



    bool hasChanged = false;
    //Vector3[] debug = new Vector3[MAXPOINTS];
    //  void FixedUpdate()
    //  {
    // }



    //  void Update()
    //{
    //  line.GetPositions(debug);
    //}


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

    // // [HideInInspector]
    // public TrailRenderer _trailRenderer;

    public static PlayerController Instance = null;
    float traillength = 0;


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
        TronTrail.sortingLayerID = _spriteRenderer.sortingLayerID;


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




        if (masktrick && masktrickfloat <= 1.0f)
        {
            //Debug.Log("" + _trailRenderer.time);
            //_trailRenderer.transform.position = this.transform.position - new Vector3(2, 0, 0);
            //Gradient gradient = new Gradient();
            //   _trailRendererGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.yellow, 0.0f),
            //     new GradientColorKey(Color.cyan, 4.9f),  new GradientColorKey(Color.cyan, 5.0f) },
            //               new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 4.9f), new GradientAlphaKey(1.0f, 5) });
            //   _trailRenderer.colorGradient = _trailRendererGradient;

            //       masktrickfloat +=  0.01f;
        }






        currentSnake.CheckIfShouldAddPointAndIfYesAddIt(transform.position);
    }




    float masktrickfloat = 0.0f;


    public void SetColor(bool iscyan)
    {            // _trailRenderer.startColor = _trailRenderer.endColor = 
                 //        _spriteRenderer.color = IsCyan ? Color.cyan : Color.green;


        IsCyan = iscyan;

        Color color = IsCyan ? Color.cyan : Color.green;

        // setgradientcolor(color);
        _spriteRenderer.color = color;

        StartForegroundSnake();
    }




    //float firstime = -1.0f;
    //float masktime = -1.0f;
    //   int maskpoint;

    /*
private void setgradientcolor(Color color)
{
    //length 0 0
    //traillength 1
    //masktrickfloat gradient?
    //gradient= masktrickfloat/traillength




    //0 headindex 0
    //1 headindex head       
    //gradient maskpoint;// masktime
    //currenthead 1
    //maskpoint ?
    //?=maskpoint/currenthead
    float gradient = masktrickfloat / traillength;
    //bla=(time.time-firstime)/(masktime-firstime)= (1-0)/(gradientime-0)
    //gradientime=1/bla
    //float bla = (Time.time - firstime) / (masktime - firstime);
    //float gradienttime = 1 / bla;

    if (masktrick == false)
        gradient = 1.0f;

    //private
    Gradient _trailRendererGradient = null;
    _trailRendererGradient = new Gradient();
    _trailRendererGradient.mode = GradientMode.Fixed;
    _trailRendererGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, gradient)
        ,new GradientColorKey(Color.red,1.0f)
    },
                   new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, gradient), new GradientAlphaKey(1.0f, 1.0f) });
    line.colorGradient = _trailRendererGradient;  //_trailRenderer.colorGradient = _trailRendererGradient;

    //  line.startColor = line.endColor = color;

    //   line.startColor=line.endColor=   // _trailRenderer.startColor = _trailRenderer.endColor = 
}

*/
    [HideInInspector]
    public RewindTimeComponent _rewindTimeComponent;


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

    public void StartBackgroundSnake()
    {
        //       case "SpriteMask":


        currentSnake.AddFinalPoint(transform.position);

        currentSnake = new TronTrail(IsCyan ? Color.cyan : Color.green, transform.position, false);// currentSnake = new TronTrail(Color.red, transform.position, false);

        //   masktrick = true;
        //  masktrickfloat = traillength;//                maskpoint = head;// masktime = Time.time;
        //  addlinepoint();

        //                GameObject gObject = new GameObject("MyGameObject");
        //              LineRenderer lRend = gObject.AddComponent<LineRenderer>();
        //
        //            lRend.SetColors(Color.red, Color.blue);
        //          lRend.material = new Material(Shader.Find("Particles/Additive"));
        //        lRend.SetWidth(1, 1);
        //      lRend.SetPosition(0, Vector3.zero);
        //    lRend.SetPosition(1, Vector3.one);

        // break;

    }
    public void StartForegroundSnake()
    {
        //       case "SpriteMask":
        //Debug.Log("sprite mask");


        if (currentSnake != null)//it can be null the first time we call this method
            currentSnake.AddFinalPoint(transform.position);

        currentSnake = new TronTrail(IsCyan ? Color.cyan : Color.green, transform.position, true); //currentSnake = new TronTrail(Color.white, transform.position, true);




        //   masktrick = true;
        //  masktrickfloat = traillength;//                maskpoint = head;// masktime = Time.time;
        //  addlinepoint();

        //                GameObject gObject = new GameObject("MyGameObject");
        //              LineRenderer lRend = gObject.AddComponent<LineRenderer>();
        //
        //            lRend.SetColors(Color.red, Color.blue);
        //          lRend.material = new Material(Shader.Find("Particles/Additive"));
        //        lRend.SetWidth(1, 1);
        //      lRend.SetPosition(0, Vector3.zero);
        //    lRend.SetPosition(1, Vector3.one);

        // break;

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

    [HideInInspector]
    public Dictionary<PowerUpTypes, PowerUp> _collectedPowerUps = new Dictionary<PowerUpTypes, PowerUp>();


    //    bool hasDoubleJump = false;
    //  bool hasGhost = false;
    //bool hasRewindTime = false;


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