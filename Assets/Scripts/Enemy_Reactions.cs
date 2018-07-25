using UnityEngine;

public class Enemy_Reactions : MonoBehaviour
{
    public Transform Player;
    public float range;
    //public float MaxDistToChange;
    //public float MinDistToChange;
    public Color colorBlue;
    public Color colorGreen;
    public bool IamBlue = false;
    public bool ChangeColor = true;
    SpriteRenderer e_SpriteRenderer;




    void Start()
    {
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

        if (Player.transform.position.x>=gameObject.transform.position.x - range)//>= distance <= MaxDistToChange && distance >= MinDistToChange)
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
            e_SpriteRenderer.color = colorBlue;
            gameObject.tag = "BlueKiller";
        }
        else
        {
            e_SpriteRenderer.color = colorGreen;
            gameObject.tag = "GreenKiller";
        }
    }



}