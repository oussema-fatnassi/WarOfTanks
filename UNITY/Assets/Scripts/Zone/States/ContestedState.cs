using UnityEngine;
using WarOfTanks.Zone;
using WarOfTanks.StateMachine;

/// <summary>
/// Tanks from both teams are present simultaneously — the zone is frozen.
/// No capture progress is made and no points are scored while contested.
/// Transitions to CapturingState when one team's tanks all leave,
/// or NeutralState if all tanks leave.
/// </summary>
public class ContestedState : State<Zone>
{
    public ContestedState(StateMachine<Zone> machine) : base(machine) {}

    /// <summary>Freezes the capture gauge and shows the contested visual indicator.</summary>
    public override void Enter()
    {
        Debug.Log("Entered Contested State");
    }

    /// <summary>Restores the zone visual to the pre-conflict state.</summary>
    public override void Exit()
    {
        Debug.Log("Exited Contested State");
    }

    /// <summary>
    /// Monitors occupancy each frame.
    /// One team leaves → CapturingState for the remaining team.
    /// All tanks leave → NeutralState.
    /// </summary>
    public override void Execute()
    {
        Debug.Log("Executing Contested State");
    }
}
