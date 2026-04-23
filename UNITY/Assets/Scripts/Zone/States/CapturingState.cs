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
        DebugLogger.Log(Context.ShowDebugLogs, "Entered Capturing State", nameof(CapturingState));
        Context.UI.SetCapturing(Context.controllingTeam == 0);
    }

    /// <summary>
    /// Increments captureProgress by captureSpeed each frame.
    /// Checks for conflict (enemy enters) → ContestedState.
    /// Checks for abandonment (all tanks leave) → decays gauge → NeutralState.
    /// Checks for full capture (progress == 100) → CapturedState.
    /// </summary>
    protected override void Execute()
    {
        DebugLogger.Log(Context.ShowDebugLogs, "Executing Capturing State", nameof(CapturingState));
        if(Context.IsContested())
        {
            Machine.ChangeState(new ContestedState(Machine));
        }
        else if(Context.PlayerTankCount == 0 && Context.AITankCount == 0)
        {
            Context.DecayGauge(Time.deltaTime);
            if(Context.captureProgress <= 0f)
            {
                Machine.ChangeState(new NeutralState(Machine));
            }
        }
        else if (Context.controllingTeam == 0 && Context.PlayerTankCount == 0 && Context.AITankCount > 0 ||
           Context.controllingTeam == 1 && Context.AITankCount == 0 && Context.PlayerTankCount > 0)
        {
            int newTeam = Context.AITankCount > 0 ? 1 : 0;
            Context.ResetProgress();
            Context.controllingTeam = newTeam;
            Context.UI.SetCapturing(newTeam == 0);
        }
        else if(Context.captureProgress >= 100f)
        {
            Machine.ChangeState(new CapturedState(Machine));
        }
        else
        {
            Context.IncrementProgress(Time.deltaTime);
        }
    }

    protected override void Exit()
    {
        DebugLogger.Log(Context.ShowDebugLogs, "Exited Capturing State", nameof(CapturingState));
    }
}
