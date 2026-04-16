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

    private float _scoringTimer;
    private float _timeoutTimer;

    /// <summary>Sets the controlling team, starts the scoring timer, and updates the zone color.</summary>
    protected override void Enter()
    {
        Debug.Log("Entered Captured State");
        Context.UI.SetCaptured(Context.controllingTeam == 0);
        _scoringTimer = 0f;
        _timeoutTimer = 0f;
    }

    /// <summary>
    /// Awards points to the controlling team at scoringRate per second while tanks are present.
    /// If no friendly tanks remain, starts the capturedTimeout countdown before decaying the gauge.
    /// Enemy tanks entering → ContestedState. Gauge dropping below 100 → CapturingState.
    /// </summary>
    protected override void Execute()
    {
        Debug.Log("Executing Captured State");
        if (Context.IsContested())
        {
            Machine.ChangeState(new ContestedState(Machine));
        }
        else if (Context.controllingTeam == 0 ? Context.PlayerTankCount > 0 : Context.AITankCount > 0)
        {
            _timeoutTimer = 0f;
            _scoringTimer += Time.deltaTime;
            if (_scoringTimer >= Context.ScoringRate)
            {
                _scoringTimer = 0f;
                // TODO: Add score to the controlling team
                Debug.Log("Scoring for team " + Context.controllingTeam);
                //Context.UI.AddScore(Context.controllingTeam);
            }
        }
        else
        {
            _timeoutTimer += Time.deltaTime;
            if (_timeoutTimer >= Context.CapturedTimeout)
            {
                Context.DecayGauge(Time.deltaTime);
                if (Context.captureProgress < 100f)
                {
                    Machine.ChangeState(new CapturingState(Machine));
                }
            }
        }
    }

    /// <summary>Stops the scoring timer and resets any timeout countdown.</summary>
    protected override void Exit()
    {
        Debug.Log("Exited Captured State");
        _scoringTimer = 0f;
        _timeoutTimer = 0f;
    }
}
