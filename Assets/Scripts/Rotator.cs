using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float RotSpeed;


    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, RotSpeed);
    }

}