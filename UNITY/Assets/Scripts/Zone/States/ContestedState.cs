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
    protected override void Enter()
    {
        DebugLogger.Log(Context.ShowDebugLogs, "Entered Contested State", nameof(ContestedState));
        Context.UI.SetContested();
    }

    /// <summary>
    /// Monitors occupancy each frame.
    /// One team leaves → CapturingState for the remaining team.
    /// All tanks leave → NeutralState.
    /// </summary>
    protected override void Execute()
    {
        DebugLogger.Log(Context.ShowDebugLogs, "Executing Contested State", nameof(ContestedState));
        if (Context.PlayerTankCount == 0 && Context.AITankCount == 0)
        {
            Machine.ChangeState(new NeutralState(Machine));
        }
        else if (Context.PlayerTankCount > 0 && Context.AITankCount == 0)
        {
            if (Context.controllingTeam != 0) 
            {
                Context.ResetProgress();
            }
            Context.controllingTeam = 0;
            Machine.ChangeState(new CapturingState(Machine));
        }
        else if (Context.AITankCount > 0 && Context.PlayerTankCount == 0)
        {
            if (Context.controllingTeam != 1) 
            {
                Context.ResetProgress();
            }
            Context.controllingTeam = 1;
            Machine.ChangeState(new CapturingState(Machine));
        }
    }
    
    /// <summary>Restores the zone visual to the pre-conflict state.</summary>
    protected override void Exit()
    {
        DebugLogger.Log(Context.ShowDebugLogs, "Exited Contested State", nameof(ContestedState));
    }

}
