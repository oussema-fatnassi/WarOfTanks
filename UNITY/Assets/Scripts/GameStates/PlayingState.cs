using UnityEngine;
using WarOfTanks.StateMachine;
/// <summary>
/// Default active match state. Starts the timer on Enter. Listens for Escape to transition to PausedState.
/// </summary>
public class PlayingState : State<GameManager>
{
    public PlayingState(StateMachine<GameManager> machine) : base(machine) {}
    protected override void Enter()
    {
        Context.StartMatch();
    }
    protected override void Execute()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Machine.ChangeState(new PausedState(Machine));
        //TODO: verify if this pause is acceptable or we use new input system
    }
    protected override void Exit(){}
}