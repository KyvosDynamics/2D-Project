using UnityEngine;

public class Enemy_Reactions : MonoBehaviour
{
    public float range;
    //public Color colorBlue = Color.blue;
    //public Color colorGreen=
    public bool IamBlue = false;
    public bool ChangeColor = true;
    SpriteRenderer e_SpriteRenderer;
    private Transform Player;



    void Start()
    {
        Player = GameObject.Find("Player").transform;

        e_SpriteRenderer = GetComponent<SpriteRenderer>();

        //set color at the start
        SetColor();
    }




    // Update is called once per frame
    void Update()
    {
        if (!ChangeColor)
            return; //(do not calculate distance for no reason)


        //checks the distance between this and the player
        // var distance = ;

        if (Player.position.x >= gameObject.transform.position.x - range)//>= distance <= MaxDistToChange && distance >= MinDistToChange)
        {
            ChangeColor = false; //do not change again
            IamBlue = !IamBlue;
            SetColor();
        }
    }

    private void SetColor()
    {
        if (IamBlue)
        {
            e_SpriteRenderer.color = Color.blue;// colorBlue;
            gameObject.tag = "BlueKiller";
        }
        else
        {
            e_SpriteRenderer.color = Color.green;// colorGreen;
            gameObject.tag = "GreenKiller";
        }
    }



}