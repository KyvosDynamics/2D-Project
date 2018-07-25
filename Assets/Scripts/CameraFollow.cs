using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Player;
    public Vector3 Offset;


    void LateUpdate()
    {
        transform.position = Player.position + Offset;
    }


}