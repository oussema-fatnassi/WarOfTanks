using UnityEngine;
using WarOfTanks.Zone;
using WarOfTanks.StateMachine;

/// <summary>
/// One team has fully captured the zone (captureProgress == 100) and is scoring points.
/// Transitions to ContestedState if the enemy enters, or back to CapturingState if the
/// gauge drops below 100 after the capturedTimeout decay delay.
/// </summary>
public class CapturedState : State<Zone>
{
    public CapturedState(StateMachine<Zone> machine) : base(machine) {}

    /// <summary>Sets the controlling team, starts the scoring timer, and updates the zone color.</summary>
    public override void Enter()
    {
        Debug.Log("Entered Captured State");
    }

    /// <summary>Stops the scoring timer and resets any timeout countdown.</summary>
    public override void Exit()
    {
        Debug.Log("Exited Captured State");
    }

    /// <summary>
    /// Awards points to the controlling team at scoringRate per second while tanks are present.
    /// If no friendly tanks remain, starts the capturedTimeout countdown before decaying the gauge.
    /// Enemy tanks entering → ContestedState. Gauge dropping below 100 → CapturingState.
    /// </summary>
    public override void Execute()
    {
        Debug.Log("Executing Captured State");
    }
}
