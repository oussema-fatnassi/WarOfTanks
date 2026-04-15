using UnityEngine;
using WarOfTanks.Zone;
using WarOfTanks.StateMachine;

/// <summary>
/// One team has tanks in the zone and is actively filling the capture gauge.
/// Transitions to CapturedState when progress hits 100%, ContestedState if the enemy enters,
/// or NeutralState if all tanks leave (gauge then decays).
/// </summary>
public class CapturingState : State<Zone>
{
    public CapturingState(StateMachine<Zone> machine) : base(machine) {}

    /// <summary>Records which team is capturing and updates the zone color to that team's color.</summary>
    protected override void Enter()
    {
        Debug.Log("Entered Capturing State");
    }

    protected override void Exit()
    {
        Debug.Log("Exited Capturing State");
    }

    /// <summary>
    /// Increments captureProgress by captureSpeed each frame.
    /// Checks for conflict (enemy enters) → ContestedState.
    /// Checks for abandonment (all tanks leave) → decays gauge → NeutralState.
    /// Checks for full capture (progress == 100) → CapturedState.
    /// </summary>
    protected override void Execute()
    {
        Debug.Log("Executing Capturing State");
    }
}
