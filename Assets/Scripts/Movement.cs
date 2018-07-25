using UnityEngine;

public class Movement : MonoBehaviour
{
    public bool ShowBoxBounds = false; //this is for debug purposes only, should be false when publishing
    public float Speed;
    public float JumpForce;
    public Color colorBlue;
    public Color colorGreen;
    public bool IamBlue = true;
    public LayerMask GroundLayer;

    //by convention private fields start with the underscore '_' character followed by a lower-case letter
    private Rigidbody2D _player;
    private SpriteRenderer _spriteRenderer;
    private bool _isTouchingGround;
    private Collider2D _collider;
    private Vector2 _size;



    void Start()
    {
        _player = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _collider = GetComponent<Collider2D>();
        _size = new Vector2(_collider.bounds.size.x, _collider.bounds.size.y);


        //set color at the start
        if (IamBlue)
        {
            _spriteRenderer.color = colorBlue;
            gameObject.tag = "BluePlayer";
        }
        else
        {
            _spriteRenderer.color = colorGreen;
            gameObject.tag = "GreenPlayer";
        }
    }



    void FixedUpdate()
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        if (ShowBoxBounds)
            DrawBoxForDebugPurposes(pos, _size, transform.eulerAngles.z); //this draws a red rectangle to show the player's bounds

        Collider2D[] colliders = Physics2D.OverlapBoxAll(pos, _size, transform.eulerAngles.z, GroundLayer); //this checks for any colliders within the boxy-area
        _isTouchingGround = colliders.Length != 0; //at least one groundlayer collider present so we are in contact




        //_isTouchingCeiling = Physics2D.Raycast(CeilingCheck.position, Vector2.up, 0.9f, GroundLayer);

        //print(IsTouchingCeiling);
        // player moving by input
        // Player.velocity = new Vector2(Input.GetAxis("Horizontal") * Speed, Player.velocity.y);


        //player moving alone
        _player.velocity = new Vector2(Speed, _player.velocity.y);

    }


    void Update()
    {
        _player.velocity = new Vector2(Speed, _player.velocity.y);

        //check for key to change color
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (IamBlue == true)
            {
                IamBlue = false;

            }
            else
            {
                IamBlue = true;

            }
        }
        //change color and tag
        if (IamBlue)
        {
            _spriteRenderer.color = colorBlue;
            gameObject.tag = "BluePlayer";
        }
        else
        {
            _spriteRenderer.color = colorGreen;
            gameObject.tag = "GreenPlayer";
        }

        //jump check
        if (Input.GetKey(KeyCode.Space) && _isTouchingGround)// && !_isTouchingCeiling)
        {
            _player.velocity = new Vector2(_player.velocity.x, JumpForce);
            //_isTouchingGround = false;
        }
    }



    void OnTriggerEnter2D(Collider2D collision)
    {
        //death check
        if (collision.gameObject.tag == "BlueKiller" && gameObject.tag != "BluePlayer")
        {
            print("Game Over");
        }
        if (collision.gameObject.tag == "GreenKiller" && gameObject.tag != "GreenPlayer")
        {
            print("Game Over");
        }

        //check ponger
        if (collision.gameObject.name == "Ponger")
        {

            _player.velocity = new Vector2(_player.velocity.x, JumpForce * 2);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //death check
        if (collision.gameObject.tag == "Killer")
        {
            print("Game Over");
            Time.timeScale = 0f;
        }
    }




    void DrawBoxForDebugPurposes(Vector2 point, Vector2 size, float angle)
    {//this draws a red rectangle to show the player's bounds
        var orientation = Quaternion.Euler(0, 0, angle);
        Vector2 right = orientation * Vector2.right * size.x / 2f;
        Vector2 up = orientation * Vector2.up * size.y / 2f;
        var topLeft = point + up - right;
        var topRight = point + up + right;
        var bottomRight = point - up + right;
        var bottomLeft = point - up - right;
        Color color = Color.red;
        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }



}