using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public bool IsBlue = true;
    public float Speed;
    public float JumpSpeed;
    public LayerMask GroundLayer;
    //by convention private fields start with the underscore '_' character followed by a lower-case letter
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    private bool _isTouchingGround;
    //  private bool _isTouchingCeiling = false;
    bool _isTouchingWall;
    //  bool _isTouchingWall;
    bool goUp = false;
    private float _halfHeight;
    private float _halfWidth;



    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        Collider2D _collider = GetComponent<Collider2D>();
        //_halfHeight = / 2;
        _halfWidth = _collider.bounds.size.x / 2;

        //set color at the start
        if (IsBlue)
            _spriteRenderer.color = Color.blue;// colorBlue;        
        else
            _spriteRenderer.color = Color.green;// colorGreen;      



        //to see if the player has hit a wall let's create 4 horizontal raycasts (if we change either the player's or the platform's size 4 may be too few or too many)
         numOfHorRaycasts = 4;
        float fullHeight = _collider.bounds.size.y;// 2 * _halfHeight;

         yIncr = fullHeight / (numOfHorRaycasts - 1);//minus one because for example 4 points form 3 intervals
                                                     //lets do them from the bottom to the top

        _halfHeight = fullHeight / 2;
    }


    int numOfHorRaycasts;
    float yIncr;



    void FixedUpdate()
    {
        Vector3 bottommostPoint = transform.position + Vector3.down * (_halfHeight +0.01f); //don't mind the 0.01f
        Vector3 bottommostRightmostPoint = bottommostPoint + Vector3.right * _halfWidth;


        float raycastingDistance = 0.1f;

        _isTouchingGround = Physics2D.Raycast(bottommostPoint, Vector2.down, raycastingDistance, GroundLayer);
        Debug.DrawLine(bottommostPoint, bottommostPoint + Vector3.down * raycastingDistance, _isTouchingGround ? Color.red : Color.green);





       

        _isTouchingWall = false;
        for (int raycastIndex = 0; raycastIndex < numOfHorRaycasts && _isTouchingWall == false; raycastIndex++)
        {
            Vector3 point = bottommostRightmostPoint + Vector3.up * raycastIndex * yIncr;
            _isTouchingWall = Physics2D.Raycast(point, Vector2.right, raycastingDistance, GroundLayer);
            Debug.DrawLine(point, point + Vector3.right * raycastingDistance, _isTouchingWall ? Color.red : Color.green);// Color.white);
        }






        //   bool 
        //  _isTouchingWall =



        //        if (_isTouchingWallMiddle == false)
        //      {
        //        _isTouchingWallLow = Physics2D.Raycast(bottommostRightmostPoint, Vector2.right, raycastingDistance, GroundLayer);
        //      Debug.DrawLine(bottommostRightmostPoint, bottommostRightmostPoint + Vector3.right * raycastingDistance, _isTouchingWallLow ? Color.red : Color.green);
        //}







        float yVel = _rigidbody.velocity.y;
        if (jumpFromGround)
        {
            jumpFromGround = false;
            yVel = JumpSpeed;
        }
        else if(jumpFromWall)
        {
            jumpFromWall = false;
            yVel = JumpSpeed - AutoClimbSpeed;

        }
        else if(_isTouchingWall)
        {//auto climb

            yVel = AutoClimbSpeed;

        }



        _rigidbody.velocity = new Vector2(Speed, yVel);





        //   if (_isTouchingWallLow || goUp)
        // {
        //   goUp = false;
        // yVel = JumpForce / 1.5f;
        // }






    }
    public float AutoClimbSpeed = 2f;





    bool switchcolor = false;



    void Update()
    {





        //check for key to change color
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (IsBlue == true)
            {
                IsBlue = false;

            }
            else
            {
                IsBlue = true;

            }
        }
        //change color and tag
        if (IsBlue)
        {
            _spriteRenderer.color = Color.cyan;// colorBlue;
            //gameObject.tag = "BluePlayer";
        }
        else
        {
            _spriteRenderer.color = Color.green;// colorGreen;
            //gameObject.tag = "GreenPlayer";
        }




        //jump check
        if (Input.GetKey(KeyCode.Space))// && _isTouchingWall==false)// && !_isTouchingCeiling)
        {
            if (_isTouchingGround)
                jumpFromGround = true;
            else if (_isTouchingWall)
                jumpFromWall = true;
            //_isTouchingGround = false;
        }
    }

    bool jumpFromGround = false;
    bool jumpFromWall = false;



    void OnTriggerEnter2D(Collider2D collision)
    {



        switch (collision.gameObject.tag)
        {






            case "BlueSaw":
                if (!IsBlue)
                    Time.timeScale = 0f; //game over
                break;
            //
            case "GreenSaw":
                if (IsBlue)
                    Time.timeScale = 0f; //game over    
                break;

            case "Ponger":
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, JumpSpeed * 1.5f);
                break;

            case "Killer":
                Time.timeScale = 0f; //game over
                break;
        }
    }




}