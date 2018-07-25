using UnityEngine;

public class Movement : MonoBehaviour
{
    public bool ShowBoxBounds = false; //this is for debug purposes only, should be false when publishing
    public Rigidbody2D Player;
    public float Speed;
    public float JumpForce;
    public Color colorBlue;
    public Color colorGreen;
    public bool IamBlue = true;
    public Transform CeilingCheck;
    public LayerMask GroundLayer;

    private SpriteRenderer m_SpriteRenderer;
    private bool IsTouchingGround;
    private bool IsTouchingCeiling;
    private Collider2D col;
    private Vector2 size;



    void Start()
    {
        Player = GetComponent<Rigidbody2D>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();

        col = GetComponent<Collider2D>();
        size = new Vector2(col.bounds.size.x, col.bounds.size.y);


        //set color at the start
        if (IamBlue)
        {
            m_SpriteRenderer.color = colorBlue;
            gameObject.tag = "BluePlayer";
        }
        else
        {
            m_SpriteRenderer.color = colorGreen;
            gameObject.tag = "GreenPlayer";
        }
    }



    void FixedUpdate()
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        if (ShowBoxBounds)
            DrawBoxForDebugPurposes(pos, size, transform.eulerAngles.z); //this draws a red rectangle to show the player's bounds

        Collider2D[] colliders = Physics2D.OverlapBoxAll(pos, size, transform.eulerAngles.z, GroundLayer); //this checks for any colliders within the boxy-area
        IsTouchingGround = colliders.Length != 0; //at least one groundlayer collider present so we are in contact





        //check groundcheck
        //IsTouchingGround = Physics2D.OverlapCircle(GroundCheck.position, OverlapRadius, GroundLayer);

        IsTouchingCeiling = Physics2D.Raycast(CeilingCheck.position, Vector2.up, 0.9f, GroundLayer);

        // IsTouchingGround = Physics2D.OverlapArea(new Vector2(transform.position.x-0.5f,transform.position.y-0.5f),new Vector2 (transform.position.x+0.5f,transform.position.y-0.51f),GroundLayer);
        //print(IsTouchingCeiling);
        // player moving by input
        // Player.velocity = new Vector2(Input.GetAxis("Horizontal") * Speed, Player.velocity.y);


        //player moving alone
        Player.velocity = new Vector2(Speed, Player.velocity.y);

    }


    void Update()
    {
        Player.velocity = new Vector2(Speed, Player.velocity.y);

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
            m_SpriteRenderer.color = colorBlue;
            gameObject.tag = "BluePlayer";
        }
        else
        {
            m_SpriteRenderer.color = colorGreen;
            gameObject.tag = "GreenPlayer";
        }
        //jump check
        if (Input.GetKey(KeyCode.Space) && IsTouchingGround && !IsTouchingCeiling)
        {
            Player.velocity = new Vector2(Player.velocity.x, JumpForce);
            IsTouchingGround = false;
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

            Player.velocity = new Vector2(Player.velocity.x, JumpForce * 2);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //death check
        if (collision.gameObject.tag == "Killer")
        {
            print("Game Over");
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