using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool VisualizeRaycasting = true; //this is for debug purposes only, should be false when releasing
    public float Speed;
    public float JumpForce;
    public LayerMask GroundLayer;

    //by convention private fields start with the underscore '_' character followed by a lower-case letter
    [HideInInspector]
    public bool IsBlue = true;
    private Rigidbody2D _player;
    private SpriteRenderer _spriteRenderer;
    private bool _isTouchingGround;
    private bool _isTouchingCeiling = false;
    private Collider2D _collider;
    private Vector2 _size;



    void Start()
    {
        _player = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _collider = GetComponent<Collider2D>();
        _size = new Vector2(_collider.bounds.size.x, _collider.bounds.size.y);


        //set color at the start
        if (IsBlue)
            _spriteRenderer.color = Color.blue;// colorBlue;        
        else
            _spriteRenderer.color = Color.green;// colorGreen;        
    }



 

    void FixedUpdate()
    {


        /*
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);

        //let's find the four vertices
        var orientation = Quaternion.Euler(0, 0, transform.eulerAngles.z);
        Vector2 right = orientation * Vector2.right * _size.x / 2f; //the right vector is the result of rotating half the square's horizontal axis by the square's z angle
        Vector2 up = orientation * Vector2.up * _size.y / 2f; //the up vector is the result of rotating half the square's vertical axis by the square's z angle


        var topLeft = pos + up - right;
        var topRight = pos + up + right;
        var bottomRight = pos - up + right;
        var bottomLeft = pos - up - right;
        //the above are ofcourse NOT absolute positions. They depend on the square's rotation (so for example if the player rotates 90 degrees clockwise the topleft vertex is actually top-right!


        List<Vector2> verticesToRaycast = new List<Vector2>();
        verticesToRaycast.Add(topLeft);
        verticesToRaycast.Add(topRight);
        verticesToRaycast.Add(bottomRight);
        verticesToRaycast.Add(bottomLeft);


        RemoveVertexWithSmallerY(verticesToRaycast);

        //we want when the z angle is a multiple of 90 to raycast the top two vertices
        //otherwise to raycast the top three vertices
        float zAngle = transform.eulerAngles.z;
        if (Mathf.Round(zAngle) % 90 == 0)
        {//it is a multiple of 90, remove one more vertex

            RemoveVertexWithSmallerY(verticesToRaycast);
        }
        */
        //print(string.Format("Raycasting {0} vertices", verticesToRaycast.Count));




        float verticalRaycastingDistance = 0.1f + _size.y / 2;// 10;// _size.x / 10;
        float horizontalRaycastingDistance = 0.1f + _size.x / 2;// 10;// _size.x / 10;


        _isTouchingCeiling = Physics2D.Raycast(transform.position, Vector2.up, verticalRaycastingDistance, GroundLayer);
        Debug.DrawLine(transform.position, transform.position + Vector3.up * verticalRaycastingDistance, _isTouchingCeiling ? Color.red : Color.green);

        /*
        foreach (Vector2 v in verticesToRaycast)
        {
            bool vTouching = Physics2D.Raycast(v, Vector2.up, raycastingDistance, GroundLayer);

            if (VisualizeRaycasting)
                Debug.DrawLine(v, v + Vector2.up * raycastingDistance, vTouching ? Color.red : Color.green);

            _isTouchingCeiling = _isTouchingCeiling | vTouching;
        }
        */





        //   Collider2D[] colliders = Physics2D.OverlapBoxAll(pos, _size, transform.eulerAngles.z, GroundLayer); //this checks for any colliders within the boxy-area
        //  _isTouchingGround = colliders.Length != 0; //at least one groundlayer collider present so we are in contact




        _isTouchingGround = Physics2D.Raycast(transform.position, Vector2.down, verticalRaycastingDistance, GroundLayer);
        // if (VisualizeRaycasting)
        Debug.DrawLine(transform.position, transform.position + Vector3.down * verticalRaycastingDistance, _isTouchingGround ? Color.red : Color.green);

        //  _isTouchingGround = _isTouchingGround | vTouching;



        _isTouchingWall = Physics2D.Raycast(transform.position + Vector3.down * _size.y / 2, Vector2.right, horizontalRaycastingDistance, GroundLayer);
        Debug.DrawLine(transform.position + Vector3.down * _size.y / 2, transform.position + Vector3.down * _size.y / 2 + Vector3.right * horizontalRaycastingDistance, _isTouchingWall ? Color.red : Color.green);

        float yVel = _player.velocity.y;

        if (_isTouchingWall || goUp)
        {
            goUp = false;
            yVel = JumpForce/1.5f;
        }
        //else


        //player moving alone
        _player.velocity = new Vector2(Speed, yVel);

    }
    bool _isTouchingWall;




    void Update()
    {
        _player.velocity = new Vector2(Speed, _player.velocity.y);





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
        if (Input.GetKey(KeyCode.Space) && _isTouchingGround && !_isTouchingCeiling)
        {
            _player.velocity = new Vector2(_player.velocity.x, JumpForce);
            //_isTouchingGround = false;
        }
    }




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
                _player.velocity = new Vector2(_player.velocity.x, JumpForce * 1.5f);
                break;

            case "Killer":
                Time.timeScale = 0f; //game over
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.GetContact(0).otherCollider.transform.gameObject.name == "RightSideCollider")
        {
            goUp = true;// _player.velocity = new Vector2(Speed, _player.velocity.y + 0.1f);
            print("yeah");
        }

}
    bool goUp = false;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.GetContact(0).otherCollider.transform.gameObject.name == "RightSideCollider")
        {
            goUp = true;// _player.velocity = new Vector2(Speed, _player.velocity.y + 0.1f);
            print("yeah");
        }

    }

}