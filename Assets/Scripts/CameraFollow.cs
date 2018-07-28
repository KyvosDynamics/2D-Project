using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Player;
    private float distanceToTarget;


    private void Start()
    {
        distanceToTarget = transform.position.x - Player.position.x;
    }

    void LateUpdate()
    {
        float targetObjectX = Player.position.x;
        Vector3 newCameraPosition = transform.position;
        newCameraPosition.x = targetObjectX + distanceToTarget;
        transform.position = newCameraPosition;
    }


}