using System.Collections.Generic;
using UnityEngine;

public class PowerUpState:IState
{
    public bool Collected;

    public PowerUpState()
    {

    }
    public PowerUpState(PowerUpState powerUpState)
    {
        this.Collected = powerUpState.Collected;
    }

    public ChangedFieldsCollection FindChangedFieldsComparedTo(IState statef)// PlayerState state)
    {
        ChangedFieldsCollection result = new ChangedFieldsCollection();


        PowerUpState state = (PowerUpState)statef;



        if (Collected != state.Collected)
            result.AddChangedField(new ChangedField(Collected, "Collected", typeof(bool)));

        return result;
    }
    public IState RetrievePreviousState(ChangedFieldsCollection cfc)
    {//it doesn't affect the original object, it creates a clone

        PowerUpState clone = new PowerUpState(this);

        foreach (ChangedField cf in cfc.ChangedFields)
        {
            typeof(PowerUpState).GetField(cf.Name).SetValue(clone, cf.OldValue);

        }

        return clone;
    }


}

public class StateMonitor : MonoBehaviour
{
    public bool CollectedChanged = false;

    StateDeltasGroupManager2 m2;// new StateDeltasGroupManager2(this);


    private void Awake()
    {
        m2 = new StateDeltasGroupManager2(this);
    }

    private void FixedUpdate()
    {
        if (PlayerController.StateGroupManager.IsRewinding)
        {
            //OneRewind();
             m2.CurrentStateGroup.OneRewind();

            //PutObjectInState(newState);


            return; //do not bother with physics when we are rewinding
        }



        PowerUpState newState = new PowerUpState(m2.CurrentState);



        if (CollectedChanged)
        {
            CollectedChanged = false;

            newState.Collected = !m2.CurrentState.Collected;
        }



        PutObjectInState(newState);

    }



    public void PutObjectInState(PowerUpState newState)
    {
        //check what changed


        if (m2.CurrentState.Collected != newState.Collected)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = !newState.Collected;
        }



        if (PlayerController.StateGroupManager.IsRewinding == false) //(don't add state while we are rewinding)
        {
            AddStateToCurrentGroup(newState);//  _states.Add(newState);
        }

        m2.CurrentState = newState;
    }


    void AddStateToCurrentGroup(PowerUpState myState)
    {
        if (m2.CurrentStateGroup != null)
            m2.CurrentStateGroup.AddState(myState);
    }


   

}
public class StateDeltasGroupManager2
{
    //public static StateDeltasGroupManager2 Instance = null;

    public StateMonitor mysm;
    public StateDeltasGroupManager2(StateMonitor sm)
    {
        mysm = sm;//        Instance = this;

        CurrentStateGroup = new PowerUpStateGroup(sm,this);
    }

    public PowerUpStateGroup CurrentStateGroup;
    public PowerUpState CurrentState = new PowerUpState();
}



public class PowerUpStateGroup
{
    //List<PowerUpState> _states = new List<PowerUpState>();
    private List<ChangedFieldsCollection> _stateTransitions = new List<ChangedFieldsCollection>();
    private int _numOfStates = 0;
    StateDeltasGroupManager2 sd;

    public PowerUpStateGroup(StateMonitor sm, StateDeltasGroupManager2 sd)
    {
        this.sm = sm;
        this.sd = sd;
    }
    StateMonitor sm;

    public PowerUpState OneRewind()
    {
//        PowerUpState newState = _states[_states.Count - 1];
  //      _states.RemoveAt(_states.Count - 1);
      //  return newState;






        ChangedFieldsCollection psd = _stateTransitions[_stateTransitions.Count - 1];

        PowerUpState previousState =(PowerUpState) sd.CurrentState.RetrievePreviousState(psd);// PlayerState.SubtractFromPlayerState(new PlayerState(), psd);



        //Debug.Log("previous state powerups " + previousState.NumOfPowerUps);//.Count);

        sm.PutObjectInState(previousState);// new PlayerState(newState));





        _stateTransitions.RemoveAt(_stateTransitions.Count - 1);
        //        _statesRemovedDuringRewinding++;

        _numOfStates--;//   _linePositions.RemoveAt(_linePositions.Count - 1);
                       //      StatesChangedSoUpdateLineRenderer();


        return previousState;
      //  return -1; //minus one indicates that we haven't finished rewinding yet
    }

    public void AddState(PowerUpState state)
    {
        //_states.Add(state);

        if (_numOfStates>0)// _linePositions.Count > 0)
            _stateTransitions.Add(sd.CurrentState.FindChangedFieldsComparedTo(state));

        _numOfStates++;
    }
}
