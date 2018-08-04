using UnityEngine;

public class Enemy_Reactions : MonoBehaviour
{
    public float range;
    public bool IamBlue = false;
    public bool ChangeColor = true;
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