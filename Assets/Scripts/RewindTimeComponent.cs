using System.Collections.Generic;
using UnityEngine;

public class RewindTimeComponent : MonoBehaviour
{


    // public class RewindTimeComponenft : MonoBehaviour
    // {
    public bool isRewinding = false;

    private float Seconds = 3f;

    List<PointInTime> pointsInTime;

    Rigidbody2D rb;
    GameObject mygameobject;

    // Use this for initialization
    void Start()
    {
        pointsInTime = new List<PointInTime>();
        rb = GetComponent<Rigidbody2D>();
        mygameobject = gameObject;
        _playerController = mygameobject.GetComponent<PlayerController>();
    }

    PlayerController _playerController;

    // Update is called once per frame
    //    void Update()
    //  {
    //    if (Input.GetKeyDown(KeyCode.Return))
    //      StartRewind();
    //        if (Input.GetKeyUp(KeyCode.Return))
    //          StopRewind();
    // }

    void FixedUpdate()
    {
        if (isRewinding)
            Rewind();
        else
            Record();
    }

    void Rewind()
    {
        if (pointsInTime.Count > 0)
        {
            PointInTime pointInTime = pointsInTime[0];
            transform.position = pointInTime.position;


            _playerController.IsCyan = pointInTime.iscyan;
            _playerController.ApplyColorAccordingToFlag();

            pointsInTime.RemoveAt(0);
        }
        else
        {
            StopRewind();
        }

    }

    void Record()
    {
        if (pointsInTime.Count > Mathf.Round(Seconds / Time.fixedDeltaTime))
        {
            pointsInTime.RemoveAt(pointsInTime.Count - 1);
        }

        pointsInTime.Insert(0, new PointInTime(transform.position, mygameobject.GetComponent<PlayerController>().IsCyan));// transform.rotation));
    }

    public void StartRewind()
    {
        isRewinding = true;
        //    rb.isKinematic = true;
    }

    public void StopRewind()
    {
        isRewinding = false;
        //   rb.isKinematic = false;
    }



}
