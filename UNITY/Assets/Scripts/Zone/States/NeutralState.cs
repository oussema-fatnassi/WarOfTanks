using UnityEngine;
using WarOfTanks.Zone;
using WarOfTanks.StateMachine;

/// <summary>
/// The zone is uncaptured — no team has tanks inside, or the zone has reset after a conflict.
/// Transitions to CapturingState when one team enters, or ContestedState when both teams are present.
/// </summary>
public class NeutralState : State<Zone>
{
    public NeutralState(StateMachine<Zone> machine) : base(machine) {}

    /// <summary>Resets the zone visual to neutral (gray) and clears capture progress.</summary>
    public override void Enter()
    {
        Debug.Log("Entered Neutral State");
    }

    public override void Exit()
    {
        Debug.Log("Exited Neutral State");
    }

    /// <summary>
    /// Checks zone occupancy each frame.
    /// One team present → CapturingState. Both teams present → ContestedState.
    /// </summary>
    public override void Execute()
    {
        Debug.Log("Executing Neutral State");
    }
}
