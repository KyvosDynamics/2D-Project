using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float XOffsetFromPlayer = 0;
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