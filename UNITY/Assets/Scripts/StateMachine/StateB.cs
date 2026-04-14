using UnityEngine;
using WarOfTanks.StateMachine;

public class StateB : IState
{
    public void Enter()
    {
        Debug.Log("State B Entered");
    }

    public void Update()
    {
        Debug.Log("State B Updated");
    }

    public void Exit()
    {
        Debug.Log("State B Exited");
    }
}