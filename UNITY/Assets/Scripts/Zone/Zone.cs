using UnityEngine;
using System.Collections.Generic;
using WarOfTanks.StateMachine;

namespace WarOfTanks.Zone
{
    /// <summary>
    /// The Control Zone — the central game mechanic.
    /// Tracks which teams have tanks inside and manages capture progress.
    /// Owns the ZoneCaptureStateMachine which drives all state transitions.
    /// </summary>
    public class Zone : MonoBehaviour
    {
        /// <summary>Team ID of the team currently controlling the zone. -1 means neutral.</summary>
        public int controllingTeam;

        /// <summary>Current capture progress as a percentage (0–100).</summary>
        public float captureProgress;

        /// <summary>List of team IDs currently contesting the zone (both teams present).</summary>
        public List<int> contestedBy = new List<int>();

        /// <summary>Returns true if tanks from more than one team are present simultaneously.</summary>
        public bool IsContested()
        {
            return contestedBy.Count > 0;
        }

        /// <summary>Returns true if a team has fully captured the zone (captureProgress == 100).</summary>
        public bool IsCaptured()
        {
            return controllingTeam != -1;
        }
    }
}