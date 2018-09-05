using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateRecorder
{
    public List<StateGroup> StateGroups = new List<StateGroup>();

    public void AddStateGroup(StateGroup stateGroup)
    {
        StateGroups.Add(stateGroup);
    }

}



public class State
{

    public Vector3 position;
    //public Quaternion rotation;
    public bool iscyan;

    public State(Vector3 _position, bool iscyan)// Quaternion _rotation)
    {
        position = _position;
        //rotation = _rotation;
        this.iscyan = iscyan;
    }

}

//public class TronTrail
//{ 
//}


public class StateGroup //: MonoBehaviour
{
    private List<State> _states = new List<State>();

    private LineRenderer _lineRenderer = null;





    public static int staticID = -1;
    public int myID;

    public bool isRewinding = false;

    private float Seconds = 3f;

    //  List<State> _states;

    // Rigidbody2D rb;

    public StateGroup(Transform transform, PlayerController playerController, Vector3 startPosition, bool isCyan, bool inForeground)
    {
        Debug.Log("inside stategroup constructor");
        staticID++;
        myID = staticID;

        GameObject TronTrailHolder = new GameObject("TronTrailHolder");
        _lineRenderer = TronTrailHolder.AddComponent<LineRenderer>();
        _lineRenderer.sortingLayerName = inForeground ? "Player" : "RoomObjectsBehindPlayer";
        _lineRenderer.sortingOrder = -10; //so that for example it is behind a portal effect

        Color color = isCyan ? Color.cyan : Color.green;
        _lineRenderer.startColor = _lineRenderer.endColor = color;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));//or Shader.Find("Particles/Additive")); ?
        _lineRenderer.startWidth = _lineRenderer.endWidth = 0.5f;

        AddState(new State(startPosition, isCyan));


        //}
        //public StateGroup()//    void Start()
        //{


        //   _states = new List<State>();
        // rb = GetComponent<Rigidbody2D>();
        //GameObject mygameobject; mygameobject = gameObject;
        this.transform = transform;// gameObject.transform;
        _playerController = playerController;// mygameobject.GetComponent<PlayerController>();
    }
    Transform transform;

    PlayerController _playerController;




    public void Rewind()
    {
        Debug.Log("inside rewind");
        if (_states.Count > 0)
        {
            State pointInTime = _states[_states.Count - 1];
            transform.position = pointInTime.position;


            _playerController.IsCyan = pointInTime.iscyan;
            _playerController.ApplyColorAccordingToFlag(false); //the false here is important, we don't want to initiate a new trail while rewinding

            _states.RemoveAt(_states.Count - 1);

            StatesChangedSoUpdateLineRenderer();
        }
        else
        {
            StopRewind();
        }

    }

    public void AddState(State state)// Vector3 position)
    {


        if (_states.Count > Mathf.Round(Seconds / Time.fixedDeltaTime))
        {
            _states.RemoveAt(0);// pointsInTime.Count - 1);
        }

        //        _states.Add();// transform.rotation));
        _states.Add(state);// new State(position, false));



        StatesChangedSoUpdateLineRenderer();
    }


    private void StatesChangedSoUpdateLineRenderer()
    {
        _lineRenderer.positionCount = _states.Count;
        _lineRenderer.SetPositions(_states.Select(p => p.position).ToArray());// .ToList().ToArray());
    }

    public void StartRewind()
    {
        Debug.Log("started rewinding");
        isRewinding = true;
        //    rb.isKinematic = true;
    }

    public void StopRewind()
    {
        Debug.Log("stopped rewinding");
        isRewinding = false;
        //   rb.isKinematic = false;
    }



}
