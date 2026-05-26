using WarOfTanks.StateMachine;
using UnityEngine;
/// <summary>
/// Pauses the match: freezes Time.timeScale, stops the timer, shows the pause panel, and disables player input.
/// Exit restores all of these. Escape toggles back to PlayingState.
/// </summary>
public class PausedState : State<GameManager>
{
    public PausedState(StateMachine<GameManager> machine) : base(machine) { }

    protected override void Enter()
    {
        Time.timeScale = 0f;
        Context.PauseMatch();
        Context.SetPauseUI(true);
        Context.SetInputEnabled(false);
    }
    protected override void Execute()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Machine.ChangeState(new PlayingState(Machine));
    }

    protected override void Exit()
    {
        Context.SetPauseUI(false);
        Context.SetInputEnabled(true);
        Time.timeScale = 1f;
    }
}
