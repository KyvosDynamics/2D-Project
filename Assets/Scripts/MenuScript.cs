using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{



    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }




}