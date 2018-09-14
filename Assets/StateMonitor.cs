using System.Collections.Generic;
using UnityEngine;

public class PowerUpState
{
    public bool Collected;

    public PowerUpState()
    {

    }
    public PowerUpState(PowerUpState powerUpState)
    {
        this.Collected = powerUpState.Collected;
    }
}

public class StateMonitor : MonoBehaviour
{
    public bool CollectedChanged = false;
  


    private void FixedUpdate()
    {
        if (PlayerController.StateGroupManager.IsRewinding)
        {
            OneRewind();
            return; //do not bother with physics when we are rewinding
        }



        PowerUpState newState = new PowerUpState(m2. CurrentState);



        if (CollectedChanged)
        {
            CollectedChanged = false;

            newState.Collected = !m2. CurrentState.Collected;
        }



        PutObjectInState(newState);

    }

    public void OneRewind()
    {
      PowerUpState newState= m2.CurrentStateGroup.OneRewind();

        PutObjectInState(newState);
    }


    private void PutObjectInState(PowerUpState newState)
    {
        //check what changed


        if (m2. CurrentState.Collected != newState.Collected)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = !newState.Collected;
        }



        if (PlayerController.StateGroupManager.IsRewinding == false) //(don't add state while we are rewinding)
        {
            AddStateToCurrentGroup(newState);//  _states.Add(newState);
        }

        m2. CurrentState = newState;
    }


     void AddStateToCurrentGroup(PowerUpState myState)
    {
        if (m2.CurrentStateGroup != null)
            m2.CurrentStateGroup.AddState(myState);
    }


    StateDeltasGroupManager2 m2 = new StateDeltasGroupManager2();
    
}
public class StateDeltasGroupManager2
{
    public PowerUpStateGroup CurrentStateGroup = new PowerUpStateGroup();
    public  PowerUpState CurrentState = new PowerUpState();
}



public class PowerUpStateGroup
{
    List<PowerUpState> _states = new List<PowerUpState>();

    public PowerUpState OneRewind()
    {
        PowerUpState newState = _states[_states.Count - 1];
        _states.RemoveAt(_states.Count - 1);
        return newState;
    }

    public void AddState(PowerUpState state)
    {
        _states.Add(state);
    }
}
