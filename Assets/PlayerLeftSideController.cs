using UnityEngine;

public class PlayerLeftSideController : MonoBehaviour
{

    PlayerController pc;

    private void Start()
    {
        pc = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag ==
           "SpriteMask")
        {
            Debug.Log("player left side exited sprite mask");
            pc.StartForegroundSnake();
        }
    }


}
