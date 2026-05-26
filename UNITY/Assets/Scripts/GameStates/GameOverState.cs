using WarOfTanks.StateMachine;
using UnityEngine;

/// <summary>
/// Terminal state — entered when the match ends by timer or score limit.
/// Freezes time and input, then shows the result screen. No exit transition; scene reload handles teardown.
/// </summary>
public class GameOverState : State<GameManager>
{
    public GameOverState(StateMachine<GameManager> machine) : base(machine) {}

    /// <summary>Order matters: freeze before showing UI so the game never renders an active frame behind the panel.</summary>
    protected override void Enter()
    {
        Context.EndMatch();
        Time.timeScale = 0f;
        Context.SetInputEnabled(false);
        Context.ShowGameOver();
    }
    protected override void Exit() { }
}

