using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SillyButtonScript : MonoBehaviour {

    public GameObject spinningImage;
    public int Index = 0;


    bool spinImage = false;

    public void PlayGame()
    {

        switch(Index)
        {
            case 0:
                RoomGenerator.StaticThemeToUse = Theme.Fire;
                RoomGenerator.StaticProcedural = false;
                break;
            case 1:
                RoomGenerator.StaticThemeToUse = Theme.Ice;
                RoomGenerator.StaticProcedural = false;
                break;
            case 2:
                RoomGenerator.StaticThemeToUse = Theme.Fire;
                RoomGenerator.StaticProcedural = true;
                break;
            case 3:
                RoomGenerator.StaticThemeToUse = Theme.Ice;
                RoomGenerator.StaticProcedural = true;
                break;
        }


        spinImage = true;


    }

    int rotation = 360;



    void FixedUpdate()
    {
        if (spinImage == false)
            return;


        rotation -= 10;
        spinningImage.transform.rotation = Quaternion.Euler(Vector3.forward * rotation);



        if (rotation == 0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);


    }




}
