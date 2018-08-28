using UnityEngine;
using UnityEngine.SceneManagement;

public class SillyButtonScript : MonoBehaviour
{
    public Transform SpinningImage;
    public int ButtonIndex = 0;
    private bool _spinImage = false;
    private int _rotation = 360;


    public void PlayGame()
    {
        GetComponent<AudioSource>().Play();

        switch (ButtonIndex)
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

        _spinImage = true;
    }



    void FixedUpdate()
    {
        if (_spinImage == false)
            return;


        _rotation -= 10;
        SpinningImage.rotation = Quaternion.Euler(Vector3.forward * _rotation);


        if (_rotation == 0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }



}