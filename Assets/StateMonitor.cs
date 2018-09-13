using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpState
{
//    public Vector3 Position;
    public bool Collected;

}

public class StateMonitor : MonoBehaviour
{
    List<PowerUpState> _states = new List<PowerUpState>();
    public bool Collected = false;

    private void FixedUpdate()
    {
        if (PlayerController. StateGroupManager.IsRewinding)
        {            
            OneRewind();
            return; //do not bother with physics when we are rewinding
        }


        PowerUpState pus = new PowerUpState() {// Position = transform.position,
            Collected = this.Collected };
        _states.Add(pus);



        PutObjectInState(pus);
            //gameObject.GetComponent<SpriteRenderer>().enabled = !Collected;
        
    }

    public void OneRewind()
    {
        PowerUpState newState = _states[_states.Count - 1];
        _states.RemoveAt(_states.Count - 1);

        PutObjectInState(newState);
            
    }

    private void PutObjectInState(PowerUpState pus)
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = !pus.Collected;
    }



}
