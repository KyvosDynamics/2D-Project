using System.Collections.Generic;
using UnityEngine;

public class StateDeltasGroupManager
{
    public StateDeltasGroup CurrentStateGroup { get; private set; }
    public bool IsRewinding { get; private set; }
    private List<StateDeltasGroup> _stateDeltasGroups = new List<StateDeltasGroup>();




    public void OneRewind()
    {
        int statesremoved = CurrentStateGroup.Rewind();
        if (statesremoved != -1)
        {//the current stategroup has finished rewinding
            //but has it finished rewinding because it reached the max allowed number of recorded states or because it has run out of states?
            if (statesremoved < StateDeltasGroup.MaxNumOfStates)
            {//it has run out of states
                //we want to remove this state group if we have others
                if (_stateDeltasGroups.Count > 1)
                {

                    _stateDeltasGroups.Remove(CurrentStateGroup);
                    CurrentStateGroup = _stateDeltasGroups[_stateDeltasGroups.Count - 1];
                    CurrentStateGroup.ResetStatesRemovedDuringRewindingCounter();
                }
                else
                {
                    IsRewinding = false;
                }


            }
            else
            {
                IsRewinding = false;
            }


        }

    }

    public void InitiateRewinding()
    {
        IsRewinding = true;
        CurrentStateGroup.ResetStatesRemovedDuringRewindingCounter();
    }

    internal void StartNewStateGroup(PlayerState firstState)
    {
        CurrentStateGroup = new StateDeltasGroup(firstState);
        _stateDeltasGroups.Add(CurrentStateGroup);
    }

    internal void AddStateToCurrentGroup(PlayerState myState)
    {
        if (CurrentStateGroup != null)
            CurrentStateGroup.AddState(myState);
    }
}




public class StateDeltasGroup
{

    private List<ChangedFieldsCollection> _stateTransitions = new List<ChangedFieldsCollection>();
    private List<Vector3> _linePositions = new List<Vector3>(); //line positions is like a record of states but only for position. It should always be larger than the statetransitions list by one element

    private LineRenderer _lineRenderer = null;


    private const float Seconds = 12f;




    public StateDeltasGroup(PlayerState initialState)
    {
        GameObject TronTrailHolder = new GameObject("TronTrailHolder");
        _lineRenderer = TronTrailHolder.AddComponent<LineRenderer>();
        _lineRenderer.sortingLayerName = initialState.IsTrailInForeground ? "Player" : "RoomObjectsBehindPlayer";
        _lineRenderer.sortingOrder = -10; //so that for example it is behind a portal effect        
        _lineRenderer.startColor = _lineRenderer.endColor = initialState.IsTrailCyan ? Color.cyan : Color.green;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));//or Shader.Find("Particles/Additive")); ?
        _lineRenderer.startWidth = _lineRenderer.endWidth = 0.5f;
    }




    public int Rewind()
    {
        //Debug.Log("inside rewind");
        if (_stateTransitions.Count > 0)
        {

            ChangedFieldsCollection psd = _stateTransitions[_stateTransitions.Count - 1];

            PlayerState previousState = PlayerController.Instance.CurrentState.RetrievePreviousState(psd);// PlayerState.SubtractFromPlayerState(new PlayerState(), psd);



            //Debug.Log("previous state powerups " + previousState.NumOfPowerUps);//.Count);

            PlayerController.Instance.PutPlayerInState(previousState);// new PlayerState(newState));





            _stateTransitions.RemoveAt(_stateTransitions.Count - 1);
            _statesRemovedDuringRewinding++;

            _linePositions.RemoveAt(_linePositions.Count - 1);
            StatesChangedSoUpdateLineRenderer();



            return -1; //minus one indicates that we haven't finished rewinding yet
        }
        else
        {
            //StopRewind();
            return _statesRemovedDuringRewinding;
        }

    }


    public static int MaxNumOfStates = (int)Mathf.Round(Seconds / Time.fixedDeltaTime);



    public void AddState(PlayerState newState)// Vector3 position)
    {
        //       if(LastState==null)
        //     {//initial state, no deltas to compute
        //
        //        LastState = state;
        //    }
        //   else
        //   {
        //      if (LastState == null)
        //    {
        //
        //      //  _stateDeltas.Add(state.FindDeltasToState(state));//zero deltas
        // }
        //else


        if (_linePositions.Count > 0)
            _stateTransitions.Add(PlayerController.Instance.CurrentState.FindDeltasToState(newState));

        _linePositions.Add(newState.Position);


        //   LatestState = state;// new PlayerState(state);

        //it records one state per fixedupdate
        //time between fixedupdate calls is Time.fixedDeltaTime seconds
        //so one state every Time.fixedDeltaTime seconds
        //in 1 second that give 1/Time.fixedDeltaTime states
        //in Seconds seconds Seconds/Time.fixedDeltaTime states



        if (_linePositions.Count > MaxNumOfStates)
        {
            _stateTransitions.RemoveAt(0);// pointsInTime.Count - 1);
            _linePositions.RemoveAt(0);
        }


        //        _states.Add();// transform.rotation));
        // state);// new State(position, false));
        //     }







        StatesChangedSoUpdateLineRenderer();

    }




    private void StatesChangedSoUpdateLineRenderer()//PlayerState newState)
    {
        _lineRenderer.positionCount = _linePositions.Count;// _stateTransitions.Count + 1; //plus 1 because for example for 3 points we have 2 intervals, so for 2 transitions 3 positions


        //Vector3[] positions = new Vector3[_lineRenderer.positionCount];
        //    Debug.Log("" + _lineRenderer.positionCount + " positions:");
        //       //from most recent one to oldest one
        //     positions[positions.Length - 1] = newState.Position;// PlayerController.Instance.CurrentState.position;
        //   int ii = positions.Length - 1;
        // //     Debug.Log("position " + ii + " =" + positions[ii].ToString());
        //
        //
        //   PlayerState clone = new PlayerState(newState);// PlayerController.Instance.CurrentState);//  LatestState);
        // for (int i = positions.Length - 2; i >= 0; i--)
        //{
        //   //  Debug.Log("about to subtract deltas " + _stateDeltas[i].ToString());
        //  clone = clone.RetrievePreviousState(_stateTransitions[i]);
        // positions[i] = clone.Position;// LastState.Subtract(_states[i], true));
        //                              //  Debug.Log("position " + i + " =" + positions[i].ToString());
        //}



        _lineRenderer.SetPositions(_linePositions.ToArray());// positions);// _states.Select(p => p.position).ToArray());// .ToList().ToArray());


        //Time.timeScale = 0;
    }




    private int _statesRemovedDuringRewinding = 0;

    public void ResetStatesRemovedDuringRewindingCounter()
    {
        _statesRemovedDuringRewinding = 0;
    }




}