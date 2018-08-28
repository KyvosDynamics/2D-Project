using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private const float _xOffsetFromPlayer = 6.5f;
    private Transform _playerTransform;



    private void Start()
    {
        _playerTransform = GameObject.Find("Runner").transform;
    }


    void LateUpdate()
    {
        Vector3 newCameraPosition = transform.position;
        newCameraPosition.x = _playerTransform.position.x + _xOffsetFromPlayer;
        transform.position = newCameraPosition;
    }


}