using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static float XOffsetFromPlayer =6;
    private Transform _playerTransform;

    private void Start()
    {
        _playerTransform = GameObject.Find("Player").transform;
    }
    

    void LateUpdate()
    {
        Vector3 newCameraPosition = transform.position;
        newCameraPosition.x = _playerTransform.position.x + XOffsetFromPlayer;
        transform.position = newCameraPosition;
    }


}