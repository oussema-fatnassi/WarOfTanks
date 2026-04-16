using UnityEngine;
using WarOfTanks.StateMachine;

namespace WarOfTanks.Zone
{
    public class ZoneCaptureStateMachine : StateMachine<Zone>
    {
        public ZoneCaptureStateMachine(Zone zone) : base(zone)
        {
            // Start in the "neutral" state, where the zone is not being captured by any team.
            ChangeState(new NeutralState(this));
        }
    }
}