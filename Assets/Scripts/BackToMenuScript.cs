using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenuScript : MonoBehaviour
{

    public void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }

}