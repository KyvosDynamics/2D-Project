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
    List<PowerUpState> _states = new List<PowerUpState>();
    public bool CollectedChanged = false;
    public PowerUpState CurrentState = new PowerUpState();


    private void FixedUpdate()
    {
        if (PlayerController.StateGroupManager.IsRewinding)
        {
            OneRewind();
            return; //do not bother with physics when we are rewinding
        }



        PowerUpState newState = new PowerUpState(CurrentState);



        if (CollectedChanged)
        {
            CollectedChanged = false;

            newState.Collected = !CurrentState.Collected;
        }



        PutObjectInState(newState);

    }

    public void OneRewind()
    {
        PowerUpState newState = _states[_states.Count - 1];
        _states.RemoveAt(_states.Count - 1);

        PutObjectInState(newState);
    }


    private void PutObjectInState(PowerUpState newState)
    {
        //check what changed


        if (CurrentState.Collected != newState.Collected)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = !newState.Collected;
        }



        if (PlayerController.StateGroupManager.IsRewinding == false) //(don't add state while we are rewinding)
        {
            _states.Add(newState);
        }

        CurrentState = newState;
    }



}
