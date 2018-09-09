using UnityEngine;

public class PlayerCenterController : MonoBehaviour
{
    private PlayerController _playerController = null;


    private void Start()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "SpriteMask")
        {
            Debug.Log("player center collided with sprite mask");
            //_playerController.StartBackgroundTronTrail();

            PlayerController.Instance.CurrentState.IsTrailInForeground = false; //because we don't want the trail to pass on top of the portal effect!
            PlayerController.Instance.StartNewTrail();

        }
    }

}