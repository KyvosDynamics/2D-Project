using UnityEngine;

public class PlayerLeftSideController : MonoBehaviour
{
    private PlayerController _playerController = null;


    private void Start()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "SpriteMask")
        {
            Debug.Log("player left side exited sprite mask");
            _playerController.StartForegroundTronTrail();
        }
    }

}