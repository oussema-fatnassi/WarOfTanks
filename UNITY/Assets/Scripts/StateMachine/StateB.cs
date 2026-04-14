// Smoke test state — remove once real states are implemented
using UnityEngine;
using WarOfTanks.StateMachine;

public class StateB : IState<MonoStateTest>
{
    public void Enter(MonoStateTest context)
    {
        Debug.Log("State B Entered");
    }

    public void Execute(MonoStateTest context)
    {
        Debug.Log("State B Executing");
    }

    public void Exit(MonoStateTest context)
    {
        Debug.Log("State B Exited");
    }
}
