// test state machine

using UnityEngine;
using WarOfTanks.StateMachine;

public class StateA : IState
{
    public void Enter()
    {
        Debug.Log("State A Entered");
    }

    public void Update()
    {
        Debug.Log("State A Updated");
    }

    public void Exit()
    {
        Debug.Log("State A Exited");
    }
}