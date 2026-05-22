
using WarOfTanks.StateMachine;

/// <summary>
/// Bootstrap subclass of StateMachine&lt;GameManager&gt;.
/// Uses the protected constructor pattern to avoid the circular dependency of passing
/// an initial state before the machine reference exists.
/// </summary>
public class GameStateMachine : StateMachine<GameManager>
{
    public GameStateMachine(GameManager manager) : base(manager)
    {
        // Enters PlayingState immediately — the match begins as soon as the scene loads.
        ChangeState(new PlayingState(this));
    }
}
