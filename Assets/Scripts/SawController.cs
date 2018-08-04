﻿using UnityEngine;

public class SawController : MonoBehaviour
{
    public float RotationSpeed;
    public float Range;
    private bool _changedColorOnce = false; //we want the Saw to only be able to change color once
    private SpriteRenderer _spriteRenderer;
    private Transform _playerTransform;



    void Start()
    {
        _playerTransform = GameObject.Find("Player").transform;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }



    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, RotationSpeed);



        if(_changedColorOnce==false && _playerTransform.position.x >= gameObject.transform.position.x - Range)
        {
            _changedColorOnce = true; //do not change again

            PlayerController playerController = _playerTransform.GetComponent<PlayerController>();
            if (playerController.IsBlue)
            {
                _spriteRenderer.color = Color.green;
                gameObject.tag = "GreenSaw";
            }
            else
            {
                _spriteRenderer.color = Color.cyan;
                gameObject.tag = "BlueSaw";

            }
        }
    }



}