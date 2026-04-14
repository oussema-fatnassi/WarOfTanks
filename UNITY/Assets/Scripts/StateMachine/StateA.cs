// Smoke test state — remove once real states are implemented
using UnityEngine;
using WarOfTanks.StateMachine;

public class StateA : IState<MonoStateTest>
{
    public void Enter(MonoStateTest context)
    {
        Debug.Log("State A Entered");
    }

    public void Execute(MonoStateTest context)
    {
        Debug.Log("State A Executing");
    }

    public void Exit(MonoStateTest context)
    {
        Debug.Log("State A Exited");
    }
}
