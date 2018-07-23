using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform Player;
    public Vector3 Offset;


    // Update is called once per frame
    void LateUpdate()
    {



        transform.position = Player.position + Offset;//new Vector3(Player.position.x + Offset.x, Player.position.y + Offset.y, Player.position.z + Offset.z);
    }




}
