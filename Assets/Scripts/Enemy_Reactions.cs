using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Reactions : MonoBehaviour {

    public Transform _Player;
    public float MaxDistToChange;
    public float MinDistToChange;
    SpriteRenderer e_SpriteRenderer;
    public Color colorBlue;
    public Color colorGreen;
    public bool IamBlue ;
    public bool ChangerColor;
    
    // Use this for initialization
    void Start ()
    {
        e_SpriteRenderer = GetComponent<SpriteRenderer>();
        //set color at the start
        if (IamBlue)
        {
            e_SpriteRenderer.color = colorBlue;
            gameObject.tag = "BlueKiller";
        }
        else
        {
            e_SpriteRenderer.color = colorGreen;
            gameObject.tag = "GreenKiller";
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        //checks the distance between this and the player
        var distance = gameObject.transform.position.x - _Player.transform.position.x;

        if (distance<= MaxDistToChange && distance>= MinDistToChange&&ChangerColor)
        {
            ChangeColor();
           
        }
        
        //change color acordingly
        //if (IamBlue)
        //{
        //    e_SpriteRenderer.color = colorBlue;
        //}
        //else
        //{
        //    e_SpriteRenderer.color = colorGreen;
        //}
    }
    void ChangeColor()
    {
        if (IamBlue)
        {
            e_SpriteRenderer.color = colorGreen;
            gameObject.tag = "GreenKiller";
        }
        else
        {
            e_SpriteRenderer.color = colorBlue;
            gameObject.tag = "BlueKiller";
        }
    }
}
