using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public GameObject spinningImage;



    bool spinImage = false;

    public void PlayGame()
    {
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