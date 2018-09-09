﻿using System.Collections.Generic;
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
        CurrentStateGroup.AddState(myState);
    }
}




public class StateDeltasGroup
{

    private List<PlayerStateDeltas> _stateDeltas = new List<PlayerStateDeltas>();

    private LineRenderer _lineRenderer = null;


    private const float Seconds = 2f;




    public StateDeltasGroup(PlayerState initialState)
    {
        LatestState = initialState;


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
        Debug.Log("inside rewind");
        if (_stateDeltas.Count > 0)
        {

            PlayerStateDeltas psd = _stateDeltas[_stateDeltas.Count - 1];

            LatestState = PlayerState.SubtractFromPlayerState(LatestState, psd);




            PlayerController.Instance.PutPlayerInState(LatestState);





            _stateDeltas.RemoveAt(_stateDeltas.Count - 1);
            _statesRemovedDuringRewinding++;

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

    public PlayerState LatestState;


    public void AddState(PlayerState state)// Vector3 position)
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


        _stateDeltas.Add(LatestState.FindDeltasToState(state));

        LatestState = state;// new PlayerState(state);



        //it records one state per fixedupdate
        //time between fixedupdate calls is Time.fixedDeltaTime seconds
        //so one state every Time.fixedDeltaTime seconds
        //in 1 second that give 1/Time.fixedDeltaTime states
        //in Seconds seconds Seconds/Time.fixedDeltaTime states

        if (_stateDeltas.Count > MaxNumOfStates)
            _stateDeltas.RemoveAt(0);// pointsInTime.Count - 1);


        //        _states.Add();// transform.rotation));

        // state);// new State(position, false));

        //     }


        StatesChangedSoUpdateLineRenderer();
    }


    private void StatesChangedSoUpdateLineRenderer()
    {
        _lineRenderer.positionCount = _stateDeltas.Count + 1; //plus 1 because for example for 3 points we have 2 intervals, so for 2 transitions 3 positions

        Vector3[] positions = new Vector3[_lineRenderer.positionCount];


        //    Debug.Log("" + _lineRenderer.positionCount + " positions:");



        //from most recent one to oldest one
        positions[positions.Length - 1] = LatestState.position;
        int ii = positions.Length - 1;
        //     Debug.Log("position " + ii + " =" + positions[ii].ToString());


        PlayerState clone = new PlayerState(LatestState);
        for (int i = positions.Length - 2; i >= 0; i--)
        {
            //  Debug.Log("about to subtract deltas " + _stateDeltas[i].ToString());
            clone = PlayerState.SubtractFromPlayerState(clone, _stateDeltas[i]);
            positions[i] = clone.position;// LastState.Subtract(_states[i], true));
                                          //  Debug.Log("position " + i + " =" + positions[i].ToString());
        }

        _lineRenderer.SetPositions(positions);// _states.Select(p => p.position).ToArray());// .ToList().ToArray());


        //Time.timeScale = 0;
    }




    private int _statesRemovedDuringRewinding = 0;

    public void ResetStatesRemovedDuringRewindingCounter()
    {
        _statesRemovedDuringRewinding = 0;
    }




}
