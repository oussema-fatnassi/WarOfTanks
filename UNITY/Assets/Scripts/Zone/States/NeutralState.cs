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
    protected override void Enter()
    {
        Debug.Log("Entered Neutral State");
        Context.ControllingTeam = -1;
        Context.ResetProgress();
        Context.UI.SetNeutral();
    }

    /// <summary>
    /// Checks zone occupancy each frame.
    /// One team present → CapturingState. Both teams present → ContestedState.
    /// </summary>
    protected override void Execute()
    {
        Debug.Log("Executing Neutral State");
        if(Context.IsContested())
        {
            Machine.ChangeState(new ContestedState(Machine));
        }
        else if(Context.PlayerTankCount > 0 && Context.AITankCount == 0)
        {
            Context.ControllingTeam = 0;
            Machine.ChangeState(new CapturingState(Machine));
        }
        else if(Context.AITankCount > 0 && Context.PlayerTankCount == 0)
        {
            Context.ControllingTeam = 1;
            Machine.ChangeState(new CapturingState(Machine));
        }
    }

    protected override void Exit()
    {
        Debug.Log("Exited Neutral State");
    }
}
