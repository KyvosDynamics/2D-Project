using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawController : MonoBehaviour {

    public float RotSpeed;
    public float range;
    private bool IamBlue = false;
    private bool ChangeColor = true;
    SpriteRenderer _spriteRenderer;
    private Transform Player;


    void Start()
    {
        Player = GameObject.Find("Player").transform;

        _spriteRenderer = GetComponent<SpriteRenderer>();

    }




    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, RotSpeed);


        if (!ChangeColor)
            return; //(do not calculate distance for no reason)


        //checks the distance between this and the player
        // var distance = ;

        if (Player.position.x >= gameObject.transform.position.x - range)//>= distance <= MaxDistToChange && distance >= MinDistToChange)
        {
            ChangeColor = false; //do not change again
            IamBlue = !IamBlue;

            Movement m = Player.GetComponent<Movement>();
            if (m.IAmBlue)
            {
                _spriteRenderer.color = Color.green;// colorGreen;
                gameObject.tag = "GreenSaw";
            }
            else
            {
                _spriteRenderer.color = Color.cyan;// colorBlue;
                gameObject.tag = "BlueSaw";

            }
        }
    }



 
	
}
