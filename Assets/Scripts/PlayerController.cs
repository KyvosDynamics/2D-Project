using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static bool IsActive = false;
    public bool IsCyan { get; private set; }

    public float Speed;
    public LayerMask GroundLayer;
    public GameObject PlayerWasKilledUI;
    public GameObject PlayerWonUI;

    //by convention private fields start with the underscore '_' character followed by a lower-case letter
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    private bool _isTouchingGround;
    private bool _isTouchingWall;
    private int _numOfHorRaycasts;
    private float _rayYIncr;
    private bool _switchcolor;
    private bool _jumpFromGround = false;
    private bool _jumpFromWall = false;
    private bool _jumpFromPonger = false;
    private const float _raycastingDistance = 0.1f;
    private const float _autoClimbSpeed = 2f;
    private const float _jumpSpeed = 8;
    private Vector3 _downVectorWithMagnitude;
    private Vector3 _rightVectorWithMagnitude;
    private TrailRenderer _trailRenderer;


    private void Awake()
    {
        IsActive = true;
    }

    void Start()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
        _trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        Gradient gradient = new Gradient();
        gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                         new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        _trailRenderer.colorGradient = gradient;


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
        _switchcolor = true;
        IsCyan = false;
    }




    void FixedUpdate()
    {
        if (!IsActive)
            return;

        if (transform.position.y < -5)
        {//player fell to the void
            PlayerWasKilled();
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
            //enable this for debugging: Debug.DrawLine(point, point + Vector3.right * _raycastingDistance, _isTouchingWall ? Color.red : Color.green);
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
            _trailRenderer.startColor = _trailRenderer.endColor = _spriteRenderer.color = IsCyan ? Color.cyan : Color.green;
        }


    }






    void Update()
    {//keyboard handling

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            Time.timeScale = 0f;


        if (Input.GetKeyDown(KeyCode.Q))
            _switchcolor = true;

        if (Input.GetKey(KeyCode.Space))
        {
            if (_isTouchingGround)
                _jumpFromGround = true;
            else if (_isTouchingWall)
                _jumpFromWall = true;
        }
    }




    void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "CyanSaw":
                if (!IsCyan)
                    PlayerWasKilled();
                break;
            case "GreenSaw":
                if (IsCyan)
                    PlayerWasKilled();
                break;

            case "Killer":
                PlayerWasKilled();
                break;

            case "Ponger":
                _jumpFromPonger = true;
                break;

            case "EndPoint":
                PlayerWon();
                break;
        }
    }



    private void PlayerWasKilled()
    {
        StopPlayer();
        PlayerWasKilledUI.SetActive(true);
    }
    private void PlayerWon()
    {
        StopPlayer();
        PlayerWonUI.SetActive(true);
    }

    private void StopPlayer()
    {
        IsActive = false;
        _rigidbody.velocity = new Vector2(0, 0);//necessary otherwise the player will continue to move while the playerKilledUI gradually appears
        GetComponent<Animator>().enabled = false;
    }




}