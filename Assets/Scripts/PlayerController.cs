﻿using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public bool IsCyan = true;
    public float Speed;
    public float JumpSpeed;
    public LayerMask GroundLayer;
    //by convention private fields start with the underscore '_' character followed by a lower-case letter
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    private bool _isTouchingGround;
    private bool _isTouchingWall;
    private float _halfHeight;
    private float _halfWidth;
    private int _numOfHorRaycasts;
    private float _yIncr;
    private bool _switchcolor = false;
    private bool _jumpFromGround = false;
    private bool _jumpFromWall = false;
    private bool _jumpFromPonger = false;
    private const float _raycastingDistance = 0.1f;
    private const float _autoClimbSpeed = 2f;



    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _spriteRenderer.color = IsCyan ? Color.cyan : Color.green;

        Collider2D _collider = GetComponent<Collider2D>();

        //to see if the player has hit a wall let's create 4 horizontal raycasts (if we change either the player's or the platform's size 4 may be too few or too many)
        _numOfHorRaycasts = 4;
        float fullHeight = _collider.bounds.size.y;

        _yIncr = fullHeight / (_numOfHorRaycasts - 1);//minus one because for example 4 points form 3 intervals
                                                    
        _halfHeight = fullHeight / 2;
        _halfWidth = _collider.bounds.size.x / 2;

    }





    void FixedUpdate()
    {
        Vector3 bottommostPoint = transform.position + Vector3.down * (_halfHeight + 0.01f); //don't mind the 0.01f
        Vector3 bottommostRightmostPoint = bottommostPoint + Vector3.right * _halfWidth;



        _isTouchingGround = Physics2D.Raycast(bottommostPoint, Vector2.down, _raycastingDistance, GroundLayer);
        Debug.DrawLine(bottommostPoint, bottommostPoint + Vector3.down * _raycastingDistance, _isTouchingGround ? Color.red : Color.green);



        _isTouchingWall = false;
        //lets do them from the bottom to the top
        for (int raycastIndex = 0; raycastIndex < _numOfHorRaycasts && _isTouchingWall == false; raycastIndex++)
        {
            Vector3 point = bottommostRightmostPoint + Vector3.up * raycastIndex * _yIncr;
            _isTouchingWall = Physics2D.Raycast(point, Vector2.right, _raycastingDistance, GroundLayer);
            Debug.DrawLine(point, point + Vector3.right * _raycastingDistance, _isTouchingWall ? Color.red : Color.green);
        }




        float yVel = _rigidbody.velocity.y;
        if (_jumpFromPonger)
        {
            _jumpFromPonger = false;
            yVel = JumpSpeed * 1.5f;
        }
        else if (_jumpFromGround)
        {
            _jumpFromGround = false;
            yVel = JumpSpeed;
        }
        else if (_jumpFromWall)
        {
            _jumpFromWall = false;
            yVel = JumpSpeed - _autoClimbSpeed;

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
            _spriteRenderer.color = IsCyan ? Color.cyan : Color.green;
        }


    }
   





    void Update()
    {//keyboard handling

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
            case "BlueSaw":
                if (!IsCyan)
                    Time.timeScale = 0f; //game over
                break;
            case "GreenSaw":
                if (IsCyan)
                    Time.timeScale = 0f; //game over    
                break;

            case "Ponger":
                _jumpFromPonger = true;
                break;

            case "Killer":
                Time.timeScale = 0f; //game over
                break;
        }
    }




}