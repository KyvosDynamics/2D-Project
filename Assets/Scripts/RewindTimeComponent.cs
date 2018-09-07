using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateGroupManager
{
    public List<StateGroup> StateGroups = new List<StateGroup>();
    public bool isRewinding = false;

    PlayerController _playerController;

    public StateGroupManager(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void AddStateGroup(StateGroup stateGroup)
    {
        StateGroups.Add(stateGroup);
        CurrentStateGroup = stateGroup;
    }

    public void Rewind()
    {
        int statesremoved=CurrentStateGroup.Rewind();
        if(statesremoved!=-1)
        {//the current stategroup has finished rewinding
            //but has it finished rewinding because it reached the max allowed number of recorded states or because it has run out of states?
            if(statesremoved<StateGroup.MaxNumOfStates)
            {//it has run out of states
                //we want to remove this state group if we have others
                if (StateGroups.Count > 1)
                {

                    StateGroups.Remove(CurrentStateGroup);
                    CurrentStateGroup = StateGroups[StateGroups.Count - 1];
                }
                else
                {
                    isRewinding = false;
                }


            }
            else
            {
                isRewinding = false;
            }


        }

    }
    public void StartRewind()
    {
        isRewinding = true;
        CurrentStateGroup.StartRewind();
    }
    public StateGroup CurrentStateGroup;
}



public class State //TODO: register deltas instead of the actual values!
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



public class StateGroup
{
    private List<State> _states = new List<State>();

    private LineRenderer _lineRenderer = null;


    public static int staticID = -1;
    public int myID;

   // public bool isRewinding = false;

    private const float Seconds = 2f;
    Transform transform;

    PlayerController _playerController;



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
   



    public int Rewind()
    {
        Debug.Log("inside rewind");
        if (_states.Count > 0)
        {
            State latestState = _states[_states.Count - 1];
            transform.position = latestState.position;

            _playerController.IsCyan = latestState.iscyan;
            _playerController.ApplyColorAccordingToFlag(false); //the false here is important, we don't want to initiate a new trail while rewinding

            _states.RemoveAt(_states.Count - 1);
            _statesRemovedDuringRewinding++;

            StatesChangedSoUpdateLineRenderer();
            return -1; //minus one indicates that we haven't finished rewinding yet
        }
        else
        {
            StopRewind();
            return _statesRemovedDuringRewinding;
        }

    }


  public static  int MaxNumOfStates =(int) Mathf.Round(Seconds / Time.fixedDeltaTime);

    public void AddState(State state)// Vector3 position)
    {
        //it records one state per fixedupdate
        //time between fixedupdate calls is Time.fixedDeltaTime seconds
        //so one state every Time.fixedDeltaTime seconds
        //in 1 second that give 1/Time.fixedDeltaTime states
        //in Seconds seconds Seconds/Time.fixedDeltaTime states

        if (_states.Count > MaxNumOfStates)
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


    private int _statesRemovedDuringRewinding = 0;

    public void StartRewind()
    {
        _statesRemovedDuringRewinding = 0;
        Debug.Log("started rewinding");
   //     isRewinding = true;
        //    rb.isKinematic = true;
    }

    public void StopRewind()
    {
        Debug.Log("stopped rewinding");
     //   isRewinding = false;
        //   rb.isKinematic = false;
    }



}
